using System;
using System.Collections.Generic;

namespace DashboardMonitoringWB.Models
{
	public class VehicleStatusViewModel
	{
		public int TotalRegIn { get; set; }
		public int TotalRegOut { get; set; }
		public double AverageDuration { get; set; }

		public List<VehicleEntry> Vehicles { get; set; } = new List<VehicleEntry>();
	}

	public class VehicleEntry
	{
		public string TicketNo { get; set; }
		public DateTime RegisterTime { get; set; }

		// Menggunakan nama yang konsisten dengan query baru Anda (LicensePlate tetap bisa digunakan)
		public string LicensePlate { get; set; }

		public string DriverName { get; set; }

		// Properti ini tetap menampung hasil dari supplier/csid/plant_dest_sender/supid
		public string SupplierName { get; set; }

		public string Status { get; set; }
		public string OperationalStatus { get; set; }
		public string SourceTable { get; set; }
	}
}