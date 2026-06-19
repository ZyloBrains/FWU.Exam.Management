# Migration Guide

## Changes Made

### 1. Removed Redundant `College.FacultyId` / `College.Faculty`
- `College` entity had both `FacultyId`/`Faculty` (reference FK) and `ICollection<Faculty> Faculties` (M2M)
- The M2M is sufficient — a college offers multiple faculties
- Removed FK property and navigation from `College.cs`
- Updated `UserController.cs` and `UserSeeder.cs` to use M2M navigation

### 2. Added EMIS-Compliant `Department.FacultyId` FK
- `Department` now has `FacultyId` → `Faculty` (a department belongs to a faculty)
- `Faculty` now has `ICollection<Department> Departments`
- Configured with `Restrict` delete in `AppDbContext.OnModelCreating`
- All seeders updated to link departments to their parent faculty

**Mapping:**
| Department | Faculty |
|---|---|
| MGMT (Management) | FO-MGT (Faculty of Management) |
| SCI (Science) | FST (Faculty of Science & Technology) |
| EDU (Education) | EDU (Faculty of Education) |
| HUM (Humanities) | FO-HSS (Faculty of Humanities) |
| ENGG (Engineering) | ENG (Faculty of Engineering) |
| LAW (Law) | FOL (Faculty of Law) |
| AGR (Agriculture) | AGR (Faculty of Agriculture) |
| HSC (Health Sciences) | HSC (Faculty of Health Sciences) |
| NRM (Natural Resource Mgmt) | NRM (Faculty of NRM) |

## Correct EMIS Hierarchy

```
Faculty (academic + administrative body)
  ├── Department (stream) ── Program ── Level
  │                                    └── Semester
  └── College (M2M)
        └── CollegeProgram ── Program
```

## Migration Commands

**Create migration for all changes:**
```
dotnet ef migrations add AddDepartmentFacultyRelationship --project FWU.Exam.Management.Infrastructure --startup-project FWU.Exam.Management.Web
```

**Apply migrations to database:**
```
dotnet ef database update --project FWU.Exam.Management.Infrastructure --startup-project FWU.Exam.Management.Web
```

**Remove last migration (if not yet applied):**
```
dotnet ef migrations remove --project FWU.Exam.Management.Infrastructure --startup-project FWU.Exam.Management.Web
```
