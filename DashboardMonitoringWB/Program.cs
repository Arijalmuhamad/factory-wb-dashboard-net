using DashboardMonitoringWB.Data;
using DashboardMonitoringWB.Hubs;
using DashboardMonitoringWB.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



// ======================================================
// 1. LOAD CONFIGURATION
// Ambil connection string dari appsettings.json
// ======================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Validasi supaya aplikasi tidak jalan jika DB belum disetting
if (string.IsNullOrEmpty(connectionString))
{
	throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}



// ======================================================
// 2. REGISTER SERVICES (DEPENDENCY INJECTION)
// Semua service aplikasi didaftarkan di sini
// ======================================================

// DbContext → koneksi ke MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseMySQL(connectionString));

// MVC Controller + Razor Views
builder.Services.AddControllersWithViews();

// SignalR → komunikasi real-time dashboard
builder.Services.AddSignalR();

// Background Worker → kirim data realtime ke dashboard
builder.Services.AddHostedService<DashboardWorker>();



var app = builder.Build();



// ======================================================
// 3. MIDDLEWARE PIPELINE
// Mengatur bagaimana request diproses
// Urutan middleware SANGAT penting
// ======================================================

// Error handling production
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

// Redirect HTTP -> HTTPS
app.UseHttpsRedirection();

// Routing harus sebelum Authorization & Endpoint
app.UseRouting();

// Authorization (login/role nanti disini)
app.UseAuthorization();

// Static files (css/js/img)
app.MapStaticAssets();



// ======================================================
// 4. ENDPOINT MAPPING
// Menentukan URL apa menuju ke fitur apa
// ======================================================

// SignalR Hub endpoint (realtime websocket)
app.MapHub<DashboardHub>("/dashboardHub");

// Default MVC route
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();



// ======================================================
// 5. RUN APPLICATION
// ======================================================
app.Run();
