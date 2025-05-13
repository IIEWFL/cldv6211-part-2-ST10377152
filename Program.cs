/*using EventEase.Data;
using Microsoft.EntityFrameworkCore;

namespace EventEase
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<POEDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("LocalConn")));

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
        }
    }
}*/

using EventEase.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

//How does ASP.NET MVC link views and controllers? : https://stackoverflow.com/questions/2030539/how-does-asp-net-mvc-link-views-and-controllers
//Generating migrations in Visual Studio: https://documentation.red-gate.com/sca/developing-databases/working-with-the-visual-studio-extension/generating-migrations-in-visual-studio
//How to migrate a Visual Studio 2012 ASP.Net MVC 4 project to use an SQL Server instance instead of a data file?: https://stackoverflow.com/questions/13819207/how-to-migrate-a-visual-studio-2012-asp-net-mvc-4-project-to-use-an-sql-server-i
//Tutorial: Get started with C# and ASP.NET Core in Visual Studio: https://learn.microsoft.com/en-us/visualstudio/get-started/csharp/tutorial-aspnet-core?view=vs-2022
//Adding a View to an MVC Application: https://learn.microsoft.com/en-us/aspnet/mvc/overview/getting-started/introduction/adding-a-view

namespace EventEase
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<POEDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("AzureConn")));

            // Add logging services
            builder.Services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Removed UseExceptionHandler("/Home/Error") to avoid needing Error.cshtml
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
        }
    }
}
