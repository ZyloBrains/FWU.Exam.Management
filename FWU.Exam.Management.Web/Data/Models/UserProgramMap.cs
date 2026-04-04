namespace fwu_examination_management_system.Data.Models;

public class UserProgramMap
{
    public int Id { get; set; }

    public string? UserId { get; set; }
    public int ProgramId { get; set; }

    public virtual AppUser? User { get; set; }

    public virtual Program? Program { get; set; }
}
