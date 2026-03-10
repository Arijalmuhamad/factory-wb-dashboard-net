using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DashboardMonitoringWB.Data;
using DashboardMonitoringWB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardMonitoringWB.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper untuk Filter Tanggal Operasional (Cut-off 07:00 AM)
        // Perbaikan: Tidak perlu memanggil DATE() dua kali di WHERE jika filter sudah mengembalikan DATE
        private string GetDateFilter() =>
            "(CASE WHEN HOUR(NOW()) >= 7 THEN DATE(NOW()) ELSE DATE(SUBDATE(NOW(), 1)) END)";

        // --- 1. MENU FFB (TBS) ---
        public async Task<IActionResult> FFB(string status, int page = 1)
        {
            string dateFilter = GetDateFilter();
            string sql = $@"
                SELECT 
                    a.wbsid AS TicketNo, 'TBS' AS SourceTable,
                    IFNULL(a.vehicleno, '-') AS VehicleNo, IFNULL(a.driver, '-') AS DriverName,
                    IFNULL(CASE WHEN a.estate = 'LUAR' THEN b.BP_NAME ELSE c.ESTNAME END, 'Unknown') AS CustomerSupplier,
                    a.estate AS EstateCode,
                    'FFB' AS ProductPart, a.datein AS DateIn, a.dateout AS DateOut,
                    a.timein AS TimeIn,
                    a.timeout AS TimeOut,
                    IFNULL(a.wbin, 0) AS WeightIn, IFNULL(a.wbout, 0) AS WeightOut, IFNULL(a.netto_ag, 0) AS Netto,
                    CASE 
                        WHEN a.regis_out = 'T' THEN 'COMPLETED'
                        WHEN a.weighing_out = 'T' THEN 'WEIGHING OUT'
                        WHEN a.weighing_in = 'T' THEN 'WEIGHING IN'
                        ELSE 'REGISTERED'
                    END AS Status,
                    '' AS TransferType, '' AS Origin, '' AS Destination
                FROM wb_automation_datatbs_tab a
                LEFT JOIN bridge_bp b ON a.supplier = b.BP_CODE
                LEFT JOIN bridge_businessunit c ON a.estate = c.PLANT
                WHERE DATE(a.dateout) = {dateFilter}";

            return await ProcessData(sql, status, page);
        }

        // --- 2. MENU SALES (CPO/PK) ---
        public async Task<IActionResult> Sales(string status, int page = 1)
        {
            string dateFilter = GetDateFilter();
            string sql = $@"
                SELECT 
                    a.wbsid AS TicketNo, 'CPO/PK' AS SourceTable,
                    IFNULL(a.vehicleno, '-') AS VehicleNo, 
                    IFNULL(a.driver, '-') AS DriverName,
                    IFNULL(b.BP_NAME, '-') AS CustomerSupplier, 
                    IFNULL(a.partname, '-') AS ProductPart,
                    a.datein AS DateIn, 
                    a.dateout AS DateOut,
                    '' AS EstateCode,
                    a.timein AS TimeIn,
                    a.timeout AS TimeOut,
                    IFNULL(a.wbin, 0) AS WeightIn, IFNULL(a.wbout, 0) AS WeightOut, IFNULL(a.netto, 0) AS Netto,
                    CASE    
                        WHEN a.regis_out = 'T' THEN 'COMPLETED'
                        WHEN a.weighing_out = 'T' THEN 'WEIGHING OUT'
                        WHEN a.weighing_in = 'T' THEN 'WEIGHING IN'
                        ELSE 'REGISTERED'
                    END AS Status,
                    '' AS TransferType, '' AS Origin, '' AS Destination
                FROM wb_automation_datacpo_tab a
                LEFT JOIN bridge_bp b ON a.csid = b.BP_CODE
                WHERE DATE(a.dateout) = {dateFilter}";

            return await ProcessData(sql, status, page);
        }

        // --- 3. MENU TRANSFER ---
        public async Task<IActionResult> Transfer(int page = 1)
        {
            string dateFilter = GetDateFilter();
            string sql = $@"
                SELECT 
                    wbsid AS TicketNo, 'TRANSFER' AS SourceTable,
                    IFNULL(vehicleno, '-') AS VehicleNo, IFNULL(driver, '-') AS DriverName,
                    '' AS CustomerSupplier, IFNULL(partcat, '-') AS ProductPart,
                    datein AS DateIn, dateout AS DateOut, '' AS EstateCode,
                    IFNULL(DATE_FORMAT(datein, '%H:%i:%s'), '') AS TimeIn,
                    IFNULL(DATE_FORMAT(dateout, '%H:%i:%s'), '') AS TimeOut,
                    IFNULL(wbin, 0) AS WeightIn, IFNULL(wbout, 0) AS WeightOut, IFNULL(netto, 0) AS Netto,
                    CASE WHEN regis_out = 'T' THEN 'COMPLETED' ELSE 'IN PROGRESS' END AS Status,
                    IFNULL(transtype, '-') AS TransferType, IFNULL(origin, '-') AS Origin, IFNULL(destination, '-') AS Destination
                FROM wb_automation_producttrans_tab
                WHERE DATE(dateout) = {dateFilter}";

            return await ProcessData(sql, null, page);
        }

        // --- 4. MENU OTHERS ---
        public async Task<IActionResult> Others(int page = 1)
        {
            string dateFilter = GetDateFilter();
            string sql = $@"
                SELECT 
                    a.wbsid AS TicketNo,
                    'OTHERS' AS SourceTable,
                    IFNULL(a.vehicleno, '-') AS VehicleNo,
                    IFNULL(a.driver, '-') AS DriverName,
                    IFNULL(c.supname, '-') AS CustomerSupplier, 
                    IFNULL(b.partname, '-') AS ProductPart,
                    a.datein AS DateIn,
                    a.dateout AS DateOut,
                    a.supid AS EstateCode,
                    a.timein AS TimeIn,
                    a.timeout AS TimeOut,
                    IFNULL(a.wbin, 0) AS WeightIn, IFNULL(a.wbout, 0) AS WeightOut, IFNULL(a.netto, 0) AS Netto,
                    CASE    
                        WHEN regis_out = 'T' THEN 'COMPLETED'
                        WHEN weighing_out = 'T' THEN 'WEIGHING OUT'
                        WHEN weighing_in = 'T' THEN 'WEIGHING IN'
                        ELSE 'REGISTERED'
                    END AS Status,
                    '' AS TransferType, '' AS Origin, '' AS Destination
                FROM wb_automation_data_tab a
                JOIN wbs_part_lain_lain_tab b ON a.partid = b.partid
                JOIN wbs_supplier_lain_lain_tab c ON a.supid = c.supid
                WHERE DATE(a.dateout) = {dateFilter}";

            return await ProcessData(sql, null, page);
        }

        // --- CORE PROCESSOR ---
        private async Task<IActionResult> ProcessData(string baseSql, string status, int page)
        {
            try
            {
                int pageSize = 10;
                string finalSql = baseSql;

                // Tambahkan Filter Status (Jika ada) sebelum Order By
                if (!string.IsNullOrEmpty(status))
                {
                    if (status == "regin") finalSql += " AND regis_in = 'T' AND weighing_in = 'F'";
                    else if (status == "weighin") finalSql += " AND weighing_in = 'T' AND weighing_out = 'F'";
                    else if (status == "weighout") finalSql += " AND weighing_out = 'T' AND regis_out = 'F'";
                    else if (status == "regout") finalSql += " AND regis_out = 'T'";
                }

                finalSql += " ORDER BY DateIn DESC";

                // Eksekusi Query
                var allData = await _context.Database.SqlQueryRaw<TransactionViewModel>(finalSql).ToListAsync();

                // Hitung Statistik
                ViewBag.TotalTonase = allData.Sum(x => (double?)x.Netto) ?? 0;
                ViewBag.TotalTruck = allData.Count;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling((double)allData.Count / pageSize));
                ViewBag.CurrentStatus = status;

                // Pagination di sisi Memory (List)
                var pagedData = allData.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                
                return View(pagedData);
            }
            catch (Exception ex)
            {
                // Log error dan kembalikan view kosong agar tidak stuck loading
                Console.WriteLine($"Error in ProcessData: {ex.Message}");
                ViewBag.ErrorMessage = "Gagal memuat data dari database.";
                return View(new List<TransactionViewModel>());
            }
        }
    }
}