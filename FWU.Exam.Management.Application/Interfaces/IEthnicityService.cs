using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IEthnicityService
{
    Task<(List<Ethnicity> Items, int TotalCount)> GetEthnicitiesAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<Ethnicity?> GetEthnicityByIdAsync(int id);
    Task CreateEthnicityAsync(Ethnicity ethnicity);
    Task UpdateEthnicityAsync(Ethnicity ethnicity);
    Task DeleteEthnicityAsync(int id);
    Task<bool> EthnicityExistsAsync(int id);
}
