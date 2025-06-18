namespace Graduation_Project.Models
{
	public class AiRequest
	{	
		public string Message { get; set; }
		public string SessionId { get; set; }

	}
	public class MessageEntry
	{
		public string Role { get; set; }
		public string Text { get; set; }
	}
	public static class ChatHistoryStore
	{
		public static Dictionary<string, List<MessageEntry>> Histories { get; set; }

		static ChatHistoryStore()
		{
			Histories = new Dictionary<string, List<MessageEntry>>();
		}
	}

}
