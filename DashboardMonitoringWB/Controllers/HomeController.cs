using System.Diagnostics;
using DashboardMonitoringWB.Data;
using DashboardMonitoringWB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

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
			// Ambil satu data pertama dari tabel wbs_site_tab
			var siteInfo = _context.WbsSites.FirstOrDefault();

			// Kirim data ke View melalui ViewBag
			ViewBag.CompanyName = siteInfo?.sitename ?? "Data Tidak Ditemukan";
			ViewBag.SiteId = siteInfo?.siteid ?? "---";

			// Mengatur budaya (culture) ke Indonesia agar nama hari otomatis Bahasa Indonesia
			var culture = new System.Globalization.CultureInfo("id-ID");
			ViewBag.Today = DateTime.Now.ToString("dddd, dd-MM-yyyy", culture);


            DashboardSummary summary = GetDashboardSummary();
			return View(summary);
		}

        private DashboardSummary GetDashboardSummary()
        {
            var result = new DashboardSummary();

			// Query SQL Anda (disesuaikan agar kompetibel dengan ADO.NET)

			string query = @"
            SELECT 
                SUM(regis_in = 'T') as regist_in,
                SUM(weighing_in = 'T') as weighing_in,
                SUM(weighing_out = 'T') as weighing_out,
                SUM(regis_out = 'T') as regist_out
            FROM (
                SELECT regis_in, weighing_in, weighing_out, regis_out, dateout FROM wb_automation_datatbs_tab
                UNION ALL
                SELECT regis_in, weighing_in, weighing_out, regis_out, dateout FROM wb_automation_datacpo_tab
                UNION ALL
                SELECT regis_in, weighing_in, weighing_out, regis_out, dateout FROM wb_automation_producttrans_tab
                UNION ALL
                SELECT regis_in, weighing_in, weighing_out, regis_out, dateout FROM wb_automation_data_tab
            ) AS combined_data
            WHERE DATE(dateout) = IF(HOUR(NOW()) >= 7, CURDATE(), DATE_SUB(CURDATE(), INTERVAL 1 DAY));";

			using (var command = _context.Database.GetDbConnection().CreateCommand())
			{
				command.CommandText = query;
				_context.Database.OpenConnection();
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						result.RegistIn = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader[0]);
						result.WeighingIn = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader[1]);
						result.WeighingOut = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader[2]);
						result.RegistOut = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader[3]);
					}
				}
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
