using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Services;
using FWU.Exam.Management.Infrastructure.Services.Permissions;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Web.Data.Seeders;
using FWU.Exam.Management.Web.Helpers;
using FWU.Exam.Management.Web.Authorization;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure.Interceptor;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Middleware;

public partial class EntryPoint
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IAuditUserProvider, HttpContextAuditUserProvider>();
        builder.Services.AddScoped<ITenantContext, TenantContext>();
        builder.Services.AddScoped<AuditableSaveChangesInterceptor>();
        builder.Services.AddScoped<TenantSaveChangesInterceptor>();

        builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            options.UseSqlServer(connectionString);
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditableSaveChangesInterceptor>());
            options.AddInterceptors(serviceProvider.GetRequiredService<TenantSaveChangesInterceptor>());
        });

        builder.Services.AddDefaultIdentity<AppUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
            {
                OnRedirectToLogin = ctx =>
                {
                    var tenantCode = ctx.HttpContext.Items["TenantCode"] as string;
                    var returnUrl = ctx.HttpContext.Items["OriginalPath"] as string ?? ctx.Request.Path;

                    if (!string.IsNullOrEmpty(tenantCode))
                    {
                        var loginPath = $"/tenant/{tenantCode}/Identity/Account/Login";
                        ctx.Response.Redirect($"{loginPath}?ReturnUrl={Uri.EscapeDataString(returnUrl!)}");
                    }
                    else
                    {
                        ctx.Response.Redirect(ctx.RedirectUri);
                    }

                    return Task.CompletedTask;
                },
                OnRedirectToAccessDenied = ctx =>
                {
                    var tenantCode = ctx.HttpContext.Items["TenantCode"] as string;
                    var returnUrl = ctx.HttpContext.Items["OriginalPath"] as string ?? ctx.Request.Path;

                    if (!string.IsNullOrEmpty(tenantCode))
                    {
                        var accessDeniedPath = $"/tenant/{tenantCode}/Identity/Account/AccessDenied";
                        ctx.Response.Redirect($"{accessDeniedPath}?ReturnUrl={Uri.EscapeDataString(returnUrl!)}");
                    }
                    else
                    {
                        ctx.Response.Redirect(ctx.RedirectUri);
                    }

                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddControllersWithViews();
        builder.Services.AddScoped<IBoardService, BoardService>();
        builder.Services.AddScoped<ICollegeProgramService, CollegeProgramService>();
        builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
        builder.Services.AddScoped<ICollegeService, CollegeService>();
        builder.Services.AddScoped<IFacultyService, FacultyService>();
        builder.Services.AddScoped<IDepartmentService, DepartmentService>();
        builder.Services.AddScoped<IDashboardService, DashboardService>();
        builder.Services.AddScoped<IStudentRegistrationService, StudentRegistrationService>();
        builder.Services.AddScoped<IExamScheduleService, ExamScheduleService>();
        builder.Services.AddScoped<IProgramService, ProgramService>();
        builder.Services.AddScoped<ILevelService, LevelService>();
        builder.Services.AddScoped<INoticeService, NoticeService>();
        builder.Services.AddScoped<ICollegeTypeService, CollegeTypeService>();
        builder.Services.AddScoped<ISubjectTypeService, SubjectTypeService>();
        builder.Services.AddScoped<IExamTypeService, ExamTypeService>();
        builder.Services.AddScoped<IDistrictService, DistrictService>();
        builder.Services.AddScoped<IProvinceService, ProvinceService>();
        builder.Services.AddScoped<ILocalLevelService, LocalLevelService>();
        builder.Services.AddScoped<IEntranceExamApplicationService, EntranceExamApplicationService>();
        builder.Services.AddScoped<IFileUploadHelper, FileUploadHelper>();
        builder.Services.AddScoped<IFacultyResolver, FacultyResolver>();
        builder.Services.AddScoped<ISubjectCatalogService, SubjectCatalogService>();
        builder.Services.AddScoped<ISubjectOfferingService, SubjectOfferingService>();
        builder.Services.AddScoped<ICurriculumVersionService, CurriculumVersionService>();
        builder.Services.AddScoped<IStudentCategoryService, StudentCategoryService>();
        builder.Services.AddScoped<ISemesterService, SemesterService>();
        builder.Services.AddScoped<IBankService, BankService>();
        builder.Services.AddScoped<IPaymentTypeService, PaymentTypeService>();
        builder.Services.AddScoped<IBillTitleService, BillTitleService>();
        builder.Services.AddScoped<IStudentDashboardService, StudentDashboardService>();
        builder.Services.AddScoped<IESewaService, ESewaService>();
        builder.Services.AddHttpClient<IESewaService, ESewaService>();
        builder.Services.AddScoped<IKhaltiService, KhaltiService>();
        builder.Services.AddHttpClient<IKhaltiService, KhaltiService>();
        builder.Services.AddScoped<IStudentAdmissionService, StudentAdmissionService>();
        builder.Services.AddScoped<IPermissionService, PermissionService>();
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        builder.Services.AddScoped<ISemesterEnrollmentService, SemesterEnrollmentService>();
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

        app.UseMiddleware<TenantResolutionMiddleware>();

        app.UseRouting();

        app.UseFacultyResolution();

        app.UseAuthorization();

        app.MapStaticAssets();

        app.MapControllerRoute(
            name: "tenantArea",
            pattern: "tenant/{tenantCode}/{area:exists}/{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.MapControllerRoute(
            name: "tenant",
            pattern: "tenant/{tenantCode}/{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.MapRazorPages()
           .WithStaticAssets();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
            var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
            tenantContext.SetTenant(1, "SEED", TenantType.Central);
            await PermissionSeeder.SeedAllAsync(scope.ServiceProvider);
            await UserSeeder.SeedRolesAsync(scope.ServiceProvider);
            await LocationSeeder.SeedLocationDataAsync(scope.ServiceProvider);
            await WorkflowTestDataSeeder.SeedWorkflowTestDataAsync(scope.ServiceProvider);
        }

        app.Run();
    }
}
