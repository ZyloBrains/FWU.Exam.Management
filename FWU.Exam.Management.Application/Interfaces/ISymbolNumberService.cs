using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ISymbolNumberService
{
    Task<int> GetNextStartSequenceAsync(int examScheduleId);
    Task<SymbolNumberGenerationDto> GetOverviewAsync(int examScheduleId, int? startSequence = null, int? sequenceWidth = null);
    Task<SymbolNumberAssignmentResult> GenerateAsync(int examScheduleId, int? startSequence = null, int? sequenceWidth = null);
    Task<string?> UpdateSymbolNumberAsync(int registrationId, string symbolNumber);
}
