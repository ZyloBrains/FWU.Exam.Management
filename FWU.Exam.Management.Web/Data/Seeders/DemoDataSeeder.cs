using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class DemoDataSeeder
{
    public static async Task SeedDemoDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        // Academic Years
        if (!await context.AcademicYears.AnyAsync())
        {
            var academicYears = new[]
            {
                new AcademicYear { AcademicYearCode = 2080, AcademicYearName = "2080/2081", IsRunning = false, IsActive = true },
                new AcademicYear { AcademicYearCode = 2081, AcademicYearName = "2081/2082", IsRunning = true, IsActive = true },
            };
            await context.AcademicYears.AddRangeAsync(academicYears);
            await context.SaveChangesAsync();

            var runningYear = academicYears[1];

            // Batch
            if (!await context.Batches.AnyAsync())
            {
                await context.Batches.AddRangeAsync(new[]
                {
                    new Batch { AcademicYearId = runningYear.Id, BatchName = "2081 Batch", IsActive = true },
                });
            }

            // Semesters
            if (!await context.Semesters.AnyAsync())
            {
                var semesters = new[]
                {
                    new Semester { Number = 1, Year = 1, Name = "First Semester", Code = "SEM1", StartDate = new DateTime(2024, 9, 1), EndDate = new DateTime(2025, 1, 30), AcademicYearId = runningYear.Id },
                    new Semester { Number = 2, Year = 1, Name = "Second Semester", Code = "SEM2", StartDate = new DateTime(2025, 2, 1), EndDate = new DateTime(2025, 6, 30), AcademicYearId = runningYear.Id },
                    new Semester { Number = 3, Year = 2, Name = "Third Semester", Code = "SEM3", StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 1, 30), AcademicYearId = runningYear.Id },
                    new Semester { Number = 4, Year = 2, Name = "Fourth Semester", Code = "SEM4", StartDate = new DateTime(2026, 2, 1), EndDate = new DateTime(2026, 6, 30), AcademicYearId = runningYear.Id },
                };
                await context.Semesters.AddRangeAsync(semesters);
            }

            await context.SaveChangesAsync();
        }

        // Fiscal Year
        if (!await context.FiscalYears.AnyAsync())
        {
            await context.FiscalYears.AddRangeAsync(new[]
            {
                new FiscalYear { FiscalYearName = "2081/2082", FiscalYearCode = "FY81", StartDate = "2081/04/01", EndDate = "2082/03/31", IsRunning = true, IsActive = true },
            });
            await context.SaveChangesAsync();
        }

        // Subject Types
        if (!await context.SubjectTypes.AnyAsync())
        {
            var subjectTypes = new[]
            {
                new SubjectType { Code = "CORE", Name = "Core", MaxAllowedSubjects = 0, IsDefault = true, IsActive = true },
                new SubjectType { Code = "ELECTIVE", Name = "Elective", MaxAllowedSubjects = 2, IsDefault = false, IsActive = true },
            };
            await context.SubjectTypes.AddRangeAsync(subjectTypes);
            await context.SaveChangesAsync();

            // Subject Catalogs
            if (!await context.SubjectCatalogs.AnyAsync())
            {
                var subjects = new[]
                {
                    // BBA subjects
                    new SubjectCatalog { SubjectCode = "ENG101", SubjectName = "English I", ShortName = "English I", CreditHours = 3, SubjectTypeId = subjectTypes[0].Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "MGT101", SubjectName = "Principles of Management", ShortName = "Management", CreditHours = 3, SubjectTypeId = subjectTypes[0].Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "ACC101", SubjectName = "Financial Accounting", ShortName = "Accounting", CreditHours = 3, SubjectTypeId = subjectTypes[0].Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "ECO101", SubjectName = "Microeconomics", ShortName = "Economics", CreditHours = 3, SubjectTypeId = subjectTypes[0].Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "MTH101", SubjectName = "Business Mathematics", ShortName = "Math", CreditHours = 3, SubjectTypeId = subjectTypes[0].Id, IsActive = true },
                    // BBS subjects
                    new SubjectCatalog { SubjectCode = "BBS101", SubjectName = "Business English", ShortName = "Bus English", CreditHours = 3, SubjectTypeId = subjectTypes[0].Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BBS102", SubjectName = "Business Economics", ShortName = "Bus Econ", CreditHours = 3, SubjectTypeId = subjectTypes[0].Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BBS103", SubjectName = "Foundation of Business Management", ShortName = "Foundation Mgmt", CreditHours = 3, SubjectTypeId = subjectTypes[0].Id, IsActive = true },
                    // BCA subjects
                    new SubjectCatalog { SubjectCode = "CSC101", SubjectName = "C Programming", ShortName = "C Prog", CreditHours = 3, SubjectTypeId = subjectTypes[0].Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSC102", SubjectName = "Digital Logic", ShortName = "Digital Logic", CreditHours = 3, SubjectTypeId = subjectTypes[0].Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSC103", SubjectName = "Discrete Mathematics", ShortName = "Discrete Math", CreditHours = 3, SubjectTypeId = subjectTypes[0].Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSC104", SubjectName = "Computer Fundamentals", ShortName = "Comp Fund", CreditHours = 3, SubjectTypeId = subjectTypes[0].Id, IsActive = true },
                };
                await context.SubjectCatalogs.AddRangeAsync(subjects);
                await context.SaveChangesAsync();
            }
        }

        // Ethnicity
        if (!await context.Ethnicities.AnyAsync())
        {
            await context.Ethnicities.AddRangeAsync(new[]
            {
                new Ethnicity { EthnicityName = "Brahmin", IsDefault = false, IsActive = true },
                new Ethnicity { EthnicityName = "Chhetri", IsDefault = false, IsActive = true },
                new Ethnicity { EthnicityName = "Janajati", IsDefault = false, IsActive = true },
                new Ethnicity { EthnicityName = "Dalit", IsDefault = false, IsActive = true },
                new Ethnicity { EthnicityName = "Madhesi", IsDefault = false, IsActive = true },
                new Ethnicity { EthnicityName = "Other", IsDefault = true, IsActive = true },
            });
            await context.SaveChangesAsync();
        }

        // Student Categories
        if (!await context.StudentCategories.AnyAsync())
        {
            await context.StudentCategories.AddRangeAsync(new[]
            {
                new StudentCategory { StudentCategoryName = "Regular", IsActive = true },
                new StudentCategory { StudentCategoryName = "Partial", IsActive = true },
                new StudentCategory { StudentCategoryName = "Full-Fee", IsActive = true },
                new StudentCategory { StudentCategoryName = "Scholarship", IsActive = true },
            });
            await context.SaveChangesAsync();
        }

        // College Type
        if (!await context.CollegeTypes.AnyAsync())
        {
            var collegeTypes = new[]
            {
                new CollegeType { Code = "CONST", Name = "Constituent Campus", IsDefault = true, IsActive = true },
                new CollegeType { Code = "AFFIL", Name = "Affiliated College", IsDefault = false, IsActive = true },
            };
            await context.CollegeTypes.AddRangeAsync(collegeTypes);
            await context.SaveChangesAsync();

            // Update colleges to use CollegeTypeId
            var existingColleges = await context.Colleges.ToListAsync();
            foreach (var c in existingColleges)
            {
                if (c.CollegeTypeId == null)
                    c.CollegeTypeId = collegeTypes[0].Id;
            }
            await context.SaveChangesAsync();
        }

        // College Programs
        if (!await context.CollegePrograms.AnyAsync())
        {
            var colleges = await context.Colleges.ToListAsync();
            var programs = await context.Programs.ToListAsync();
            foreach (var college in colleges)
            {
                foreach (var program in programs)
                {
                    if (!await context.CollegePrograms.AnyAsync(cp => cp.CollegeId == college.Id && cp.ProgramId == program.Id))
                    {
                        await context.CollegePrograms.AddAsync(new CollegeProgram
                        {
                            CollegeId = college.Id,
                            ProgramId = program.Id,
                            IsActive = true,
                        });
                    }
                }
            }
            await context.SaveChangesAsync();
        }

        // Subject Offerings
        if (!await context.SubjectOfferings.AnyAsync())
        {
            var programs = await context.Programs.ToListAsync();
            var semesters = await context.Semesters.ToListAsync();
            var subjects = await context.SubjectCatalogs.ToListAsync();
            var firstSem = semesters.FirstOrDefault();
            var bbaProgram = programs.FirstOrDefault(p => p.ProgramCode == "BBA");
            var bbsProgram = programs.FirstOrDefault(p => p.ProgramCode == "BBS");
            var bcaProgram = programs.FirstOrDefault(p => p.ProgramCode == "BCA");

            if (firstSem != null)
            {
                var offerings = new List<SubjectOffering>();

                if (bbaProgram != null)
                {
                    var bbaSubjects = subjects.Take(5).ToList();
                    foreach (var (subj, idx) in bbaSubjects.Select((s, i) => (s, i)))
                    {
                        offerings.Add(new SubjectOffering
                        {
                            SubjectCatalogId = subj.Id,
                            ProgramId = bbaProgram.Id,
                            SemesterId = firstSem.Id,
                            IsCompulsory = true,
                            DisplayOrder = idx + 1,
                            HasTheory = true,
                            HasPractical = idx < 2,
                            HasInternal = true,
                            TheoryFullMarks = 60,
                            TheoryPassMarks = 24,
                            PracticalFullMarks = idx < 2 ? 40 : null,
                            PracticalPassMarks = idx < 2 ? 16 : null,
                            InternalTheoryFullMarks = 40,
                            InternalTheoryPassMarks = 16,
                        });
                    }
                }

                if (bbsProgram != null)
                {
                    var bbsSubjects = subjects.Skip(5).Take(3).ToList();
                    foreach (var (subj, idx) in bbsSubjects.Select((s, i) => (s, i)))
                    {
                        offerings.Add(new SubjectOffering
                        {
                            SubjectCatalogId = subj.Id,
                            ProgramId = bbsProgram.Id,
                            SemesterId = firstSem.Id,
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
                }

                if (bcaProgram != null)
                {
                    var bcaSubjects = subjects.Skip(8).Take(4).ToList();
                    foreach (var (subj, idx) in bcaSubjects.Select((s, i) => (s, i)))
                    {
                        offerings.Add(new SubjectOffering
                        {
                            SubjectCatalogId = subj.Id,
                            ProgramId = bcaProgram.Id,
                            SemesterId = firstSem.Id,
                            IsCompulsory = true,
                            DisplayOrder = idx + 1,
                            HasTheory = true,
                            HasPractical = idx == 0 || idx == 3,
                            HasInternal = true,
                            TheoryFullMarks = 60,
                            TheoryPassMarks = 24,
                            PracticalFullMarks = idx == 0 || idx == 3 ? 40 : null,
                            PracticalPassMarks = idx == 0 || idx == 3 ? 16 : null,
                            InternalTheoryFullMarks = 40,
                            InternalTheoryPassMarks = 16,
                        });
                    }
                }

                await context.SubjectOfferings.AddRangeAsync(offerings);
                await context.SaveChangesAsync();
            }
        }

        // Exam Type
        if (!await context.ExamTypes.AnyAsync())
        {
            await context.ExamTypes.AddRangeAsync(new[]
            {
                new ExamType { Name = "Regular", Code = 1, IsActive = true },
                new ExamType { Name = "Partial", Code = 2, IsActive = true },
                new ExamType { Name = "Supplementary", Code = 3, IsActive = true },
            });
            await context.SaveChangesAsync();
        }

        // Board
        if (!await context.Boards.AnyAsync())
        {
            await context.Boards.AddRangeAsync(new[]
            {
                new Board { CountryId = 1, BoardName = "NEB - National Examination Board", IsActive = true },
                new Board { CountryId = 1, BoardName = "TU - Tribhuvan University", IsActive = true },
                new Board { CountryId = 1, BoardName = "CTEVT", IsActive = true },
                new Board { CountryId = 1, BoardName = "FWU - Far Western University", IsActive = true },
            });
            await context.SaveChangesAsync();
        }
    }
}
