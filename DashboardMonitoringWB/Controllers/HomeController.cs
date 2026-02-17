using System.Diagnostics;
using DashboardMonitoringWB.Data;
using DashboardMonitoringWB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DashboardMonitoringWB.Controllers
{
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext _context;

		public HomeController(ApplicationDbContext context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
			// 1. Ambil data informasi Site/Pabrik
			var siteInfo = _context.WbsSites.FirstOrDefault();

			// 2. Kirim data ke View melalui ViewBag
			// Pastikan menggunakan 'companyName' (c kecil) sesuai yang dipanggil di Index.cshtml
			ViewBag.companyName = siteInfo?.sitename ?? "Data Tidak Ditemukan";
			ViewBag.SiteId = siteInfo?.siteid ?? "---";

			// 3. Mengatur tanggal operasional (Bahasa Indonesia)
			var culture = new CultureInfo("id-ID");
			ViewBag.Today = DateTime.Now.ToString("dddd, dd-MM-yyyy", culture);

			// 4. Ambil data summary dari 4 tabel berbeda melalui fungsi bantuan
			DashboardSummary summary = GetDashboardSummary();

			// 5. Kirim objek 'summary' sebagai Model utama ke View
			return View(summary);
		}

		private DashboardSummary GetDashboardSummary()
		{
			var result = new DashboardSummary();

			// Query SQL diperluas untuk mengambil total truck
			string query = @"
        SELECT 
            SUM(regis_in = 'T') as regist_in,
            SUM(weighing_in = 'T') as weighing_in,
            SUM(weighing_out = 'T') as weighing_out,
            SUM(regis_out = 'T') as regist_out,
            -- Tambahan untuk Total Truck
            SUM(source = 'tbs' AND weighing_out = 'T' AND wbout <> 0 AND regis_out = 'T') as total_ffb,
            SUM(source = 'cpo' AND weighing_out = 'T' AND wbout <> 0 AND regis_out = 'T') as total_sales,
            SUM(source = 'trans' AND weighing_out = 'T' AND wbout <> 0 AND regis_out = 'T') as total_transfer,
            SUM(source = 'data' AND weighing_out = 'T' AND wbout <> 0 AND regis_out = 'T') as total_others,
			-- Tambahan untuk Total Tonnage
			SUM(CASE WHEN source = 'tbs' AND weighing_out = 'T' AND wbout <> 0 AND regis_out = 'T' THEN netto ELSE 0 END) as tonnage_ffb,
			SUM(CASE WHEN source = 'cpo' AND weighing_out = 'T' AND wbout <> 0 AND regis_out = 'T' THEN netto ELSE 0 END) as tonnage_sales,
			SUM(CASE WHEN source = 'trans' AND weighing_out = 'T' AND wbout <> 0 AND regis_out = 'T' THEN netto ELSE 0 END) as tonnage_transfer,
			SUM(CASE WHEN source = 'data' AND weighing_out = 'T' AND wbout <> 0 AND regis_out = 'T' THEN netto ELSE 0 END) as tonnage_others

        FROM (
            SELECT 'tbs' as source, regis_in, weighing_in, weighing_out, regis_out, wbout, dateout, netto_ag AS netto FROM wb_automation_datatbs_tab
            UNION ALL
            SELECT 'cpo' as source, regis_in, weighing_in, weighing_out, regis_out, wbout, dateout, netto FROM wb_automation_datacpo_tab
            UNION ALL
            SELECT 'trans' as source, regis_in, weighing_in, weighing_out, regis_out, wbout, dateout, netto FROM wb_automation_producttrans_tab
            UNION ALL
            SELECT 'data' as source, regis_in, weighing_in, weighing_out, regis_out, wbout, dateout, netto FROM wb_automation_data_tab
        ) AS combined_data
        WHERE DATE(dateout) = IF(HOUR(NOW()) >= 7, CURDATE(), DATE_SUB(CURDATE(), INTERVAL 1 DAY));";

			try
			{
				using (var command = _context.Database.GetDbConnection().CreateCommand())
				{
					command.CommandText = query;
					_context.Database.OpenConnection();

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							// Mapping Statistik Atas
							result.RegistIn = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader[0]);
							result.WeighingIn = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader[1]);
							result.WeighingOut = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader[2]);
							result.RegistOut = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader[3]);

							// Mapping Total Truck
							result.TotalTruckFFB = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader[4]);
							result.TotalTruckSales = reader.IsDBNull(5) ? 0 : Convert.ToInt32(reader[5]);
							result.TotalTruckTransfer = reader.IsDBNull(6) ? 0 : Convert.ToInt32(reader[6]);
							result.TotalTruckOthers = reader.IsDBNull(7) ? 0 : Convert.ToInt32(reader[7]);

							// Mapping Total Tonnage
							result.TotalTonnageFFB = reader.IsDBNull(8) ? 0 : Convert.ToInt32(reader[8]);
							result.TotalTonnageSales = reader.IsDBNull(9) ? 0 : Convert.ToInt32(reader[9]);
							result.TotalTonnageTransfer = reader.IsDBNull(10) ? 0 : Convert.ToInt32(reader[10]);
							result.TotalTonnageOthers = reader.IsDBNull(11) ? 0 : Convert.ToInt32(reader[11]);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Database Error: " + ex.Message);
			}
			finally
			{
				_context.Database.CloseConnection();
			}

			return result;
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}