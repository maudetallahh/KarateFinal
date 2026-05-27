using KarateFinal.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<KarateFinal.Services.EmailService>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
}); builder.Services.AddDbContext<KarateContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("KarateDB")
    ));

var app = builder.Build();

// استدعاء الادمن من ملف السيييد
using (var scope = app.Services.CreateScope())
{
    SeedData.Initialize(scope.ServiceProvider);
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// حماية الصفحات
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();
    var role = context.Session.GetString("Role");

    if (path != null && path.StartsWith("/admin") && role != "Admin")
    {
        context.Response.Redirect("/Account/Login");
        return;
    }

    if (path != null && path.StartsWith("/club") && role != "Club" && role != "Admin")
    {
        context.Response.Redirect("/Account/Login");
        return;
    }

    if (path != null && path.StartsWith("/player") && role != "Player" && role != "Admin")
    {
        context.Response.Redirect("/Account/Login");
        return;
    }
    // منع اللاعب من صفحة الأندية
    if (path != null && path.StartsWith("/tour") && role == "Player")
    {
        context.Response.Redirect("/Player/Dashboard");
        return;
    }

    await next();
});

app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();