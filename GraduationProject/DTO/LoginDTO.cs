using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
	public class LoginDTO
	{
		[Display(Name ="Email")]
		public string username { get; set; }
		public string Password { get; set; }
	}
}
