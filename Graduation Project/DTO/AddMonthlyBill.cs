namespace Graduation_Project.DTO
{
	public class AddMonthlyBill
	{
		public string Name { get; set; }
		public string Issuer { get; set; }
		public string Category { get; set; }
		public float Amount { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int  Duration { get; set; }
	}
}
