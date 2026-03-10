namespace DashboardMonitoringWB.Models
{
	public class TransactionViewModel
	{
		// Identitas & Asal Tabel
		public string TicketNo { get; set; }
		public string SourceTable { get; set; }

		// Kendaraan & Supir
		public string VehicleNo { get; set; }
		public string DriverName { get; set; }

		// Pihak Terkait & Barang
		public string CustomerSupplier { get; set; }
		public string EstateCode { get; set; }
		public string ProductPart { get; set; }

		// Tanggal & Waktu (String untuk memudahkan View)
		public DateTime? DateIn { get; set; }
		public DateTime? DateOut { get; set; }
		public string TimeIn { get; set; }
		public string TimeOut { get; set; }

		// Berat (Netto)
		public decimal WeightIn { get; set; }
		public decimal WeightOut { get; set; }
		public decimal Netto { get; set; }

		// Status & Info Tambahan
		public string Status { get; set; }
		public string TransferType { get; set; }
		public string Origin { get; set; }
		public string Destination { get; set; }
	}
}