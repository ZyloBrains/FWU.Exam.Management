using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class MarksheetDataSeeder
{
    public static async Task SeedMarksheetDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        var csitProgram = await context.Programs.FirstOrDefaultAsync(p => p.ProgramCode == "BSCSIT");
        var academicYear = await context.AcademicYears.FirstOrDefaultAsync(ay => ay.IsRunning);
        var regularExamType = await context.ExamTypes.FirstOrDefaultAsync(et => et.Code == "1");
        var semester8 = await context.Semesters.FirstOrDefaultAsync(s => s.Number == 8);
        var csitCollege = await context.Colleges.FirstOrDefaultAsync(c => c.Code == "CDC-CSIT");
        var fstFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "FST");
        var level = await context.Levels.FirstOrDefaultAsync(l => l.LevelCode == "BL");
        var genderMale = await context.Genders.FirstOrDefaultAsync(g => g.GenderName == "Male");
        var category = await context.StudentCategories.FirstOrDefaultAsync(sc => sc.StudentCategoryName == "Regular");
        var ethnicity = await context.Ethnicities.FirstOrDefaultAsync(e => e.EthnicityName == "Other");

        if (csitProgram == null || academicYear == null || regularExamType == null ||
            semester8 == null || csitCollege == null || level == null || genderMale == null ||
            category == null)
            return;

        // 1. Create Exam Schedule for BSCSIT Semester 8
        var examSchedule = await context.ExamSchedules.FirstOrDefaultAsync(es => es.ExamScheduleCode == "BSCSIT-SEM8-2024");
        if (examSchedule == null)
        {
            examSchedule = new ExamSchedule
            {
                TenantId = 1,
                ExamScheduleName = "Undergraduate Eighth Semester 2024",
                ExamScheduleCode = "BSCSIT-SEM8-2024",
                AcademicYearId = academicYear.Id,
                ProgramId = csitProgram.Id,
                SemesterId = semester8.Id,
                ExamTypeId = regularExamType.Id,
                CollegeId = csitCollege.Id,
                StartDateBs = "2081/09/15",
                EndDateBs = "2081/10/15",
                StartDate = new DateOnly(2025, 1, 15),
                EndDate = new DateOnly(2025, 2, 15),
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(11, 0),
                ExamFee = 1000m,
                IsActive = true,
                Remarks = "Regular examination for B.Sc. CSIT 8th Semester"
            };
            context.ExamSchedules.Add(examSchedule);
            await context.SaveChangesAsync();
        }

        // 2. Create or get Student Registration for Hari Krishna Gautam (linked to student@gmail.com)
        var studentReg = await context.StudentRegistrations.FirstOrDefaultAsync(sr => sr.Email == "student@gmail.com");
        if (studentReg == null)
        {
            studentReg = new StudentRegistration
            {
                FirstName = "Hari Krishna",
                LastName = "Gautam",
                Email = "student@gmail.com",
                RegistrationNumber = "SC-2021-1-1-0282",
                DateOfBirthBS = "2059-01-15",
                DateOfBirthAD = "2002-04-28",
                ContactNumber = "9841234580",
                GenderId = genderMale.Id,
                CollegeId = csitCollege.Id,
                FacultyId = fstFaculty?.Id,
                LevelId = level.Id,
                ProgramId = csitProgram.Id,
                StudentCategoryId = category.Id,
                EthnicityId = ethnicity?.Id,
                AcademicYearId = academicYear.Id,
                IsActive = true
            };
            context.StudentRegistrations.Add(studentReg);
            await context.SaveChangesAsync();
        }
        else
        {
            // Ensure RegistrationNumber is set on existing record
            if (string.IsNullOrEmpty(studentReg.RegistrationNumber))
            {
                studentReg.RegistrationNumber = "SC-2021-1-1-0282";
                studentReg.CollegeId = csitCollege.Id;
                studentReg.FacultyId = fstFaculty?.Id;
                studentReg.LevelId = level.Id;
                studentReg.ProgramId = csitProgram.Id;
                await context.SaveChangesAsync();
            }
        }

        // 3. Create Student Admission (linked to existing Identity user)
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "student@gmail.com");
        var existingUserId = existingUser?.Id;
        var existingAdmission = await context.StudentAdmissions.FirstOrDefaultAsync(sa => sa.AppUserId == existingUserId);
        if (existingAdmission == null)
        {
            var admission = new StudentAdmission
            {
                ProgramsId = csitProgram.Id,
                CollegeId = csitCollege.Id,
                AppUserId = existingUser?.Id,
                AdmissionDate = new DateTime(2021, 8, 15),
                IsActive = true
            };
            context.StudentAdmissions.Add(admission);
            await context.SaveChangesAsync();
        }
        else if (existingAdmission.ProgramsId != csitProgram.Id)
        {
            // Update existing admission to link to BSCSIT program
            existingAdmission.ProgramsId = csitProgram.Id;
            existingAdmission.CollegeId = csitCollege.Id;
            await context.SaveChangesAsync();
        }

        // 4. Create Exam Registration
        var examRegistration = await context.ExamRegistrations.FirstOrDefaultAsync(er => er.ExamScheduleId == examSchedule.Id && er.ProgramsId == csitProgram.Id);
        if (examRegistration == null)
        {
            examRegistration = new ExamRegistration
            {
                TenantId = 1,
                AcademicYearId = academicYear.Id,
                CollegeId = csitCollege.Id,
                ProgramsId = csitProgram.Id,
                ExamScheduleId = examSchedule.Id,
                ExamRollNumber = "8180117",
                SymbolNumber = "8180117",
                FeeEnclosed = 1000m,
                AttendancePercentage = 95m,
                RegistrationDate = DateTime.UtcNow.AddDays(-30),
                Status = RegistrationStatus.Registered,
                Sgpa = "3.45",
                IsActive = true,
                IsAppliedByStudent = true,
                VerifiedByUsername = "admin",
                VerifiedDate = DateTime.UtcNow.AddDays(-20)
            };
            context.ExamRegistrations.Add(examRegistration);
            await context.SaveChangesAsync();
        }

        // 5. Ensure Semester 8 Subject Catalogs and Offerings exist
        var coreType = await context.SubjectTypes.FirstOrDefaultAsync(st => st.Name == "Core");
        var electiveType = await context.SubjectTypes.FirstOrDefaultAsync(st => st.Name == "Elective");

        var sem8SubjectCodes = new[] { "CSIT481", "CSIT482", "CSIT483", "CSIT484", "CSIT485" };
        var sem8SubjectNames = new Dictionary<string, (string Name, string Short, bool IsElective)>
        {
            ["CSIT481"] = ("Digital Image Processing", "Image Proc", false),
            ["CSIT482"] = ("Data Mining and Warehousing", "Data Mining", false),
            ["CSIT483"] = ("Project Work and Internship", "Project", false),
            ["CSIT484"] = ("Network and Cyber Security", "Cyber Sec", false),
            ["CSIT485"] = ("Distributed Systems (Elective)", "Dist Sys", true),
        };

        var sem8CatalogIds = new Dictionary<string, int>();
        foreach (var code in sem8SubjectCodes)
        {
            var catalog = await context.SubjectCatalogs.FirstOrDefaultAsync(sc => sc.SubjectCode == code);
            if (catalog == null)
            {
                var info = sem8SubjectNames[code];
                catalog = new SubjectCatalog
                {
                    SubjectCode = code,
                    SubjectName = info.Name,
                    ShortName = info.Short,
                    CreditHours = 3,
                    SubjectTypeId = info.IsElective ? electiveType?.Id ?? coreType!.Id : coreType!.Id,
                    IsActive = true
                };
                context.SubjectCatalogs.Add(catalog);
                await context.SaveChangesAsync();
            }
            sem8CatalogIds[code] = catalog.Id;
        }

        var hasSem8Offerings = await context.SubjectOfferings.AnyAsync(so => so.ProgramId == csitProgram.Id && so.SemesterId == semester8.Id);
        if (!hasSem8Offerings)
        {
            var csitSubjectsWithPractical = new HashSet<string> { "CSIT481", "CSIT482" };
            var offerings = new List<SubjectOffering>();
            foreach (var (code, idx) in sem8SubjectCodes.Select((s, i) => (s, i)))
            {
                var hasPractical = csitSubjectsWithPractical.Contains(code);
                offerings.Add(new SubjectOffering
                {
                    SubjectCatalogId = sem8CatalogIds[code],
                    ProgramId = csitProgram.Id,
                    SemesterId = semester8.Id,
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
            await context.SubjectOfferings.AddRangeAsync(offerings);
            await context.SaveChangesAsync();
        }

        // 6. Get Semester 8 Subject Offerings for BSCSIT
        var semester8Offerings = await context.SubjectOfferings
            .Include(so => so.SubjectCatalog)
            .Where(so => so.ProgramId == csitProgram.Id && so.SemesterId == semester8.Id)
            .ToListAsync();

        if (semester8Offerings.Count == 0) return;

        // 7. Create Exam Subject Results with marks (skip if already exist)
        var existingResults = await context.ExamSubjectResults.AnyAsync(esr => esr.ExamRegistrationId == examRegistration.Id);
        if (!existingResults)
        {
            var random = new Random(42); // Fixed seed for reproducibility
            var examSubjectResults = new List<ExamSubjectResult>();

            foreach (var offering in semester8Offerings)
            {
                var theoryMax = offering.TheoryFullMarks;
                var practicalMax = offering.PracticalFullMarks ?? 0f;
                var internalMax = offering.InternalTheoryFullMarks ?? 40f;

                // Generate realistic marks
                var theoryMarks = (float)(random.NextDouble() * theoryMax * 0.4 + theoryMax * 0.5); // 50-90% of max
                var practicalMarks = offering.HasPractical == true
                    ? (float)(random.NextDouble() * practicalMax * 0.4 + practicalMax * 0.5)
                    : (float?)null;
                var internalMarks = (float)(random.NextDouble() * internalMax * 0.4 + internalMax * 0.5);

                var totalObtained = theoryMarks + (practicalMarks ?? 0) + internalMarks;
                var totalMax = theoryMax + practicalMax + internalMax;
                var percentage = (totalObtained / totalMax) * 100;

                // Determine grade based on percentage
                var gradeLetter = percentage switch
                {
                    >= 90 => "A+",
                    >= 80 => "A",
                    >= 70 => "B+",
                    >= 60 => "B",
                    >= 50 => "C+",
                    >= 45 => "C",
                    >= 40 => "D",
                    _ => "F"
                };

                var esr = new ExamSubjectResult
                {
                    TenantId = 1,
                    ExamRegistrationId = examRegistration.Id,
                    ExamTypeId = regularExamType.Id,
                    SubjectOfferingId = offering.Id,
                    ExamScheduleId = examSchedule.Id,
                    ObtainedMarksTheory = theoryMarks,
                    ObtainedMarksTheoryConfirm = theoryMarks,
                    ObtainedMarksPractical = practicalMarks,
                    ObtainedMarksPracticalConfirm = practicalMarks,
                    ObtainedMarksTheoryInternal = internalMarks,
                    GradeLetter = gradeLetter,
                    IsActive = true,
                    IsSubmitted = true,
                    IsTheoryRegistered = true,
                    IsPracticalRegistered = offering.HasPractical == true,
                    ObtainedMarks = totalObtained,
                    Remarks = null
                };

                examSubjectResults.Add(esr);
            }

            context.ExamSubjectResults.AddRange(examSubjectResults);
            await context.SaveChangesAsync();

            // 8. Calculate overall result
            var totalMarks = examSubjectResults.Sum(esr => esr.ObtainedMarks ?? 0);
            var allMaxMarks = semester8Offerings.Sum(so =>
                so.TheoryFullMarks + (so.PracticalFullMarks ?? 0f) + (so.InternalTheoryFullMarks ?? 40f));
            var overallPercentage = allMaxMarks > 0 ? (totalMarks / allMaxMarks) * 100 : 0;
            var overallResult = overallPercentage >= 40 ? "Pass" : "Fail";
            var sgpa = overallPercentage >= 40 ? "3.45" : null;

            // 9. Create Result Record (skip if already exists)
            var existingResultRecord = await context.ResultRecords.AnyAsync(rr => rr.ExamScheduleId == examSchedule.Id && rr.SymbolNumber == "8180117");
            if (!existingResultRecord)
            {
                var resultRecord = new ResultRecord
                {
                    TenantId = 1,
                    AcademicYearId = academicYear.Id,
                    ProgramsId = csitProgram.Id,
                    ExamTypeId = regularExamType.Id,
                    CollegeId = csitCollege.Id,
                    Year = "4",
                    Part = "II",
                    RegistrationNumber = "SC-2021-1-1-0282",
                    SymbolNumber = "8180117",
                    DateOfBirthBs = "2059/01/15",
                    Sex = "M",
                    StudentName = "HARI KRISHNA GAUTAM",
                    TheoryObtainedMarks = examSubjectResults.Average(esr => esr.ObtainedMarksTheory ?? 0).ToString("F1"),
                    InternalObtainedMarks = examSubjectResults.Average(esr => esr.ObtainedMarksTheoryInternal ?? 0).ToString("F1"),
                    PracticalObtainedMarks = examSubjectResults.Where(esr => esr.ObtainedMarksPractical.HasValue)
                        .Average(esr => esr.ObtainedMarksPractical ?? 0).ToString("F1"),
                    TotalObtainedMarks = totalMarks.ToString("F1"),
                    TotalObtainedGrade = overallPercentage >= 90 ? "A+" :
                        overallPercentage >= 80 ? "A" :
                        overallPercentage >= 70 ? "B+" :
                        overallPercentage >= 60 ? "B" :
                        overallPercentage >= 50 ? "C+" :
                        overallPercentage >= 45 ? "C" :
                        overallPercentage >= 40 ? "D" : "F",
                    Gpa = sgpa,
                    Result = overallResult,
                    ResultRecordMasterId = 1,
                    ExamScheduleId = examSchedule.Id,
                    CreatedDate = DateTime.UtcNow
                };

                context.ResultRecords.Add(resultRecord);
                await context.SaveChangesAsync();
            }
        }
    }
}
