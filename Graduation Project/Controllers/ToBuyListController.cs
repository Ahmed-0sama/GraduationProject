using Google.Apis.Drive.v3.Data;
using gp.Models;
using Graduation_Project.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
		public ToBuyListController(UserManager<User> userManager, AppDbContext db)
		{
			this.userManager = userManager;
			this.db = db;
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
				ToBuyList toBuyList = new ToBuyList
				{
					UserId = user.Id,
					ProductName = dto.ProductName,
					Date = DateTime.Now
				};
				db.ToBuyLists.Add(toBuyList);
				db.SaveChanges();
				return Ok("Item Saved");
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
			List<ToBuyList> list = db.ToBuyLists.Where(x => x.UserId == user.Id).ToList();
			List<ToBuyListReceive> dto = new List<ToBuyListReceive>();
			foreach (var item in list)
			{
				dto.Add(new ToBuyListReceive
				{
					id = item.ListId,
					ProductName = item.ProductName,
					Date = (DateTime)item.Date
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
