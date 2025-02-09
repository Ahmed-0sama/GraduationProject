using gp.Models;
using Graduation_Project.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Graduation_Project.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PurchasedProductController : ControllerBase
	{
		AppDbContext db;
		UserManager<User> userManager;
		List<string> AllowedCategories = new List<string> { "Drinks", "Clothes", "Electronics", "Other" , "Food&Groceries" };
		public PurchasedProductController(AppDbContext db, UserManager<User> userManager)
		{
			this.db = db;
			this.userManager = userManager;
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
				PurchasedProduct product = new PurchasedProduct
				{
					UserId = user.Id,
					ItemName = model.ProductName,
					Category = model.Category,
					Date = model.Date,
					Price = model.Price,
					Quantity = model.Quantity,
					ShopName = model.ShopName,
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
	}

}