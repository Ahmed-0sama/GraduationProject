namespace Graduation_Project.DTO
{
	public class getitemPriceDetailsDTO
	{
		public string? Category { get; set; }

		public string? Image { get; set; }

		public DateOnly? Date { get; set; }

		public double Price { get; set; }

		public string? Url { get; set; }

		//public int Quantity { get; set; }

		public string? ShopName { get; set; }

		public string? ProductName { get; set; }

		//public bool? IsBought { get; set; }
		public List<PriceHistoryDTO> PriceHistory { get; set; } = new List<PriceHistoryDTO>();
	}
}
