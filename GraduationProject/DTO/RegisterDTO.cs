using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.DTO
{
	public class RegisterDTO
	{
		[Required]
		public string lname { get; set; }
		public string fname { get; set; }

		[Required, EmailAddress]
		public string Email { get; set; }

		[Required, MinLength(6)]
		public string Password { get; set; }
	}
}
