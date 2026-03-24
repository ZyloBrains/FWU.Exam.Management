using fwu_examination_management_system.Data;
using fwu_examination_management_system.Helpers;
using fwu_examination_management_system.Models;
using fwu_examination_management_system.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IFileUploadHelper, FileUploadHelper>();
// For Identity UI (non‑generic IEmailSender)
builder.Services.AddTransient<IEmailSender, EmailSender>();
// Register email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// Register the non‑generic IEmailSender (used by Identity UI pages)
//builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();
// Register IEmailSender<AppUser>
builder.Services.AddTransient<IEmailSender<AppUser>, EmailSender>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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

app.MapRazorPages()
   .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedRolesAsync(scope.ServiceProvider);
    await DbSeeder.SeedSuperAdminAsync(scope.ServiceProvider);
}

app.Run();
