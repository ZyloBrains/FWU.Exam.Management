using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Subjects;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Students;

public class StudentAdmission : IAuditable
{
    public int Id { get; set; }

    public int ProgramsId { get; set; }
    public int CollegeId { get; set; }
    public DateTime AdmissionDate { get; set; }
    public int? CheckedBy { get; set; }
    public bool IsCompleted { get; set; }

    public bool IsActive { get; set; }

    [MaxLength(50)]
    public string? CollegeRollNumber { get; set; }

    public bool HasFeeExemption { get; set; }

    public virtual Program? Program { get; set; }

    public virtual College? College { get; set; }
}
