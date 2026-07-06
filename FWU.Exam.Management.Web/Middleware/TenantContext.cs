using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;

namespace FWU.Exam.Management.Web.Middleware;

public class TenantContext : ITenantContext
{
    private int _tenantId;
    private string _tenantCode = string.Empty;
    private TenantType _type;
    private bool _resolved;
    private readonly HashSet<Type> _ignoredFilters = [];

    public int TenantId => _tenantId;
    public string TenantCode => _tenantCode;
    public TenantType Type => _type;
    public bool IsCentralTenant => _type == TenantType.Central;
    public bool IsResolved => _resolved;

    public void SetTenant(int tenantId, string tenantCode, TenantType type)
    {
        _tenantId = tenantId;
        _tenantCode = tenantCode;
        _type = type;
        _resolved = true;
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
