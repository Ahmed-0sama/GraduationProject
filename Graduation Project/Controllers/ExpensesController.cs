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
	public class ExpensesController : ControllerBase
	{
		private readonly IConfiguration configuration;
		AppDbContext db;
		UserManager<User> userManager;
		public ExpensesController(AppDbContext db, UserManager<User> userManager, IConfiguration configuration)
		{
			this.db = db;
			this.userManager = userManager;
			this.configuration = configuration;
		}
	}
}
