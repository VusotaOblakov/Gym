using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlServer("workstation id=kursachdb.mssql.somee.com;packet size=4096;user id=Alex_san9_SQLLogin_1;pwd=yzyivtua7f;data source=kursachdb.mssql.somee.com;persist security info=False;initial catalog=kursachdb; TrustServerCertificate=True"));
//builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=test;Integrated Security=True; TrustServerCertificate=True"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
