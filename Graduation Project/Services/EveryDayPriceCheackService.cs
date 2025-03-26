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

				if (newprice.HasValue)
				{
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
		//public async Task checkandupdate()
		//{
		//	string url = "https://www.noon.com/egypt-en/classic-coffee-95grams/N28968510A/p/?o=dcf5e5c734b8957b&shareId=51bb71ba-f4b3-49ce-9eb5-097752af6068";

		//	try
		//	{
		//		string result = RunPythonScript(url);

		//		if (!string.IsNullOrEmpty(result))
		//		{
		//			Console.WriteLine("[INFO] Python Script Output: " + result);
		//		}
		//		else
		//		{
		//			Console.WriteLine("[WARNING] Python script did not return any output.");
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine("[ERROR] " + ex.Message);
		//	}
		//}
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

					return output;  // ✅ Return the output of the script
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error running Python script: " + ex.Message);
				return null;  // ✅ Return null if an error occurs
			}
		}
	}
}
