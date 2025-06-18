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
using System.Text.RegularExpressions;

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
			var bills = await db.MonthlyBills
				.Where(m => m.UserId == user.Id)
				.Select(m => new ReturnMonthlyBills
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

					history.Add(new MessageEntry { Role = "model", Text = reply });

					ChatHistoryStore.Histories[request.SessionId] = history;

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
		[Authorize]
		[HttpGet("getaisuggestionforfinancial")]
		public async Task<IActionResult> getaisuggestionforfinancial(int salary)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound("User Not Found");
			}
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
			var bills = await db.MonthlyBills
				.Where(m => m.UserId == user.Id)
				.Select(m => new ReturnMonthlyBills
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
			var prompt = @$"
				A user earns a salary of {salary} in egyptian money and its my salary in month.

				Here is their recent purchase history:
				{JsonSerializer.Serialize(purchased, new JsonSerializerOptions { WriteIndented = true })}

				Here are their monthly bills:
				{JsonSerializer.Serialize(bills, new JsonSerializerOptions { WriteIndented = true })}

				Based on this data, suggest the best financial goals they can work toward. Be practical, realistic, and data-driven.
				 make it in 5-8 lines only and tell me specific financial goal to save each month";
			var requestPayload = new
			{
				contents = new[]
				{
					new
					{
				parts = new object[]
				{
					new { text = prompt }
				}
					}
				}
			};

			var geminiApiKey = configuration["GeminiApi:geminiApiKey"];

			using var client = new HttpClient();
			var requestJson = JsonSerializer.Serialize(requestPayload);
			var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

			var response = await client.PostAsync(
				$"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={geminiApiKey}",
				httpContent
			);

			if (!response.IsSuccessStatusCode)
			{
				var error = await response.Content.ReadAsStringAsync();
				return StatusCode((int)response.StatusCode, new { error = "Gemini API error", details = error });
			}

			var jsonResponse = await response.Content.ReadAsStringAsync();
			var geminiResponse = JsonDocument.Parse(jsonResponse);

			var replyText = geminiResponse.RootElement
				.GetProperty("candidates")[0]
				.GetProperty("content")
				.GetProperty("parts")[0]
				.GetProperty("text")
				.GetString();

			return Ok(new { suggestion = replyText });
		}
	}
}
