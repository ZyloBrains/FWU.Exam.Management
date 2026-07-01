using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
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
                new AcademicYear { AcademicYearCode = "2080", AcademicYearName = "2080/2081", AcademicYearNameNepali = "२०८०/२०८१", IsRunning = false, IsActive = true },
                new AcademicYear { AcademicYearCode = "2081", AcademicYearName = "2081/2082", AcademicYearNameNepali = "२०८१/२०८२", IsRunning = true, IsActive = true },
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
                new ExamType { Name = "Regular", Code = "1", IsActive = true },
                new ExamType { Name = "Partial", Code = "2", IsActive = true },
                new ExamType { Name = "Supplementary", Code = "3", IsActive = true },
                new ExamType { Name = "Entrance", Code = "4", IsActive = true },
            });
            await context.SaveChangesAsync();
        }
        else if (!await context.ExamTypes.AnyAsync(et => et.Name == "Entrance"))
        {
            context.ExamTypes.Add(new ExamType { Name = "Entrance", Code = "4", IsActive = true });
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

        // Demo Student Registration
        var demoStudentEmail = "student@gmail.com";
        var demoUser = await context.Users.FirstOrDefaultAsync(u => u.Email == demoStudentEmail);
        var collegeCoc = await context.Colleges.FirstOrDefaultAsync(c => c.Code == "COC");
        var collegeSom = await context.Colleges.FirstOrDefaultAsync(c => c.Code == "SOM");
        var level = await context.Levels.FirstOrDefaultAsync(l => l.LevelCode == "BL");
        var deptMgmt = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "MGMT");
        var deptSci = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "SCI");
        var faculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "SOE");
        var genderMale = await context.Genders.FirstOrDefaultAsync(g => g.GenderName == "Male");
        var genderFemale = await context.Genders.FirstOrDefaultAsync(g => g.GenderName == "Female");
        var category = await context.StudentCategories.FirstOrDefaultAsync(sc => sc.StudentCategoryName == "Regular");
        var ethnicity = await context.Ethnicities.FirstOrDefaultAsync(e => e.EthnicityName == "Other");
        var academicYear = await context.AcademicYears.FirstOrDefaultAsync(ay => ay.IsRunning);

        if (collegeCoc != null && level != null && deptMgmt != null && genderMale != null && category != null && academicYear != null)
        {
            var bbaProgram = await context.Programs.FirstOrDefaultAsync(p => p.ProgramCode == "BBA");
            var bbsProgram = await context.Programs.FirstOrDefaultAsync(p => p.ProgramCode == "BBS");
            var bcaProgram = await context.Programs.FirstOrDefaultAsync(p => p.ProgramCode == "BCA");
            var firstSemester = await context.Semesters.FirstOrDefaultAsync(s => s.Number == 1 && s.Year == 1);

            if (bbaProgram != null)
            {
                // Existing demo student (student@gmail.com)
                if (!await context.StudentRegistrations.AnyAsync(sr => sr.Email == demoStudentEmail))
                {
                    var studentRegistration = new StudentRegistration
                    {
                        FirstName = "Test",
                        LastName = "Student",
                        Email = demoStudentEmail,
                        DateOfBirthBS = "2055-03-15",
                        DateOfBirthAD = "1998-12-30",
                        ContactNumber = "9841234567",
                        GenderId = genderMale.Id,
                        CollegeId = collegeCoc.Id,
                        FacultyId = faculty?.Id,
                        LevelId = level.Id,
                        DepartmentId = deptMgmt.Id,
                        ProgramId = bbaProgram?.Id,
                        StudentCategoryId = category.Id,
                        EthnicityId = ethnicity?.Id,
                        AcademicYearId = academicYear.Id,
                        IsActive = true
                    };
                    context.StudentRegistrations.Add(studentRegistration);
                    await context.SaveChangesAsync();

                    // Student Admission
                    if (demoUser != null)
                    {
                        var admission = new StudentAdmission
                        {
                            ProgramsId = bbaProgram.Id,
                            CollegeId = collegeCoc.Id,
                            AppUserId = demoUser.Id,
                            AdmissionDate = DateTime.UtcNow,
                            IsActive = true
                        };
                        context.StudentAdmissions.Add(admission);
                        await context.SaveChangesAsync();

                        // Semester Enrollment for Semester 1
                        if (firstSemester != null)
                        {
                            var enrollment = new SemesterEnrollment
                            {
                                StudentAdmissionId = admission.Id,
                                SemesterId = firstSemester.Id,
                                EnrollmentStatus = StudentEnrollmentStatus.Active,
                                EnrollmentType = EnrollmentType.FullTime,
                                PaymentStatus = PaymentStatus.Paid,
                                EnrolledDate = DateTime.UtcNow,
                                TotalCredits = 15,
                                GradePoints = 0,
                                TotalFee = 5000,
                                PaidAmount = 5000
                            };
                            context.Set<SemesterEnrollment>().Add(enrollment);
                            await context.SaveChangesAsync();
                        }
                    }
                }

                // Additional demo students for program column verification
                var extraStudents = new List<StudentRegistration>();

                if (!await context.StudentRegistrations.AnyAsync(sr => sr.Email == "bbs.student@example.com") && bbsProgram != null)
                {
                    extraStudents.Add(new StudentRegistration
                    {
                        FirstName = "Sita", LastName = "Sharma", Email = "bbs.student@example.com",
                        DateOfBirthBS = "2056-05-20", DateOfBirthAD = "1999-08-15",
                        ContactNumber = "9841234568", GenderId = genderMale.Id,
                        CollegeId = collegeCoc.Id, FacultyId = faculty?.Id,
                        LevelId = level.Id, DepartmentId = deptMgmt.Id, ProgramId = bbsProgram.Id,
                        StudentCategoryId = category.Id, EthnicityId = ethnicity?.Id,
                        AcademicYearId = academicYear.Id, IsActive = true
                    });
                }

                if (!await context.StudentRegistrations.AnyAsync(sr => sr.Email == "bca.student@example.com") && bcaProgram != null && deptSci != null)
                {
                    extraStudents.Add(new StudentRegistration
                    {
                        FirstName = "Ram", LastName = "Poudel", Email = "bca.student@example.com",
                        DateOfBirthBS = "2056-07-15", DateOfBirthAD = "1999-10-10",
                        ContactNumber = "9841234569", GenderId = genderMale.Id,
                        CollegeId = collegeCoc.Id, FacultyId = faculty?.Id,
                        LevelId = level.Id, DepartmentId = deptSci.Id, ProgramId = bcaProgram.Id,
                        StudentCategoryId = category.Id, EthnicityId = ethnicity?.Id,
                        AcademicYearId = academicYear.Id, IsActive = true
                    });
                }

                if (collegeSom != null && !await context.StudentRegistrations.AnyAsync(sr => sr.Email == "som.student@example.com"))
                {
                    extraStudents.Add(new StudentRegistration
                    {
                        FirstName = "Gita", LastName = "Adhikari", Email = "som.student@example.com",
                        DateOfBirthBS = "2055-01-10", DateOfBirthAD = "1998-04-20",
                        ContactNumber = "9841234570", GenderId = (genderFemale ?? genderMale).Id,
                        CollegeId = collegeSom.Id, FacultyId = faculty?.Id,
                        LevelId = level.Id, DepartmentId = deptMgmt.Id, ProgramId = bbaProgram.Id,
                        StudentCategoryId = category.Id, EthnicityId = ethnicity?.Id,
                        AcademicYearId = academicYear.Id, IsActive = true
                    });
                }

                if (extraStudents.Count > 0)
                {
                    context.StudentRegistrations.AddRange(extraStudents);
                    await context.SaveChangesAsync();
                }
            }
        }

        // Engineering & CSIT Demo Students
        var facultyFoe = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "ENG");
        var facultyFst = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "FST");
        var engCollege = await context.Colleges.FirstOrDefaultAsync(c => c.Code == "ENG-SOE");
        var csitCollege = await context.Colleges.FirstOrDefaultAsync(c => c.Code == "CDC-CSIT");
        var enggDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "ENGG");

        if (!await context.StudentRegistrations.AnyAsync(sr => sr.Email == "civil.student@example.com") && engCollege != null && enggDept != null && level != null && genderMale != null && category != null && academicYear != null)
        {
            var beCivil = await context.Programs.FirstOrDefaultAsync(p => p.ProgramCode == "BECT");
            if (beCivil != null)
            {
                context.StudentRegistrations.Add(new StudentRegistration
                {
                    FirstName = "Krishna", LastName = "Joshi", Email = "civil.student@example.com",
                    DateOfBirthBS = "2057-02-10", DateOfBirthAD = "2000-05-15",
                    ContactNumber = "9841234571", GenderId = genderMale.Id,
                    CollegeId = engCollege.Id, FacultyId = facultyFoe?.Id,
                    LevelId = level.Id, DepartmentId = enggDept.Id, ProgramId = beCivil.Id,
                    StudentCategoryId = category.Id, EthnicityId = ethnicity?.Id,
                    AcademicYearId = academicYear!.Id, IsActive = true
                });
                await context.SaveChangesAsync();
            }
        }

        if (!await context.StudentRegistrations.AnyAsync(sr => sr.Email == "comp.student@example.com") && engCollege != null && enggDept != null && level != null && genderMale != null && category != null && academicYear != null)
        {
            var beComp = await context.Programs.FirstOrDefaultAsync(p => p.ProgramCode == "BECP");
            if (beComp != null)
            {
                context.StudentRegistrations.Add(new StudentRegistration
                {
                    FirstName = "Hari", LastName = "Bhatta", Email = "comp.student@example.com",
                    DateOfBirthBS = "2057-04-20", DateOfBirthAD = "2000-07-25",
                    ContactNumber = "9841234572", GenderId = genderMale.Id,
                    CollegeId = engCollege.Id, FacultyId = facultyFoe?.Id,
                    LevelId = level.Id, DepartmentId = enggDept.Id, ProgramId = beComp.Id,
                    StudentCategoryId = category.Id, EthnicityId = ethnicity?.Id,
                    AcademicYearId = academicYear!.Id, IsActive = true
                });
                await context.SaveChangesAsync();
            }
        }

        if (!await context.StudentRegistrations.AnyAsync(sr => sr.Email == "csit.student@example.com") && csitCollege != null && deptSci != null && level != null && genderMale != null && category != null && academicYear != null)
        {
            var bscCsit = await context.Programs.FirstOrDefaultAsync(p => p.ProgramCode == "BSCSIT");
            if (bscCsit != null)
            {
                context.StudentRegistrations.Add(new StudentRegistration
                {
                    FirstName = "Mina", LastName = "Kunwar", Email = "csit.student@example.com",
                    DateOfBirthBS = "2057-01-15", DateOfBirthAD = "2000-04-10",
                    ContactNumber = "9841234573", GenderId = (genderFemale ?? genderMale).Id,
                    CollegeId = csitCollege.Id, FacultyId = facultyFst?.Id,
                    LevelId = level.Id, DepartmentId = deptSci.Id, ProgramId = bscCsit.Id,
                    StudentCategoryId = category.Id, EthnicityId = ethnicity?.Id,
                    AcademicYearId = academicYear!.Id, IsActive = true
                });
                await context.SaveChangesAsync();
            }
        }

        if (!await context.StudentRegistrations.AnyAsync(sr => sr.Email == "bit.student@example.com") && csitCollege != null && deptSci != null && level != null && genderFemale != null && category != null && academicYear != null)
        {
            var bitProgram = await context.Programs.FirstOrDefaultAsync(p => p.ProgramCode == "BIT");
            if (bitProgram != null)
            {
                context.StudentRegistrations.Add(new StudentRegistration
                {
                    FirstName = "Sarita", LastName = "Thapa", Email = "bit.student@example.com",
                    DateOfBirthBS = "2057-06-05", DateOfBirthAD = "2000-09-18",
                    ContactNumber = "9841234574", GenderId = genderFemale.Id,
                    CollegeId = csitCollege.Id, FacultyId = facultyFst?.Id,
                    LevelId = level.Id, DepartmentId = deptSci.Id, ProgramId = bitProgram.Id,
                    StudentCategoryId = category.Id, EthnicityId = ethnicity?.Id,
                    AcademicYearId = academicYear!.Id, IsActive = true
                });
                await context.SaveChangesAsync();
            }
        }

        // Demo Exam Schedules
        if (!await context.ExamSchedules.AnyAsync())
        {
            var runningYear = await context.AcademicYears.FirstOrDefaultAsync(ay => ay.IsRunning);
            var bbaProgram = await context.Programs.FirstOrDefaultAsync(p => p.ProgramCode == "BBA");
            var firstSemester = await context.Semesters.FirstOrDefaultAsync(s => s.Number == 1 && s.Year == 1);
            var entranceExamType = await context.ExamTypes.FirstOrDefaultAsync(et => et.Name == "Entrance");
            var bachelorLevel = await context.Levels.FirstOrDefaultAsync(l => l.LevelCode == "BL");

            if (runningYear != null && bbaProgram != null && firstSemester != null && entranceExamType != null)
            {
                var examSchedule = new ExamSchedule
                {
                    ExamScheduleName = "BBA First Semester Exam 2081",
                    ExamScheduleCode = "BBA-SEM1-2081",
                    ProgramId = bbaProgram.Id,
                    SemesterId = firstSemester.Id,
                    AcademicYearId = runningYear.Id,
                    ExamTypeId = entranceExamType.Id,
                    LevelId = bachelorLevel?.Id,
                    StartDateBs = "2081-10-01",
                    EndDateBs = "2081-10-15",
                    StartDate = new DateOnly(2026, 7, 14),
                    EndDate = new DateOnly(2026, 8, 28),
                    StartTime = new TimeOnly(7, 0),
                    EndTime = new TimeOnly(10, 0),
                    IsActive = true,
                    ExamFee = 1500m
                };
                context.ExamSchedules.Add(examSchedule);
                await context.SaveChangesAsync();

                // Exam Fee
                if (!await context.ExamFees.AnyAsync())
                {
                    var examFee = new ExamFee
                    {
                        Name = "BBA SEM1 Regular Fee",
                        ExamScheduleId = examSchedule.Id,
                        Amount = 1500m
                    };
                    context.ExamFees.Add(examFee);
                    await context.SaveChangesAsync();
                }
            }
        }

        // ===== B.Sc. CSIT Subjects (Semesters 1-8) =====
        var csitProgram = await context.Programs.FirstOrDefaultAsync(p => p.ProgramCode == "BSCSIT");
        if (csitProgram != null)
        {
            // Ensure semesters 5-8 exist
            var runningYearCsit = await context.AcademicYears.FirstOrDefaultAsync(ay => ay.IsRunning);
            if (runningYearCsit != null && !await context.Semesters.AnyAsync(s => s.Number == 5))
            {
                var csitSemesters = new[]
                {
                    new Semester { Number = 5, Year = 3, Name = "Fifth Semester", Code = "SEM5", StartDate = new DateTime(2026, 9, 1), EndDate = new DateTime(2027, 1, 30), AcademicYearId = runningYearCsit.Id },
                    new Semester { Number = 6, Year = 3, Name = "Sixth Semester", Code = "SEM6", StartDate = new DateTime(2027, 2, 1), EndDate = new DateTime(2027, 6, 30), AcademicYearId = runningYearCsit.Id },
                    new Semester { Number = 7, Year = 4, Name = "Seventh Semester", Code = "SEM7", StartDate = new DateTime(2027, 9, 1), EndDate = new DateTime(2028, 1, 30), AcademicYearId = runningYearCsit.Id },
                    new Semester { Number = 8, Year = 4, Name = "Eighth Semester", Code = "SEM8", StartDate = new DateTime(2028, 2, 1), EndDate = new DateTime(2028, 6, 30), AcademicYearId = runningYearCsit.Id },
                };
                await context.Semesters.AddRangeAsync(csitSemesters);
                await context.SaveChangesAsync();
            }

            // Add CSIT subject catalogs
            var coreType = await context.SubjectTypes.FirstOrDefaultAsync(st => st.Code == "CORE");
            var electiveType = await context.SubjectTypes.FirstOrDefaultAsync(st => st.Code == "ELECTIVE");
            if (coreType != null && !await context.SubjectCatalogs.AnyAsync(sc => sc.SubjectCode == "CSIT111"))
            {
                var csitSubjects = new List<SubjectCatalog>
                {
                    // Semester I
                    new SubjectCatalog { SubjectCode = "CSIT111", SubjectName = "Introduction to Information Technology", ShortName = "Intro IT", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT112", SubjectName = "Digital Logic", ShortName = "Digital Logic", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT113", SubjectName = "Discrete Mathematics", ShortName = "Discrete Math", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT114", SubjectName = "C Programming", ShortName = "C Prog", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT115", SubjectName = "English I", ShortName = "English I", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    // Semester II
                    new SubjectCatalog { SubjectCode = "CSIT121", SubjectName = "Mathematics I", ShortName = "Math I", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT122", SubjectName = "Statistics I", ShortName = "Statistics", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT123", SubjectName = "Microprocessor and Assembly Language", ShortName = "Microprocessor", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT124", SubjectName = "Physics I", ShortName = "Physics", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT125", SubjectName = "Financial Accounting", ShortName = "Accounting", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    // Semester III
                    new SubjectCatalog { SubjectCode = "CSIT231", SubjectName = "Data Structures and Algorithms", ShortName = "DSA", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT232", SubjectName = "Object Oriented Programming (Java)", ShortName = "OOP Java", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT233", SubjectName = "Numerical Methods", ShortName = "Num Methods", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT234", SubjectName = "Web Technology", ShortName = "Web Tech", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT235", SubjectName = "Operating Systems", ShortName = "OS", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    // Semester IV
                    new SubjectCatalog { SubjectCode = "CSIT241", SubjectName = "Software Engineering", ShortName = "SW Engg", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT242", SubjectName = "Database Management Systems", ShortName = "DBMS", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT243", SubjectName = "Artificial Intelligence", ShortName = "AI", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT244", SubjectName = "Computer Architecture", ShortName = "Comp Arch", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT245", SubjectName = "Computer Graphics", ShortName = "CG", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    // Semester V
                    new SubjectCatalog { SubjectCode = "CSIT351", SubjectName = "Multimedia Computing", ShortName = "Multimedia", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT352", SubjectName = "Computer Networks", ShortName = "Networks", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT353", SubjectName = "Design and Analysis of Algorithms", ShortName = "DAA", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT354", SubjectName = "Cryptography", ShortName = "Cryptography", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT355", SubjectName = "Network Security (Elective)", ShortName = "Net Security", CreditHours = 3, SubjectTypeId = electiveType!.Id, IsActive = true },
                    // Semester VI
                    new SubjectCatalog { SubjectCode = "CSIT361", SubjectName = "Simulation and Modeling", ShortName = "Simulation", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT362", SubjectName = "Compiler Design and Construction", ShortName = "Compiler", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT363", SubjectName = "Theory of Computation", ShortName = "TOC", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT364", SubjectName = "E-Governance", ShortName = "E-Gov", CreditHours = 3, SubjectTypeId = electiveType!.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT365", SubjectName = "Fundamentals of Management", ShortName = "Mgmt", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    // Semester VII
                    new SubjectCatalog { SubjectCode = "CSIT471", SubjectName = "Advanced Java Programming", ShortName = "Adv Java", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT472", SubjectName = "Real Time Systems", ShortName = "Real Time", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT473", SubjectName = "Machine Learning", ShortName = "ML", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT474", SubjectName = "Advanced Database Management Systems", ShortName = "Adv DBMS", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT475", SubjectName = "Cloud Computing (Elective)", ShortName = "Cloud", CreditHours = 3, SubjectTypeId = electiveType!.Id, IsActive = true },
                    // Semester VIII
                    new SubjectCatalog { SubjectCode = "CSIT481", SubjectName = "Digital Image Processing", ShortName = "Image Proc", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT482", SubjectName = "Data Mining and Warehousing", ShortName = "Data Mining", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT483", SubjectName = "Project Work and Internship", ShortName = "Project", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT484", SubjectName = "Network and Cyber Security", ShortName = "Cyber Sec", CreditHours = 3, SubjectTypeId = coreType.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "CSIT485", SubjectName = "Distributed Systems (Elective)", ShortName = "Dist Sys", CreditHours = 3, SubjectTypeId = electiveType!.Id, IsActive = true },
                };
                await context.SubjectCatalogs.AddRangeAsync(csitSubjects);
                await context.SaveChangesAsync();
            }

            // Add CSIT subject offerings
            if (!await context.SubjectOfferings.AnyAsync(so => so.ProgramId == csitProgram.Id))
            {
                var allSemesters = await context.Semesters.ToListAsync();
                var allCsitSubjects = await context.SubjectCatalogs
                    .Where(sc => sc.SubjectCode.StartsWith("CSIT"))
                    .OrderBy(sc => sc.SubjectCode)
                    .ToListAsync();

                var semesterSubjectMap = new Dictionary<int, List<SubjectCatalog>>
                {
                    { 1, allCsitSubjects.Where(s => s.SubjectCode is "CSIT111" or "CSIT112" or "CSIT113" or "CSIT114" or "CSIT115").ToList() },
                    { 2, allCsitSubjects.Where(s => s.SubjectCode is "CSIT121" or "CSIT122" or "CSIT123" or "CSIT124" or "CSIT125").ToList() },
                    { 3, allCsitSubjects.Where(s => s.SubjectCode is "CSIT231" or "CSIT232" or "CSIT233" or "CSIT234" or "CSIT235").ToList() },
                    { 4, allCsitSubjects.Where(s => s.SubjectCode is "CSIT241" or "CSIT242" or "CSIT243" or "CSIT244" or "CSIT245").ToList() },
                    { 5, allCsitSubjects.Where(s => s.SubjectCode is "CSIT351" or "CSIT352" or "CSIT353" or "CSIT354" or "CSIT355").ToList() },
                    { 6, allCsitSubjects.Where(s => s.SubjectCode is "CSIT361" or "CSIT362" or "CSIT363" or "CSIT364" or "CSIT365").ToList() },
                    { 7, allCsitSubjects.Where(s => s.SubjectCode is "CSIT471" or "CSIT472" or "CSIT473" or "CSIT474" or "CSIT475").ToList() },
                    { 8, allCsitSubjects.Where(s => s.SubjectCode is "CSIT481" or "CSIT482" or "CSIT483" or "CSIT484" or "CSIT485").ToList() },
                };

                var csitSubjectsWithPractical = new HashSet<string>
                {
                    "CSIT114", "CSIT123", "CSIT231", "CSIT232", "CSIT234", "CSIT235",
                    "CSIT242", "CSIT245", "CSIT351", "CSIT352", "CSIT353",
                    "CSIT362", "CSIT471", "CSIT473", "CSIT481", "CSIT482",
                };

                var csitOfferings = new List<SubjectOffering>();
                foreach (var (semNum, semSubjects) in semesterSubjectMap)
                {
                    var semester = allSemesters.FirstOrDefault(s => s.Number == semNum);
                    if (semester == null) continue;

                    foreach (var (subj, idx) in semSubjects.Select((s, i) => (s, i)))
                    {
                        var hasPractical = csitSubjectsWithPractical.Contains(subj.SubjectCode);
                        csitOfferings.Add(new SubjectOffering
                        {
                            SubjectCatalogId = subj.Id,
                            ProgramId = csitProgram.Id,
                            SemesterId = semester.Id,
                            IsCompulsory = subj.SubjectTypeId == coreType!.Id,
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
                }

                await context.SubjectOfferings.AddRangeAsync(csitOfferings);
                await context.SaveChangesAsync();
            }
        }

        // ===== BIT Subjects (Semesters 1-8) =====
        var bitProg = await context.Programs.FirstOrDefaultAsync(p => p.ProgramCode == "BIT");
        if (bitProg != null)
        {
            var coreTypeBit = await context.SubjectTypes.FirstOrDefaultAsync(st => st.Code == "CORE");
            var electiveTypeBit = await context.SubjectTypes.FirstOrDefaultAsync(st => st.Code == "ELECTIVE");
            if (coreTypeBit != null && !await context.SubjectCatalogs.AnyAsync(sc => sc.SubjectCode == "BIT111"))
            {
                var bitSubjects = new List<SubjectCatalog>
                {
                    // Semester I
                    new SubjectCatalog { SubjectCode = "BIT111", SubjectName = "Introduction to Information Technology", ShortName = "Intro IT", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT112", SubjectName = "C Programming", ShortName = "C Prog", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT113", SubjectName = "Digital Logic", ShortName = "Digital Logic", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT114", SubjectName = "Discrete Mathematics", ShortName = "Discrete Math", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT115", SubjectName = "English I", ShortName = "English I", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    // Semester II
                    new SubjectCatalog { SubjectCode = "BIT121", SubjectName = "Mathematics I", ShortName = "Math I", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT122", SubjectName = "Statistics I", ShortName = "Statistics", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT123", SubjectName = "Microprocessor and Assembly Language", ShortName = "Microprocessor", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT124", SubjectName = "Object Oriented Programming (C++)", ShortName = "OOP C++", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT125", SubjectName = "Financial Accounting", ShortName = "Accounting", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    // Semester III
                    new SubjectCatalog { SubjectCode = "BIT231", SubjectName = "Data Structures and Algorithms", ShortName = "DSA", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT232", SubjectName = "Web Technology", ShortName = "Web Tech", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT233", SubjectName = "Numerical Methods", ShortName = "Num Methods", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT234", SubjectName = "Operating Systems", ShortName = "OS", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT235", SubjectName = "Database Management Systems", ShortName = "DBMS", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    // Semester IV
                    new SubjectCatalog { SubjectCode = "BIT241", SubjectName = "Software Engineering", ShortName = "SW Engg", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT242", SubjectName = "Computer Architecture", ShortName = "Comp Arch", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT243", SubjectName = "Computer Networks", ShortName = "Networks", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT244", SubjectName = "Artificial Intelligence", ShortName = "AI", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT245", SubjectName = "Computer Graphics", ShortName = "CG", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    // Semester V
                    new SubjectCatalog { SubjectCode = "BIT351", SubjectName = "Multimedia Computing", ShortName = "Multimedia", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT352", SubjectName = "Java Programming", ShortName = "Java", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT353", SubjectName = "Design and Analysis of Algorithms", ShortName = "DAA", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT354", SubjectName = "Cryptography", ShortName = "Cryptography", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT355", SubjectName = "Network Security (Elective)", ShortName = "Net Security", CreditHours = 3, SubjectTypeId = electiveTypeBit!.Id, IsActive = true },
                    // Semester VI
                    new SubjectCatalog { SubjectCode = "BIT361", SubjectName = "Simulation and Modeling", ShortName = "Simulation", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT362", SubjectName = "Compiler Design and Construction", ShortName = "Compiler", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT363", SubjectName = "E-Commerce", ShortName = "E-Commerce", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT364", SubjectName = "Fundamentals of Management", ShortName = "Mgmt", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT365", SubjectName = "Information Security (Elective)", ShortName = "Info Security", CreditHours = 3, SubjectTypeId = electiveTypeBit!.Id, IsActive = true },
                    // Semester VII
                    new SubjectCatalog { SubjectCode = "BIT471", SubjectName = "Advanced Java Programming", ShortName = "Adv Java", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT472", SubjectName = "Data Mining and Warehousing", ShortName = "Data Mining", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT473", SubjectName = "Machine Learning", ShortName = "ML", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT474", SubjectName = "Cloud Computing", ShortName = "Cloud", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT475", SubjectName = "Project Work I", ShortName = "Project I", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    // Semester VIII
                    new SubjectCatalog { SubjectCode = "BIT481", SubjectName = "Digital Image Processing", ShortName = "Image Proc", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT482", SubjectName = "Network and Cyber Security", ShortName = "Cyber Sec", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT483", SubjectName = "Project Work II / Internship", ShortName = "Project II", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT484", SubjectName = "Distributed Systems (Elective)", ShortName = "Dist Sys", CreditHours = 3, SubjectTypeId = electiveTypeBit!.Id, IsActive = true },
                    new SubjectCatalog { SubjectCode = "BIT485", SubjectName = "Mobile Application Development", ShortName = "Mobile Dev", CreditHours = 3, SubjectTypeId = coreTypeBit.Id, IsActive = true },
                };
                await context.SubjectCatalogs.AddRangeAsync(bitSubjects);
                await context.SaveChangesAsync();
            }

            if (!await context.SubjectOfferings.AnyAsync(so => so.ProgramId == bitProg.Id))
            {
                var allSemestersBit = await context.Semesters.ToListAsync();
                var allBitSubjects = await context.SubjectCatalogs
                    .Where(sc => sc.SubjectCode.StartsWith("BIT"))
                    .OrderBy(sc => sc.SubjectCode)
                    .ToListAsync();

                var semesterSubjectMapBit = new Dictionary<int, List<SubjectCatalog>>
                {
                    { 1, allBitSubjects.Where(s => s.SubjectCode is "BIT111" or "BIT112" or "BIT113" or "BIT114" or "BIT115").ToList() },
                    { 2, allBitSubjects.Where(s => s.SubjectCode is "BIT121" or "BIT122" or "BIT123" or "BIT124" or "BIT125").ToList() },
                    { 3, allBitSubjects.Where(s => s.SubjectCode is "BIT231" or "BIT232" or "BIT233" or "BIT234" or "BIT235").ToList() },
                    { 4, allBitSubjects.Where(s => s.SubjectCode is "BIT241" or "BIT242" or "BIT243" or "BIT244" or "BIT245").ToList() },
                    { 5, allBitSubjects.Where(s => s.SubjectCode is "BIT351" or "BIT352" or "BIT353" or "BIT354" or "BIT355").ToList() },
                    { 6, allBitSubjects.Where(s => s.SubjectCode is "BIT361" or "BIT362" or "BIT363" or "BIT364" or "BIT365").ToList() },
                    { 7, allBitSubjects.Where(s => s.SubjectCode is "BIT471" or "BIT472" or "BIT473" or "BIT474" or "BIT475").ToList() },
                    { 8, allBitSubjects.Where(s => s.SubjectCode is "BIT481" or "BIT482" or "BIT483" or "BIT484" or "BIT485").ToList() },
                };

                var bitSubjectsWithPractical = new HashSet<string>
                {
                    "BIT112", "BIT123", "BIT124", "BIT231", "BIT232", "BIT234",
                    "BIT235", "BIT243", "BIT245", "BIT351", "BIT352", "BIT353",
                    "BIT362", "BIT471", "BIT472", "BIT473", "BIT481", "BIT485",
                };

                var bitOfferings = new List<SubjectOffering>();
                foreach (var (semNum, semSubjects) in semesterSubjectMapBit)
                {
                    var semester = allSemestersBit.FirstOrDefault(s => s.Number == semNum);
                    if (semester == null) continue;

                    foreach (var (subj, idx) in semSubjects.Select((s, i) => (s, i)))
                    {
                        var hasPractical = bitSubjectsWithPractical.Contains(subj.SubjectCode);
                        bitOfferings.Add(new SubjectOffering
                        {
                            SubjectCatalogId = subj.Id,
ProgramId = bitProg.Id,
                            SemesterId = semester.Id,
                            IsCompulsory = subj.SubjectTypeId == coreTypeBit!.Id,
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
                }

                await context.SubjectOfferings.AddRangeAsync(bitOfferings);
                await context.SaveChangesAsync();
            }
        }

        // Demo Admit Cards
        if (!await context.AdmitCards.AnyAsync())
        {
            var examSchedule = await context.ExamSchedules.FirstOrDefaultAsync();
            var examRegistration = await context.ExamRegistrations.FirstOrDefaultAsync();
            var studentReg = await context.StudentRegistrations.FirstOrDefaultAsync();

            if (examSchedule != null && examRegistration != null)
            {
                var admitCards = new List<AdmitCard>
                {
                    new AdmitCard
                    {
                        ExamRegistrationId = examRegistration.Id,
                        ExamScheduleId = examSchedule.Id,
                        StudentRegistrationId = studentReg?.Id,
                        AdmitCardNumber = $"AC-{examSchedule.Id:D4}-{examRegistration.Id:D6}",
                        GeneratedDate = DateTime.UtcNow,
                        IsDownloaded = false,
                        IsActive = true
                    }
                };

                if (studentReg != null)
                {
                    var secondReg = await context.ExamRegistrations
                        .Where(er => er.Id != examRegistration.Id)
                        .FirstOrDefaultAsync();
                    if (secondReg != null)
                    {
                        admitCards.Add(new AdmitCard
                        {
                            ExamRegistrationId = secondReg.Id,
                            ExamScheduleId = examSchedule.Id,
                            StudentRegistrationId = studentReg.Id,
                            AdmitCardNumber = $"AC-{examSchedule.Id:D4}-{secondReg.Id:D6}",
                            GeneratedDate = DateTime.UtcNow,
                            IsDownloaded = true,
                            DownloadedDate = DateTime.UtcNow.AddDays(-1),
                            IsActive = true
                        });
                    }
                }

                await context.AdmitCards.AddRangeAsync(admitCards);
                await context.SaveChangesAsync();
            }
        }

        // Demo Retotal Requests
        if (!await context.RetotalRequests.AnyAsync())
        {
            var examSubjectResult = await context.ExamSubjectResults
                .Include(esr => esr.SubjectOffering)
                .FirstOrDefaultAsync();
            var studentReg = await context.StudentRegistrations.FirstOrDefaultAsync();
            var examRegistration = await context.ExamRegistrations.FirstOrDefaultAsync();

            if (examSubjectResult != null && studentReg != null && examRegistration != null)
            {
                var retotalRequests = new List<RetotalRequest>
                {
                    new RetotalRequest
                    {
                        ExamSubjectResultId = examSubjectResult.Id,
                        StudentRegistrationId = studentReg.Id,
                        ExamRegistrationId = examRegistration.Id,
                        RequestedDate = DateTime.UtcNow.AddDays(-5),
                        Reason = "Marks miscalculated - total does not match individual section scores",
                        Status = RetotalStatus.Pending,
                        OriginalGradeLetter = examSubjectResult.GradeLetter,
                        OriginalObtainedMarks = examSubjectResult.ObtainedMarks,
                        FeeAmount = 500m,
                        FeePaid = true,
                        IsActive = true
                    }
                };

                var secondResult = await context.ExamSubjectResults
                    .Where(esr => esr.Id != examSubjectResult.Id)
                    .FirstOrDefaultAsync();
                if (secondResult != null)
                {
                    retotalRequests.Add(new RetotalRequest
                    {
                        ExamSubjectResultId = secondResult.Id,
                        StudentRegistrationId = studentReg.Id,
                        ExamRegistrationId = examRegistration.Id,
                        RequestedDate = DateTime.UtcNow.AddDays(-3),
                        Reason = "Grade seems incorrect based on obtained marks",
                        Status = RetotalStatus.UnderReview,
                        OriginalGradeLetter = secondResult.GradeLetter,
                        OriginalObtainedMarks = secondResult.ObtainedMarks,
                        ReviewedByUsername = "admin",
                        ReviewedDate = DateTime.UtcNow.AddDays(-1),
                        FeeAmount = 500m,
                        FeePaid = true,
                        IsActive = true
                    });
                }

                await context.RetotalRequests.AddRangeAsync(retotalRequests);
                await context.SaveChangesAsync();
            }
        }
    }
}

