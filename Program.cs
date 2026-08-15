using Microsoft.AspNetCore.Authentication.Cookies;
using MarwadiGheeSweetsWeb.Configuration;
using MarwadiGheeSweetsWeb.Repositories;
using MarwadiGheeSweetsWeb.Services;

var builder = WebApplication.CreateBuilder(args);

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

// ── MVC ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

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
