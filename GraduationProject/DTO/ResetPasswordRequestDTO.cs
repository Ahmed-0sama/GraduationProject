﻿namespace Graduation_Project.DTO
{
	public class ResetPasswordRequestDTO
	{
		public string Email { get; set; }
		public string Token { get; set; }
		public string NewPassword { get; set; }
	}
}
