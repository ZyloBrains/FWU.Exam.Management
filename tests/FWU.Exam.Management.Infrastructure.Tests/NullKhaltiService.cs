using FWU.Exam.Management.Application.Interfaces;

namespace FWU.Exam.Management.Infrastructure.Tests;

/// <summary>
/// A no-op IKhaltiService used to satisfy the StudentDashboardService constructor in tests
/// that do not exercise Khalti payment initiation.
/// </summary>
public sealed class NullKhaltiService : IKhaltiService
{
    public Task<KhaltiInitiateResponse?> InitiatePaymentAsync(KhaltiInitiateRequest request)
        => Task.FromResult<KhaltiInitiateResponse?>(null);

    public Task<KhaltiLookupResponse?> LookupPaymentAsync(string pidx)
        => Task.FromResult<KhaltiLookupResponse?>(null);
}