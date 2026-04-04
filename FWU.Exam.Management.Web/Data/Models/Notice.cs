using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class Notice
{
    public int Id { get; set; }

    [Required, MaxLength(1024)]
    public string NoticeTitle { get; set; }

    [Required, MaxLength(1024)]
    public string NoticePreview { get; set; }

    public DateTime? PublishedDate { get; set; }

    [Required]
    public string NoticeContent { get; set; }
}
