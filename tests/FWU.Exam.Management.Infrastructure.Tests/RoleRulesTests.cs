using FWU.Exam.Management.Domain.Constants;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class RoleRulesTests
{
    [Fact]
    public void FromRoles_ClassifiesHighestBuiltInTier()
    {
        Assert.Equal(Role.SuperAdmin, RoleRules.FromRoles([Role.SuperAdmin]));
        Assert.Equal(Role.SuperAdmin, RoleRules.FromRoles([Role.FacultyAdmin, Role.SuperAdmin]));
        Assert.Equal(Role.SuperAdmin, RoleRules.FromRoles([Role.Student, Role.SuperAdmin, Role.CollegeAdmin]));
        Assert.Equal(Role.FacultyAdmin, RoleRules.FromRoles([Role.FacultyAdmin]));
        Assert.Equal(Role.FacultyAdmin, RoleRules.FromRoles([Role.FacultyAdmin, Role.CollegeAdmin]));
        Assert.Equal(Role.CollegeAdmin, RoleRules.FromRoles([Role.CollegeAdmin]));
        Assert.Equal(Role.CollegeAdmin, RoleRules.FromRoles([Role.CollegeAdmin, Role.Student]));
    }

    [Fact]
    public void FromRoles_NoBuiltInAdminRole_ReturnsNull()
    {
        Assert.Null(RoleRules.FromRoles([Role.Student]));
        Assert.Null(RoleRules.FromRoles(["CustomRole"]));
        Assert.Null(RoleRules.FromRoles([]));
    }

    [Fact]
    public void AssignableRoles_SuperAdmin_GetsAllRoles()
    {
        var roles = RoleRules.AssignableRoles(Role.SuperAdmin);

        Assert.Equal(Role.AllRoles.OrderBy(r => r), roles.OrderBy(r => r));
    }

    [Fact]
    public void AssignableRoles_FacultyAdmin_GetsCollegeAdminAndStudent()
    {
        var roles = RoleRules.AssignableRoles(Role.FacultyAdmin);

        Assert.Equal(new[] { Role.CollegeAdmin, Role.Student }, roles.OrderBy(r => r));
        Assert.DoesNotContain(Role.SuperAdmin, roles);
        Assert.DoesNotContain(Role.FacultyAdmin, roles);
    }

    [Fact]
    public void AssignableRoles_CollegeAdmin_GetsStudentOnly()
    {
        var roles = RoleRules.AssignableRoles(Role.CollegeAdmin);

        Assert.Equal(new[] { Role.Student }, roles);
    }

    [Theory]
    [InlineData(Role.FacultyAdmin, Role.SuperAdmin, false)]
    [InlineData(Role.FacultyAdmin, Role.FacultyAdmin, false)]
    [InlineData(Role.FacultyAdmin, Role.CollegeAdmin, true)]
    [InlineData(Role.FacultyAdmin, Role.Student, true)]
    [InlineData(Role.CollegeAdmin, Role.SuperAdmin, false)]
    [InlineData(Role.CollegeAdmin, Role.FacultyAdmin, false)]
    [InlineData(Role.CollegeAdmin, Role.CollegeAdmin, false)]
    [InlineData(Role.CollegeAdmin, Role.Student, true)]
    [InlineData(Role.SuperAdmin, Role.SuperAdmin, true)]
    [InlineData(Role.SuperAdmin, Role.Student, true)]
    public void IsAssignableRole_MatchesHierarchy(string callerRole, string role, bool expected)
    {
        Assert.Equal(expected, RoleRules.IsAssignableRole(callerRole, role));
    }

    [Fact]
    public void CanManageTarget_SuperAdmin_CanManageAnyone()
    {
        Assert.True(RoleRules.CanManageTarget(Role.SuperAdmin, [Role.SuperAdmin]));
        Assert.True(RoleRules.CanManageTarget(Role.SuperAdmin, [Role.FacultyAdmin]));
        Assert.True(RoleRules.CanManageTarget(Role.SuperAdmin, [Role.Student]));
        Assert.True(RoleRules.CanManageTarget(Role.SuperAdmin, []));
    }

    [Fact]
    public void CanManageTarget_FacultyAdmin_CannotManageSuperAdminOrFacultyAdmin()
    {
        Assert.False(RoleRules.CanManageTarget(Role.FacultyAdmin, [Role.SuperAdmin]));
        Assert.False(RoleRules.CanManageTarget(Role.FacultyAdmin, [Role.FacultyAdmin]));
        Assert.True(RoleRules.CanManageTarget(Role.FacultyAdmin, [Role.CollegeAdmin]));
        Assert.True(RoleRules.CanManageTarget(Role.FacultyAdmin, [Role.Student]));
    }

    [Fact]
    public void CanManageTarget_CollegeAdmin_CanManageStudentsOnly()
    {
        Assert.False(RoleRules.CanManageTarget(Role.CollegeAdmin, [Role.CollegeAdmin]));
        Assert.False(RoleRules.CanManageTarget(Role.CollegeAdmin, [Role.FacultyAdmin]));
        Assert.False(RoleRules.CanManageTarget(Role.CollegeAdmin, [Role.SuperAdmin]));
        Assert.True(RoleRules.CanManageTarget(Role.CollegeAdmin, [Role.Student]));
        Assert.False(RoleRules.CanManageTarget(Role.CollegeAdmin, [Role.Student, Role.CollegeAdmin]));
    }
}
