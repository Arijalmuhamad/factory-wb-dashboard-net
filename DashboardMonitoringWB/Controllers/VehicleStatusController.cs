using Microsoft.AspNetCore.Mvc;
using DashboardMonitoringWB.Models;
using DashboardMonitoringWB.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardMonitoringWB.Controllers
{
	public class VehicleStatusController : Controller
	{
		private readonly ApplicationDbContext _context;

		public VehicleStatusController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> TruckRegistration(int page = 1)
		{
			var viewModel = new VehicleStatusViewModel();
			int pageSize = 10; // Jumlah data per halaman
			string dateFilter = "DATE(datein) = IF(HOUR(NOW()) >= 7, CURDATE(), DATE_SUB(CURDATE(), INTERVAL 1 DAY))";

			try
			{
				var connection = _context.Database.GetDbConnection();
				if (connection.State != ConnectionState.Open) await connection.OpenAsync();

				using (var command = connection.CreateCommand())
				{
					// 1. Hitung Total Data untuk Paginasi (Total Reg In)
					command.CommandText = $"SELECT COUNT(*) FROM vw_db_monitoring_vehicle_status WHERE regis_in = 'T' AND weighing_out = 'F' AND {dateFilter}";
					var totalInResult = await command.ExecuteScalarAsync();
					int totalIn = Convert.ToInt32(totalInResult);

					// 2. Query Data dengan LIMIT dan OFFSET
					int offset = (page - 1) * pageSize;
					command.CommandText = $@"
                        SELECT Source, RegisterTime, vehicleno, driver, SupplierName 
                        FROM vw_db_monitoring_vehicle_status 
                        WHERE regis_in = 'T' AND weighing_out = 'F' AND {dateFilter} 
                        ORDER BY RegisterTime DESC
                        LIMIT {pageSize} OFFSET {offset}";

					using (var result = await command.ExecuteReaderAsync())
					{
						while (await result.ReadAsync())
						{
							viewModel.Vehicles.Add(new VehicleEntry
							{
								SourceTable = result["Source"]?.ToString(),
								RegisterTime = result["RegisterTime"] != DBNull.Value
									? Convert.ToDateTime(result["RegisterTime"])
									: DateTime.Now,
								LicensePlate = result["vehicleno"]?.ToString(),
								DriverName = result["driver"]?.ToString(),
								SupplierName = result["SupplierName"]?.ToString(),
								Status = "REG IN",
								OperationalStatus = "WAITING WEIGH IN"
							});
						}
					}

					// 3. Hitung Total Reg Out
					command.CommandText = $"SELECT COUNT(*) FROM vw_db_monitoring_vehicle_status WHERE regis_out = 'T' AND {dateFilter}";
					var countOutResult = await command.ExecuteScalarAsync();
					viewModel.TotalRegOut = countOutResult != DBNull.Value ? Convert.ToInt32(countOutResult) : 0;

					ViewBag.CurrentPage = page;
					ViewBag.TotalPages = (int)Math.Ceiling((double)totalIn / pageSize);
					viewModel.TotalRegIn = totalIn;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex.Message);
			}

			return View(viewModel);
		}

		public async Task<IActionResult> AlreadyWeighed(int page = 1)
		{
			var viewModel = new VehicleStatusViewModel();
			int pageSize = 10;
			string dateFilter = "DATE(datein) = IF(HOUR(NOW()) >= 7, CURDATE(), DATE_SUB(CURDATE(), INTERVAL 1 DAY))";

			try
			{
				var connection = _context.Database.GetDbConnection();
				if (connection.State != ConnectionState.Open) await connection.OpenAsync();

				using (var command = connection.CreateCommand())
				{
					// 1. Ambil Statistik untuk Info-Box (Disesuaikan dengan properti di ViewModel)

					// Total Weighing In (Sudah masuk WB In tapi belum WB Out)
					command.CommandText = $"SELECT COUNT(*) FROM vw_db_monitoring_vehicle_status WHERE weighing_in = 'T' AND weighing_out = 'F' AND {dateFilter}";
					viewModel.TotalWeighingIn = Convert.ToInt32(await command.ExecuteScalarAsync());

					// Total Weighing Out (Sudah selesai timbang keluar)
					command.CommandText = $"SELECT COUNT(*) FROM vw_db_monitoring_vehicle_status WHERE weighing_out = 'T' AND {dateFilter}";
					viewModel.TotalWeighingOut = Convert.ToInt32(await command.ExecuteScalarAsync());

					// 2. Hitung Rata-rata Durasi aktual (Menit) dari pendaftaran sampai timbang keluar
					// Menggunakan TIMESTAMPDIFF untuk menghitung selisih menit antara RegisterTime dan WeighingOutTime
					command.CommandText = $@"
						SELECT ROUND(AVG(
						TIME_TO_SEC(WeighingOutTime) - TIME_TO_SEC(RegisterTime)) / 60) AS avg_minutes
						FROM vw_db_monitoring_vehicle_status
						WHERE weighing_out = 'T' AND {dateFilter}
						AND WeighingOutTime IS NOT NULL";

					var avgResult = await command.ExecuteScalarAsync();
					viewModel.AvgDuration = Convert.ToInt32(Math.Round(Convert.ToDouble(avgResult)));

					// 3. Query Data untuk Tabel dengan Paginasi
					int offset = (page - 1) * pageSize;
					int totalRecords = viewModel.TotalWeighingIn + viewModel.TotalWeighingOut;

					command.CommandText = $@"
                        SELECT Source, RegisterTime, vehicleno, driver, SupplierName, weighing_in, weighing_out
                        FROM vw_db_monitoring_vehicle_status 
                        WHERE weighing_in = 'T' AND {dateFilter}
                        ORDER BY RegisterTime DESC
                        LIMIT {pageSize} OFFSET {offset}";

					using (var result = await command.ExecuteReaderAsync())
					{
						while (await result.ReadAsync())
						{
							bool isOut = result["weighing_out"]?.ToString() == "T";
							viewModel.Vehicles.Add(new VehicleEntry
							{
								SourceTable = result["Source"]?.ToString(),
								RegisterTime = result["RegisterTime"] != DBNull.Value
									? Convert.ToDateTime(result["RegisterTime"])
									: DateTime.Now,
								LicensePlate = result["vehicleno"]?.ToString(),
								DriverName = result["driver"]?.ToString(),
								SupplierName = result["SupplierName"]?.ToString(),
								OperationalStatus = isOut ? "WAITING WEIGH OUT" : "WAITING WEIGH IN"
							});
						}
					}

					// 4. Set ViewBag untuk Paginasi di View
					ViewBag.CurrentPage = page;
					ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error AlreadyWeighed: " + ex.Message);
			}

			return View(viewModel);
		}
	}
}