namespace FWU.Exam.Management.Domain.Constants;

public static class TenantDefaults
{
    /// <summary>
    /// The Office of Controller of Examinations tenant. This is the row every
    /// SuperAdmin/central context runs as, and it is the tenant a faculty falls back to
    /// when the faculty has no tenant of its own.
    /// </summary>
    public const int CentralTenantId = 1;

    /// <summary>
    /// Resolves the tenant that owns a faculty.
    /// <para>
    /// A faculty may legitimately have no tenant: Faculty.TenantId is nullable and
    /// TenantSaveChangesInterceptor deliberately skips Faculty, so a faculty created
    /// without an explicit tenant keeps a null one. Those faculties are only visible to
    /// central users, whose context already is the central tenant, so falling back to it
    /// can never move a standard-tenant student sideways.
    /// </para>
    /// </summary>
    public static int Resolve(int? facultyTenantId) =>
        facultyTenantId ?? CentralTenantId;
}
