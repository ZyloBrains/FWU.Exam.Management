using Microsoft.AspNetCore.Mvc.Rendering;

namespace fwu_examination_management_system.ViewModels
{
    public class InternalMarksEntryListViewModel
    {
        public int? AcademicYearId { get; set; }
        public int? CollegeId { get; set; }
        public int? LevelId { get; set; }
        public int? ProgramsId { get; set; }
        public int? YearPartId { get; set; }
        public int? SubjectDetailId { get; set; }

        public bool HasSearched { get; set; }
        public List<InternalMarksEntryResultViewModel> Results { get; set; } = [];

        public List<SelectListItem> AcademicYears { get; set; } = [];
        public List<SelectListItem> Colleges { get; set; } = [];
        public List<SelectListItem> Levels { get; set; } = [];
        public List<SelectListItem> Programs { get; set; } = [];
        public List<SelectListItem> YearParts { get; set; } = [];
        public List<SelectListItem> Subjects { get; set; } = [];
    }

    public class InternalMarksEntryResultViewModel
    {
        public int ExamSubjectRegistrationInternalId { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;
        public string CollegeName { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public string YearPartName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public decimal? ObtainedMarksTheoryInternal { get; set; }
        public decimal? ObtainedMarksPracticalInternal { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
