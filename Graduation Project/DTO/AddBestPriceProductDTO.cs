namespace Graduation_Project.DTO
{
	public class AddBestPriceProductDTO
	{
		public string productName { get; set; }
		public string category { get; set; }
		public double price { get; set; }
		public string shopName { get; set; }
		public string url { get; set; }
		public string image { get; set; }
		public int quantity { get; set; }
		public DateOnly? date { get; set; }

	}
}
