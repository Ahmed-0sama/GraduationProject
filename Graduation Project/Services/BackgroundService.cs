using Graduation_Project.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

public class PriceCheckBackgroundService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<PriceCheckBackgroundService> _logger;
	private readonly TimeSpan _period = TimeSpan.FromHours(24);

	public PriceCheckBackgroundService(IServiceProvider serviceProvider, ILogger<PriceCheckBackgroundService> logger)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// Run immediately on startup
		await RunPriceCheck();

		// Then run periodically
		using var timer = new PeriodicTimer(_period);

		while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
		{
			await RunPriceCheck();
		}
	}

	private async Task RunPriceCheck()
	{
		try
		{
			using var scope = _serviceProvider.CreateScope();
			var priceCheckService = scope.ServiceProvider.GetRequiredService<EveryDayPriceCheackService>();

			_logger.LogInformation("Starting price check at {Time}", DateTime.UtcNow);
			await priceCheckService.checkandupdate();
			_logger.LogInformation("Price check completed successfully at {Time}", DateTime.UtcNow);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred during price check at {Time}", DateTime.UtcNow);
		}
	}
}
