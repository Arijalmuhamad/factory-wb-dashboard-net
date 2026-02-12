using Microsoft.EntityFrameworkCore;
using DashboardMonitoringWB.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Ambil connection string dan simpan di variabel
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Tambahkan pengecekan jika ternyata string-nya kosong/null
if (string.IsNullOrEmpty(connectionString))
{
	throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

// 3. Masukkan ke DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseMySQL(connectionString));


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
