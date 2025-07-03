using Google.Apis.Drive.v3.Data;
using gp.Models;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Graduation_Project.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;
using User = gp.Models.User;

namespace Graduation_Project.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ToBuyListController : ControllerBase
	{
		private readonly UserManager<User> userManager;
		private readonly AppDbContext db;
		private readonly AmazonScrappingService amazon;
		private readonly NoonScrappingService noon;
		private readonly JumiaScrappingService jumia;
		private readonly IConfiguration configuration;
		public ToBuyListController(UserManager<User> userManager, AppDbContext db, AmazonScrappingService amazon, NoonScrappingService noon, JumiaScrappingService jumia, IConfiguration configuration)
		{
			this.userManager = userManager;
			this.db = db;
			this.amazon = amazon;
			this.noon = noon;
			this.jumia = jumia;
			this.configuration = configuration;
		}
		[HttpPost("AddToBuyList")]
		[Authorize]
		public async Task<IActionResult> AddToBuyList(SendToBuyListDTO dto)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
				if (user == null)
				{
					return NotFound("User not found");
				}
				using var transaction = await db.Database.BeginTransactionAsync();
				try
				{
					var existingList = db.ToBuyLists
					.FirstOrDefault(l => l.ProductName == dto.ProductName);
					if (existingList == null)
					{
						ToBuyList toBuyList = new ToBuyList
						{
							ProductName = dto.ProductName,
						};

						await db.ToBuyLists.AddAsync(toBuyList);
						await db.SaveChangesAsync();

						string category = await GetProductCategoryFromGeminiAsync(dto.ProductName);
						var amazonTask = amazon.StartScraping(dto.ProductName, toBuyList.ListId, category);
						var noonTask = noon.StartScraping(dto.ProductName, toBuyList.ListId, category);
						var jumiaTask = jumia.StartScraping(dto.ProductName, toBuyList.ListId, category);
						await Task.WhenAll(amazonTask, noonTask, jumiaTask);
						UserToBuyList userlist = new UserToBuyList
						{
							UserId = user.Id,
							ToBuyListId = toBuyList.ListId
						};
						await db.UserToBuyLists.AddAsync(userlist);
					}
					else
					{
						var isAlreadyPresent = db.UserToBuyLists
						.Any(ul => ul.UserId == user.Id && ul.ToBuyListId == existingList.ListId);
						if (isAlreadyPresent)
						{
							return Ok("Item already Present");
						}
						var userlist = new UserToBuyList
						{
							UserId = user.Id,
							ToBuyListId = existingList.ListId
						};
						await db.UserToBuyLists.AddAsync(userlist);
					}
					await db.SaveChangesAsync();
					await transaction.CommitAsync(); // Commit transaction
					return Ok("Item Saved");

				}
				catch
				{
					await transaction.RollbackAsync(); // Rollback on error
					return StatusCode(500, "An error occurred while saving the item.");
				}
			}
			return BadRequest(ModelState);
		}
		private async Task<string> GetProductCategoryFromGeminiAsync(string itemName)
		{

			var prompt = $@"
				Classify the following item into one of these categories: Clothes, Electronics, Food & Groceries, Other.

				Item: {itemName}

				Respond only with a valid JSON object like this (no code block or markdown):

				{{ ""category"": ""Clothes"" }}";

			var requestPayload = new
			{
				contents = new[]
				{
			new
			{
				parts = new[]
				{
					new { text = prompt }
				}
			}
		}
			};

			var apiKey = configuration["GeminiApi:geminiApiKey"];

			using var client = new HttpClient();
			var json = JsonSerializer.Serialize(requestPayload);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await client.PostAsync(
				$"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-lite:generateContent?key={apiKey}",
				content
			);

			try
			{
				var responseString = await response.Content.ReadAsStringAsync();
				Console.WriteLine("Gemini raw response: " + responseString);
				using var doc = JsonDocument.Parse(responseString);
				var candidates = doc.RootElement.GetProperty("candidates");
				var parts = candidates[0].GetProperty("content").GetProperty("parts");
				var text = parts[0].GetProperty("text").GetString();

				// Clean the text from markdown formatting
				var cleanedText = text.Replace("```json", "").Replace("```", "").Trim();

				using var categoryJson = JsonDocument.Parse(cleanedText);
				if (categoryJson.RootElement.TryGetProperty("category", out var category))
				{
					return category.GetString();
				}
			}
			catch
			{
				Console.WriteLine("categrising the data error " + response.ToString());
			}

			return null;
		}
		[Authorize]
		[HttpGet("GetToBuyList")]
		public async Task<IActionResult> GetToBuyList()
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("User not found");
			}
			var list = await db.ToBuyLists
	.Where(tl => db.UserToBuyLists.Any(ut => ut.UserId == user.Id && ut.ToBuyListId == tl.ListId))
	.ToListAsync();

			List<ToBuyListReceive> dto = new List<ToBuyListReceive>();
			foreach (var item in list)
			{
				dto.Add(new ToBuyListReceive
				{
					id = item.ListId,
					ProductName = item.ProductName,
				});
			}
			return Ok(dto);
		}
		[Authorize]
		[HttpDelete("DeleteFromToBuyList")]
		public async Task<IActionResult> DeleteFromToBuyList(int id)
		{
			if (id == null)
			{
				return BadRequest("Id is required");
			}
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("User not found");
			}
			var item = db.ToBuyLists.FirstOrDefault(x => x.ListId == id);
			if (item == null)
			{
				return NotFound("Item not found");
			}
			db.ToBuyLists.Remove(item);
			db.SaveChanges();
			return Ok("Item Deleted");
		}
		[Authorize]
		[HttpGet("aisuggestion")]
		public async Task<IActionResult> aisuggestfortobuy()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound("User Not Found");
			}

			var PurchasedProducts = await db.PurchasedProducts
				.Where(p => p.UserId == user.Id)
				.ToListAsync();
			var productList = string.Join(", ", PurchasedProducts.Select(p => p.ItemName));
			var prompt = $@"
			Based on the following purchased products by the user: {productList},
			predict 1 product the user might buy next.

			Then classify the predicted product into one of the following categories:
			- Clothes
			- Electronics
			- Food & Groceries
			- Other

			Return the result strictly as a valid JSON object, like this:
			{{
			  ""suggestion"": ""sports shoes"",
			  ""category"": ""Clothes""
			}}
			Do not include any code block formatting or markdown. Just return pure JSON.
			";

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
				$"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={geminiApiKey}",
				httpContent
			);

			if (!response.IsSuccessStatusCode)
			{
				var error = await response.Content.ReadAsStringAsync();
				return StatusCode((int)response.StatusCode, error);
			}

			var jsonResponse = await response.Content.ReadAsStringAsync();
			var geminiResponse = JsonDocument.Parse(jsonResponse);
			var rawText = geminiResponse.RootElement
				.GetProperty("candidates")[0]
				.GetProperty("content")
				.GetProperty("parts")[0]
				.GetProperty("text")
				.GetString();
			var aiResult = JsonSerializer.Deserialize<Dictionary<string, string>>(rawText);
			var productName = aiResult["suggestion"];
			var category = aiResult["category"];

			using var transaction = await db.Database.BeginTransactionAsync();
			try
			{
				var existingList = db.ToBuyLists.FirstOrDefault(l => l.ProductName == productName);
				if (existingList == null)
				{
					var toBuyList = new ToBuyList
					{
						ProductName = productName
					};

					await db.ToBuyLists.AddAsync(toBuyList);
					await db.SaveChangesAsync();

					// Scraping tasks
					var amazonTask = amazon.StartScraping(productName, toBuyList.ListId, category);
					var noonTask = noon.StartScraping(productName, toBuyList.ListId, category);
					var jumiaTask = jumia.StartScraping(productName, toBuyList.ListId, category);
					await Task.WhenAll(amazonTask, noonTask, jumiaTask);

					var userlist = new UserToBuyList
					{
						UserId = user.Id,
						ToBuyListId = toBuyList.ListId
					};
					await db.UserToBuyLists.AddAsync(userlist);
				}
				else
				{
					var isAlreadyPresent = db.UserToBuyLists
						.Any(ul => ul.UserId == user.Id && ul.ToBuyListId == existingList.ListId);
					if (isAlreadyPresent)
					{
						return Ok("Item already Present");
					}

					var userlist = new UserToBuyList
					{
						UserId = user.Id,
						ToBuyListId = existingList.ListId
					};
					await db.UserToBuyLists.AddAsync(userlist);
				}

				await db.SaveChangesAsync();
				await transaction.CommitAsync();

				return Ok(new
				{
					Message = "Item Saved",
					SuggestedProduct = productName,
					Category = category
				});
			}
			catch
			{
				await transaction.RollbackAsync();
				return StatusCode(500, "An error occurred while saving the item.");
			}
		}
	}
}

