using gp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation_Project.Models
{
	public class ProductPriceHistory
	{
		[Key]
		public int PriceID { get; set; }
		
		public int ItemId { get; set; }
		public double Price { get; set; }
		public DateOnly? DateRecorded { get; set; }

		[ForeignKey("ItemId")]
		public virtual BestPriceProduct Product { get; set; }
	}
}
