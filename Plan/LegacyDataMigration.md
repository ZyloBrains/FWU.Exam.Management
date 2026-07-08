# Legacy Exam Data Migration Plan

## Overview

Migrate denormalized exam result data from 3 source tables into the normalized FUExams database schema.

| Source Table | ProgramCode | ProgramName | Level | AcademicYear | Rows |
|---|---|---|---|---|---|
| `dbo.CivilEngineering` | L092 | Bachelor's Degree in Civil Engineering | Undergraduate → Bachelor (ID=1) | 2014 | 43,615 |
| `dbo.ComputerEngineering` | L117 | Bachelor's Degree in Computer Engineering | Undergraduate → Bachelor (ID=1) | 2021 | 11,515 |
| `dbo.CPM` | L131 | M.Sc. in Construction Project Management | Graduate → Master (ID=2) | 2023 | 636 |

**Total: ~55,766 rows across 3 tables**

## Key Decisions

| Decision | Choice |
|---|---|
| TenantId | 1 (OCE) for all records |
| Level mapping | "Undergraduate" → existing Bachelor (ID=1), "Graduate" → existing Master (ID=2) |
| College | Create new: SCH001 / UNIVERSITY CENTRAL CAMPUS |
| Programs | Create new: L092, L117, L131 |
| Faculty | Map "Engineering" → existing Faculty with OfficeCode='ENG' |
| SubjectType | Create: COMP / Compulsory |
| StudentCategory | Create: Regular |
| Semester mapping | Year I Part I = Sem 1, Year I Part II = Sem 2, etc. |
| ExamTypes | Regular (Code=1) exists, Partial (Code=2) create if missing |
| Approach | Raw SQL script reading from source tables |

## Source Data Distinct Values

- **Years**: I, II, III
- **Parts**: I, II
- **ExamTypes**: Regular, Partial
- **SubjectTypes**: Comp (Compulsory)
- **CollegeCode**: SCH001 (all tables)
- **~32 unique subjects** in CivilEngineering alone

## Migration Steps

### Step 1: Create Reference/Lookup Data

1. **AcademicYears** — 2014, 2021, 2023
2. **College** — SCH001 / UNIVERSITY CENTRAL CAMPUS
3. **Programs** — L092, L117, L131
4. **ExamType** — Partial (Code=2) if not exists
5. **SubjectType** — COMP / Compulsory
6. **StudentCategory** — Regular
7. **Semesters** — Per Year+Part combination per program/AY

### Step 2: Create SubjectCatalogs

Distinct `SubjectCode + SubjectName + CreditHour` from all 3 source tables.

### Step 3: Create SubjectOfferings

Per `SubjectCatalog + Program + Semester` with marks configuration from source data:
- TheoryFullMarks, TheoryPassMarks
- PracticalFullMarks, PracticalPassMarks
- InternalTheoryFullMarks, InternalTheoryPassMarks
- IsCompulsory, HasTheory, HasPractical, HasInternal

### Step 4: Create ExamSchedules

Per `Program + AcademicYear + Semester`:
- ExamScheduleName constructed from program + year + part
- Links to AcademicYear, Program, Semester, ExamType, Level, College

### Step 5: Create ExamCenters

Per unique `ExamCenterName` per ExamSchedule.

### Step 6: Create StudentRegistrations

Deduplicated by `RegistrationNo` (e.g., `EG-2014-1-1-1438`):
- FirstName, MiddleName, LastName, ContactNumber, Email
- DateOfBirthAD, DateOfBirthBS
- GenderId (lookup by GenderName)
- CollegeId, LevelId, FacultyId, ProgramId, AcademicYearId
- DepartmentId (ENGG dept)

### Step 7: Create ExamRegistrations

Deduplicated by `ExamRegistrationID`:
- ExamRollNumber, ExamRollNumberCoding
- Sgpa, GradeLetter
- Links to AcademicYear, College, Program, ExamSchedule, ExamCenter, StudentRegistration

### Step 8: Create ExamSubjectResults

One per source row:
- ObtainedMarksTheory, ObtainedMarksPractical, ObtainedMarksTheoryInternal (as float)
- GradeLetter, Remarks
- Links to ExamRegistration, ExamType, SubjectOffering, ExamSchedule

## FK Resolution Strategy

Use temp tables with `IDENTITY` to capture new auto-generated IDs:
1. Insert lookup data, capture IDs in temp tables
2. Use temp table IDs when inserting dependent records
3. Use `MERGE ... OUTPUT` or `SELECT SCOPE_IDENTITY()` for FK resolution

## Verification

After migration:
1. Count records per table vs source
2. Spot-check specific students across all tables
3. Verify no orphan FK references
4. Check marks values match source data
