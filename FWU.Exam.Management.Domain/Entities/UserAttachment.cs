using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class UserAttachment
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    [Display(Name = "File Name")]
    public string FileName { get; set; } = string.Empty;

    [Required, MaxLength(1024)]
    [Display(Name = "File Path")]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "Content Type")]
    public string? ContentType { get; set; }

    [Display(Name = "File Size")]
    public long? FileSize { get; set; }
    public string? UploadedByUserId { get; set; }
    public DateTime UploadedDate { get; set; }

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }
}
