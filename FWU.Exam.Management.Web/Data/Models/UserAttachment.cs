using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models;

public class UserAttachment
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public string FileName { get; set; }

    [Required, MaxLength(1024)]
    public string FilePath { get; set; }

    [MaxLength(100)]
    public string ContentType { get; set; }

    public long? FileSize { get; set; }
    public string UploadedByUserId { get; set; }
    public DateTime UploadedDate { get; set; }

    [MaxLength(255)]
    public string Remarks { get; set; }

    [ForeignKey(nameof(UploadedByUserId))]
    [ValidateNever]
    public virtual AppUser UploadedByUser { get; set; }
}
