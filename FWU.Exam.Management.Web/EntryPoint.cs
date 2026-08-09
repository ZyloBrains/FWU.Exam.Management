using FWU.Exam.Management.Domain.Entities;
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
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure.Interceptor;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Middleware;

using Serilog;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;

public partial class EntryPoint
{
    private static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog();

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        // Add services to the container.
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IAuditUserProvider, HttpContextAuditUserProvider>();
        builder.Services.AddScoped<ITenantContext, TenantContext>();
        builder.Services.AddScoped<IUserContext, UserContext>();
        builder.Services.AddScoped<AuditableSaveChangesInterceptor>();
        builder.Services.AddScoped<TenantSaveChangesInterceptor>();
        builder.Services.AddScoped<AuditLogInterceptor>();

        builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            options.UseSqlServer(connectionString);
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditableSaveChangesInterceptor>());
            options.AddInterceptors(serviceProvider.GetRequiredService<TenantSaveChangesInterceptor>());
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditLogInterceptor>());
        });

        builder.Services.AddDefaultIdentity<AppUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        var isDevelopment = builder.Environment.IsDevelopment();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;

            options.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
            {
                OnRedirectToLogin = async ctx =>
                {
                    if (IsAjaxRequest(ctx.HttpContext.Request))
                    {
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.WriteAsJsonAsync(new { error = "Authentication required." });
                        return;
                    }

                    var currentPath = ctx.Request.Path.Value ?? "";
                    if (currentPath.StartsWith("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    var returnUrl = ctx.HttpContext.Items["OriginalPath"] as string ?? ctx.Request.Path;
                    ctx.Response.Redirect($"/Identity/Account/Login?ReturnUrl={Uri.EscapeDataString(returnUrl!)}");
                },
                OnRedirectToAccessDenied = async ctx =>
                {
                    if (IsAjaxRequest(ctx.HttpContext.Request))
                    {
                        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.WriteAsJsonAsync(new { error = "Access denied. You do not have permission to perform this action." });
                        return;
                    }

                    var currentPath = ctx.Request.Path.Value ?? "";
                    if (currentPath.StartsWith("/Identity/Account/AccessDenied", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    var returnUrl = ctx.HttpContext.Items["OriginalPath"] as string ?? ctx.Request.Path;
                    ctx.Response.Redirect($"/Identity/Account/AccessDenied?ReturnUrl={Uri.EscapeDataString(returnUrl!)}");
                }
            };
        });

        builder.Services.AddControllersWithViews();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
            {
                Title = "FWU Examination Management API",
                Version = "v1",
                Description = "API for FWU Examination Management System"
            });
        });

        builder.Services.AddAntiforgery(options =>
        {
            options.Cookie.Path = "/";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("fixed", limiterOptions =>
            {
                limiterOptions.PermitLimit = 100;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("login", limiterOptions =>
            {
                limiterOptions.PermitLimit = 10;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueLimit = 0;
            });
        });

        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        builder.Services.AddScoped<ISemesterEnrollmentService, SemesterEnrollmentService>();
        builder.Services.AddHostedService<SemesterPromotionBackgroundService>();
        builder.Services.AddScoped<ISmtpConfigurationService, SmtpConfigurationService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IEmailSender, IdentityEmailSender>();
        builder.Services.AddScoped<ISmsConfigurationService, SmsConfigurationService>();
        builder.Services.AddHttpClient<ISmsService, SmsService>();
        builder.Services.AddScoped<IGumpNowEmailConfigurationService, GumpNowEmailConfigurationService>();
        builder.Services.AddHttpClient<IGumpNowEmailService, GumpNowEmailService>();

        builder.Services.AddScoped<IBoardService, BoardService>();
        builder.Services.AddScoped<ICollegeProgramService, CollegeProgramService>();
        builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
        builder.Services.AddScoped<ICollegeService, CollegeService>();
        builder.Services.AddScoped<IFacultyService, FacultyService>();
        builder.Services.AddScoped<IDashboardService, DashboardService>();
        builder.Services.AddScoped<IStudentRegistrationService, StudentRegistrationService>();
        builder.Services.AddScoped<IExamScheduleService, ExamScheduleService>();
        builder.Services.AddScoped<IProgramService, ProgramService>();
        builder.Services.AddScoped<ILevelService, LevelService>();
        builder.Services.AddScoped<IGenderService, GenderService>();
        builder.Services.AddScoped<IEthnicityService, EthnicityService>();
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
        builder.Services.AddHttpClient<IKhaltiService, KhaltiService>();
        builder.Services.AddScoped<IStudentAdmissionService, StudentAdmissionService>();
        builder.Services.AddScoped<ICountryService, CountryService>();
        builder.Services.AddScoped<IPermissionService, PermissionService>();
        builder.Services.AddMemoryCache();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });
        builder.Services.AddScoped<IGradingSchemeService, GradingSchemeService>();
        builder.Services.AddScoped<IExamRegistrationService, ExamRegistrationService>();
        builder.Services.AddScoped<IExamSubjectResultService, ExamSubjectResultService>();
        builder.Services.AddScoped<IResultRecordService, ResultRecordService>();
        builder.Services.AddScoped<IExamCenterService, ExamCenterService>();
        builder.Services.AddScoped<IAdmitCardService, AdmitCardService>();
        builder.Services.AddScoped<IExamCenterDistributionService, ExamCenterDistributionService>();
        builder.Services.AddScoped<IRetotalRequestService, RetotalRequestService>();
        builder.Services.AddScoped<ICollegeAdminMarksService, CollegeAdminMarksService>();
        builder.Services.AddScoped<ICollegeAdminSubjectAssignmentService, CollegeAdminSubjectAssignmentService>();
        builder.Services.AddScoped<IExamScheduleApprovalService, ExamScheduleApprovalService>();
        builder.Services.AddScoped<IGradeCalculationService, GradeCalculationService>();
        builder.Services.AddScoped<IAuditLogService, AuditLogService>();
        builder.Services.AddScoped<IExamRollNumberService, ExamRollNumberService>();
        builder.Services.AddScoped<IBackupRestoreService, BackupRestoreService>();
        builder.Services.AddScoped<IBulkUserCreationService, BulkUserCreationService>();
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            var services = app.Services;

            await RunSeederAsync(services, ReferenceDataSeeder.SeedTenantsAsync);
            await RunSeederAsync(services, UserSeeder.SeedRolesAsync);
            await RunSeederAsync(services, PermissionSeeder.SeedAllAsync);
            await RunSeederAsync(services, LocationSeeder.SeedLocationDataAsync);
            await RunSeederAsync(services, ReferenceDataSeeder.SeedReferenceDataAsync);
            await RunSeederAsync(services, ReferenceDataSeeder.SeedCollegeTypesAsync);
            await RunSeederAsync(services, AcademicYearSeeder.SeedAcademicYearsAsync);
            await RunSeederAsync(services, FacultySeeder.SeedFacultiesAsync);
            await RunSeederAsync(services, ProgramSeeder.SeedProgramsAsync);
            await RunSeederAsync(services, CollegeSeeder.SeedCollegesAsync);
            await RunSeederAsync(services, CollegeProgramSeeder.SeedCollegeProgramsAsync);
            await RunSeederAsync(services, GradingSeeder.SeedGradingDataAsync);
            await RunSeederAsync(services, ReferenceDataSeeder.SeedPaymentTypesAsync);
            await RunSeederAsync(services, ReferenceDataSeeder.SeedESewaConfigurationAsync);
            await RunSeederAsync(services, ReferenceDataSeeder.SeedKhaltiConfigurationAsync);
            await RunSeederAsync(services, ReferenceDataSeeder.SeedConnectIPSConfigurationAsync);
            await RunSeederAsync(services, ReferenceDataSeeder.SeedSmsConfigurationAsync);
            await RunSeederAsync(services, UserSeeder.SeedUsersAsync);
        }

        EmailTemplateHelper.LogoUrl = builder.Configuration["EmailSettings:LogoUrl"];
        EmailTemplateHelper.SiteUrl = builder.Configuration["EmailSettings:SiteUrl"];

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
            app.UseForwardedHeaders();
        }

        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseRateLimiter();

        app.UseMiddleware<TenantResolutionMiddleware>();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();

        app.UseFacultyResolution();

        app.UseAuthorization();
        app.UseSession();

        app.UseMiddleware<UserContextMiddleware>();

        app.MapStaticAssets();

        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.MapRazorPages();

        app.Run();
    }

    private static async Task RunSeederAsync(IServiceProvider appServices, Func<IServiceProvider, Task> seeder)
    {
        using var scope = appServices.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var tenantContext = serviceProvider.GetRequiredService<ITenantContext>();
        tenantContext.SetTenant(1, "OCE", TenantType.Central);

        await seeder(serviceProvider);
    }

    private static bool IsAjaxRequest(HttpRequest request)
    {
        return string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest",
            StringComparison.OrdinalIgnoreCase)
            || string.Equals(request.ContentType, "application/json", StringComparison.OrdinalIgnoreCase);
    }
}
