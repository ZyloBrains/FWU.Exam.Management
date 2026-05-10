using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=FUExamsDb;Trusted_Connection=True;TrustServerCertificate=True");
        return new AppDbContext(optionsBuilder.Options, new LoggerFactory().CreateLogger<AppDbContext>());
    }
}
