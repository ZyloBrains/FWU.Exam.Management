using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.ViewModels
{
    public class PrintAdmitCardViewModel
    {
        [Display(Name = "Academic Year")]
        public int? AcademicYearId { get; set; }

        [Display(Name = "College")]
        public int? CollegeId { get; set; }

        [Display(Name = "Exam Schedule")]
        public int? ExamScheduleId { get; set; }

        [Display(Name = "Program")]
        public int? ProgramsId { get; set; }

        [Display(Name = "Year Part")]
        public int? YearPartId { get; set; }

        [Display(Name = "Exam Type")]
        public int? ExamTypeId { get; set; }

        [Display(Name = "Applied By Students Only")]
        public bool AppliedByStudentsOnly { get; set; }

        public bool HasSearched { get; set; }
        public List<PrintAdmitCardResultViewModel> Results { get; set; } = [];

        public List<SelectListItem> AcademicYears { get; set; } = [];
        public List<SelectListItem> Colleges { get; set; } = [];
        public List<SelectListItem> ExamSchedules { get; set; } = [];
        public List<SelectListItem> Programs { get; set; } = [];
        public List<SelectListItem> YearParts { get; set; } = [];
        public List<SelectListItem> ExamTypes { get; set; } = [];
    }

    public class PrintAdmitCardResultViewModel
    {
        public int ExamRegistrationId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string ExamRollNumber { get; set; } = string.Empty;
        public string AcademicYearName { get; set; } = string.Empty;
        public string CollegeName { get; set; } = string.Empty;
        public string ExamScheduleName { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public string YearPartName { get; set; } = string.Empty;
        public string ExamTypeName { get; set; } = string.Empty;
        public bool? IsAppliedByStudent { get; set; }
        public bool IsActive { get; set; }
    }
}
