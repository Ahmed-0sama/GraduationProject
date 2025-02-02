using gp.Models;
using Graduation_Project.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace gp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly UserManager<User> userManager;
		private readonly IConfiguration configuration;
		public UserController(UserManager<User> userManager, IConfiguration configuration)
		{
			this.userManager = userManager;
			this.configuration = configuration;
		}
		[HttpPost("Register")]
		public async Task<IActionResult> Signup(RegisterDTO registerfromform)
		{
			if (ModelState.IsValid) {
				User user = new User
				{
					Name = registerfromform.Name,
					Email = registerfromform.Email,
					UserName = registerfromform.Email
				};
				IdentityResult result = await userManager.CreateAsync(user, registerfromform.Password);
				if(result.Succeeded)
				{
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
		public async Task <IActionResult> Login(LoginDTO log)
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
						foreach( var role in userRole)
						{
							userClaims.Add(new Claim(ClaimTypes.Role, role));
						}
						var secretKey=configuration["JWT:key"];
						var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:key"]));
						var singinCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
						var token =new JwtSecurityToken(
							issuer: configuration["JWT:Issuer"],
							audience: configuration["JWT:Audience"],
							claims: userClaims,
							expires: DateTime.Now.AddDays(30),
							signingCredentials: singinCredentials
						);
						return Ok(new
						{
							token = new JwtSecurityTokenHandler().WriteToken(token),
							expiration = token.ValidTo
						});
					}
					ModelState.AddModelError("UserName", "Invalid UserName or Password");
				}
			}
			return BadRequest(ModelState);
		}
		[Authorize]
		[HttpGet]
		public async Task<IActionResult> GetUsers()
		{
			var users = userManager.Users.ToList();
			return Ok(users);
		}

	}
}
