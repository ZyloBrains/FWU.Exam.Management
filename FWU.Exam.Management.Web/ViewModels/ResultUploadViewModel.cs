using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.ViewModels
{
    public class ResultUploadViewModel
    {
        [Display(Name = "Exam Schedule")]
        [Required(ErrorMessage = "Exam Schedule is required.")]
        public int? ExamScheduleId { get; set; }

        [Display(Name = "Remarks")]
        [Required(ErrorMessage = "Remarks is required.")]
        [MaxLength(255)]
        public string Remarks { get; set; } = string.Empty;

        [Display(Name = "File")]
        [Required(ErrorMessage = "Please choose a CSV file.")]
        public IFormFile? UploadFile { get; set; }

        public List<SelectListItem> ExamSchedules { get; set; } = [];

        public bool HasSubmitted { get; set; }
        public int UploadedCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}
