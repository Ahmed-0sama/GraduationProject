using Azure;
using gp.Models;
using Graduation_Project.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Graduation_Project.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PurchasedProductController : ControllerBase
	{
		private readonly IConfiguration _configuration;
		AppDbContext db;
		UserManager<User> userManager;
		List<string> AllowedCategories = new List<string> { "Drinks", "Clothes", "Electronics", "Other" , "Food&Groceries" };
		public PurchasedProductController(AppDbContext db, UserManager<User> userManager,IConfiguration configuration)
		{
			this.db = db;
			this.userManager = userManager;
			_configuration = configuration;
		}
		[Authorize]
		[HttpPost("AddPurchasedProduct")]
		public async Task<IActionResult> AddPurshasedProduct(PurchasedProductDTO model)
		{
			if (ModelState.IsValid)
			{
				if(!AllowedCategories.Contains(model.Category))
				{
					return BadRequest("Invalid category");
				}
				var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
				if (user == null)
				{
					return NotFound("User not found");
				}
				var expense = await db.Expenses
				.Where(e => e.UserId == user.Id)
				.FirstOrDefaultAsync();
				PurchasedProduct product = new PurchasedProduct
				{
					UserId = user.Id,
					ItemName = model.ProductName,
					Category = model.Category,
					Date = model.Date,
					Price = model.Price,
					Quantity = model.Quantity,
					ShopName = model.ShopName,
					ExpenseId=expense.ExpenseId
				};
				db.PurchasedProducts.Add(product);
				db.SaveChanges();
				return Ok("Product added successfully");
			}
			return BadRequest(ModelState);
		}
		[Authorize]
		[HttpGet("GetPurchasedProducts")]
		public async Task<IActionResult> GetPurchasedProducts()
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("User not found");
			}
			List<PurchasedProduct>products = db.PurchasedProducts.Where(p => p.UserId == user.Id).ToList();
			List<PurchasedProductDTO>purchaseddto= new List<PurchasedProductDTO>();
			foreach (var product in products)
			{
				purchaseddto.Add(new PurchasedProductDTO
				{
					ProductName = product.ItemName,
					Category = product.Category,
					Date = product.Date,
					Price = product.Price,
					Quantity = product.Quantity,
					ShopName = product.ShopName
				});
			}
			return Ok(purchaseddto);
		}
		[Authorize]
		[HttpDelete("DeletePurchasedProduct")]
		public async Task<IActionResult>DeletePurchasedProduct(int id)
		{
			var product = db.PurchasedProducts.Find(id);
			if (product == null)
			{
				return NotFound("Product not found");
			}
			db.PurchasedProducts.Remove(product);
			db.SaveChanges();
			return Ok("Product deleted successfully");
		}
		[Authorize]
		[HttpPut("UpdatePurchasedProduct")]
		public async Task<IActionResult>updateProduct(int id, PurchasedProductDTO model)
		{
			var product = db.PurchasedProducts.Find(id);
			if (product == null)
			{
				return NotFound("Product not found");
			}
			product.ItemName = model.ProductName;
			product.Category = model.Category;
			product.Date = model.Date;
			product.Price = model.Price;
			product.Quantity = model.Quantity;
			product.ShopName = model.ShopName;
			db.SaveChanges();
			return Ok("Product updated successfully");
		}
		[Authorize]
		[HttpGet("GetPurchasedProductByDateRange")]
		public async Task<IActionResult> GetPurchasedProductByDate(DateOnly startDate, DateOnly EndDate)
		{
			if(startDate > EndDate)
			{
				return BadRequest("Invalid date range");
			}
			if(startDate == null || EndDate == null)
			{
				return BadRequest("Invalid date range");
			}
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("User not found");
			}
			List<PurchasedProduct> products = db.PurchasedProducts.Where(p => p.UserId == user.Id && p.Date >= startDate && p.Date <= EndDate).ToList();

			List<PurchasedProductDTO> purchaseddto = new List<PurchasedProductDTO>();
			foreach (var product in products)
			{
				purchaseddto.Add(new PurchasedProductDTO
				{
					ProductName = product.ItemName,
					Category = product.Category,
					Date = product.Date,
					Price = product.Price,
					Quantity = product.Quantity,
					ShopName = product.ShopName
				});
			}
			return Ok(purchaseddto);
		}
		[Authorize]
		[HttpGet("GetPurchasedProductByCategory")]
		public async Task<IActionResult> GetPurchasedProductByCategory(string category)
		{
			if (!AllowedCategories.Contains(category))
			{
				return BadRequest("Invalid category");
			}
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("User not found");
			}
			List<PurchasedProduct> products = db.PurchasedProducts.Where(p => p.UserId == user.Id && p.Category == category).ToList();
			List<PurchasedProductDTO> purchaseddto = new List<PurchasedProductDTO>();
			foreach (var product in products)
			{
				purchaseddto.Add(new PurchasedProductDTO
				{
					ProductName = product.ItemName,
					Category = product.Category,
					Date = product.Date,
					Price = product.Price,
					Quantity = product.Quantity,
					ShopName = product.ShopName
				});
			}
			return Ok(purchaseddto);
		}
		[Authorize]
		[HttpGet("GetPurchasedProductByShop")]
		public IActionResult GetProductByShop(string shopname)
		{
			if (shopname==null)
			{
				return BadRequest("Shop name is required");
			}
			var user= userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)).Result;
			if (user == null)
			{
				return NotFound("User not found");
			}
			List<PurchasedProduct> products = db.PurchasedProducts.Where(p => p.UserId == user.Id && p.ShopName == shopname).ToList();
			List<PurchasedProductDTO> purchaseddto = new List<PurchasedProductDTO>();
			foreach (var product in products)
			{
				purchaseddto.Add(new PurchasedProductDTO
				{
					ProductName = product.ItemName,
					Category = product.Category,
					Date = product.Date,
					Price = product.Price,
					Quantity = product.Quantity,
					ShopName = product.ShopName
				});
			}
			return Ok(purchaseddto);
		}
		[Authorize]
		[HttpGet("GetTotalSpendingByUser")]
		public async Task<IActionResult> GetTotalSpendingByUser()
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("User not found");
			}
			double total = db.PurchasedProducts.Where(p => p.UserId == user.Id).Sum(p => p.Price * p.Quantity);
			return Ok(total);
		}
		[Authorize]
		[HttpPost("AddReceipt")]
		public async Task<IActionResult>uploadRecite(IFormFile formFile)
		{
			if (formFile == null || formFile.Length == 0)
			{
				return BadRequest("No file uploaded.");
			}
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			string base64Image;
			using (var ms = new MemoryStream())
			{
				await formFile.CopyToAsync(ms);
				var imageBytes = ms.ToArray();
				base64Image = Convert.ToBase64String(imageBytes);
			}
			var prompt = @"This is a photo of a receipt. 
			Extract the following:
			- The shop name
			- The date of the receipt (in a standard format like yyyy-MM-dd)
			- The items, grouped under these categories: Clothes, Electronics, Food & Groceries, Other. 

			For each item, include:
			- name
			- quantity (if available)
			- price

			Return the result as a JSON object, like this:

			{
			  ""shop_name"": ""Walmart"",
			  ""receipt_date"": ""2024-12-15"",
			  ""items"": {
				""Clothes"": [
				  { ""name"": ""T-Shirt"", ""quantity"": 2, ""price"": 15.99 }
				],
				""Electronics"": [],
				""Food & Groceries"": [],
				""Other"": []
			  }
			}";
			var requestPayload = new
			{
				contents = new[]
				{
				new
				{
				parts = new object[]
				{
					new { text = prompt },
					new
					{
						inline_data = new
						{
							mime_type = formFile.ContentType,
							data = base64Image
						}
					}
				}
			}
				}
			};
			var geminiApiKey = _configuration["GeminiApi:geminiApiKey"];
			using var client = new HttpClient();
			var requestJson = JsonSerializer.Serialize(requestPayload);
			var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

			var response = await client.PostAsync(
				$"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-lite:generateContent?key={geminiApiKey}",
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

			var jsonOnly = Regex.Match(rawText!, @"```json\s*(.*?)\s*```", RegexOptions.Singleline).Groups[1].Value;
			var parsed = JsonDocument.Parse(jsonOnly);
			var root = parsed.RootElement;

			var shopName = root.GetProperty("shop_name").GetString() ?? "Unknown Shop";
			var receiptDate = DateOnly.Parse(DateTime.Parse(root.GetProperty("receipt_date").GetString()!).ToShortDateString());
			var itemsObj = root.GetProperty("items");
			var expense = await db.Expenses
				.Where(e => e.UserId == user.Id)
				.FirstOrDefaultAsync();
			if (expense.ExpenseId == null)
			{
				return NotFound("No expenses found");
			}

			foreach (var category in itemsObj.EnumerateObject())
			{
				foreach (var item in category.Value.EnumerateArray())
				{
					var product = new PurchasedProduct
					{
						UserId = user.Id,
						Category = category.Name,
						Date = receiptDate,
						Price = item.GetProperty("price").GetDouble(),
						Quantity = item.TryGetProperty("quantity", out var q) ? q.GetInt32() : 1,
						ShopName = shopName,
						ItemName = item.GetProperty("name").GetString() ?? "Unknown Item",
						ReceiptImage = base64Image,
						ExpenseId = expense.ExpenseId
					};

					db.PurchasedProducts.Add(product);
					await db.SaveChangesAsync();
				}
			}
			return Ok("Receipt items saved successfully.");
		}
	}
}