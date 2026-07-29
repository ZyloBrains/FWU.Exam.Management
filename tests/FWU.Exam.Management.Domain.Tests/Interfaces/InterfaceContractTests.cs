using FWU.Exam.Management.Domain.Interfaces;
using FluentAssertions;

namespace FWU.Exam.Management.Domain.Tests.Interfaces;

public class InterfaceContractTests
{
    [Fact]
    public void IAuditable_ShouldBeInterface()
    {
        typeof(IAuditable).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void ITenantScoped_ShouldBeInterface()
    {
        typeof(ITenantScoped).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void ITenantContext_ShouldHaveRequiredMembers()
    {
        var members = typeof(ITenantContext).GetMethods()
            .Select(m => m.Name)
            .Concat(typeof(ITenantContext).GetProperties().Select(p => p.Name))
            .ToArray();

        members.Should().Contain("TenantId");
        members.Should().Contain("TenantCode");
        members.Should().Contain("Type");
        members.Should().Contain("IsCentralTenant");
        members.Should().Contain("IsFilterIgnored");
        members.Should().Contain("IgnoreFilter");
        members.Should().Contain("SetTenant");
    }

    [Fact]
    public void IUserContext_ShouldHaveRequiredMembers()
    {
        var members = typeof(IUserContext).GetProperties().Select(p => p.Name).ToArray();

        members.Should().Contain("UserId");
        members.Should().Contain("FacultyId");
        members.Should().Contain("CollegeId");
        members.Should().Contain("FacultyCollegeIds");
        members.Should().Contain("Roles");
        members.Should().Contain("IsSuperAdmin");
        members.Should().Contain("IsFacultyAdmin");
        members.Should().Contain("IsCollegeAdmin");
        members.Should().Contain("IsAuthenticated");
    }
}
