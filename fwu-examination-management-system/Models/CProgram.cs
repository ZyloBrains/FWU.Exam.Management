namespace fwu_examination_management_system.Models
{
    public class CProgram
    {
        public int Id { get; set; }
        public int LevelId { get; set; }
        public int FacultyId { get; set; }
        public  string? ProgramName { get; set; }
        public int Duration { get; set; }
        public bool IsActive { get; set; }
    }
}
