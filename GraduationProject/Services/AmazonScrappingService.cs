using System.Diagnostics;
using gp.Models;
using Graduation_Project.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
namespace Graduation_Project.Services
{
	public class AmazonScrappingService
	{
		private readonly AppDbContext db;
		public AmazonScrappingService(AppDbContext db)
		{
			this.db = db;
		}

		public async Task StartScraping(string name,int listid,string category)
		{
			string searchQuery = name;
			string pythonScriptPath = Path.Combine(Directory.GetCurrentDirectory(), "webscrapping", "amazon.py"); 

			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = "py",
				Arguments = $"\"{pythonScriptPath}\" \"{searchQuery}\"",
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

					Console.WriteLine("Python Output:");
					Console.WriteLine(output);

					if (!string.IsNullOrWhiteSpace(errors))
					{
						Console.WriteLine("Python Error: " + errors);
					}

					if (!string.IsNullOrWhiteSpace(output))
					{
						List<pythonProduct> products = new List<pythonProduct>();
						products = JsonConvert.DeserializeObject<List<pythonProduct>>(output);
						foreach (var product in products)
						{
							BestPriceProduct pro = new BestPriceProduct();
							pro.ProductName = product.Name;
							pro.Url = product.Link;
							pro.Category = category;
							pro.CurrentDate = DateOnly.FromDateTime(DateTime.UtcNow);
							pro.ShopName = "Amazon";
							//pro.IsBought = false;
							pro.CurrentPrice = Convert.ToDouble(product.Price.Replace(" EGP", "").Trim());
							pro.Image = product.Image;
							pro.ToBuyListID = listid;
							await db.BestPriceProducts.AddAsync(pro);
							await db.SaveChangesAsync();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex.Message);
			}
		}
		public class pythonProduct
		{
			public string Name { get; set; }
			public string Price { get; set; }
			public string Image { get; set; }
			public string Link { get; set; }
		}
	}
}
