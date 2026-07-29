using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class ResultRecordServiceTests : TestBase
{
    private IUserContext CreateSuperAdminContext()
    {
        var ctx = Substitute.For<IUserContext>();
        ctx.IsSuperAdmin.Returns(true);
        return ctx;
    }

    [Fact]
    public async Task GetResultRecordsAsync_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new ResultRecordService(context, userCtx);

        var year = new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true };
        context.Set<AcademicYear>().Add(year);

        var examType = new ExamType { Name = "Regular", Code = "REG", IsActive = true };
        context.Set<ExamType>().Add(examType);

        var college = new College { Code = "CLG", Name = "Test College", Email = "clg@test.com", PrincipalName = "P", PrincipalContactNumber = "123", IsActive = true, TenantId = TestTenantId };
        context.Set<College>().Add(college);

        var program = new Domain.Entities.Program
        {
            ProgramCode = "BSC", ProgramName = "B.Sc.", ShortName = "BSc", LevelId = levelId, Duration = 4, IsActive = true
        };
        context.Set<Domain.Entities.Program>().Add(program);
        await context.SaveChangesAsync();

        for (int i = 1; i <= 3; i++)
        {
            context.Set<ResultRecord>().Add(new ResultRecord
            {
                TenantId = TestTenantId,
                AcademicYearId = year.Id,
                ProgramsId = program.Id,
                ExamTypeId = examType.Id,
                CollegeId = college.Id,
                Year = "1", Part = "1",
                SymbolNumber = $"SN{i:D4}",
                StudentName = $"Student {i}",
                DateOfBirthBs = "2055-01-01",
                ResultRecordMasterId = 1
            });
        }
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetResultRecordsAsync(1, 2, null, "id", "asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetResultRecordsAsync_WithSearch_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new ResultRecordService(context, userCtx);

        var year = new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true };
        context.Set<AcademicYear>().Add(year);
        var examType = new ExamType { Name = "Regular", Code = "REG", IsActive = true };
        context.Set<ExamType>().Add(examType);
        var college = new College { Code = "CLG", Name = "Test College", Email = "clg@test.com", PrincipalName = "P", PrincipalContactNumber = "123", IsActive = true, TenantId = TestTenantId };
        context.Set<College>().Add(college);
        var program = new Domain.Entities.Program { ProgramCode = "BSC", ProgramName = "B.Sc.", ShortName = "BSc", LevelId = levelId, Duration = 4, IsActive = true };
        context.Set<Domain.Entities.Program>().Add(program);
        await context.SaveChangesAsync();

        context.Set<ResultRecord>().Add(new ResultRecord { TenantId = TestTenantId, AcademicYearId = year.Id, ProgramsId = program.Id, ExamTypeId = examType.Id, CollegeId = college.Id, Year = "1", Part = "1", SymbolNumber = "SN001", StudentName = "Alice", DateOfBirthBs = "2055-01-01", ResultRecordMasterId = 1 });
        context.Set<ResultRecord>().Add(new ResultRecord { TenantId = TestTenantId, AcademicYearId = year.Id, ProgramsId = program.Id, ExamTypeId = examType.Id, CollegeId = college.Id, Year = "1", Part = "1", SymbolNumber = "SN002", StudentName = "Bob", DateOfBirthBs = "2055-01-01", ResultRecordMasterId = 1 });
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetResultRecordsAsync(1, 10, "Alice", "id", "asc");

        totalCount.Should().Be(1);
        items.Should().ContainSingle(i => i.StudentName == "Alice");
    }

    [Fact]
    public async Task GetFilteredItemsAsync_ShouldReturnAll()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new ResultRecordService(context, userCtx);

        var year = new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true };
        context.Set<AcademicYear>().Add(year);
        var examType = new ExamType { Name = "Regular", Code = "REG", IsActive = true };
        context.Set<ExamType>().Add(examType);
        var college = new College { Code = "CLG", Name = "Test College", Email = "clg@test.com", PrincipalName = "P", PrincipalContactNumber = "123", IsActive = true, TenantId = TestTenantId };
        context.Set<College>().Add(college);
        var program = new Domain.Entities.Program { ProgramCode = "BSC", ProgramName = "B.Sc.", ShortName = "BSc", LevelId = levelId, Duration = 4, IsActive = true };
        context.Set<Domain.Entities.Program>().Add(program);
        await context.SaveChangesAsync();

        context.Set<ResultRecord>().Add(new ResultRecord { TenantId = TestTenantId, AcademicYearId = year.Id, ProgramsId = program.Id, ExamTypeId = examType.Id, CollegeId = college.Id, Year = "1", Part = "1", SymbolNumber = "SN001", StudentName = "Alice", DateOfBirthBs = "2055-01-01", ResultRecordMasterId = 1 });
        await context.SaveChangesAsync();

        var items = await service.GetFilteredItemsAsync(null);

        items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetResultRecordByIdAsync_ShouldReturnCorrectRecord()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new ResultRecordService(context, userCtx);

        var year = new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true };
        context.Set<AcademicYear>().Add(year);
        var examType = new ExamType { Name = "Regular", Code = "REG", IsActive = true };
        context.Set<ExamType>().Add(examType);
        var college = new College { Code = "CLG", Name = "Test College", Email = "clg@test.com", PrincipalName = "P", PrincipalContactNumber = "123", IsActive = true, TenantId = TestTenantId };
        context.Set<College>().Add(college);
        var program = new Domain.Entities.Program { ProgramCode = "BSC", ProgramName = "B.Sc.", ShortName = "BSc", LevelId = levelId, Duration = 4, IsActive = true };
        context.Set<Domain.Entities.Program>().Add(program);
        await context.SaveChangesAsync();

        var record = new ResultRecord { TenantId = TestTenantId, AcademicYearId = year.Id, ProgramsId = program.Id, ExamTypeId = examType.Id, CollegeId = college.Id, Year = "1", Part = "1", SymbolNumber = "SN001", StudentName = "Alice", DateOfBirthBs = "2055-01-01", ResultRecordMasterId = 1 };
        context.Set<ResultRecord>().Add(record);
        await context.SaveChangesAsync();

        var result = await service.GetResultRecordByIdAsync(record.Id);

        result.Should().NotBeNull();
        result!.SymbolNumber.Should().Be("SN001");
        result.StudentName.Should().Be("Alice");
    }
}
