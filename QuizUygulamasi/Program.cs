using Microsoft.EntityFrameworkCore;
using QuizUygulamasi.Data;
using QuizUygulamasi.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// SignalR'ý düzgün yapýlandýrýn
builder.Services.AddSignalR(options => {
    options.EnableDetailedErrors = true; // Geliþtirme ortamýnda hata ayýklama
});
// Program.cs'e bu ayarlarý ekleyin
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder => builder
        .WithOrigins("https://localhost:7038", "http://localhost:7038") // hem HTTP hem HTTPS
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

// app.UseRouting()'den sonra ekleyin

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<QuizHub>("/quizhub");
app.Run();
