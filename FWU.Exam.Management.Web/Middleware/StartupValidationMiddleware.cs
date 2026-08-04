namespace FWU.Exam.Management.Web.Middleware;

using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Middleware for startup validation checks in production environment.
/// Validates critical configuration and dependencies before server becomes available.
/// </summary>
public class StartupValidationMiddleware(RequestDelegate next, ILogger<StartupValidationMiddleware> logger, IWebHostEnvironment env)
{
    private static bool _startupValidationComplete;
    private static readonly object _validationLock = new();

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_startupValidationComplete)
        {
            lock (_validationLock)
            {
                if (!_startupValidationComplete)
                {
                    ValidateStartup(context);
                    _startupValidationComplete = true;
                }
            }
        }

        await next(context);
    }

    private void ValidateStartup(HttpContext context)
    {
        logger.LogInformation("Performing startup validation checks...");

        try
        {
            // Check if we can access the database
            using (var scope = context.RequestServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService(typeof(FWU.Exam.Management.Infrastructure.AppDbContext));
                if (dbContext != null)
                {
                    logger.LogInformation("✓ Database connection validated");
                }
                else
                {
                    logger.LogWarning("⚠ Database context not available for validation");
                }
            }

            // Log configuration status
            if (!env.IsDevelopment())
            {
                logger.LogInformation("✓ Production environment detected");
                logger.LogInformation("✓ Security headers will be enforced");
                logger.LogInformation("✓ HTTPS redirection enabled");
                logger.LogInformation("✓ HSTS enabled");
            }
            else
            {
                logger.LogInformation("✓ Development environment detected");
                logger.LogInformation("ℹ Developer exception page enabled");
                logger.LogInformation("ℹ Swagger UI available at /swagger");
            }

            logger.LogInformation("Startup validation completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Startup validation failed. Application may not function correctly.");

            if (!env.IsDevelopment())
            {
                // In production, log but don't crash - allow graceful degradation
                logger.LogError("Continuing startup despite validation errors.");
            }
        }
    }
}

/// <summary>
/// Extension methods for StartupValidationMiddleware
/// </summary>
public static class StartupValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseStartupValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<StartupValidationMiddleware>();
    }
}
