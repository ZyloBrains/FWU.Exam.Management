using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Data;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class SymbolNumberServiceTests
{
    private static void SeedWithSymbols(AppDbContext ctx)
    {
        TestData.SeedBase(ctx);
        ctx.Faculties.Add(new Faculty { Id = 1, Name = "Management", OfficeCode = "L001", ShortName = "MG", TenantId = TestData.TenantId });
        ctx.CollegeFaculties.Add(new CollegeFaculty { TenantId = TestData.TenantId, CollegeId = TestData.CollegeId, FacultyId = 1 });

        ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, "stu1@test.com"));
        ctx.StudentRegistrations.Add(TestData.StudentRegistration(2, "stu2@test.com"));
        ctx.StudentRegistrations.Add(TestData.StudentRegistration(3, "stu3@test.com"));

        ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), DateTime.UtcNow.AddMonths(2)));
        ctx.ExamSchedules.Add(TestData.Schedule(22, 1, TestData.Regular,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), DateTime.UtcNow.AddMonths(2)));

        ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 21));
        ctx.ApplicationVouchers.Add(TestData.Voucher(2, 2, 22));
        ctx.ApplicationVouchers.Add(TestData.Voucher(3, 3, 22));

        var prefix = SymbolNumberDefaults.BuildPrefix(TestData.Regular);

        var alreadyAssigned = TestData.ExamRegistration(1, 21, 1);
        alreadyAssigned.Status = RegistrationStatus.CollegeVerified;
        alreadyAssigned.SymbolNumber = prefix + "0001";

        var unassignedA = TestData.ExamRegistration(2, 22, 2);
        unassignedA.Status = RegistrationStatus.CollegeVerified;

        var unassignedB = TestData.ExamRegistration(3, 22, 3);
        unassignedB.Status = RegistrationStatus.CollegeVerified;

        ctx.ExamRegistrations.AddRange(alreadyAssigned, unassignedA, unassignedB);
    }

    [Fact]
    public async Task GenerateAsync_Throws_WhenAssignedSymbolCollidesWithExistingRegistration()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedWithSymbols);
        var svc = new SymbolNumberService(db.Context);

        var prefix = SymbolNumberDefaults.BuildPrefix(TestData.Regular);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.GenerateAsync(22, startSequence: 1));

        Assert.Contains("collision", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(prefix + "0001", ex.Message);
    }

    [Fact]
    public async Task GenerateAsync_SkipsExistingSymbols_AndAssignsFreshSequences()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedWithSymbols);
        var svc = new SymbolNumberService(db.Context);

        var prefix = SymbolNumberDefaults.BuildPrefix(TestData.Regular);

        var result = await svc.GenerateAsync(22);

        Assert.Equal(2, result.Assigned);
        Assert.Equal(0, result.Skipped);

        var symbols = await db.Context.ExamRegistrations
            .Where(er => er.ExamScheduleId == 22)
            .Select(er => er.SymbolNumber!)
            .ToListAsync();

        Assert.Equal(2, symbols.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Contains(prefix + "0002", symbols);
        Assert.Contains(prefix + "0003", symbols);
    }

    [Fact]
    public async Task UpdateSymbolNumberAsync_Throws_WhenSymbolBelongsToAnotherRegistration()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedWithSymbols);
        var svc = new SymbolNumberService(db.Context);

        var prefix = SymbolNumberDefaults.BuildPrefix(TestData.Regular);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.UpdateSymbolNumberAsync(2, prefix + "0001"));

        Assert.Contains("already assigned", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateSymbolNumberAsync_AllowsUniqueSymbol()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedWithSymbols);
        var svc = new SymbolNumberService(db.Context);

        var prefix = SymbolNumberDefaults.BuildPrefix(TestData.Regular);
        var old = await svc.UpdateSymbolNumberAsync(2, prefix + "0010");

        Assert.Null(old);
        Assert.Equal(prefix + "0010", db.Context.ExamRegistrations.First(er => er.Id == 2).SymbolNumber);
    }

    // Partial (re-exam) schedule with registrations spanning two batch academic years
    // across two colleges. Alpha College sorts before the shared "Test College", so an
    // alphabetical college ordering assigns Alpha first.
    private static void SeedPartialMultiYear(AppDbContext ctx)
    {
        TestData.SeedBase(ctx);

        ctx.AcademicYears.Add(TestData.AcademicYear(2, "2082"));
        ctx.AcademicYears.Add(TestData.AcademicYear(3, "2083"));

        ctx.Colleges.Add(new College
        {
            Id = 2,
            Code = "ALPH",
            Name = "Alpha College",
            Email = "a@a.com",
            PrincipalName = "Principal",
            PrincipalContactNumber = "000",
            CollegeTypeId = 1,
            IsActive = true
        });

        ctx.Faculties.Add(new Faculty { Id = 1, Name = "Management", OfficeCode = "L001", ShortName = "MG", TenantId = TestData.TenantId });
        ctx.Faculties.Add(new Faculty { Id = 2, Name = "Science", OfficeCode = "L002", ShortName = "SC", TenantId = TestData.TenantId });
        ctx.CollegeFaculties.Add(new CollegeFaculty { TenantId = TestData.TenantId, CollegeId = TestData.CollegeId, FacultyId = 1 });
        ctx.CollegeFaculties.Add(new CollegeFaculty { TenantId = TestData.TenantId, CollegeId = 2, FacultyId = 2 });

        ctx.ExamSchedules.Add(TestData.Schedule(30, 1, TestData.Partial,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), DateTime.UtcNow.AddMonths(2)));

        var sr1 = TestData.StudentRegistration(1, "s1@test.com");
        sr1.AcademicYearId = 2;
        var sr2 = TestData.StudentRegistration(2, "s2@test.com");
        sr2.AcademicYearId = 2;
        var sr3 = TestData.StudentRegistration(3, "s3@test.com");
        sr3.AcademicYearId = 3;
        ctx.StudentRegistrations.AddRange(sr1, sr2, sr3);

        ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 30));
        ctx.ApplicationVouchers.Add(TestData.Voucher(2, 2, 30));
        ctx.ApplicationVouchers.Add(TestData.Voucher(3, 3, 30));

        var reg1 = TestData.ExamRegistration(1, 30, 1);
        reg1.Status = RegistrationStatus.CollegeVerified;
        reg1.CollegeId = 2;                                  // Alpha College, batch 2082
        reg1.AcademicYearId = 2;

        var reg2 = TestData.ExamRegistration(2, 30, 2);
        reg2.Status = RegistrationStatus.CollegeVerified;
        reg2.CollegeId = TestData.CollegeId;                 // Test College, batch 2082
        reg2.AcademicYearId = 2;

        var reg3 = TestData.ExamRegistration(3, 30, 3);
        reg3.Status = RegistrationStatus.CollegeVerified;
        reg3.CollegeId = TestData.CollegeId;                 // Test College, batch 2083
        reg3.AcademicYearId = 3;

        ctx.ExamRegistrations.AddRange(reg1, reg2, reg3);
    }

    [Fact]
    public async Task GenerateAsync_WithAcademicYearSelection_AssignsOnlySelectedYearsStudents()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedPartialMultiYear);
        var svc = new SymbolNumberService(db.Context);

        var result = await svc.GenerateAsync(30, academicYearIds: new[] { 2 });

        Assert.Equal(2, result.Assigned);
        Assert.Equal(0, result.Skipped);

        var symbols = await db.Context.ExamRegistrations
            .Where(er => er.ExamScheduleId == 30)
            .ToDictionaryAsync(er => er.Id, er => er.SymbolNumber);

        Assert.Equal(2, symbols.Count(kv => !string.IsNullOrEmpty(kv.Value)));
        Assert.Null(symbols[3]); // 2083 registration untouched
    }

    [Fact]
    public async Task GenerateAsync_WithAcademicYearSelection_AssignsAlphabeticallyAcrossColleges()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedPartialMultiYear);
        var svc = new SymbolNumberService(db.Context);

        var prefix = SymbolNumberDefaults.BuildPrefix(TestData.Partial);

        await svc.GenerateAsync(30, academicYearIds: new[] { 2 });

        var alpha = await db.Context.ExamRegistrations.AsNoTracking().FirstAsync(er => er.Id == 1);
        var testCollege = await db.Context.ExamRegistrations.AsNoTracking().FirstAsync(er => er.Id == 2);

        Assert.Equal(prefix + "0001", alpha.SymbolNumber);      // Alpha College first
        Assert.Equal(prefix + "0002", testCollege.SymbolNumber);
    }

    [Fact]
    public async Task GenerateAsync_WithMultipleYearsSelected_AssignsAllSelectedYears()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedPartialMultiYear);
        var svc = new SymbolNumberService(db.Context);

        var result = await svc.GenerateAsync(30, academicYearIds: new[] { 2, 3 });

        Assert.Equal(3, result.Assigned);
        Assert.Empty(await db.Context.ExamRegistrations
            .Where(er => er.ExamScheduleId == 30 && string.IsNullOrEmpty(er.SymbolNumber))
            .ToListAsync());
    }

    [Fact]
    public async Task GenerateAsync_SelectedYear_ManualStartCollidingWithAnotherYearsSymbol_Throws()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedPartialMultiYear);
        var svc = new SymbolNumberService(db.Context);

        var prefix = SymbolNumberDefaults.BuildPrefix(TestData.Partial);

        // 2083 already owns this symbol; a manual start that reuses it inside the
        // 2082 selection must be rejected.
        db.Context.ExamRegistrations.First(er => er.Id == 3).SymbolNumber = prefix + "0005";
        await db.Context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.GenerateAsync(30, startSequence: 5, academicYearIds: new[] { 2 }));

        Assert.Contains("collision", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(prefix + "0005", ex.Message);
    }

    [Fact]
    public async Task UnassignSymbolNumberAsync_ClearsAssignedSymbol()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedWithSymbols);
        var svc = new SymbolNumberService(db.Context);

        var prefix = SymbolNumberDefaults.BuildPrefix(TestData.Regular);
        var removed = await svc.UnassignSymbolNumberAsync(1);

        Assert.Equal(prefix + "0001", removed);
        Assert.Null(db.Context.ExamRegistrations.First(er => er.Id == 1).SymbolNumber);
    }

    [Fact]
    public async Task UnassignSymbolNumberAsync_WhenNoneAssigned_Throws()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedWithSymbols);
        var svc = new SymbolNumberService(db.Context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => svc.UnassignSymbolNumberAsync(2));

        Assert.Contains("no symbol", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnassignSymbolNumberAsync_ThenGenerateReusesFreedNumber()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedWithSymbols);
        var svc = new SymbolNumberService(db.Context);

        var prefix = SymbolNumberDefaults.BuildPrefix(TestData.Regular);

        await svc.GenerateAsync(22);   // assign 0002 (reg 2) and 0003 (reg 3)

        await svc.UnassignSymbolNumberAsync(3);   // free the max sequence 0003

        var result = await svc.GenerateAsync(22, startSequence: 3);

        Assert.Equal(1, result.Assigned);
        Assert.Equal(prefix + "0003", db.Context.ExamRegistrations.First(er => er.Id == 3).SymbolNumber);
    }

    [Fact]
    public async Task UnassignAllSymbolNumbersAsync_ClearsOnlyPassedIds()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedWithSymbols);
        var svc = new SymbolNumberService(db.Context);

        await svc.GenerateAsync(22);   // reg 2 = 0002, reg 3 = 0003

        var count = await svc.UnassignAllSymbolNumbersAsync(22, new[] { 2 });

        Assert.Equal(1, count);
        Assert.Null(db.Context.ExamRegistrations.First(er => er.Id == 2).SymbolNumber);
        Assert.NotNull(db.Context.ExamRegistrations.First(er => er.Id == 3).SymbolNumber);
    }

    [Fact]
    public async Task UnassignAllSymbolNumbersAsync_IgnoresIdsFromOtherSchedules()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedWithSymbols);
        var svc = new SymbolNumberService(db.Context);

        await svc.GenerateAsync(22);   // reg 2 = 0002, reg 3 = 0003
        var count = await svc.UnassignAllSymbolNumbersAsync(22, new[] { 1, 2 });

        Assert.Equal(1, count);        // reg 1 belongs to schedule 21, so untouched
        Assert.NotNull(db.Context.ExamRegistrations.First(er => er.Id == 1).SymbolNumber);
        Assert.Null(db.Context.ExamRegistrations.First(er => er.Id == 2).SymbolNumber);
    }

    [Fact]
    public async Task UnassignAllSymbolNumbersAsync_WhenNothingAssigned_ReturnsZero()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedWithSymbols);
        var svc = new SymbolNumberService(db.Context);

        var count = await svc.UnassignAllSymbolNumbersAsync(22, new[] { 2, 3 });

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetOverviewAsync_WithAcademicYearSelection_FiltersPlanToSelectedYear()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedPartialMultiYear);
        var svc = new SymbolNumberService(db.Context);

        var dto = await svc.GetOverviewAsync(30, academicYearIds: new[] { 2 });

        Assert.Equal(2, dto.TotalRegistrations);
        Assert.Equal(2, dto.UnassignedCount);
        Assert.Equal(2, dto.Students.Count);
        Assert.All(dto.Students, s => Assert.Equal(2, s.AcademicYearId));
        Assert.Equal(2, dto.Blocks.Count);
        Assert.All(dto.Blocks, b => Assert.Equal(2, b.AcademicYearId));

        Assert.Equal(2, dto.AvailableAcademicYears.Count);
        Assert.Contains(dto.AvailableAcademicYears, y => y.Id == 2);
        Assert.Contains(dto.AvailableAcademicYears, y => y.Id == 3);
        Assert.Equal(2, dto.AvailableColleges.Count);
        Assert.Contains(dto.AvailableColleges, c => c.Name == "Alpha College");
        Assert.Contains(dto.AvailableColleges, c => c.Name == "Test College");

        var prefix = SymbolNumberDefaults.BuildPrefix(TestData.Partial);
        var alpha = dto.Blocks.First(b => b.CollegeName == "Alpha College");
        Assert.Equal(prefix + "0001", alpha.FromSymbol);
        Assert.Equal(prefix + "0001", alpha.ToSymbol);
    }

    [Fact]
    public async Task GetOverviewAsync_WithoutAcademicYearSelection_IncludesAllYears()
    {
        using var db = new TestDb(TestTenantContext.Standard(TestData.TenantId), SeedPartialMultiYear);
        var svc = new SymbolNumberService(db.Context);

        var dto = await svc.GetOverviewAsync(30);

        Assert.Equal(3, dto.TotalRegistrations);
        Assert.Equal(3, dto.Blocks.Count);
        Assert.Equal(3, dto.Students.Count);
        Assert.Equal(2, dto.AvailableAcademicYears.Count);
        Assert.Equal(2, dto.AvailableColleges.Count);
    }
}