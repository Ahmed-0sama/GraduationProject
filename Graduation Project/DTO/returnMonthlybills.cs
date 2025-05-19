namespace Graduation_Project.DTO
{
	public class returnMonthlybills
	{
		public string? Name { get; set; }

		public string? Issuer { get; set; }

		public string? Category { get; set; }

		public double? Amount { get; set; }

		public int? Duration { get; set; }

		public DateTime? StartDate { get; set; }

		public DateTime? EndDate { get; set; }
	}
}
