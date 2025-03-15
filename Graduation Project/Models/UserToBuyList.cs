using gp.Models;

namespace Graduation_Project.Models
{
	public class UserToBuyList
	{
		public string UserId { get; set; }
		public virtual User User { get; set; }

		public int ToBuyListId { get; set; }
		public virtual ToBuyList ToBuyList { get; set; }
	}
}
