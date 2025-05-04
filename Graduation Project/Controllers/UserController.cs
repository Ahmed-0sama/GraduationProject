using Azure.Core;
using gp.Models;
using Graduation_Project.DTO;
using Graduation_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static EmailService;

namespace gp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		AppDbContext db;
		private readonly UserManager<User> userManager;
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly IConfiguration configuration;
		public UserController(AppDbContext db,UserManager<User> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
		{
			this.db = db;
			this.userManager = userManager;
			this.configuration = configuration;
			this.roleManager = roleManager;
		}

		[HttpPost("Register")]
		public async Task<IActionResult> Signup(RegisterDTO registerfromform)
		{
			if (ModelState.IsValid)
			{
				User user = new User
				{
					FirstName = registerfromform.fname,
					lastName = registerfromform.lname,
					Email = registerfromform.Email,
					UserName = registerfromform.Email,
					RefreshToken = TokenRequest.GenerateRefreshToken(),
					RefreshTokenExpirytime = DateTime.Now.AddDays(7)
				};
				IdentityResult result = await userManager.CreateAsync(user, registerfromform.Password);
				if (result.Succeeded)
				{
					Expense expense = new Expense
					{
						UserId = user.Id
					};

					db.Expenses.Add(expense);
					await db.SaveChangesAsync();

					return Ok("Created");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}
			return BadRequest(ModelState);
		}
		[HttpPost("Login")]
		public async Task<IActionResult> Login(LoginDTO log)
		{
			if (ModelState.IsValid)
			{
				var userdb = await userManager.FindByNameAsync(log.username);
				if (userdb != null)
				{
					bool isPasswordCorrect = await userManager.CheckPasswordAsync(userdb, log.Password);
					if (isPasswordCorrect)
					{
						var userClaims = new List<Claim>
						{
							new Claim (JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
							new Claim(ClaimTypes.NameIdentifier,userdb.Id),
							new Claim(ClaimTypes.Name,userdb.UserName),
						};
						var userRole = await userManager.GetRolesAsync(userdb);
						foreach (var role in userRole)
						{
							userClaims.Add(new Claim(ClaimTypes.Role, role));
						}
						var secretKey = configuration["JWT:key"];
						var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:key"]));
						var singinCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
						var token = new JwtSecurityToken(
							issuer: configuration["JWT:Issuer"],
							audience: configuration["JWT:Audience"],
							claims: userClaims,
							expires: DateTime.Now.AddDays(30),
							signingCredentials: singinCredentials
						);
						var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
						string refreshToken = TokenRequest.GenerateRefreshToken();
						userdb.RefreshToken = refreshToken;
						userdb.RefreshTokenExpirytime = DateTime.Now.AddDays(7);
						await userManager.UpdateAsync(userdb);
						return Ok(new
						{
							token = accessToken,
							expiration = token.ValidTo,
							refreshToken = refreshToken
						});
					}
					ModelState.AddModelError("UserName", "Invalid UserName or Password");
				}
			}
			return BadRequest(ModelState);
		}
		[HttpPost("RefreshToken")]
		public async Task<IActionResult> RefreshToken(RefreshTokenDTO refreshTokenDTO)
		{
			if (string.IsNullOrEmpty(refreshTokenDTO.RefreshToken) || string.IsNullOrEmpty(refreshTokenDTO.AccessToken))
			{
				return BadRequest("Invalid token request");
			}
			var principal = TokenRequest.GetPrincipalFromExpiredToken(
			refreshTokenDTO.AccessToken,
			configuration["JWT:Key"],
			configuration["JWT:Issuer"],
			configuration["JWT:Audience"]
			);
			if (principal == null)
			{
				return Unauthorized("Invalid or expired access token");
			}

			var username = principal.Identity?.Name;
			var userdb = await userManager.FindByNameAsync(username);
			if (userdb == null || userdb.RefreshTokenExpirytime <= DateTime.Now)
			{
				return Unauthorized("Invalid or expired refresh token");
			}
			var userClaims = new List<Claim>
			{
				new Claim (JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
				new Claim(ClaimTypes.NameIdentifier,userdb.Id),
				new Claim(ClaimTypes.Name,userdb.UserName)
			};
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var newToken = new JwtSecurityToken(
			issuer: configuration["JWT:Issuer"],
			audience: configuration["JWT:Audience"],
			claims: userClaims,
			expires: DateTime.Now.AddMinutes(30),
			signingCredentials: creds
			);

			var newAccessToken = new JwtSecurityTokenHandler().WriteToken(newToken);

			string newRefreshToken = TokenRequest.GenerateRefreshToken();
			userdb.RefreshToken = newRefreshToken;
			userdb.RefreshTokenExpirytime = DateTime.Now.AddDays(7);
			await userManager.UpdateAsync(userdb);

			return Ok(new
			{
				token = newAccessToken,
				expiration = newToken.ValidTo,
				refreshToken = newRefreshToken
			});
		}
		[Authorize(Roles = "Admin")]
		[HttpGet("AllUsers")]
		public async Task<IActionResult> GetUsers()
		{
			var users = userManager.Users.ToList();
			return Ok(users);
		}

		[Authorize]
		[HttpPut("UpdateInfo")]
		public async Task<IActionResult> UpdateInfo([FromBody] UpdateinfoDTO updateInfoDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			// Extract user ID from token
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User ID not found in token.");
			}

			// Retrieve user from database
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound($"User with ID {userId} not found.");
			}

			// Update user details
			user.FirstName = updateInfoDto.fname;
			user.lastName = updateInfoDto.lname;
			// Save changes
			var result = await userManager.UpdateAsync(user);
			if (result.Succeeded)
			{
				return Ok(new { Message = "User information updated successfully." });
			}
			else
			{
				return BadRequest(result.Errors);
			}
		}
		[Authorize]
		[HttpPut("AddPhoto")]
		public async Task<IActionResult> uploadPhoto(IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				return BadRequest("Invalid File");
			}
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			using (var memorystream = new MemoryStream())
			{
				await file.CopyToAsync(memorystream);
				user.Photo = memorystream.ToArray();

			}
			await userManager.UpdateAsync(user);
			return Ok(new { Message = "Photo uploaded Sucessfully!" });
		}
		[Authorize]
		[HttpGet("GetPhoto")]
		public async Task<IActionResult> GetPhoto()
		{
			var user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			if (user.Photo == null)
			{
				return NotFound("No Photo Found");
			}
			return File(user.Photo, "image/png");
		}
		[Authorize(Roles = "Admin")]
		[HttpGet("UserByEmail{email}")]
		public async Task<IActionResult> GetUserByEmail(string email)
		{
			var user = await userManager.FindByEmailAsync(email);
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			return Ok(user);
		}
		[Authorize(Roles="Admin")]
		[HttpDelete("DeleteUser{email}")]
		public async Task<IActionResult> DeleteUserByEmail(string email)
		{
			if (email == null)
			{
				return BadRequest("Email is required");
			}
			var user = await userManager.FindByEmailAsync(email);
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			var result = await userManager.DeleteAsync(user);
			if (result.Succeeded)
			{
				return Ok("User Deleted Successfully");
			}
			return BadRequest(result.Errors);
		}
		[Authorize(Roles = "Admin")]
		[HttpPut("AssignUserAdmin{userId}")]
		public async Task<IActionResult> AssignUserAdmin(string userId)
		{
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			var result = await userManager.AddToRoleAsync(user, "Admin");
			if (result.Succeeded)
			{
				return Ok("User Assigned Admin Role Successfully");
			}
			return BadRequest(result.Errors);
		}
	}
}