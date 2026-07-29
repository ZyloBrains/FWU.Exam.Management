using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;


public class ExamRollNumberServiceTests : TestBase
{
    private async Task<int> SeedDataAsync(AppDbContext context)
    {
        context.Set<Level>().Add(new Level { LevelName = "Bachelor", IsActive = true });
        await context.SaveChangesAsync();

        context.Set<Program>().Add(new Program { LevelId = 1, ProgramCode = "BCA", ProgramName = "BCA", ShortName = "BCA", Duration = 4, IsActive = true });
        await context.SaveChangesAsync();

        context.Set<AcademicYear>().Add(new AcademicYear
        {
            AcademicYearCode = "2081/082", AcademicYearName = "2081/082",
            AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२",
            IsRunning = true, IsActive = true
        });
        await context.SaveChangesAsync();

        context.Set<ExamType>().Add(new ExamType { Name = "Regular", Code = "REG", IsActive = true });
        await context.SaveChangesAsync();

        context.Set<Semester>().Add(new Semester { Number = 1, Year = 1, Name = "First Semester", Code = "SEM1", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6), AcademicYearId = 1 });
        await context.SaveChangesAsync();

        context.Set<ExamSchedule>().Add(new ExamSchedule
        {
            TenantId = TestTenantId,
            ExamScheduleName = "Final 2081",
            AcademicYearId = 1, ProgramId = 1, SemesterId = 1, ExamTypeId = 1,
            IsActive = true, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0)
        });
        await context.SaveChangesAsync();

        context.Set<College>().Add(new College { TenantId = TestTenantId, Code = "COL01", Name = "Test College", Email = "c@test.com", PrincipalName = "P", PrincipalContactNumber = "9800000000", IsActive = true });
        await context.SaveChangesAsync();

        return 1;
    }

    [Fact]
    public async Task GenerateRollNumbers_ShouldAssignRollNumbers()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId, AcademicYearId = 1, CollegeId = 1, ExamScheduleId = 1,
            Status = RegistrationStatus.Registered, IsActive = true
        });
        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId, AcademicYearId = 1, CollegeId = 1, ExamScheduleId = 1,
            Status = RegistrationStatus.Registered, IsActive = true
        });
        await context.SaveChangesAsync();

        var service = new ExamRollNumberService(context);
        var count = await service.GenerateRollNumbersAsync(1);

        count.Should().Be(2);

        var registrations = context.Set<ExamRegistration>().Where(r => r.ExamScheduleId == 1).ToList();
        registrations.Should().OnlyContain(r => !string.IsNullOrEmpty(r.ExamRollNumber));
        registrations[0].ExamRollNumber.Should().NotBe(registrations[1].ExamRollNumber);
        registrations[0].RollNumberIndex.Should().Be(0);
        registrations[1].RollNumberIndex.Should().Be(1);
    }

    [Fact]
    public async Task GenerateRollNumbers_ShouldUseRollNumberSetup()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId, AcademicYearId = 1, CollegeId = 1, ExamScheduleId = 1,
            Status = RegistrationStatus.Registered, IsActive = true
        });
        await context.SaveChangesAsync();

        context.Set<ExamRollNumberSetup>().Add(new ExamRollNumberSetup
        {
            TenantId = TestTenantId, ExamScheduleId = 1, FirstExamRollNumber = 100,
            MinimumRollNumberLength = 5, Prefix = "FWU", Suffix = "R",
            IsActive = true, Round = 1, MinimumGap = 0
        });
        await context.SaveChangesAsync();

        var service = new ExamRollNumberService(context);
        await service.GenerateRollNumbersAsync(1);

        var reg = await context.Set<ExamRegistration>().FirstAsync();
        reg.ExamRollNumber!.Should().StartWith("FWU");
        reg.ExamRollNumber.Should().EndWith("R");
    }

    [Fact]
    public async Task GenerateRollNumbers_ShouldThrow_WhenScheduleNotFound()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new ExamRollNumberService(context);

        var act = () => service.GenerateRollNumbersAsync(999);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Exam schedule*not found*");
    }

    [Fact]
    public async Task HasRollNumbers_ShouldReturnTrue_WhenExist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId, AcademicYearId = 1, CollegeId = 1, ExamScheduleId = 1,
            ExamRollNumber = "R001", Status = RegistrationStatus.Registered, IsActive = true
        });
        await context.SaveChangesAsync();

        var service = new ExamRollNumberService(context);
        var has = await service.HasRollNumbersAsync(1);

        has.Should().BeTrue();
    }

    [Fact]
    public async Task ClearRollNumbers_ShouldRemoveRollNumbers()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId, AcademicYearId = 1, CollegeId = 1, ExamScheduleId = 1,
            ExamRollNumber = "R001", ExamRollNumberCoding = 1, RollNumberIndex = 0,
            Status = RegistrationStatus.Registered, IsActive = true
        });
        await context.SaveChangesAsync();

        var service = new ExamRollNumberService(context);
        var count = await service.ClearRollNumbersAsync(1);

        count.Should().Be(1);

        var reg = await context.Set<ExamRegistration>().FirstAsync();
        reg.ExamRollNumber.Should().BeNull();
        reg.ExamRollNumberCoding.Should().BeNull();
        reg.RollNumberIndex.Should().BeNull();
    }
}
