using gp.Models;
using Graduation_Project.DTO;

using Graduation_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;


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

					ProductName = dto.productName,
					CurrentPrice = dto.price,
					ShopName = dto.shopName,
					Quantity = dto.quantity,
					Image = dto.image,
					Category = dto.category,
					CurrentDate = dto.date,
					Url = dto.url
				};
				db.BestPriceProducts.Add(product);
				await db.SaveChangesAsync();
				ProductPriceHistory priceHistory = new ProductPriceHistory
				{
					ItemId = product.ItemId,
					Price = dto.price,
					DateRecorded = dto.date
				};
				db.ProductPriceHistories.Add(priceHistory);
				await db.SaveChangesAsync();
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
			BestPriceProduct product = db.BestPriceProducts.FirstOrDefault(x => x.ToBuyListID == id);
			List<getitemPriceDetailsDTO> productsdto = new List<getitemPriceDetailsDTO>();
			productsdto.Add(new getitemPriceDetailsDTO
			{
				ProductName = product.ProductName,
				Price = product.CurrentPrice,
				ShopName = product.ShopName,
				Quantity = product.Quantity,
				Image = product.Image,
				Category = product.Category,
				Date = (DateOnly)product.CurrentDate,
				Url = product.Url,
				IsBought = product.IsBought
			});

			return Ok(productsdto);
		}
		[Authorize]
		[HttpDelete("DeleteBestPriceProduct/{id}")]
		public async Task<IActionResult> DeleteBestPriceProduct(int id)
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
			db.BestPriceProducts.Remove(product);
			db.SaveChanges();
			return Ok("Product deleted successfully");
		}

		[Authorize]
		[HttpGet("GetBestPriceProduct/{listid}")]
		public async Task<IActionResult> GetBestPriceProduct(int listid)
		{
			if (listid <= null)
			{
				return BadRequest("Product id is required");
			}
			List<BestPriceProduct> products = await db.BestPriceProducts.Where(l => l.ToBuyListID == listid).ToListAsync();
			if (products == null)
			{
				return NotFound("Product not found");
			}
			List<getItemPriceDTO> productdto = products.Select(item => new getItemPriceDTO
			{
				ProductName = item.ProductName,
				Price = item.CurrentPrice,
				ShopName = item.ShopName,
				IsBought = item.IsBought,
				Image = item.Image
			}).ToList();

			return Ok(productdto);
		}
		[Authorize]
		[HttpPut("MarkPurchased/{id}")]
		public async Task<IActionResult> MarkPurchased(int id)
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
			if (user == null)
			{
				return NotFound("User not found");
			}
			if (product.IsBought == true)
			{
				return BadRequest("Product already marked as purchased");
			}
			product.IsBought = true;

			PurchasedProduct purchasedProduct = new PurchasedProduct
			{
				UserId = user.Id,
				Category = product.Category,
				Date = (DateOnly)product.CurrentDate,
				Price = product.CurrentPrice,
				Quantity = product.Quantity,
				ShopName = product.ShopName,
				ItemName = product.ProductName,
			};
			db.PurchasedProducts.Add(purchasedProduct);
			db.SaveChanges();
			return Ok("Product marked as purchased successfully");
		}
		[Authorize]
		[HttpGet("GetProductPriceHistory{id}")]
		public async Task<IActionResult> GetProductHistory(int id)
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
			List<ProductPriceHistory> priceHistories = db.ProductPriceHistories.Where(x => x.ItemId == product.ItemId).ToList();
			List<PriceHistoryDTO> priceHistoriesdto = new List<PriceHistoryDTO>();
			foreach (var priceHistory in priceHistories)
			{
				priceHistoriesdto.Add(new PriceHistoryDTO
				{
					Price = priceHistory.Price,
					DateRecorded = (DateOnly)priceHistory.DateRecorded
				});
			}
			return Ok(priceHistoriesdto);
		}

	}
}
