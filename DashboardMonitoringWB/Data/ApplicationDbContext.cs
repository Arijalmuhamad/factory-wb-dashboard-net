using Microsoft.EntityFrameworkCore;
using DashboardMonitoringWB.Models;

namespace DashboardMonitoringWB.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
		// Mendaftarkan tabel wbs_site_tab agar bisa diakses
		public DbSet<WbsSite> WbsSites { get; set; }
    
    }
}
