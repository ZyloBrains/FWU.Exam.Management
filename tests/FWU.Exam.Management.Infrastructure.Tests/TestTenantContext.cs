using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class TestTenantContext : ITenantContext
{
    public int TenantId { get; private set; }
    public string TenantCode { get; private set; } = string.Empty;
    public TenantType Type { get; private set; }
    public bool IsCentralTenant => Type == TenantType.Central;
    public bool IsCollegeAdmin { get; private set; }
    public int? CollegeId { get; private set; }
    public IReadOnlyList<int> CollegeTenantIds { get; private set; } = [];

    public static TestTenantContext Standard(int tenantId = TestData.TenantId)
    {
        var ctx = new TestTenantContext();
        ctx.SetTenant(tenantId, "T1", TenantType.Standard);
        return ctx;
    }

    public static TestTenantContext Central()
    {
        var ctx = new TestTenantContext();
        ctx.SetTenant(0, "CENTRAL", TenantType.Central);
        return ctx;
    }

    public void SetTenant(int tenantId, string tenantCode, TenantType type)
    {
        TenantId = tenantId;
        TenantCode = tenantCode;
        Type = type;
    }

    public void SetCollegeAdmin(bool isCollegeAdmin, int? collegeId, IReadOnlyList<int> collegeTenantIds)
    {
        IsCollegeAdmin = isCollegeAdmin;
        CollegeId = collegeId;
        CollegeTenantIds = collegeTenantIds;
    }

    public bool IsFilterIgnored(Type entityType) => false;

    public IDisposable IgnoreFilter<T>() where T : class => new NoOpDisposable();

    private sealed class NoOpDisposable : IDisposable
    {
        public void Dispose() { }
    }
}
