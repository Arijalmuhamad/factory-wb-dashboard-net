namespace DashboardMonitoringWB.Models
{
    public class DashboardSummary
	{
		// Data Statistik Atas (Small Boxes)
		public int RegistIn { get; set; }
		public int WeighingIn { get; set; }
		public int WeighingOut { get; set; }
		public int RegistOut { get; set; }

		// Data Total Truck (Info Cards)
		public int TotalTruckFFB { get; set; }
		public int TotalTruckSales { get; set; }
		public int TotalTruckTransfer { get; set; }
		public int TotalTruckOthers { get; set; }

		// Data Total Tonase
		public int TotalTonnageFFB { get; set; }
		public int TotalTonnageSales { get; set; }
		public int TotalTonnageTransfer { get; set; }
		public int TotalTonnageOthers { get; set; }
	}

}
