using FluentAssertions;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Web.Helpers;
using Microsoft.AspNetCore.Http;

namespace FWU.Exam.Management.Web.Tests.Helpers;

public class FacultyContextHelperTests
{
    [Fact]
    public void GetCurrentFaculty_WhenItemExists_ReturnsFaculty()
    {
        var faculty = new CurrentFaculty { Id = 1, Name = "Science", OfficeCode = "SC" };
        var ctx = new DefaultHttpContext();
        ctx.Items["CurrentFaculty"] = faculty;

        var result = ctx.GetCurrentFaculty();

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Science");
    }

    [Fact]
    public void GetCurrentFaculty_WhenItemIsWrongType_ReturnsNull()
    {
        var ctx = new DefaultHttpContext();
        ctx.Items["CurrentFaculty"] = "not a faculty";

        var result = ctx.GetCurrentFaculty();

        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentFaculty_WhenItemIsNull_ReturnsNull()
    {
        var ctx = new DefaultHttpContext();
        var result = ctx.GetCurrentFaculty();
        result.Should().BeNull();
    }

    [Fact]
    public void HasFacultyContext_WhenFacultyExists_ReturnsTrue()
    {
        var ctx = new DefaultHttpContext();
        ctx.Items["CurrentFaculty"] = new CurrentFaculty { Id = 2 };

        var result = ctx.HasFacultyContext();

        result.Should().BeTrue();
    }

    [Fact]
    public void HasFacultyContext_WhenNoFaculty_ReturnsFalse()
    {
        var ctx = new DefaultHttpContext();
        var result = ctx.HasFacultyContext();
        result.Should().BeFalse();
    }

    [Fact]
    public void HasFacultyContext_WhenItemIsWrongType_ReturnsFalse()
    {
        var ctx = new DefaultHttpContext();
        ctx.Items["CurrentFaculty"] = "some string";
        var result = ctx.HasFacultyContext();
        result.Should().BeFalse();
    }
}
