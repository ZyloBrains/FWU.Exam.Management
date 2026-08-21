namespace FWU.Exam.Management.Application.DTOs;

public class SymbolNumberGenerationDto
{
    public int ExamScheduleId { get; set; }
    public string? ExamScheduleName { get; set; }
    public int ExamTypeId { get; set; }
    public string? Prefix { get; set; }
    public int SequenceWidth { get; set; }
    public long RemainingCapacity { get; set; }
    public bool NearCapacity { get; set; }
    public bool OverCapacity { get; set; }
    public int TotalRegistrations { get; set; }
    public int AssignedCount { get; set; }
    public int UnassignedCount { get; set; }
    public int NextStartSequence { get; set; }
    public List<SymbolBlockInfo> Blocks { get; set; } = [];
    public List<StudentSymbolInfo> Students { get; set; } = [];
}

public class SymbolBlockInfo
{
    public int? ProgramId { get; set; }
    public string? ProgramName { get; set; }
    public int CollegeId { get; set; }
    public string? CollegeName { get; set; }
    public int RegularCount { get; set; }
    public int SupplementaryCount { get; set; }
    public string? FromSymbol { get; set; }
    public string? ToSymbol { get; set; }
}

public class StudentSymbolInfo
{
    public int RegistrationId { get; set; }
    public string? SymbolNumber { get; set; }
    public string? StudentName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? ProgramName { get; set; }
    public string? CollegeName { get; set; }
    public bool IsSupplementary { get; set; }
}

public class DistributedStudentInfo
{
    public int RegistrationId { get; set; }
    public string? SymbolNumber { get; set; }
    public string? StudentName { get; set; }
    public string? CollegeName { get; set; }
    public bool IsSupplementary { get; set; }
    public int? ExamCenterId { get; set; }
    public string? ExamCenterCode { get; set; }
}
