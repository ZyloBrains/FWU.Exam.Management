using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class ProgramSeeder
{
    public static async Task SeedProgramsAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.Programs.AnyAsync())
            return;

        var undergradLevel = await context.Levels.FirstOrDefaultAsync(l => l.LevelCode == "1");
        var graduateLevel = await context.Levels.FirstOrDefaultAsync(l => l.LevelCode == "2");
        var mphilLevel = await context.Levels.FirstOrDefaultAsync(l => l.LevelCode == "3");
        var phdLevel = await context.Levels.FirstOrDefaultAsync(l => l.LevelCode == "4");

        if (undergradLevel == null || graduateLevel == null || mphilLevel == null || phdLevel == null)
            return;

        var programs = new[]
        {
            // Undergraduate (LevelCode "1")
            new Program { LevelId = undergradLevel.Id, FacultyId = 7, ProgramCode = "L008", ProgramName = "B.Sc. Computer Science and Information Technology", ShortName = "B.Sc. CSIT", Duration = 8, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 4, ProgramCode = "L011", ProgramName = "Bachelor of Arts", ShortName = "B.A.", Duration = 8, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 5, ProgramCode = "L115", ProgramName = "Bachelor of Arts, Bachelor of Laws", ShortName = "B.A., LL.B.", Duration = 10, GrandTotalMarks = 6500, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 1, ProgramCode = "L104", ProgramName = "Bachelor of Science in Agriculture", ShortName = "B.Sc. Ag", Duration = 8, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 6, ProgramCode = "L005", ProgramName = "Bachelor's Degree in Business Administration", ShortName = "BBA", Duration = 8, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 6, ProgramCode = "L113", ProgramName = "Bachelor's Degree in Business Studies", ShortName = "BBS", Duration = 8, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 6, ProgramCode = "L004", ProgramName = "Bachelor's Degree in Business Studies (Yearly)", ShortName = "BBS", Duration = 4, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 3, ProgramCode = "L092", ProgramName = "Bachelor's Degree in Civil Engineering", ShortName = "BCE", Duration = 8, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 3, ProgramCode = "L117", ProgramName = "Bachelor's Degree in Computer Engineering", ShortName = "BCT", Duration = 8, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 2, ProgramCode = "L013", ProgramName = "Bachelor's Degree in Education", ShortName = "B.Ed.", Duration = 8, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 7, ProgramCode = "L094", ProgramName = "Bachelor's Degree in Science", ShortName = "B. Sc", Duration = 8, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 8, ProgramCode = "L010", ProgramName = "Bachelor of Science in Medical Laboratory Technology", ShortName = "B.Sc. MLT", Duration = 4, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 9, ProgramCode = "L003", ProgramName = "B.Sc. Forestry", ShortName = "B.Sc. Forestry", Duration = 4, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 6, ProgramCode = "L110", ProgramName = "Bachelor's Degree in Hotel Management", ShortName = "BHM", Duration = 4, GrandTotalMarks = 0, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 3, ProgramCode = "L118", ProgramName = "Bachelors in Architecture", ShortName = "B. Arch.", Duration = 4, GrandTotalMarks = null, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 8, ProgramCode = "L014", ProgramName = "Bachelor of Public Health", ShortName = "BPH", Duration = 8, GrandTotalMarks = null, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 8, ProgramCode = "L015", ProgramName = "Bachelor of Science in Nursing Program", ShortName = "B.Sc. Nursing", Duration = 4, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 2, ProgramCode = "L154", ProgramName = "Professional Bachelor of Education (P.B.Ed.) Program", ShortName = "P.B.Ed.", Duration = 2, GrandTotalMarks = 1000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 4, ProgramCode = "L018", ProgramName = "Bachelor's Degree in Computer Application", ShortName = "BCA", Duration = 8, GrandTotalMarks = null, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = undergradLevel.Id, FacultyId = 7, ProgramCode = "L016", ProgramName = "Bachelor in Information Technology", ShortName = "BIT", Duration = 8, GrandTotalMarks = null, HasMultipleIntakes = false, IsActive = true },
            // Graduate (LevelCode "2")
            new Program { LevelId = graduateLevel.Id, FacultyId = 4, ProgramCode = "L126", ProgramName = "Master of Arts in Economics", ShortName = "MA Eco", Duration = 4, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = graduateLevel.Id, FacultyId = 4, ProgramCode = "L097", ProgramName = "Master of Arts in English", ShortName = "MA Eng", Duration = 4, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = graduateLevel.Id, FacultyId = 4, ProgramCode = "L009", ProgramName = "Master of Arts in Rural Development Studies", ShortName = "MA RD", Duration = 4, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = graduateLevel.Id, FacultyId = 4, ProgramCode = "L096", ProgramName = "Master of Arts in Sociology", ShortName = "MA Soc", Duration = 4, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = graduateLevel.Id, FacultyId = 6, ProgramCode = "L134", ProgramName = "Master of Business Administration", ShortName = "MBA", Duration = 4, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = graduateLevel.Id, FacultyId = 6, ProgramCode = "L093", ProgramName = "Master's Degree in Business Management", ShortName = "MBM", Duration = 4, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = graduateLevel.Id, FacultyId = 2, ProgramCode = "L012", ProgramName = "Master's Degree in Education", ShortName = "M.Ed.", Duration = 4, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = graduateLevel.Id, FacultyId = 4, ProgramCode = "L124", ProgramName = "Masters in Development Studies", ShortName = "MDS", Duration = 4, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = graduateLevel.Id, FacultyId = 6, ProgramCode = "L119", ProgramName = "Masters of Business Studies", ShortName = "MBS", Duration = 4, GrandTotalMarks = 4000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = graduateLevel.Id, FacultyId = 3, ProgramCode = "L131", ProgramName = "Master of Science (M.Sc.) in Construction Project Management", ShortName = "M.Sc.CPM", Duration = 4, GrandTotalMarks = 1600, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = graduateLevel.Id, FacultyId = 7, ProgramCode = "L108", ProgramName = "M.Sc. Computer Science and Information Technology", ShortName = "M.Sc. CSIT", Duration = 2, GrandTotalMarks = 2000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = graduateLevel.Id, FacultyId = 2, ProgramCode = "L153", ProgramName = "SNE Bridge Course", ShortName = "SNE.BC", Duration = 2, GrandTotalMarks = 200, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = graduateLevel.Id, FacultyId = 4, ProgramCode = "L098", ProgramName = "Master of Arts in Nepali", ShortName = "MA Nep", Duration = 4, GrandTotalMarks = null, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = graduateLevel.Id, FacultyId = 7, ProgramCode = "L095", ProgramName = "Master's Degree in Environmental Science and Management", ShortName = "M.Sc. ESM", Duration = 4, GrandTotalMarks = null, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = graduateLevel.Id, FacultyId = 4, ProgramCode = "L099", ProgramName = "Master in Public Administration and Leadership", ShortName = "MPAL", Duration = 2, GrandTotalMarks = null, HasMultipleIntakes = false, Remarks = "One Year Master Program", IsActive = true },
            new Program { LevelId = graduateLevel.Id, FacultyId = 1, ProgramCode = "L105", ProgramName = "M.Sc.Ag (Agronomy)", ShortName = "M.Sc.Ag", Duration = 2, GrandTotalMarks = null, HasMultipleIntakes = true, IsActive = true },
            // MPhil (LevelCode "3")
            new Program { LevelId = mphilLevel.Id, FacultyId = 6, ProgramCode = "L143", ProgramName = "Master of Philosophy in Management", ShortName = "Mphil", Duration = 4, GrandTotalMarks = 2000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = mphilLevel.Id, FacultyId = 2, ProgramCode = "L144", ProgramName = "Master of Philosophy in Nepali Education", ShortName = "M.phil.", Duration = 3, GrandTotalMarks = 2000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = mphilLevel.Id, FacultyId = 2, ProgramCode = "L149", ProgramName = "Master of Philosophy in Teaching English to the Speakers of Other Languages", ShortName = "M.Phil", Duration = 3, GrandTotalMarks = 1200, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = mphilLevel.Id, FacultyId = 2, ProgramCode = "L150", ProgramName = "Master of Philosophy in Curriculum, Planning and Leadership", ShortName = "M.Phil.", Duration = 3, GrandTotalMarks = 1200, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = mphilLevel.Id, FacultyId = 4, ProgramCode = "L151", ProgramName = "Master of Philosophy in Nepali", ShortName = "M.Phil.", Duration = 3, GrandTotalMarks = 1200, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = mphilLevel.Id, FacultyId = 4, ProgramCode = "L152", ProgramName = "Master of Philosophy in English", ShortName = "M.Phil.", Duration = 3, GrandTotalMarks = 1200, HasMultipleIntakes = false, IsActive = true },
            // Ph.D (LevelCode "4")
            new Program { LevelId = phdLevel.Id, FacultyId = 2, ProgramCode = "L146", ProgramName = "Ph.D", ShortName = "Ph.D", Duration = 4, GrandTotalMarks = 2000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = phdLevel.Id, FacultyId = 4, ProgramCode = "L147", ProgramName = "Ph.D", ShortName = "Ph.D", Duration = 4, GrandTotalMarks = 2000, HasMultipleIntakes = false, IsActive = true },
            new Program { LevelId = phdLevel.Id, FacultyId = 6, ProgramCode = "L148", ProgramName = "Ph.D", ShortName = "Ph.D", Duration = 4, GrandTotalMarks = 2000, HasMultipleIntakes = false, IsActive = true },
        };

        await context.Programs.AddRangeAsync(programs);
        await context.SaveChangesAsync();
    }
}
