using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class WorkflowTestDataSeeder
{
    public static async Task SeedWorkflowTestDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

        if (await context.Tenants.AnyAsync(t => t.OfficeCode == "OCE"))
            return;

        await ClearExistingDataAsync(context);

        // ===================================================================
        // 1. TENANT
        // ===================================================================
        var oceTenant = new Tenant
        {
            Name = "Office of Controller of Examinations",
            OfficeCode = "OCE",
            ContactNumber = "01-2345678",
            Address = "Kathmandu, Nepal",
            Email = "info@oce.gov.np",
            TenantType = TenantType.Central,
            IsActive = true
        };
        context.Tenants.Add(oceTenant);
        await context.SaveChangesAsync();

        // ===================================================================
        // 2. GENDERS
        // ===================================================================
        var genders = new[]
        {
            new Gender { GenderName = "Male", IsActive = true },
            new Gender { GenderName = "Female", IsActive = true },
            new Gender { GenderName = "Other", IsActive = true },
        };
        await context.Genders.AddRangeAsync(genders);
        await context.SaveChangesAsync();
        var genderMale = genders[0];
        var genderFemale = genders[1];

        // ===================================================================
        // 3. LEVELS
        // ===================================================================
        var levels = new[]
        {
            new Level { LevelCode = "BL", LevelName = "Bachelor", IsActive = true },
            new Level { LevelCode = "MA", LevelName = "Master", IsActive = true },
        };
        await context.Levels.AddRangeAsync(levels);
        await context.SaveChangesAsync();
        var bachelorLevel = levels[0];

        // ===================================================================
        // 4. DEPARTMENTS
        // ===================================================================
        var departments = new[]
        {
            new Department { DepartmentCode = "SCI", DepartmentName = "Science and Technology", ShortName = "SCI", IsActive = true },
            new Department { DepartmentCode = "HUM", DepartmentName = "Humanities and Social Sciences", ShortName = "HUM", IsActive = true },
        };
        await context.Departments.AddRangeAsync(departments);
        await context.SaveChangesAsync();
        var deptSci = departments[0];
        var deptHum = departments[1];

        // ===================================================================
        // 5. PROGRAMS
        // ===================================================================
        var programs = new[]
        {
            new Program
            {
                ProgramCode = "BSCSIT",
                ProgramName = "Bachelor of Science in Computer Science and Information Technology",
                ShortName = "B.Sc. CSIT",
                LevelId = bachelorLevel.Id,
                DepartmentId = deptSci.Id,
                Duration = 4,
                IsActive = true,
                RollNumberPrefix = "CSIT"
            },
            new Program
            {
                ProgramCode = "BA",
                ProgramName = "Bachelor of Arts",
                ShortName = "BA",
                LevelId = bachelorLevel.Id,
                DepartmentId = deptHum.Id,
                Duration = 4,
                IsActive = true,
                RollNumberPrefix = "BA"
            },
        };
        await context.Programs.AddRangeAsync(programs);
        await context.SaveChangesAsync();
        var csitProgram = programs[0];
        var baProgram = programs[1];

        // ===================================================================
        // 6. FACULTIES
        // ===================================================================
        var faculties = new[]
        {
            new Faculty
            {
                Name = "Faculty of Science and Technology",
                OfficeCode = "FST",
                ContactNumber = "099-524000",
                Address = "Mahendranagar, Kanchanpur",
                Email = "science@fwu.edu.np",
            },
            new Faculty
            {
                Name = "Faculty of Humanities and Social Sciences",
                OfficeCode = "HUM",
                ContactNumber = "099-520729",
                Address = "Mahendranagar, Kanchanpur",
                Email = "humanities@fwu.edu.np",
            },
        };
        await context.Faculties.AddRangeAsync(faculties);
        await context.SaveChangesAsync();
        var fstFaculty = faculties[0];
        var humFaculty = faculties[1];

        // Assign departments to faculties
        deptSci.FacultyId = fstFaculty.Id;
        deptHum.FacultyId = humFaculty.Id;
        await context.SaveChangesAsync();

        // ===================================================================
        // 7. COLLEGES (with Faculty M2M links)
        // ===================================================================
        var colleges = new[]
        {
            new College
            {
                Code = "CST",
                Name = "College of Science and Technology",
                IsActive = true,
                Faculties = new List<Faculty> { fstFaculty }
            },
            new College
            {
                Code = "CH",
                Name = "College of Humanities and Social Sciences",
                IsActive = true,
                Faculties = new List<Faculty> { humFaculty }
            },
        };
        await context.Colleges.AddRangeAsync(colleges);
        await context.SaveChangesAsync();
        var collegeCst = colleges[0];
        var collegeHum = colleges[1];

        // ===================================================================
        // 8. COLLEGE-PROGRAM LINKS
        // ===================================================================
        await context.CollegePrograms.AddRangeAsync(new[]
        {
            new CollegeProgram { CollegeId = collegeCst.Id, ProgramId = csitProgram.Id, IsActive = true },
            new CollegeProgram { CollegeId = collegeHum.Id, ProgramId = baProgram.Id, IsActive = true },
        });
        await context.SaveChangesAsync();

        // ===================================================================
        // 9. ACADEMIC YEAR
        // ===================================================================
        var runningYear = new AcademicYear
        {
            AcademicYearCode = 2081,
            AcademicYearName = "2081/2082",
            AcademicYearNameNepali = "२०८१/२०८२",
            IsRunning = true,
            IsActive = true
        };
        await context.AcademicYears.AddAsync(runningYear);
        await context.SaveChangesAsync();

        // ===================================================================
        // 10. SEMESTERS
        // ===================================================================
        var semesters = new[]
        {
            new Semester { Number = 1, Year = 1, Name = "First Semester", Code = "SEM1", StartDate = new DateTime(2024, 9, 1), EndDate = new DateTime(2025, 1, 30), AcademicYearId = runningYear.Id },
            new Semester { Number = 2, Year = 1, Name = "Second Semester", Code = "SEM2", StartDate = new DateTime(2025, 2, 1), EndDate = new DateTime(2025, 6, 30), AcademicYearId = runningYear.Id },
        };
        await context.Semesters.AddRangeAsync(semesters);
        await context.SaveChangesAsync();
        var sem1 = semesters[0];

        // ===================================================================
        // 11. SUBJECT TYPES
        // ===================================================================
        var subjectTypes = new[]
        {
            new SubjectType { Code = "CORE", Name = "Core", MaxAllowedSubjects = 0, IsDefault = true, IsActive = true },
            new SubjectType { Code = "ELECTIVE", Name = "Elective", MaxAllowedSubjects = 2, IsDefault = false, IsActive = true },
        };
        await context.SubjectTypes.AddRangeAsync(subjectTypes);
        await context.SaveChangesAsync();
        var coreType = subjectTypes[0];
        var electiveType = subjectTypes[1];

        // ===================================================================
        // 12. SUBJECT CATALOGS
        // ===================================================================
        var subjects = new List<SubjectCatalog>
        {
            // B.Sc. CSIT SEM1
            new SubjectCatalog { SubjectCode = "CSIT111", SubjectName = "Introduction to Information Technology", ShortName = "Intro IT", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
            new SubjectCatalog { SubjectCode = "CSIT112", SubjectName = "Digital Logic", ShortName = "Digital Logic", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
            new SubjectCatalog { SubjectCode = "CSIT113", SubjectName = "Discrete Mathematics", ShortName = "Discrete Math", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
            new SubjectCatalog { SubjectCode = "CSIT114", SubjectName = "C Programming", ShortName = "C Prog", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
            new SubjectCatalog { SubjectCode = "CSIT115", SubjectName = "English I", ShortName = "English I", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
            // BA SEM1
            new SubjectCatalog { SubjectCode = "BA101", SubjectName = "English I", ShortName = "English I", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
            new SubjectCatalog { SubjectCode = "BA102", SubjectName = "Nepali I", ShortName = "Nepali I", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
            new SubjectCatalog { SubjectCode = "BA103", SubjectName = "Introduction to Sociology", ShortName = "Sociology", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
            new SubjectCatalog { SubjectCode = "BA104", SubjectName = "Political Theory", ShortName = "Pol Theory", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
            new SubjectCatalog { SubjectCode = "BA105", SubjectName = "Principles of Economics", ShortName = "Economics", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
        };
        await context.SubjectCatalogs.AddRangeAsync(subjects);
        await context.SaveChangesAsync();

        var csitSubjects = subjects.Where(s => s.SubjectCode.StartsWith("CSIT")).OrderBy(s => s.SubjectCode).ToList();
        var baSubjects = subjects.Where(s => s.SubjectCode.StartsWith("BA")).OrderBy(s => s.SubjectCode).ToList();

        // ===================================================================
        // 13. SUBJECT OFFERINGS
        // ===================================================================
        var offerings = new List<SubjectOffering>();

        // CSIT SEM1
        foreach (var (subj, idx) in csitSubjects.Select((s, i) => (s, i)))
        {
            var hasPractical = subj.SubjectCode is "CSIT111" or "CSIT114";
            offerings.Add(new SubjectOffering
            {
                SubjectCatalogId = subj.Id,
                ProgramId = csitProgram.Id,
                SemesterId = sem1.Id,
                IsCompulsory = true,
                DisplayOrder = idx + 1,
                HasTheory = true,
                HasPractical = hasPractical,
                HasInternal = true,
                TheoryFullMarks = 60,
                TheoryPassMarks = 24,
                PracticalFullMarks = hasPractical ? 40 : null,
                PracticalPassMarks = hasPractical ? 16 : null,
                InternalTheoryFullMarks = 40,
                InternalTheoryPassMarks = 16,
            });
        }

        // BA SEM1
        foreach (var (subj, idx) in baSubjects.Select((s, i) => (s, i)))
        {
            offerings.Add(new SubjectOffering
            {
                SubjectCatalogId = subj.Id,
                ProgramId = baProgram.Id,
                SemesterId = sem1.Id,
                IsCompulsory = true,
                DisplayOrder = idx + 1,
                HasTheory = true,
                HasPractical = false,
                HasInternal = true,
                TheoryFullMarks = 60,
                TheoryPassMarks = 24,
                InternalTheoryFullMarks = 40,
                InternalTheoryPassMarks = 16,
            });
        }

        await context.SubjectOfferings.AddRangeAsync(offerings);
        await context.SaveChangesAsync();

        // ===================================================================
        // 14. EXAM TYPES
        // ===================================================================
        var examTypes = new[]
        {
            new ExamType { Name = "Regular", Code = 1, IsActive = true },
            new ExamType { Name = "Partial", Code = 2, IsActive = true },
            new ExamType { Name = "Supplementary", Code = 3, IsActive = true },
        };
        await context.ExamTypes.AddRangeAsync(examTypes);
        await context.SaveChangesAsync();
        var regularExamType = examTypes[0];

        // ===================================================================
        // 15. BOARDS
        // ===================================================================
        await context.Boards.AddRangeAsync(new[]
        {
            new Board { CountryId = 1, BoardName = "NEB - National Examination Board", IsActive = true },
            new Board { CountryId = 1, BoardName = "FWU - Far Western University", IsActive = true },
        });
        await context.SaveChangesAsync();

        // ===================================================================
        // 16. ETHNICITIES
        // ===================================================================
        var ethnicities = new[]
        {
            new Ethnicity { EthnicityName = "Brahmin", IsDefault = false, IsActive = true },
            new Ethnicity { EthnicityName = "Chhetri", IsDefault = false, IsActive = true },
            new Ethnicity { EthnicityName = "Janajati", IsDefault = false, IsActive = true },
            new Ethnicity { EthnicityName = "Dalit", IsDefault = false, IsActive = true },
            new Ethnicity { EthnicityName = "Madhesi", IsDefault = false, IsActive = true },
            new Ethnicity { EthnicityName = "Other", IsDefault = true, IsActive = true },
        };
        await context.Ethnicities.AddRangeAsync(ethnicities);
        await context.SaveChangesAsync();
        var ethnicityOther = ethnicities[5];

        // ===================================================================
        // 17. STUDENT CATEGORIES
        // ===================================================================
        var categories = new[]
        {
            new StudentCategory { StudentCategoryName = "Regular", IsActive = true },
            new StudentCategory { StudentCategoryName = "Partial", IsActive = true },
            new StudentCategory { StudentCategoryName = "Full-Fee", IsActive = true },
            new StudentCategory { StudentCategoryName = "Scholarship", IsActive = true },
        };
        await context.StudentCategories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
        var categoryRegular = categories[0];

        // ===================================================================
        // 18. COLLEGE TYPES
        // ===================================================================
        var collegeTypes = new[]
        {
            new CollegeType { Code = "CONST", Name = "Constituent Campus", IsDefault = true, IsActive = true },
            new CollegeType { Code = "AFFIL", Name = "Affiliated College", IsDefault = false, IsActive = true },
        };
        await context.CollegeTypes.AddRangeAsync(collegeTypes);
        await context.SaveChangesAsync();
        var collegeTypeConst = collegeTypes[0];

        foreach (var c in new[] { collegeCst, collegeHum })
        {
            c.CollegeTypeId = collegeTypeConst.Id;
        }
        await context.SaveChangesAsync();

        // ===================================================================
        // 19. FISCAL YEAR
        // ===================================================================
        await context.FiscalYears.AddAsync(new FiscalYear
        {
            FiscalYearName = "2081/2082",
            FiscalYearCode = "FY81",
            StartDate = "2081/04/01",
            EndDate = "2082/03/31",
            IsRunning = true,
            IsActive = true
        });
        await context.SaveChangesAsync();

        // ===================================================================
        // 20. USERS
        // ===================================================================
        var adminUser = new AppUser
        {
            UserName = "admin@fwu.edu.np",
            Email = "admin@fwu.edu.np",
            EmailConfirmed = true,
            FullName = "Super Administrator",
            IsActive = true
        };
        var adminResult = await userManager.CreateAsync(adminUser, "Admin@123");
        if (!adminResult.Succeeded)
            throw new Exception($"Failed to create admin: {string.Join(", ", adminResult.Errors.Select(e => e.Description))}");
        await userManager.AddToRoleAsync(adminUser, Role.SuperAdmin);

        var scienceStudentUser = new AppUser
        {
            UserName = "science.student@fwu.edu.np",
            Email = "science.student@fwu.edu.np",
            EmailConfirmed = true,
            FullName = "Ram Sharma (B.Sc. CSIT)",
            IsActive = true,
            CollegeId = collegeCst.Id,
            FacultyId = fstFaculty.Id
        };
        var sciResult = await userManager.CreateAsync(scienceStudentUser, "Admin@123");
        if (!sciResult.Succeeded)
            throw new Exception($"Failed to create science student: {string.Join(", ", sciResult.Errors.Select(e => e.Description))}");
        await userManager.AddToRoleAsync(scienceStudentUser, Role.Student);

        var humanityStudentUser = new AppUser
        {
            UserName = "humanity.student@fwu.edu.np",
            Email = "humanity.student@fwu.edu.np",
            EmailConfirmed = true,
            FullName = "Sita Adhikari (BA)",
            IsActive = true,
            CollegeId = collegeHum.Id,
            FacultyId = humFaculty.Id
        };
        var humResult = await userManager.CreateAsync(humanityStudentUser, "Admin@123");
        if (!humResult.Succeeded)
            throw new Exception($"Failed to create humanity student: {string.Join(", ", humResult.Errors.Select(e => e.Description))}");
        await userManager.AddToRoleAsync(humanityStudentUser, Role.Student);

        // ===================================================================
        // 21. STUDENT REGISTRATIONS
        // ===================================================================
        var regScience = new StudentRegistration
        {
            FirstName = "Ram",
            LastName = "Sharma",
            Email = "science.student@fwu.edu.np",
            DateOfBirthBS = "2056-05-15",
            DateOfBirthAD = "1999-08-20",
            ContactNumber = "9841234567",
            GenderId = genderMale.Id,
            CollegeId = collegeCst.Id,
            FacultyId = fstFaculty.Id,
            LevelId = bachelorLevel.Id,
            DepartmentId = deptSci.Id,
            ProgramId = csitProgram.Id,
            StudentCategoryId = categoryRegular.Id,
            EthnicityId = ethnicityOther.Id,
            AcademicYearId = runningYear.Id,
            IsActive = true
        };
        context.StudentRegistrations.Add(regScience);

        var regHum = new StudentRegistration
        {
            FirstName = "Sita",
            LastName = "Adhikari",
            Email = "humanity.student@fwu.edu.np",
            DateOfBirthBS = "2056-03-10",
            DateOfBirthAD = "1999-06-15",
            ContactNumber = "9841234568",
            GenderId = genderFemale.Id,
            CollegeId = collegeHum.Id,
            FacultyId = humFaculty.Id,
            LevelId = bachelorLevel.Id,
            DepartmentId = deptHum.Id,
            ProgramId = baProgram.Id,
            StudentCategoryId = categoryRegular.Id,
            EthnicityId = ethnicityOther.Id,
            AcademicYearId = runningYear.Id,
            IsActive = true
        };
        context.StudentRegistrations.Add(regHum);
        await context.SaveChangesAsync();

        // ===================================================================
        // 22. STUDENT ADMISSIONS
        // ===================================================================
        var admissionScience = new StudentAdmission
        {
            ProgramsId = csitProgram.Id,
            CollegeId = collegeCst.Id,
            AppUserId = scienceStudentUser.Id,
            AdmissionDate = DateTime.UtcNow,
            IsActive = true
        };
        context.Set<StudentAdmission>().Add(admissionScience);

        var admissionHum = new StudentAdmission
        {
            ProgramsId = baProgram.Id,
            CollegeId = collegeHum.Id,
            AppUserId = humanityStudentUser.Id,
            AdmissionDate = DateTime.UtcNow,
            IsActive = true
        };
        context.Set<StudentAdmission>().Add(admissionHum);
        await context.SaveChangesAsync();

        // ===================================================================
        // 23. SEMESTER ENROLLMENTS
        // ===================================================================
        context.Set<SemesterEnrollment>().AddRange(new[]
        {
            new SemesterEnrollment
            {
                StudentAdmissionId = admissionScience.Id,
                SemesterId = sem1.Id,
                EnrollmentStatus = StudentEnrollmentStatus.Active,
                EnrollmentType = EnrollmentType.FullTime,
                PaymentStatus = PaymentStatus.Paid,
                EnrolledDate = DateTime.UtcNow,
                TotalCredits = 15,
                GradePoints = 0,
                TotalFee = 5000,
                PaidAmount = 5000
            },
            new SemesterEnrollment
            {
                StudentAdmissionId = admissionHum.Id,
                SemesterId = sem1.Id,
                EnrollmentStatus = StudentEnrollmentStatus.Active,
                EnrollmentType = EnrollmentType.FullTime,
                PaymentStatus = PaymentStatus.Paid,
                EnrolledDate = DateTime.UtcNow,
                TotalCredits = 15,
                GradePoints = 0,
                TotalFee = 5000,
                PaidAmount = 5000
            }
        });
        await context.SaveChangesAsync();

        // ===================================================================
        // 24. EXAM SCHEDULES
        // ===================================================================
        var csitExamSchedule = new ExamSchedule
        {
            ExamScheduleName = "B.Sc. CSIT First Semester Exam 2081",
            ExamScheduleCode = "CSIT-SEM1-2081",
            CollegeId = collegeCst.Id,
            ProgramId = csitProgram.Id,
            SemesterId = sem1.Id,
            AcademicYearId = runningYear.Id,
            ExamTypeId = regularExamType.Id,
            LevelId = bachelorLevel.Id,
            StartDateBs = "2081-10-01",
            EndDateBs = "2081-10-15",
            StartDate = new DateOnly(2025, 1, 14),
            EndDate = new DateOnly(2025, 1, 28),
            StartTime = new TimeOnly(7, 0),
            EndTime = new TimeOnly(10, 0),
            IsActive = true
        };
        context.ExamSchedules.Add(csitExamSchedule);

        var baExamSchedule = new ExamSchedule
        {
            ExamScheduleName = "BA First Semester Exam 2081",
            ExamScheduleCode = "BA-SEM1-2081",
            CollegeId = collegeHum.Id,
            ProgramId = baProgram.Id,
            SemesterId = sem1.Id,
            AcademicYearId = runningYear.Id,
            ExamTypeId = regularExamType.Id,
            LevelId = bachelorLevel.Id,
            StartDateBs = "2081-11-01",
            EndDateBs = "2081-11-15",
            StartDate = new DateOnly(2025, 2, 14),
            EndDate = new DateOnly(2025, 2, 28),
            StartTime = new TimeOnly(7, 0),
            EndTime = new TimeOnly(10, 0),
            IsActive = true
        };
        context.ExamSchedules.Add(baExamSchedule);
        await context.SaveChangesAsync();

        // ===================================================================
        // 25. EXAM FEES
        // ===================================================================
        await context.ExamFees.AddRangeAsync(new[]
        {
            new ExamFee { Name = "CSIT SEM1 Regular Exam Fee", ExamScheduleId = csitExamSchedule.Id, Amount = 1500m },
            new ExamFee { Name = "BA SEM1 Regular Exam Fee", ExamScheduleId = baExamSchedule.Id, Amount = 1200m },
        });
        await context.SaveChangesAsync();

        // ===================================================================
        // 26. BATCH
        // ===================================================================
        await context.Batches.AddAsync(new Batch { AcademicYearId = runningYear.Id, BatchName = "2081 Batch", IsActive = true });
        await context.SaveChangesAsync();
    }

    private static async Task<bool> TableExistsAsync(AppDbContext context, string tableName)
    {
        try
        {
            var conn = context.Database.GetDbConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT 1 WHERE OBJECT_ID(N'[{tableName}]', N'U') IS NOT NULL";
            var result = await cmd.ExecuteScalarAsync();
            await conn.CloseAsync();
            return result != null;
        }
        catch
        {
            return false;
        }
    }

    private static async Task ClearExistingDataAsync(AppDbContext context)
    {
        var tableNames = new[]
        {
            "SemesterEnrollments",
            "StudentAdmissions",
            "StudentQualifications",
            "StudentGuardians",
            "ExamFees",
            "ExamRegistrations",
            "ExamSlots",
            "ExamSubjectResults",
            "ExamSchedules",
            "SubjectOfferings",
            "SubjectCatalogs",
            "PaymentRequestLogs",
            "PaymentResponseLogs",
            "ApplicationVouchers",
            "EntranceExamApplications",
            "ProgramSubjectPracticalCharges",
            "CollegePrograms",
            "CollegeFaculty",
            "CollegeFaculties",
            "FacultyCollege",
            "Colleges",
            "Faculties",
            "Programs",
            "Departments",
            "Levels",
            "AcademicYears",
            "Batches",
            "Semesters",
            "SubjectTypes",
            "ExamTypes",
            "Boards",
            "Ethnicities",
            "StudentCategories",
            "CollegeTypes",
            "CollegeProfiles",
            "Genders",
            "PreviousLevels",
            "GradeDefinitions",
            "GradingSchemes",
            "FiscalYears",
            "CurriculumVersions",
            "EntryFormats",
            "PeriodTypes",
            "SchoolTypes",
            "IndexGroups",
            "QuestionSets",
            "Notices",
            "ExamCenters",
            "ExamRollNumberSetup",
            "Addresses",
            "PaymentPracticalSubjects",
            "UserAttachments",
            "NepaliDates",
        };

        foreach (var table in tableNames)
        {
            if (!await TableExistsAsync(context, table))
                continue;
            try
            {
                await context.Database.ExecuteSqlRawAsync($"DELETE FROM [{table}]");
            }
            catch
            {
                // Skip if FK constraint or other transient error
            }
        }

        // Clear Identity-related tables
        var identityTables = new[] { "AspNetUserRoles", "AspNetRoleClaims", "AspNetUserClaims", "AspNetUserLogins", "AspNetUserTokens", "AspNetUsers", "AspNetRoles" };
        foreach (var table in identityTables)
        {
            if (!await TableExistsAsync(context, table))
                continue;
            try { await context.Database.ExecuteSqlRawAsync($"DELETE FROM [{table}]"); } catch { }
        }

        // Clear permissions
        var permTables = new[] { "RolePermissions", "Permissions" };
        foreach (var table in permTables)
        {
            if (!await TableExistsAsync(context, table))
                continue;
            try { await context.Database.ExecuteSqlRawAsync($"DELETE FROM [{table}]"); } catch { }
        }

        // Clear Tenants
        if (await TableExistsAsync(context, "Tenants"))
        {
            try { await context.Database.ExecuteSqlRawAsync("DELETE FROM [Tenants]"); } catch { }
        }

        // Clear location data
        var locationTables = new[] { "LocalLevels", "Districts", "Provinces" };
        foreach (var table in locationTables)
        {
            if (!await TableExistsAsync(context, table))
                continue;
            try { await context.Database.ExecuteSqlRawAsync($"DELETE FROM [{table}]"); } catch { }
        }
    }
}
