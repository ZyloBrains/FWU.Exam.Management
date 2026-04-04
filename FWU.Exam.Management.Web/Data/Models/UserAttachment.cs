using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class UserAttachment
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public string? FileName { get; set; }

    [Required, MaxLength(1024)]
    public string? FilePath { get; set; }

    [MaxLength(100)]
    public string? ContentType { get; set; }

    public long? FileSize { get; set; }
    public string? UploadedByUserId { get; set; }
    public DateTime UploadedDate { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public virtual AppUser? UploadedByUser { get; set; }
}
