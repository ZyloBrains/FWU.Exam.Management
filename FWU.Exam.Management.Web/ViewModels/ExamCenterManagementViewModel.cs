using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.ViewModels
{
    public class ExamCenterManagementViewModel
    {
        public int? ExamScheduleId { get; set; }
        public string StatusFilter { get; set; } = "All";

        public List<SelectListItem> ExamSchedules { get; set; } = [];
        public List<SelectListItem> StatusOptions { get; set; } = [];
        public List<ExamCenterManagementItemViewModel> Items { get; set; } = [];
    }

    public class ExamCenterManagementItemViewModel
    {
        public int ExamCenterId { get; set; }
        public string ExamScheduleName { get; set; } = string.Empty;
        public string ExamCenterCollege { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CreateExamCenterViewModel
    {
        [Required]
        [Display(Name = "Exam Schedule")]
        public int? ExamScheduleId { get; set; }

        [Required]
        [Display(Name = "Exam Center College")]
        public int? CollegeId { get; set; }

        [Display(Name = "Remarks")]
        [MaxLength(255)]
        public string Remarks { get; set; } = string.Empty;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public List<SelectListItem> ExamSchedules { get; set; } = [];
        public List<SelectListItem> Colleges { get; set; } = [];
    }

    public class ExamCenterDetailsListViewModel
    {
        public List<ExamCenterDetailItemViewModel> Items { get; set; } = [];
    }

    public class ExamCenterDetailItemViewModel
    {
        public string ExamScheduleName { get; set; } = string.Empty;
        public string ExamCenterCollege { get; set; } = string.Empty;
        public string College { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public long RollNumberFrom { get; set; }
        public long RollNumberTo { get; set; }
    }
}
