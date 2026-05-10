namespace FWU.Exam.Management.Domain.Entities.Location;

public class Ward
{
    public int Id { get; set; }

    public int LocalLevelId { get; set; }

    public int WardNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual LocalLevel? LocalLevel { get; set; }
}
