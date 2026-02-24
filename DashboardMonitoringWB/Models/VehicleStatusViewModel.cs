using System;
using System.Collections.Generic;

namespace DashboardMonitoringWB.Models
{
	public class VehicleStatusViewModel
	{
		// Properti untuk Statistik di Info-Box
		public int TotalWeighingIn { get; set; }  // Untuk "Total Weighing In"
		public int TotalWeighingOut { get; set; } // Untuk "Total Weighing Out"
		public int AvgDuration { get; set; }      // Untuk "Rata-rata Durasi"

		// Properti yang sudah ada sebelumnya
		public int TotalRegIn { get; set; }
		public int TotalRegOut { get; set; }
		public double AverageDuration { get; set; }

		public List<VehicleEntry> Vehicles { get; set; } = new List<VehicleEntry>();
	}

	public class VehicleEntry
	{
		public string TicketNo { get; set; }
		public DateTime RegisterTime { get; set; }
		public string LicensePlate { get; set; }
		public string DriverName { get; set; }
		public string SupplierName { get; set; }
		public string Status { get; set; }
		public string OperationalStatus { get; set; }
		public string SourceTable { get; set; }
	}
}