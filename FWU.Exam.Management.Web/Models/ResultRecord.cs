using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ResultRecord
    {
        [Key]
        public int ResultRecordId { get; set; } 
        //public int ResultId { get; set; }

        public int AcademicYearId { get; set; }
        public int ProgramsId { get; set; }
        public int ExamTypeId { get; set; }
        public int CollegeId { get; set; }
        public int SubjectDetailId { get; set; }

        [Required, MaxLength(3)]
        public string Year { get; set; }

        [Required, MaxLength(2)]
        public string Part { get; set; }

        [MaxLength(50)]
        public string? RegistrationNumber { get; set; }

        [Required, MaxLength(50)]
        public string SymbolNumber { get; set; }

        [MaxLength(1)]
        public string? Alphabet { get; set; }

        [Required, MaxLength(10)]
        public string DateOfBirthBs { get; set; }

        [MaxLength(10)]
        public string? Sex { get; set; }

        [MaxLength(5)]
        public string? TheoryObtainedMarks { get; set; }

        [MaxLength(5)]
        public string? InternalObtainedMarks { get; set; }

        [MaxLength(5)]
        public string? PracticalObtainedMarks { get; set; }

        [MaxLength(5)]
        public string? TheoryObtainedGrade { get; set; }

        [MaxLength(5)]
        public string? InternalObtainedGrade { get; set; }

        [MaxLength(5)]
        public string? PracticalObtainedGrade { get; set; }

        [MaxLength(5)]
        public string? TotalObtainedMarks { get; set; }

        [MaxLength(5)]
        public string? TotalObtainedGrade { get; set; }

        [MaxLength(5)]
        public string? TotalGradePoints { get; set; }

        [MaxLength(4)]
        public string? Gpa { get; set; }

        [MaxLength(50)]
        public string? Result { get; set; }

        [MaxLength(255)]
        public string? StudentName { get; set; }

        public int ResultRecordMasterId { get; set; }
        public int? ExamScheduleId { get; set; }
        public DateTime? CreatedDate { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        [ValidateNever]
        public virtual AcademicYear AcademicYear { get; set; }

        [ForeignKey(nameof(ProgramsId))]
        [ValidateNever]
        public virtual Programs Program { get; set; }

        [ForeignKey(nameof(ExamTypeId))]
        [ValidateNever]
        public virtual ExamType ExamType { get; set; }

        [ForeignKey(nameof(CollegeId))]
        [ValidateNever]
        public virtual College College { get; set; }

        [ForeignKey(nameof(SubjectDetailId))]
        [ValidateNever]
        public virtual SubjectDetail SubjectDetail { get; set; }

        [ForeignKey(nameof(ExamScheduleId))]
        [ValidateNever]
        public virtual ExamSchedule ExamSchedule { get; set; }
    }
}
