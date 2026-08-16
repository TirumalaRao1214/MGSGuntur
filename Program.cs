using Microsoft.AspNetCore.Authentication.Cookies;
using MarwadiGheeSweetsWeb.Configuration;
using MarwadiGheeSweetsWeb.Repositories;
using MarwadiGheeSweetsWeb.Services;

// Render injects PORT; bind to it so the health-check passes.
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://+:{port}");

// ── Strongly-typed configuration ──────────────────────────────────────────────
builder.Services.Configure<ShopSettings>(
    builder.Configuration.GetSection(ShopSettings.SectionName));

// ── Cookie Authentication ─────────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan   = TimeSpan.FromHours(8);
        options.SlidingExpiration= true;
        options.Cookie.HttpOnly  = true;
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
    });

// ── Repositories  (swap these lines for EF Core migration) ────────────────────
builder.Services.AddSingleton<IProductRepository,  JsonProductRepository>();
builder.Services.AddSingleton<ICategoryRepository, JsonCategoryRepository>();
builder.Services.AddSingleton<IUserRepository,     JsonUserRepository>();

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IProductService,       ProductService>();
builder.Services.AddScoped<ICategoryService,      CategoryService>();
builder.Services.AddScoped<IWhatsAppOrderService, WhatsAppOrderService>();
builder.Services.AddSession();

// ── MVC ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // HSTS / HTTPS-redirect are handled by Render's TLS termination proxy.
}

// ── Security headers ──────────────────────────────────────────────────────────
app.Use(async (ctx, next) =>
{
    var headers = ctx.Response.Headers;
    headers["X-Frame-Options"]        = "SAMEORIGIN";
    headers["X-Content-Type-Options"] = "nosniff";
    headers["Referrer-Policy"]        = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"]     = "geolocation=(), microphone=(), camera=()";
    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://verify.msg91.com https://pass.hustnsoft.com https://pass.hostnsoft.com https://hcaptcha.com https://*.hcaptcha.com; " +
        "style-src 'self' 'unsafe-inline'; " +
        "font-src 'self'; " +
        "img-src 'self' data: https:; " +
        "frame-src https://maps.google.com https://www.google.com https://hcaptcha.com https://*.hcaptcha.com; " +
        "connect-src 'self' https://api.msg91.com https://verify.msg91.com https://control.msg91.com https://pass.hustnsoft.com https://pass.hostnsoft.com https://hcaptcha.com https://*.hcaptcha.com";
    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// ── Custom routes ──────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "sweetsIndex",
    pattern: "sweets",
    defaults: new { controller = "Products", action = "Index" });

app.MapControllerRoute(
    name: "productCategory",
    pattern: "sweets/category/{category}",
    defaults: new { controller = "Products", action = "Index" });

app.MapControllerRoute(
    name: "productDetail",
    pattern: "sweets/{slug}",
    defaults: new { controller = "Products", action = "Details" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
