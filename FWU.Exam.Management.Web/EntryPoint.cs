using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Web.Data;
using FWU.Exam.Management.Web.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

public partial class EntryPoint
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IAuditUserProvider, HttpContextAuditUserProvider>();
        builder.Services.AddScoped<AuditableSaveChangesInterceptor>();

        builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            options.UseSqlServer(connectionString);
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditableSaveChangesInterceptor>());
        });

        builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();
        builder.Services.AddControllersWithViews();
        builder.Services.AddScoped<IFileUploadHelper, FileUploadHelper>();
        var app = builder.Build();

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
        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.MapRazorPages()
           .WithStaticAssets();

        using (var scope = app.Services.CreateScope())
        {
            await UserSeeder.SeedRolesAsync(scope.ServiceProvider);
            await UserSeeder.SeedSuperAdminAsync(scope.ServiceProvider);            
            await LocationSeeder.SeedLocationDataAsync(scope.ServiceProvider);
        }

        app.Run();
    }
}
