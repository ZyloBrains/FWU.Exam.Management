using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ISymbolNumberService
{
    Task<int> GetNextStartSequenceAsync(int examScheduleId, string? prefix = null);
    Task<SymbolNumberGenerationDto> GetOverviewAsync(int examScheduleId, int? startSequence = null, int? sequenceWidth = null, string? prefix = null);
    Task<SymbolNumberAssignmentResult> GenerateAsync(int examScheduleId, int? startSequence = null, int? sequenceWidth = null, string? prefix = null, int[]? academicYearIds = null);
    Task<string?> UpdateSymbolNumberAsync(int registrationId, string symbolNumber);
}
