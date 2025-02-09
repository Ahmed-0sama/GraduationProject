namespace Graduation_Project.DTO
{
	public class PurchasedProductDTO
	{
		public string ProductName { get; set; }
		public string Category { get; set; }
		public DateOnly Date { get; set; }
		public double Price { get; set; }
		public int Quantity { get; set; }
		public string ShopName { get; set; }
	}
}
