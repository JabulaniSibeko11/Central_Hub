using Central_Hub.Data;
using Central_Hub.Filter;
using Central_Hub.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<Central_HubDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ILicenseService, LicenseService>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<Central_HubDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<CompanyAuthFilter>();

// ? ADD THIS BEFORE builder.Build() - This was your main issue!
builder.Services.AddAuthentication()
    .AddCookie("CentralAdminScheme", options =>
    {
        options.Cookie.Name = "CentralAdminCookie";  // ? Distinct cookie name (recommended)
        options.LoginPath = "/Home/AdminLogin";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

var app = builder.Build();  // ? All services must be before this line

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();  // ? Ensure this comes before UseAuthorization()
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=LandingPage}/{id?}");

app.MapRazorPages();

app.Run();