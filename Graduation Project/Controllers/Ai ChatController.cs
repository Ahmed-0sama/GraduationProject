using gp.Models;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Graduation_Project.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AiChatController : ControllerBase
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration configuration;
		private readonly AppDbContext db;
		private readonly UserManager<User> userManager;

		public AiChatController(IHttpClientFactory httpClientFactory, IConfiguration configuration, AppDbContext db, UserManager<User> userManager)
		{
			_httpClient = httpClientFactory.CreateClient();
			this.configuration = configuration;
			this.db = db;
			this.userManager = userManager;
		}

		[Authorize]
		[HttpPost("chat")]
		public async Task<IActionResult> Chat([FromBody] AiRequest request)
		{
			if (string.IsNullOrEmpty(request.SessionId))
			{
				return BadRequest(new { error = "SessionId is required" });
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound("User Not Found");
			}

			// Get user's purchased products
			var purchased = await db.PurchasedProducts
				.Where(u => u.UserId == user.Id)
				.Select(p => new PurchasedProductDTO
				{
					ProductName = p.ItemName,
					Category = p.Category,
					Date = p.Date,
					Price = p.Price,
					Quantity = p.Quantity,
					ShopName = p.ShopName
				})
				.ToListAsync();

			// Get user's monthly bills
			var bills = await db.MonthlyBills
				.Where(m => m.UserId == user.Id)
				.Select(m => new returnMonthlybills
				{
					Name = m.Name,
					Issuer = m.Issuer,
					Category = m.Category,
					Amount = m.Amount,
					Duration = m.Duration,
					StartDate = m.StartDate,
					EndDate = m.EndDate
				})
				.ToListAsync();

			// Retrieve or create chat history for this session
			if (!ChatHistoryStore.Histories.TryGetValue(request.SessionId, out var history))
			{
				history = new List<MessageEntry>();

				// Add system message to guide AI behavior
				history.Add(new MessageEntry
				{
					Role = "user",
					Text = "You are given the data about purchased products and monthly bills. Wait for the user's questions before responding."
				});

				// Add initial user context with data serialized
				var initialContext = new
				{
					purchasedProducts = purchased,
					monthlyBills = bills
				};

				history.Add(new MessageEntry
				{
					Role = "user",
					Text = JsonSerializer.Serialize(initialContext)
				});

				ChatHistoryStore.Histories[request.SessionId] = history;
			}

			// Serialize current user message along with data context
			var userContext = new
			{
				message = request.Message,
				purchasedProducts = purchased,
				monthlyBills = bills
			};

			string userContextJson = JsonSerializer.Serialize(userContext, new JsonSerializerOptions
			{
				WriteIndented = false
			});

			// Add current user message to history
			history.Add(new MessageEntry
			{
				Role = "user",
				Text = userContextJson
			});

			// Prepare request body for Gemini API
			var contentsList = history.Select(msg => new
			{
				role = msg.Role,
				parts = new[] { new { text = msg.Text } }
			}).ToList();

			var body = new { contents = contentsList };

			var geminiApiKey = configuration["GeminiApi:geminiApiKey"];
			var httpContent = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync(
				$"https://generativelanguage.googleapis.com/v1/models/gemini-2.0-flash:generateContent?key={geminiApiKey}",
				httpContent
			);

			var responseString = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
			{
				return BadRequest(new { error = "Request failed", statusCode = response.StatusCode, raw = responseString });
			}

			try
			{
				var json = JsonDocument.Parse(responseString);

				if (json.RootElement.TryGetProperty("candidates", out var candidates) &&
					candidates.GetArrayLength() > 0 &&
					candidates[0].TryGetProperty("content", out var content) &&
					content.TryGetProperty("parts", out var parts) &&
					parts.GetArrayLength() > 0 &&
					parts[0].TryGetProperty("text", out var textElement))
				{
					var reply = textElement.GetString();

					// Add AI model reply to chat history
					history.Add(new MessageEntry { Role = "model", Text = reply });

					// Save updated history
					ChatHistoryStore.Histories[request.SessionId] = history;

					// Return AI reply and updated history
					return Ok(new { reply, history });
				}
				else
				{
					return BadRequest(new { error = "Unexpected response format", raw = responseString });
				}
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = "Exception while parsing response", ex.Message, raw = responseString });
			}
		}
	}
}
