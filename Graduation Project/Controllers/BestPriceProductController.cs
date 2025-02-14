using gp.Models;
using Graduation_Project.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BestPriceProductController : ControllerBase
	{
		UserManager<User> userManager;
		AppDbContext db;
		public BestPriceProductController(UserManager<User> userManager, AppDbContext db)
		{
			this.userManager = userManager;
			this.db = db;
		}
		[Authorize]
		[HttpPost("AddBestPriceProduct/{listid}")]
		public async Task<IActionResult> AddItemPrice(AddBestPriceProductDTO dto, int listid)
		{
			if (ModelState.IsValid)
			{
				if (listid == null)
				{
					return BadRequest("List id is required");
				}
				var list = db.ToBuyLists.Where(x => x.ListId == listid).FirstOrDefault();
				if (list == null)
				{
					return NotFound("List not found");
				}
				BestPriceProduct product = new BestPriceProduct
				{
					ListId = list.ListId,
					ProductName = dto.productName,
					Price = dto.price,
					ShopName = dto.shopName,
					Quantity = dto.quantity,
					Image = dto.image,
					Category = dto.category,
					Date = dto.date,
					Url = dto.url
				};
				db.BestPriceProducts.Add(product);
				db.SaveChanges();
				return Ok("Product added successfully");
			}
			return BadRequest(ModelState);

		}
		[Authorize]
		[HttpGet("GetBestPriceProductsDetails/{id}")]
		public async Task<IActionResult> GetBestPriceProductsDetails(int id)
		{
			if (id == null)
			{
				return BadRequest("List id is required");
			}
			var list = db.ToBuyLists.Where(x => x.ListId == id).FirstOrDefault();
			if (list == null)
			{
				return NotFound("List not found");
			}
			List<BestPriceProduct> products = db.BestPriceProducts.Where(x => x.ListId == id).ToList();
			List<getitemPriceDetailsDTO> productsdto = new List<getitemPriceDetailsDTO>();
			foreach (var product in products)
			{
				productsdto.Add(new getitemPriceDetailsDTO
				{
					ProductName = product.ProductName,
					Price = product.Price,
					ShopName = product.ShopName,
					Quantity = product.Quantity,
					Image = product.Image,
					Category = product.Category,
					Date = (DateOnly)product.Date,
					Url = product.Url,
					IsBought = product.IsBought
				});
			}
			return Ok(productsdto);
		}
		[Authorize]
		[HttpDelete("DeleteBestPriceProduct/{id}")]
		public async Task<IActionResult> DeleteBestPriceProduct(int id)
		{
			if(id == null)
			{
				return BadRequest("Product id is required");
			}
			var product = db.BestPriceProducts.Find(id);
			if (product == null)
			{
				return NotFound("Product not found");
			}
			db.BestPriceProducts.Remove(product);
			db.SaveChanges();
			return Ok("Product deleted successfully");
		}

		[Authorize]
		[HttpGet("GetBestPriceProduct/{id}")]
		public async Task<IActionResult> GetBestPriceProduct(int id)
		{
			if (id == null)
			{
				return BadRequest("Product id is required");
			}
			var product = db.BestPriceProducts.Find(id);
			if (product == null)
			{
				return NotFound("Product not found");
			}
			getItemPriceDTO productdto = new getItemPriceDTO
			{
				ProductName = product.ProductName,
				Price = product.Price,
				ShopName = product.ShopName,
				IsBought = product.IsBought,
				Image = product.Image,
			};
			return Ok(productdto);
		}
		[Authorize]
		[HttpPut("MarkPurchased/{id}")]
		public async Task<IActionResult>MarkPurchased(int id)
		{
			if (id == null)
			{
				return BadRequest("Product id is required");
			}
			var product = db.BestPriceProducts.Find(id);
			if (product == null)
			{
				return NotFound("Product not found");
			}
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if(user == null)
			{
				return NotFound("User not found");
			}
			product.IsBought = true;
			PurchasedProduct purchasedProduct = new PurchasedProduct
			{
				UserId = user.Id,
				Category = product.Category,
				Date =(DateOnly)product.Date,
				Price = product.Price,
				Quantity = product.Quantity,
				ShopName = product.ShopName,
				ItemName = product.ProductName,
			};
			db.PurchasedProducts.Add(purchasedProduct);
			db.SaveChanges();
			return Ok("Product marked as purchased successfully");

		}
	}
}
