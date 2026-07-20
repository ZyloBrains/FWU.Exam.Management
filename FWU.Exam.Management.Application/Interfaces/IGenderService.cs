using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IGenderService
{
    Task<(List<Gender> Items, int TotalCount)> GetGendersAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<Gender?> GetGenderByIdAsync(int id);
    Task CreateGenderAsync(Gender gender);
    Task UpdateGenderAsync(Gender gender);
    Task DeleteGenderAsync(int id);
    Task<bool> GenderExistsAsync(int id);
}
