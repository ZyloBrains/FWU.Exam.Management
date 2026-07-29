using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Enums;
using FluentAssertions;

namespace FWU.Exam.Management.Domain.Tests.Entities;

public class TenantTests
{
    [Fact]
    public void CreateTenant_ShouldSetProperties()
    {
        var tenant = new Tenant
        {
            Id = 1,
            Name = "Far Western University",
            OfficeCode = "FWU001",
            ContactNumber = "01-5551234",
            Address = "Mahendranagar, Kanchanpur",
            Email = "info@fwu.edu.np",
            TenantType = TenantType.Central,
            IsActive = true
        };

        tenant.Id.Should().Be(1);
        tenant.Name.Should().Be("Far Western University");
        tenant.OfficeCode.Should().Be("FWU001");
        tenant.ContactNumber.Should().Be("01-5551234");
        tenant.Address.Should().Be("Mahendranagar, Kanchanpur");
        tenant.Email.Should().Be("info@fwu.edu.np");
        tenant.TenantType.Should().Be(TenantType.Central);
        tenant.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Tenant_DefaultTenantType_ShouldBeStandard()
    {
        var tenant = new Tenant();
        tenant.TenantType.Should().Be(TenantType.Standard);
    }

    [Fact]
    public void Tenant_DefaultIsActive_ShouldBeTrue()
    {
        var tenant = new Tenant();
        tenant.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Tenant_LogoPath_ShouldBeNullable()
    {
        var tenant = new Tenant();
        tenant.LogoPath.Should().BeNull();
    }

    [Fact]
    public void Tenant_ControllerSignaturePath_ShouldBeNullable()
    {
        var tenant = new Tenant();
        tenant.ControllerSignaturePath.Should().BeNull();
    }
}
