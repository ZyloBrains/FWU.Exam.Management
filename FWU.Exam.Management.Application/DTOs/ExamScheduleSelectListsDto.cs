using System.Collections.Generic;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Subjects;

namespace FWU.Exam.Management.Application.DTOs;

public class ExamScheduleSelectListsDto
{
    public List<SelectOption> AcademicYears { get; set; } = [];
    public List<SelectOption> ExamTypes { get; set; } = [];
    public List<SelectOption> Programs { get; set; } = [];
    public List<SelectOption> Semesters { get; set; } = [];
}

public class ExamScheduleDetailsDto
{
    public ExamSchedule Schedule { get; set; } = null!;
    public int TotalRegistrations { get; set; }
    public int PaidCount { get; set; }
    public int PendingCount { get; set; }
    public int RegisteredCount { get; set; }
    public int PendingVerificationCount { get; set; }
    public List<ExamSlot> ExamSlots { get; set; } = [];
    public List<SubjectOffering> SubjectOfferings { get; set; } = [];
    public Dictionary<int, ExamSlot> ExistingSlotsByOfferingId { get; set; } = [];
    public List<SelectOption> ExamCenters { get; set; } = [];
    public List<SelectOption> Batches { get; set; } = [];
}

public class ExamSlotSaveResultDto
{
    public int Added { get; set; }
    public int Updated { get; set; }
    public List<string> Errors { get; set; } = [];
}
