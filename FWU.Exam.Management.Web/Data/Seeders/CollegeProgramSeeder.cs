using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class CollegeProgramSeeder
{
    public static async Task SeedCollegeProgramsAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.CollegePrograms.AnyAsync())
            return;

        var colleges = await context.Colleges.IgnoreQueryFilters()
            .ToDictionaryAsync(c => c.Code);
        var programs = await context.Programs
            .ToDictionaryAsync(p => p.ProgramCode!);

        var mappings = new (string CollegeCode, string ProgramCode)[]
        {
            ("SCH001", "L008"), ("SCH001", "L011"), ("SCH001", "L115"), ("SCH001", "L005"),
            ("SCH001", "L113"), ("SCH001", "L004"), ("SCH001", "L092"), ("SCH001", "L117"),
            ("SCH001", "L013"), ("SCH001", "L094"), ("SCH001", "L126"), ("SCH001", "L097"),
            ("SCH001", "L009"), ("SCH001", "L096"), ("SCH001", "L134"), ("SCH001", "L093"),
            ("SCH001", "L012"), ("SCH001", "L124"), ("SCH001", "L119"), ("SCH001", "L143"),
            ("SCH001", "L131"), ("SCH001", "L144"), ("SCH001", "L146"), ("SCH001", "L149"),
            ("SCH001", "L150"), ("SCH001", "L151"), ("SCH001", "L152"), ("SCH001", "L153"),
            ("SCH001", "L108"), ("SCH001", "L110"), ("SCH001", "L118"), ("SCH001", "L154"),
            ("SCH001", "L018"), ("SCH001", "L016"),

            ("SCH002", "L011"), ("SCH002", "L115"), ("SCH002", "L104"), ("SCH002", "L005"),
            ("SCH002", "L113"), ("SCH002", "L004"), ("SCH002", "L013"), ("SCH002", "L097"),
            ("SCH002", "L009"), ("SCH002", "L096"), ("SCH002", "L093"), ("SCH002", "L012"),
            ("SCH002", "L124"), ("SCH002", "L119"), ("SCH002", "L154"), ("SCH002", "L105"),

            ("SCH003", "L011"), ("SCH003", "L005"), ("SCH003", "L113"), ("SCH003", "L004"),
            ("SCH003", "L013"), ("SCH003", "L096"), ("SCH003", "L093"), ("SCH003", "L012"),
            ("SCH003", "L119"), ("SCH003", "L018"),

            ("SCH010", "L011"), ("SCH010", "L113"), ("SCH010", "L004"), ("SCH010", "L013"),

            ("SCH004", "L011"), ("SCH004", "L113"), ("SCH004", "L004"), ("SCH004", "L013"),

            ("SCH005", "L013"),

            ("SCH006", "L004"), ("SCH006", "L013"),

            ("SCH007", "L011"), ("SCH007", "L113"), ("SCH007", "L004"), ("SCH007", "L013"),
            ("SCH007", "L012"),

            ("SCH008", "L011"), ("SCH008", "L113"), ("SCH008", "L004"), ("SCH008", "L013"),
            ("SCH008", "L009"), ("SCH008", "L012"), ("SCH008", "L124"),

            ("SCH009", "L011"), ("SCH009", "L113"), ("SCH009", "L004"), ("SCH009", "L013"),
            ("SCH009", "L009"), ("SCH009", "L012"), ("SCH009", "L124"),

            ("SCH011", "L113"), ("SCH011", "L004"), ("SCH011", "L013"),

            ("SCH012", "L011"), ("SCH012", "L113"), ("SCH012", "L004"), ("SCH012", "L013"),
            ("SCH012", "L018"),

            ("SCH013", "L011"), ("SCH013", "L113"), ("SCH013", "L004"), ("SCH013", "L013"),
            ("SCH013", "L012"),

            ("SCH014", "L011"), ("SCH014", "L113"), ("SCH014", "L004"), ("SCH014", "L013"),
            ("SCH014", "L094"), ("SCH014", "L012"),

            ("SCH015", "L011"), ("SCH015", "L113"), ("SCH015", "L004"), ("SCH015", "L013"),
            ("SCH015", "L009"), ("SCH015", "L012"), ("SCH015", "L124"),

            ("SCH016", "L008"), ("SCH016", "L011"), ("SCH016", "L115"), ("SCH016", "L005"),
            ("SCH016", "L113"), ("SCH016", "L004"), ("SCH016", "L013"), ("SCH016", "L094"),
            ("SCH016", "L126"), ("SCH016", "L097"), ("SCH016", "L096"), ("SCH016", "L134"),
            ("SCH016", "L012"), ("SCH016", "L124"), ("SCH016", "L119"), ("SCH016", "L003"),
            ("SCH016", "L110"), ("SCH016", "L154"), ("SCH016", "L018"), ("SCH016", "L016"),

            ("SCH017", "L010"), ("SCH017", "L014"), ("SCH017", "L015"),

            ("SCH101", "L008"), ("SCH101", "L005"), ("SCH101", "L134"),

            ("SCH102", "L005"),

            ("SCH104", "L005"), ("SCH104", "L092"),

            ("SCH105", "L005"),

            ("SCH107", "L008"),

            ("SCH108", "L008"),

            ("SCH109", "L005"),

            ("SCH110", "L008"), ("SCH110", "L005"),

            ("SCH111", "L113"), ("SCH111", "L013"),

            ("SCH112", "L008"), ("SCH112", "L005"),

            ("SCH113", "L005"),

            ("SCH114", "L008"), ("SCH114", "L005"), ("SCH114", "L134"),

            ("SCH115", "L005"),

            ("SCH117", "L005"),

            ("SCH118", "L008"), ("SCH118", "L005"),

            ("SCH119", "L008"), ("SCH119", "L134"),

            ("SCH120", "L005"),

            ("SCH121", "L005"),

            ("SCH122", "L005"),

            ("SCH123", "L013"),

            ("SCH125", "L005"), ("SCH125", "L134"),

            ("SCH126", "L005"),

            ("SCH127", "L008"), ("SCH127", "L005"),

            ("SCH128", "L005"),

            ("SCH129", "L005"), ("SCH129", "L092"),

            ("SCH130", "L008"), ("SCH130", "L005"),

            ("SCH131", "L005"),

            ("SCH132", "L013"),

            ("SCH133", "L013"),

            ("SCH134", "L013"),

            ("SCH135", "L013"),

            ("SCH136", "L013"),
        };

        var collegePrograms = new List<CollegeProgram>();

        foreach (var (collegeCode, programCode) in mappings)
        {
            if (colleges.TryGetValue(collegeCode, out var college) &&
                programs.TryGetValue(programCode, out var program))
            {
                collegePrograms.Add(new CollegeProgram
                {
                    CollegeId = college.Id,
                    ProgramId = program.Id,
                    TenantId = college.TenantId,
                    IsActive = true,
                });
            }
        }

        context.CollegePrograms.AddRange(collegePrograms);
        await context.SaveChangesAsync();
    }
}
