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
		public ToBuyListController(UserManager<User> userManager, AppDbContext db,AmazonScrappingService amazon)
		{
			this.userManager = userManager;
			this.db = db;
			this.amazon = amazon;
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
						db.ToBuyLists.Add(toBuyList);
						db.SaveChanges();
						await amazon.StartScraping(dto.ProductName, toBuyList.ListId);
						UserToBuyList userlist = new UserToBuyList
						{
							UserId = user.Id,
							ToBuyListId = toBuyList.ListId
						};
						db.UserToBuyLists.Add(userlist);
						db.SaveChanges();
						return Ok("Item Saved");
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
						await db.SaveChangesAsync();
						await transaction.CommitAsync(); // Commit transaction
						return Ok("Item Saved");
					}
				}
				catch
				{
					await transaction.RollbackAsync(); // Rollback on error
					return StatusCode(500, "An error occurred while saving the item.");
				}

			}
			return BadRequest(ModelState);
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
	}
}
