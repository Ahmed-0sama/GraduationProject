using Graduation_Project.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

public class PriceCheckBackgroundService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;

	public PriceCheckBackgroundService(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}


	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using (var scope = _serviceProvider.CreateScope())
		{
			var priceCheckService = scope.ServiceProvider.GetRequiredService<EveryDayPriceCheackService>();
			await priceCheckService.checkandupdate(); 
		}

		Console.WriteLine("Price check completed. Waiting for next run...");
		while (!stoppingToken.IsCancellationRequested)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var priceCheckService = scope.ServiceProvider.GetRequiredService<EveryDayPriceCheackService>();
				await priceCheckService.checkandupdate();
			}

			Console.WriteLine("Price check completed. Waiting for next run...");
			await Task.Delay(TimeSpan.FromHours(24), stoppingToken); 
		}
	}
}
