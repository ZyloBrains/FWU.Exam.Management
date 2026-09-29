using FWU.Exam.Management.Domain.Constants;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class TenantDefaultsTests
{
    [Fact]
    public void Resolve_UsesFacultyTenant_WhenFacultyHasOne()
    {
        Assert.Equal(5, TenantDefaults.Resolve(5));
    }

    [Fact]
    public void Resolve_FallsBackToCentralTenant_WhenFacultyHasNoTenant()
    {
        // Faculty.TenantId is nullable and TenantSaveChangesInterceptor deliberately skips
        // Faculty, so a faculty created without an explicit tenant keeps a null one.
        Assert.Equal(TenantDefaults.CentralTenantId, TenantDefaults.Resolve(null));
    }

    [Fact]
    public void CentralTenantId_MatchesTheSeedTenant()
    {
        // Tenant 1 is the Office of Controller of Examinations row that EntryPoint and the
        // background services hard-code as the central context.
        Assert.Equal(1, TenantDefaults.CentralTenantId);
    }
}
