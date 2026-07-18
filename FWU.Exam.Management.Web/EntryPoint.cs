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

public partial class EntryPoint
{
    private static async Task Main(string[] args)
    {
        // Log.Logger = new LoggerConfiguration()
        //     .WriteTo.Console()
        //     .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
        //     .MinimumLevel.Information()
        //     .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        //     .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        //     .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
        //     .Enrich.FromLogContext()
        //     .CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // builder.Host.UseSerilog();

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
                    OnRedirectToLogin = ctx =>
                    {
                        var returnUrl = ctx.HttpContext.Items["OriginalPath"] as string ?? ctx.Request.Path;
                        ctx.Response.Redirect($"/Identity/Account/Login?ReturnUrl={Uri.EscapeDataString(returnUrl!)}");
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = ctx =>
                    {
                        var returnUrl = ctx.HttpContext.Items["OriginalPath"] as string ?? ctx.Request.Path;
                        ctx.Response.Redirect($"/Identity/Account/AccessDenied?ReturnUrl={Uri.EscapeDataString(returnUrl!)}");
                        return Task.CompletedTask;
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
            builder.Services.AddAuthorization();
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            builder.Services.AddScoped<ISemesterEnrollmentService, SemesterEnrollmentService>();
            builder.Services.AddScoped<ISmtpConfigurationService, SmtpConfigurationService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IEmailSender, IdentityEmailSender>();
            builder.Services.AddScoped<ISmsConfigurationService, SmsConfigurationService>();
            builder.Services.AddHttpClient<ISmsService, SmsService>();
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
            builder.Services.AddScoped<IGradeCalculationService, GradeCalculationService>();
            builder.Services.AddScoped<IAuditLogService, AuditLogService>();
            builder.Services.AddScoped<IExamRollNumberService, ExamRollNumberService>();
            builder.Services.AddScoped<IBackupRestoreService, BackupRestoreService>();
            var app = builder.Build();

            EmailTemplateHelper.LogoUrl = builder.Configuration["EmailSettings:LogoUrl"];

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
            }

            app.UseHttpsRedirection();
            app.UseMiddleware<SecurityHeadersMiddleware>();
            app.UseRateLimiter();

            app.UseMiddleware<TenantResolutionMiddleware>();

            app.UseRouting();

            app.UseFacultyResolution();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<UserContextMiddleware>();

            app.UseStaticFiles();
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

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();

            if (!await dbContext.Tenants.AnyAsync())
            {
                dbContext.Tenants.Add(new Tenant
                {
                    Name = "Office of Controller of Examinations",
                    OfficeCode = "OCE",
                    ContactNumber = "01-2345678",
                    Address = "Kathmandu, Nepal",
                    Email = "info@oce.gov.np",
                    TenantType = TenantType.Central,
                    IsActive = true,
                });
                    dbContext.Tenants.Add(new Tenant
                    {
                        Name = "Engineering Office",
                        OfficeCode = "ENG",
                        ContactNumber = "01-2345670",
                        Address = "Mahendranagar,Kanchanpur, Nepal",
                        Email = "eng@fwu.edu.np",
                        TenantType = TenantType.Central,
                        IsActive = true,
                    });
                    await dbContext.SaveChangesAsync();
            }

            var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
            var centralTenant = await dbContext.Tenants.FirstAsync(t => t.TenantType == TenantType.Central);
            tenantContext.SetTenant(centralTenant.Id, centralTenant.OfficeCode, centralTenant.TenantType);

            tenantContext.SetTenant(1, "SEED", TenantType.Central);

            if (app.Environment.IsDevelopment())
            {
                await PermissionSeeder.SeedAllAsync(scope.ServiceProvider);
                await UserSeeder.SeedRolesAsync(scope.ServiceProvider);

                var refreshedTenant = await dbContext.Tenants.FirstAsync(t => t.TenantType == TenantType.Central);
                tenantContext.SetTenant(refreshedTenant.Id, refreshedTenant.OfficeCode, refreshedTenant.TenantType);

                await UserSeeder.SeedRolesAsync(scope.ServiceProvider);
                await PermissionSeeder.SeedAllAsync(scope.ServiceProvider);

                await LocationSeeder.SeedLocationDataAsync(scope.ServiceProvider);

                await ReferenceDataSeeder.SeedTenantsAsync(scope.ServiceProvider);
                await ReferenceDataSeeder.SeedReferenceDataAsync(scope.ServiceProvider);

                await CollegeSeeder.SeedCollegesAsync(scope.ServiceProvider);

                await FacultySeeder.SeedFacultiesAsync(scope.ServiceProvider);

                await AcademicYearSeeder.SeedAcademicYearsAsync(scope.ServiceProvider);

                await ReferenceDataSeeder.SeedAdditionalReferenceDataAsync(scope.ServiceProvider);

                await ProgramSeeder.SeedProgramsAsync(scope.ServiceProvider);

                await CollegeProgramSeeder.SeedCollegeProgramsAsync(scope.ServiceProvider);

                await AcademicStructureSeeder.SeedAcademicStructureAsync(scope.ServiceProvider);
                await NaturalResourceManagementSeeder.SeedNaturalResourceManagementAsync(scope.ServiceProvider);

                await DemoDataSeeder.SeedDemoDataAsync(scope.ServiceProvider);

                await GradingSeeder.SeedGradingDataAsync(scope.ServiceProvider);

                await ReferenceDataSeeder.SeedPaymentTypesAsync(scope.ServiceProvider);
                await ReferenceDataSeeder.SeedESewaConfigurationAsync(scope.ServiceProvider);
                await ReferenceDataSeeder.SeedKhaltiConfigurationAsync(scope.ServiceProvider);
                await ReferenceDataSeeder.SeedConnectIPSConfigurationAsync(scope.ServiceProvider);
                await ReferenceDataSeeder.SeedSmsConfigurationAsync(scope.ServiceProvider);

                await UserSeeder.SeedSuperAdminAsync(scope.ServiceProvider);

                await MarksheetDataSeeder.SeedMarksheetDataAsync(scope.ServiceProvider);
            }
            else
            {
                await PermissionSeeder.SeedAllAsync(scope.ServiceProvider);
                await UserSeeder.SeedRolesAsync(scope.ServiceProvider);
                await GradingSeeder.SeedGradingDataAsync(scope.ServiceProvider);
                await LocationSeeder.SeedLocationDataAsync(scope.ServiceProvider);
                await ProgramSeeder.SeedProgramsAsync(scope.ServiceProvider);
            }
        }

            //             // Log.Information("FWU Examination Management System starting up...");
            app.Run();
        }
        catch (Exception ex)
        {
            //             // Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            // Log.CloseAndFlush();
        }
    }
}
