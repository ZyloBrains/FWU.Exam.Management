using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Exams;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IHallTicketService
{
    Task<(List<HallTicket> Items, int TotalCount)> GetHallTicketsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? collegeId = null, int? facultyId = null, int? examScheduleId = null);
    Task<List<HallTicket>> GetFilteredItemsAsync(string? search, int? collegeId = null, int? facultyId = null);
    Task<HallTicket?> GetHallTicketByIdAsync(int id);
    Task CreateHallTicketAsync(HallTicket hallTicket);
    Task UpdateHallTicketAsync(HallTicket hallTicket);
    Task DeleteHallTicketAsync(int id);
    Task<bool> HallTicketExistsAsync(int id);
    Task<HallTicket> GenerateHallTicketAsync(int examRegistrationId);
    Task<List<HallTicket>> GenerateBulkHallTicketsAsync(int examScheduleId);
    HallTicketSelectListsDto GetSelectListData(HallTicket? hallTicket = null);
}
