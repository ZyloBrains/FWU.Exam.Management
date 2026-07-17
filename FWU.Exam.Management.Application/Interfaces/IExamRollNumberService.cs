namespace FWU.Exam.Management.Application.Interfaces;

public interface IExamRollNumberService
{
    Task<int> GenerateRollNumbersAsync(int examScheduleId);
    Task<int> ClearRollNumbersAsync(int examScheduleId);
    Task<bool> HasRollNumbersAsync(int examScheduleId);
}
