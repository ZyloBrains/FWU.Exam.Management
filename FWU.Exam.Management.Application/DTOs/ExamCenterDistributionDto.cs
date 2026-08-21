using FWU.Exam.Management.Domain.Entities.Exams;

namespace FWU.Exam.Management.Application.DTOs;

public class ExamCenterDistributionDto
{
    public int ExamScheduleId { get; set; }
    public string? ExamScheduleName { get; set; }
    public int TotalRegistrations { get; set; }
    public int AssignedCount { get; set; }
    public int UnassignedCount { get; set; }
    public bool SymbolNumbersAssigned { get; set; }
    public bool RollNumbersAssigned { get; set; }
    public int RollNumberCount { get; set; }
    public List<CenterDistributionInfo> Centers { get; set; } = [];
    public List<DistributedStudentInfo> Students { get; set; } = [];
}

public class CenterDistributionInfo
{
    public int ExamCenterId { get; set; }
    public string? CenterCode { get; set; }
    public string? CollegeName { get; set; }
    public List<string> VenueColleges { get; set; } = [];
    public long? FromSymbolNumber { get; set; }
    public long? ToSymbolNumber { get; set; }
    public int StudentCount { get; set; }
    public List<string> SourceColleges { get; set; } = [];
}

public class SymbolNumberAssignmentResult
{
    public int TotalRegistrations { get; set; }
    public int Assigned { get; set; }
    public int Skipped { get; set; }
    public string? Message { get; set; }
}
