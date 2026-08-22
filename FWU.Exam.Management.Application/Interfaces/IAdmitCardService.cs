using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Exams;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IAdmitCardService
{
    Task<(List<AdmitCard> Items, int TotalCount)> GetAdmitCardsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? examScheduleId = null);
    Task<List<AdmitCard>> GetFilteredItemsAsync(string? search, int? examScheduleId = null);
    Task<AdmitCard?> GetAdmitCardByIdAsync(int id);
    Task<AdmitCard?> GetAdmitCardByIdForStudentAsync(int id, string userId, int studentRegistrationId);
    Task<List<AdmitCard>> GetAdmitCardsForPrintAsync(int? examScheduleId = null, string? search = null);
    Task CreateAdmitCardAsync(AdmitCard admitCard);
    Task UpdateAdmitCardAsync(AdmitCard admitCard);
    Task DeleteAdmitCardAsync(int id);
    Task<bool> AdmitCardExistsAsync(int id);
    Task<AdmitCard> GenerateAdmitCardAsync(int examRegistrationId);
    Task<List<AdmitCard>> GenerateBulkAdmitCardsAsync(int examScheduleId);
    Task<AdmitCardSelectListsDto> GetSelectListDataAsync(AdmitCard? admitCard = null);
}
