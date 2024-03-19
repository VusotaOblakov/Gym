using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication2.Data;
;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
//builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlServer("workstation id=kursachdb.mssql.somee.com;packet size=4096;user id=Alex_san9_SQLLogin_1;pwd=yzyivtua7f;data source=kursachdb.mssql.somee.com;persist security info=False;initial catalog=kursachdb; TrustServerCertificate=True"));
builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=test2;Integrated Security=True; TrustServerCertificate=True"));
//builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=test;Integrated Security=true;"));
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddTransient<GoogleCalendarService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
//app.UseStatusCodePagesWithReExecute("/Error/NotFound", "?statusCode={0}");
app.UseStatusCodePagesWithReExecute("/Error/NotFound");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
