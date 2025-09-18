using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using WebThiTracNghiem.Models;
using WebThiTracNghiem.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<ApplicationDbContext>(
	options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.AddRazorPages();

builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddMemoryCache();

builder.Services.AddSession();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/identity/account/login";
	options.AccessDeniedPath = "/identity/account/accessdenied";
	options.LogoutPath = "//identity/account/logout";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseStaticFiles();

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
    ApplicationUser.SeedUserAsync(services).GetAwaiter().GetResult();
}

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
	name: "areas",
	pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();