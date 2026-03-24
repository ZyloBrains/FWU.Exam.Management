using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.ViewModels
{
    public class RollNoGenerationSetupViewModel
    {
        [Display(Name = "Exam Schedule")]
        [Required(ErrorMessage = "Please select parent exam schedule.")]
        public int? ExamScheduleParentId { get; set; }

        public List<SelectListItem> ExamScheduleParents { get; set; } = [];
        public List<RollNoSetupItemViewModel> Setups { get; set; } = [];

        public string StatusMessage { get; set; } = string.Empty;
    }

    public class RollNoSetupItemViewModel
    {
        public int ExamRollNumberSetupId { get; set; }
        public int FirstExamRollNumber { get; set; }
        public string Prefix { get; set; } = string.Empty;
        public string Suffix { get; set; } = string.Empty;
        public int MinimumRollNumberLength { get; set; }
        public int Round { get; set; }
        public int MinimumGap { get; set; }
        public bool IsActive { get; set; }
    }
}
