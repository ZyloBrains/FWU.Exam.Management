using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Interceptor;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace FWU.Exam.Management.Infrastructure.Tests;

public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContext Context { get; }
    public TestTenantContext Tenant { get; }

    public TestDb(TestTenantContext tenant, Action<AppDbContext>? seed = null)
    {
        Tenant = tenant;
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(new TenantSaveChangesInterceptor(tenant, NullLogger<TenantSaveChangesInterceptor>.Instance))
            .Options;

        Context = new AppDbContext(options, NullLogger<AppDbContext>.Instance, tenant);
        Context.Database.EnsureCreated();

        seed?.Invoke(Context);
        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
