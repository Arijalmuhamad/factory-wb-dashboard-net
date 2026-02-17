using DashboardMonitoringWB.Data;
using DashboardMonitoringWB.Hubs;
using DashboardMonitoringWB.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DashboardMonitoringWB.Services
{
	public class DashboardWorker : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IHubContext<DashboardHub> _hubContext;
		private readonly ILogger<DashboardWorker> _logger;

		public DashboardWorker(IServiceScopeFactory scopeFactory, IHubContext<DashboardHub> hubContext, ILogger<DashboardWorker> logger)
		{
			_scopeFactory = scopeFactory;
			_hubContext = hubContext;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using (var scope = _scopeFactory.CreateScope())
					{
						var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

						// Jalankan query SQL UNION Anda
						var summary = await GetDataFromDatabase(context);

						// Kirim data ke SEMUA browser yang sedang buka dashboard
						await _hubContext.Clients.All.SendAsync("UpdateDashboard", summary, stoppingToken);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError($"Worker Error: {ex.Message}");
				}

				await Task.Delay(5000, stoppingToken); // Cek database setiap 5 detik
			}
		}

		private async Task<DashboardSummary> GetDataFromDatabase(ApplicationDbContext context)
		{
			var result = new DashboardSummary();

			// Query yang menggabungkan hitungan status (atas) dan hitungan total truk (bawah)
			string query = @"
        SELECT 
            -- Bagian 1: Statistik Status (Small Boxes)
            SUM(regis_in = 'T') as regist_in,
            SUM(weighing_in = 'T') as weighing_in,
            SUM(weighing_out = 'T') as weighing_out,
            SUM(regis_out = 'T') as regist_out,

            -- Bagian 2: Total Truck Berdasarkan Tabel Masing-masing (Info Cards)
            -- Kriteria: weighing_out='T', wbout != 0, regis_out='T'
            SUM(source = 'tbs' AND weighing_out = 'T' AND wbout <> 0 AND regis_out = 'T') as total_ffb,
            SUM(source = 'cpo' AND weighing_out = 'T' AND wbout <> 0 AND regis_out = 'T') as total_sales,
            SUM(source = 'trans' AND weighing_out = 'T' AND wbout <> 0 AND regis_out = 'T') as total_transfer,
            SUM(source = 'data' AND weighing_out = 'T' AND wbout <> 0 AND regis_out = 'T') as total_others,

			 -- Bagian 3: Total Tonase Berdasarkan Tabel Masing-masing (Info Cards)
			SUM(CASE WHEN source='tbs' AND weighing_out='T' AND wbout<>0 AND regis_out='T' THEN netto ELSE 0 END) AS tonnage_ffb,
            SUM(CASE WHEN source='cpo' AND weighing_out='T' AND wbout<>0 AND regis_out='T' THEN netto ELSE 0 END) AS tonnage_sales,
            SUM(CASE WHEN source='trans' AND weighing_out='T' AND wbout<>0 AND regis_out='T' THEN netto ELSE 0 END) AS tonnage_transfer,
           SUM(CASE WHEN source='data' AND weighing_out='T' AND wbout<>0 AND regis_out='T' THEN netto ELSE 0 END) AS tonnage_others

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

			var connection = context.Database.GetDbConnection();
			if (connection.State != System.Data.ConnectionState.Open) await connection.OpenAsync();

			using (var command = connection.CreateCommand())
			{
				command.CommandText = query;
				using (var reader = await command.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						// Mapping Statistik Atas
						result.RegistIn = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader[0]);
						result.WeighingIn = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader[1]);
						result.WeighingOut = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader[2]);
						result.RegistOut = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader[3]);

						// Mapping Total Truck (Baru)
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
			return result;
		}
	}
}