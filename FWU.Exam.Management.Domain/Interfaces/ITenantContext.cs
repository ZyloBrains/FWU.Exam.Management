using FWU.Exam.Management.Domain.Enums;

namespace FWU.Exam.Management.Domain.Interfaces;

public interface ITenantContext
{
    int TenantId { get; }
    string TenantCode { get; }
    TenantType Type { get; }
    bool IsCentralTenant { get; }
    bool IsCollegeAdmin { get; }
    int? CollegeId { get; }
    IReadOnlyList<int> CollegeTenantIds { get; }
    bool IsFilterIgnored(Type entityType);
    IDisposable IgnoreFilter<T>() where T : class;
    void SetTenant(int tenantId, string tenantCode, TenantType type);
    void SetCollegeAdmin(bool isCollegeAdmin, int? collegeId, IReadOnlyList<int> collegeTenantIds);
}
