using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class ExamCenterDistributionServiceTests : TestBase
{
    private async Task<int> SeedFullDataAsync(AppDbContext context)
    {
        context.Set<Level>().Add(new Level { LevelName = "Bachelor", IsActive = true });
        await context.SaveChangesAsync();

        context.Set<Program>().Add(new Program { LevelId = 1, ProgramCode = "BCA", ProgramName = "BCA", ShortName = "BCA", Duration = 4, IsActive = true });
        await context.SaveChangesAsync();

        context.Set<AcademicYear>().Add(new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true });
        await context.SaveChangesAsync();

        context.Set<ExamType>().Add(new ExamType { Name = "Regular", Code = "REG", IsActive = true });
        await context.SaveChangesAsync();

        context.Set<Semester>().Add(new Semester { Number = 1, Year = 1, Name = "First Semester", Code = "SEM1", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6), AcademicYearId = 1 });
        await context.SaveChangesAsync();

        context.Set<ExamSchedule>().Add(new ExamSchedule
        {
            TenantId = TestTenantId,
            ExamScheduleName = "Final 2081",
            AcademicYearId = 1,
            ProgramId = 1,
            SemesterId = 1,
            ExamTypeId = 1,
            IsActive = true,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(12, 0)
        });
        await context.SaveChangesAsync();

        context.Set<College>().Add(new College { TenantId = TestTenantId, Code = "COL01", Name = "College A", Email = "a@test.com", PrincipalName = "P1", PrincipalContactNumber = "9800000000", IsActive = true });
        await context.SaveChangesAsync();

        context.Set<ExamCenter>().Add(new ExamCenter { TenantId = TestTenantId, ExamScheduleId = 1, Code = "EC001", IsActive = true });
        context.Set<ExamCenter>().Add(new ExamCenter { TenantId = TestTenantId, ExamScheduleId = 1, Code = "EC002", IsActive = true });
        await context.SaveChangesAsync();

        return 1;
    }

    [Fact]
    public async Task AssignSymbolNumbers_ShouldGenerateSymbolNumbers()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedFullDataAsync(context);

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

        var service = new ExamCenterDistributionService(context);
        await service.AssignSymbolNumbersAsync(1);

        var registrations = context.Set<ExamRegistration>().Where(er => er.ExamScheduleId == 1).ToList();
        registrations.Should().OnlyContain(er => !string.IsNullOrEmpty(er.SymbolNumber));
        registrations[0].SymbolNumber.Should().NotBe(registrations[1].SymbolNumber);
    }

    [Fact]
    public async Task DistributeStudents_ShouldRoundRobinAssign()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedFullDataAsync(context);

        for (int i = 1; i <= 5; i++)
        {
            var seq = i.ToString("D3");
            context.Set<ExamRegistration>().Add(new ExamRegistration
            {
                TenantId = TestTenantId, AcademicYearId = 1, CollegeId = 1, ExamScheduleId = 1,
                Status = RegistrationStatus.Registered, IsActive = true, SymbolNumber = $"SYM{seq}"
            });
        }
        await context.SaveChangesAsync();

        var service = new ExamCenterDistributionService(context);
        var count = await service.DistributeStudentsAsync(1);

        count.Should().Be(5);

        var registrations = context.Set<ExamRegistration>().Where(er => er.ExamScheduleId == 1).ToList();
        registrations.Should().OnlyContain(er => er.ExamCenterId != null);
        registrations.Select(er => er.ExamCenterId).Distinct().Should().HaveCount(2);
    }

    [Fact]
    public async Task ResetDistribution_ShouldClearExamCenterIds()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedFullDataAsync(context);

        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId, AcademicYearId = 1, CollegeId = 1, ExamScheduleId = 1,
            ExamCenterId = 1, Status = RegistrationStatus.Registered, IsActive = true
        });
        await context.SaveChangesAsync();

        var service = new ExamCenterDistributionService(context);
        await service.ResetDistributionAsync(1);

        var reg = await context.Set<ExamRegistration>().FirstAsync();
        reg.ExamCenterId.Should().BeNull();
    }

    [Fact]
    public async Task GetCounts_ShouldReturnCorrectValues()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedFullDataAsync(context);

        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId, AcademicYearId = 1, CollegeId = 1, ExamScheduleId = 1, ExamCenterId = 1,
            Status = RegistrationStatus.Registered, IsActive = true
        });
        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId, AcademicYearId = 1, CollegeId = 1, ExamScheduleId = 1, ExamCenterId = 2,
            Status = RegistrationStatus.Registered, IsActive = true
        });
        context.Set<ExamRegistration>().Add(new ExamRegistration
        {
            TenantId = TestTenantId, AcademicYearId = 1, CollegeId = 1, ExamScheduleId = 1,
            Status = RegistrationStatus.Registered, IsActive = true
        });
        await context.SaveChangesAsync();

        var service = new ExamCenterDistributionService(context);

        var registered = await service.GetRegisteredCountAsync(1);
        var assigned = await service.GetAssignedCountAsync(1);
        var unassigned = await service.GetUnassignedCountAsync(1);
        var dist = await service.GetCenterDistributionCountsAsync(1);

        registered.Should().Be(3);
        assigned.Should().Be(2);
        unassigned.Should().Be(1);
        dist.Should().HaveCount(2);
    }
}
