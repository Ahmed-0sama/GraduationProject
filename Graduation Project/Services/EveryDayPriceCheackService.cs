using gp.Models;
using Graduation_Project.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using static Google.Apis.Requests.BatchRequest;

namespace Graduation_Project.Services
{
	public class EveryDayPriceCheackService
	{
		private readonly AppDbContext db;

		public EveryDayPriceCheackService(AppDbContext db)
		{
			this.db = db;

		}
		//if the price is set to 0 then the price is removed form the website and have to be  
		// appear as removed from the website  so take care please!!!
		public async Task checkandupdate()
		{
			var products = await db.BestPriceProducts
				 .Select(p => new { p.ItemId, p.Url, p.CurrentPrice })
				 .ToListAsync();
			var pricehistoryentries = new List<ProductPriceHistory>();
			var productstoupdate = new List<BestPriceProduct>();
			foreach (var product in products)
			{
				string jsonresponse = await Task.Run(() => RunPythonScript(product.Url));
				double? newprice = null;
				try
				{
					using JsonDocument doc = JsonDocument.Parse(jsonresponse);
					if (doc.RootElement.TryGetProperty("price", out JsonElement priceElement))
					{
						if (priceElement.ValueKind == JsonValueKind.Number)
						{
							newprice = priceElement.GetDouble();
						}
						else if (priceElement.ValueKind == JsonValueKind.String &&
								 double.TryParse(priceElement.GetString(), out double parsedPrice))
						{
							newprice = parsedPrice;
						}
						else
						{
							Console.Error.WriteLine($"[warning] Unexpected price format in JSON for {product.Url}: {priceElement}");
						}
					}
				}
				catch (JsonException ex)
				{
					Console.Error.WriteLine($"[error] json parsing error for {product.Url}: {ex.Message}");
					throw new Exception($"[error] failed to parse json response for url: {product.Url}", ex);
				}

				if (newprice.HasValue&&newprice.Value!=product.CurrentPrice)
				{
					var lastPriceEntry = await db.ProductPriceHistories
					.Where(h => h.ItemId == product.ItemId)
					.OrderByDescending(h => h.DateRecorded)
					.FirstOrDefaultAsync();
					if(lastPriceEntry == null || lastPriceEntry.Price != newprice.Value){
						pricehistoryentries.Add(new ProductPriceHistory
						{
							ItemId = product.ItemId,
							Price = newprice.Value,
							DateRecorded = DateOnly.FromDateTime(DateTime.UtcNow)
						});

						var existingproduct = await db.BestPriceProducts.FindAsync(product.ItemId);
						if (existingproduct != null)
						{
							existingproduct.CurrentPrice = newprice.Value;
							productstoupdate.Add(existingproduct);
						}
					}
				}
			}
			if (pricehistoryentries.Count > 0)
			{
				await db.ProductPriceHistories.AddRangeAsync(pricehistoryentries);
			}

			if (productstoupdate.Count > 0)
			{
				db.BestPriceProducts.UpdateRange(productstoupdate);
			}

			if (pricehistoryentries.Count > 0 || productstoupdate.Count > 0)
			{
				await db.SaveChangesAsync();
			}

		}
		public string RunPythonScript(string productUrl)
		{
			string scriptPath = @"D:\enviroment\Graduation Project\Graduation Project\webscrapping\PriceScrappingService.py";

			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = "py",  // Use "python" if "py" doesn't work
				Arguments = $"\"{scriptPath}\" \"{productUrl}\"",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			try
			{
				using (Process process = new Process { StartInfo = psi })
				{
					process.Start();
					string output = process.StandardOutput.ReadToEnd();
					string errors = process.StandardError.ReadToEnd();

					process.WaitForExit();

					if (!string.IsNullOrWhiteSpace(errors))
					{
						Console.WriteLine("Python Error: " + errors);
					}

					return output;  
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
