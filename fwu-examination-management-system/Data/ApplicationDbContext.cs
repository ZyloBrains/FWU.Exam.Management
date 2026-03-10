using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using fwu_examination_management_system.Models;

namespace fwu_examination_management_system.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Organization> Organizations { get; set; }
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public  DbSet<ActiveExamSchedule> ActiveExamSchedules { get; set; }
        public DbSet<ApplicationVoucher> ApplicationVouchers { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<BankVoucher> BankVouchers { get; set; }
        public DbSet<Batch> Batches { get; set; }
        public DbSet<BillTitle> BillTitles { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<College> Colleges { get; set; }
        public DbSet<ExamRegistration> ExamRegistrations { get; set; }
        public DbSet<ExamSchedule> ExamSchedules { get; set; }
        public DbSet<CProgram> CPrograms { get; set; }
        public DbSet<StudentRegistration> StudentRegistrations { get; set; }
        public DbSet<SubjectDetail> SubjectDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Removed problematic explicit primary key configuration:
            // The original line:
            //     builder.Entity<Program>().HasKey(p => p.Id);
            // failed to compile because the resolved `Program` type did not contain an `Id` member.
            // Let EF Core infer the key from the actual `Program` entity, or re-add an explicit key
            // using the correct property name (for example: HasKey(p => p.ProgramId)) if your model uses
            // a different key property.
            //
            // If you must keep an explicit configuration but cannot reference the CLR type here due to
            // ambiguity, you can use the string-based API:
            //     builder.Entity("Program").HasKey("Id");
            //
            // Only reintroduce the explicit key once you confirm the correct property name on the model.

            // Map Identity table names to the names used in migrations/snapshot
            //builder.Entity<Program>().ToTable("Program");
            builder.Entity<AppUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        }
    }
}
