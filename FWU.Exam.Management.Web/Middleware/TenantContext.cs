using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;

namespace FWU.Exam.Management.Web.Middleware;

public class TenantContext : ITenantContext
{
    private int _tenantId;
    private string _tenantCode = string.Empty;
    private TenantType _type;
    private bool _resolved;
    private bool _isCollegeAdmin;
    private int? _collegeId;
    private IReadOnlyList<int> _collegeTenantIds = [];
    private readonly HashSet<Type> _ignoredFilters = [];

    public int TenantId => _tenantId;
    public string TenantCode => _tenantCode;
    public TenantType Type => _type;
    public bool IsCentralTenant => _type == TenantType.Central;
    public bool IsResolved => _resolved;
    public bool IsCollegeAdmin => _isCollegeAdmin;
    public int? CollegeId => _collegeId;
    public IReadOnlyList<int> CollegeTenantIds => _collegeTenantIds;

    public void SetTenant(int tenantId, string tenantCode, TenantType type)
    {
        _tenantId = tenantId;
        _tenantCode = tenantCode;
        _type = type;
        _resolved = true;
    }

    public void SetCollegeAdmin(bool isCollegeAdmin, int? collegeId, IReadOnlyList<int> collegeTenantIds)
    {
        _isCollegeAdmin = isCollegeAdmin;
        _collegeId = collegeId;
        _collegeTenantIds = collegeTenantIds;
    }

    public bool IsFilterIgnored(Type entityType) => _ignoredFilters.Contains(entityType);

    public IDisposable IgnoreFilter<T>() where T : class
    {
        _ignoredFilters.Add(typeof(T));
        return new DisposableAction(() => _ignoredFilters.Remove(typeof(T)));
    }

    private sealed class DisposableAction(Action action) : IDisposable
    {
        public void Dispose() => action();
    }
}
