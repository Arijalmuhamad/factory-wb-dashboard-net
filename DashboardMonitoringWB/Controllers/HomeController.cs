using System.Diagnostics;
using DashboardMonitoringWB.Data;
using DashboardMonitoringWB.Models;
using Microsoft.AspNetCore.Mvc;

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

			return View();
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
