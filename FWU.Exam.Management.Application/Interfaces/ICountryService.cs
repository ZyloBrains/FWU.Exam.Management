using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ICountryService
{
    Task<List<Country>> GetAllAsync();
    Task<Country?> FindByNameAsync(string name);
    Task<Country> CreateAsync(string name);
    Task<(List<Country> Items, int TotalCount)> GetCountriesAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<Country?> GetCountryByIdAsync(int id);
    Task CreateCountryAsync(Country country);
    Task UpdateCountryAsync(Country country);
    Task DeleteCountryAsync(int id);
    Task<bool> CountryExistsAsync(int id);
}
