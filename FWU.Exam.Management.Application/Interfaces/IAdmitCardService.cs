using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Exams;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IAdmitCardService
{
    Task<(List<AdmitCard> Items, int TotalCount)> GetAdmitCardsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? collegeId = null, int? facultyId = null, int? examScheduleId = null);
    Task<List<AdmitCard>> GetFilteredItemsAsync(string? search, int? collegeId = null, int? facultyId = null);
    Task<AdmitCard?> GetAdmitCardByIdAsync(int id);
    Task CreateAdmitCardAsync(AdmitCard admitCard);
    Task UpdateAdmitCardAsync(AdmitCard admitCard);
    Task DeleteAdmitCardAsync(int id);
    Task<bool> AdmitCardExistsAsync(int id);
    Task<AdmitCard> GenerateAdmitCardAsync(int examRegistrationId);
    Task<List<AdmitCard>> GenerateBulkAdmitCardsAsync(int examScheduleId);
    AdmitCardSelectListsDto GetSelectListData(AdmitCard? admitCard = null);
}
