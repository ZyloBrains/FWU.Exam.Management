# FWU Exam Management System

An exam management system built with ASP.NET Core, Entity Framework Core, and SQL Server. Supports multi-tenant architecture with college and faculty scoping, online exam registration, payment integrations (eSewa, Khalti, ConnectIPS), and result management.

## EMIS Hierarchy

```
Faculty (academic + administrative body)
  ├── Department (stream) ── Program ── Level
  │                                    └── Semester
  └── College (M2M via CollegeFaculty)
        └── CollegeProgram ── Program
```

- **Faculty** → **Department**: One-to-many (a faculty has many departments)
- **Faculty** ↔ **College**: Many-to-many (a faculty oversees many colleges, a college belongs to multiple faculties)
- **College** → **CollegeProgram** → **Program**: College offers Programs via CollegeProgram join table
- **Program** → **Level**: Each program has a level (Bachelor/Master)
- **Program** → **Department**: Each program belongs to a department
- **Semester** → **AcademicYear**: Semester belongs to an academic year

## Features

- **Multi-Tenant**: Each tenant (college) gets scoped data via global query filters
- **Faculty Scoping**: Subdomain-based faculty routing (e.g., `faculty.domain.com`)
- **Student Registration**: Registration with address, guardian, qualification details
- **Exam Management**: Exam schedules, centers, roll numbers, subject results
- **Online Payments**: eSewa, Khalti, ConnectIPS payment gateway integrations
- **Entrance Exam Applications**: Public application + admin review workflow
- **Role-Based Access**: Admin, FacultyAdmin, CollegeAdmin, Student roles with permissions
- **Result Processing**: Grade schemes, grade definitions, result records (view-based)
- **File Uploads**: Student documents, bank vouchers, college profiles

## Prerequisites

- .NET 10 SDK
- SQL Server
- dotnet-ef tool (`dotnet tool install --global dotnet-ef`)

## Run the Project

```bash
# From the solution root
cd FWU.Exam.Management.Web
dotnet run
```

Or from the solution root:

```bash
dotnet run --project FWU.Exam.Management.Web
```

The app uses `Properties/launchSettings.json` for startup configuration.