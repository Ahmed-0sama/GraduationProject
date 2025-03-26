using gp.Models;
using Graduation_Project.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Graduation_Project.Services
{
	public class EveryDayPriceCheackService
	{
		private readonly AppDbContext db;
		
		public EveryDayPriceCheackService(AppDbContext db)
		{
			this.db = db;
			
		}
		public async Task checkandupdate()
		{
			var products = await db.BestPriceProducts.ToListAsync();
			foreach (var product in products)
			{
			 string newprice = RunPythonScript(product.Url);
				if (double.TryParse(newprice, out double newPrice))
				{
					var priceHistory = new ProductPriceHistory
					{
						ItemId = product.ItemId,
						Price = newPrice,
						DateRecorded = DateOnly.FromDateTime(DateTime.UtcNow)
					};
					db.ProductPriceHistories.Add(priceHistory);
					product.CurrentPrice = newPrice;
				}
			}
			await db.SaveChangesAsync();
		}
		public string RunPythonScript(string productUrl)
		{
			string pythonExePath = "py"; // Ensure Python is in PATH
			string scriptPath = "D:\\enviroment\\Graduation Project\\Graduation Project\\webscrapping\\PriceScrappingService.py";

			try
			{
				ProcessStartInfo psi = new ProcessStartInfo
				{
					FileName = pythonExePath,
					Arguments = $"\"{scriptPath}\" \"{productUrl}\"",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
				};

				using (Process process = Process.Start(psi))
				{
					process.WaitForExit();
					return process.StandardOutput.ReadToEnd().Trim(); 
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error running Python script: " + ex.Message);
				return null;
			}
		}
	}
}
