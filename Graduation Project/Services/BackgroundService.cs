
namespace Graduation_Project.Services
{
	public class EveryDayPriceCheckBackgroundService : BackgroundService
	{
		private readonly EveryDayPriceCheackService _priceCheckService;
		public EveryDayPriceCheckBackgroundService(EveryDayPriceCheackService priceCheckService)
		{
			_priceCheckService = priceCheckService;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				await _priceCheckService.checkandupdate();

				await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
			}
		}
	}
}
