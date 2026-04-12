using Central_Hub.Data;
using Central_Hub.Filter;
using Central_Hub.Infrastructure.Options;
using Central_Hub.Services;
using Central_Hub.Services.Email;
using Central_Hub.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ───────────────────────────────────────────────
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<Central_HubDbContext>(o => o.UseSqlServer(conn));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ── Identity ───────────────────────────────────────────────
builder.Services.AddDefaultIdentity<IdentityUser>(o =>
    o.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<Central_HubDbContext>();

// ── Cookie auth for admin UI ───────────────────────────────
builder.Services.AddAuthentication()
    .AddCookie("CentralAdminScheme", o =>
    {
        o.Cookie.Name = "CentralAdminCookie";
        o.LoginPath = "/Login";
        o.AccessDeniedPath = "/Home/AccessDenied";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Cookie.SameSite = SameSiteMode.Strict;
    });



//builder.Services.AddControllersWithViews(options =>
//{
//    var policy = new AuthorizationPolicyBuilder("CentralAdminScheme")
//        .RequireAuthenticatedUser()
//        .Build();

//    options.Filters.Add(new AuthorizeFilter(policy));
//});
// ── Business services ──────────────────────────────────────
builder.Services.AddScoped<ILicenseService, LicenseService>();
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// ── Security services ──────────────────────────────────────
builder.Services.AddSingleton<IAesEncryptionService, AesEncryptionService>();
builder.Services.AddSingleton<IHmacSigningService, HmacSigningService>();
builder.Services.AddSingleton<INonceCache, InMemoryNonceCache>();
// Register the nonce cache as a hosted service so its timer runs
builder.Services.AddHostedService(sp =>
    (InMemoryNonceCache)sp.GetRequiredService<INonceCache>());

// ── API filter ─────────────────────────────────────────────
builder.Services.AddScoped<CompanyAuthFilter>();

// ── MVC + compression ──────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddResponseCompression();

var app = builder.Build();

// ── Pipeline ───────────────────────────────────────────────
if (app.Environment.IsDevelopment())
    app.UseMigrationsEndPoint();
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseResponseCompression();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Security response headers
app.Use(async (ctx, next) =>
{
    var h = ctx.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";
    h["X-Frame-Options"] = "DENY";
    h["X-XSS-Protection"] = "1; mode=block";
    h["Referrer-Policy"] = "strict-origin-when-cross-origin";
    h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    h["Cache-Control"] = "no-store";
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=LandingPage}/{id?}",
    defaults: new { controller = "Home" });


app.MapRazorPages();
app.Run();