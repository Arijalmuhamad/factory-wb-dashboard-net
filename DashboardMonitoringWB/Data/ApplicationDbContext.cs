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

		// 1. Registrasi Tabel Fisik (CRUD Standard)
		public DbSet<WbsSite> WbsSites { get; set; }

		// 2. Registrasi DTO untuk Hasil Raw SQL (Monitoring)
		// Kita gunakan nama yang berbeda dari modelnya agar tidak membingungkan
		public DbSet<TransactionViewModel> TransactionResults { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Konfigurasi Tabel Fisik (opsional jika nama tabel di DB berbeda dengan nama DbSet)
			modelBuilder.Entity<WbsSite>().ToTable("wbs_site_tab");

			// Konfigurasi Keyless Entity: 
			// Memberitahu EF Core bahwa TransactionViewModel tidak memiliki Primary Key 
			// karena merupakan hasil gabungan (UNION) beberapa tabel.
			modelBuilder.Entity<TransactionViewModel>().HasNoKey();
		}
	}
}