# FUEMIS Legacy Data Migration Plan

## Purpose

Migrate normalized exam management data from the legacy **`FWUEMIS`** database into the new **`FUExamsDb`** database. Unlike the previous migration (`LegacyDataMigration.md`) which moved data from denormalized flat tables in `FWUExams.Legacy`, this migration sources from `FWUEMIS` which already has a structured, normalized schema (26+ tables).

## Source Database

**`FWUEMIS`** (LocalDB) - 26 core tables + `StudentAdmission` + `ExamSubjectAndMarksRegistration`

## Target Database

**`FUExamsDb`** (LocalDB) - Current app database (2 seeded Tenants, 7 Provinces, 77 Districts, 769 LocalLevels, 2 Countries)

## Data Volume

| Table | Rows |
|---|---|
| Organization | 1 |
| AcademicYear | 14 |
| Bank | 2 |
| Batch | 14 |
| Board | 8 |
| College | 53 |
| CollegeProgram | 194 |
| CollegeType | 2 |
| District | 77 (skip - already seeded) |
| EthnicGroup | 6 |
| ExamCenter | 0 |
| ExamRegistration | 248,797 |
| ExamSchedule | 98 |
| ExamScheduleDetail | 26 |
| ExamType | 5 |
| Faculty | 9 |
| Gender | 3 |
| Level | 4 |
| LocalLevel | 753 (map by name, skip unmatched) |
| PreviousLevel | 4 |
| Program | 45 |
| StudentAdmission | 46,860 |
| StudentQualification | 8,427 |
| StudentRegistration | 44,640 |
| SubjectDetail | 2,275 |
| SubjectType | 11 |
| ExamSubjectAndMarksRegistration | 1,084,096 |

## Key Decisions

### 1. Tenant Mapping
- `Organization` (Title1="Far Western University") maps to existing **Tenant Id=1** (Office of Controller of Examinations)
- All tenant-scoped entities in FUExamsDb will use **TenantId=1**
- Organization fields: Title1->Name, Title2->OfficeCode, Title3->Address (update existing Tenant Id=1)

### 2. Location Data (Already Seeded)
- **Provinces, Districts, LocalLevels, Countries**: Already seeded in FUExamsDb - DO NOT migrate
- Create `#DistrictMap` to map FWUEMIS DistrictId -> FUExamsDb District.Id by name
- 70 of 77 districts match exactly by name
- 7 districts need explicit name mapping:
  - Chitawan -> Chitwan
  - Dhanusa -> Dhanusha
  - Tanahu -> Tanahun
  - Nawalparasi East -> Nawalparasi West
  - Kapilbastu -> Kapilvastu
  - East Rukum -> Eastern Rukum
  - West Rukum -> Western Rukum
- LocalLevels mapped by name where possible (494 of 769 match), unmatched skipped

### 3. Source -> Target Table Mapping

| FWUEMIS Source | FUExamsDb Target | Notes |
|---|---|---|
| Organization | Tenants | Update existing Tenant Id=1 |
| AcademicYear | AcademicYears | Direct mapping |
| Bank | Banks | Direct mapping |
| Batch | Batches | Direct mapping |
| Board | Boards | Add CountryId FK (NULL) |
| College | Colleges | Add TenantId, map CollegeTypeId, map DistrictId via #DistrictMap |
| CollegeProgram | CollegePrograms | Add TenantId, map FKs |
| CollegeType | CollegeTypes | Direct mapping |
| EthnicGroup | Ethnicities | Rename field |
| ExamCenter | ExamCenters | Add TenantId, map ExamScheduleId + CollegeId |
| ExamRegistration | ExamRegistrations | Add TenantId, map all FKs |
| ExamSchedule | ExamSchedules | Add TenantId, map all FKs |
| ExamScheduleDetail | ExamSlots | Map to ExamSlots with SubjectOfferingId |
| ExamType | ExamTypes | Map Code from ExamTypeCode |
| Faculty | Faculties | Add TenantId |
| Gender | Genders | Direct mapping |
| Level | Levels | Direct mapping |
| PreviousLevel | PreviousLevels | Direct mapping |
| Program | Programs | Map LevelId, FacultyId |
| StudentAdmission | StudentAdmissions | Add TenantId, map FKs |
| StudentQualification | StudentQualifications | Add TenantId, map FKs |
| StudentRegistration | StudentRegistrations | Add TenantId, map all FKs |
| SubjectDetail | SubjectCatalogs + SubjectOfferings | Split into catalog + per-program offerings |
| SubjectType | SubjectTypes | Direct mapping |
| ExamSubjectAndMarksRegistration | ExamSubjectResults | Add TenantId, map all FKs |

### 4. SubjectDetail Split Strategy
FWUEMIS `SubjectDetail` combines catalog info and program-specific offering info in one table:
- **SubjectCatalogs**: Deduplicated by `SubjectCode` (SubjectCode, SubjectName, CreditHour, SubjectTypeId)
- **SubjectOfferings**: Per Subject + Program + Semester, with marks config (TheoryFullMark, PracticalFullMark, InternalFullMark, etc.)
- Year/Part -> Semester mapping: I/I->Sem1, I/II->Sem2, II/I->Sem3, II/II->Sem4, III/I->Sem5, III/II->Sem6, IV/I->Sem7, IV/II->Sem8

### 5. ExamSchedule Strategy
FWUEMIS `ExamSchedule` has LevelId and AcademicYearId but no ProgramId directly. Strategy:
- Create ExamSchedules from source ExamSchedule records (keep original IDs with IDENTITY_INSERT)
- Create ExamCenters per ExamSchedule (default center)
- Link ExamRegistration to ExamSchedule via source ExamScheduleID

### 6. FK Resolution Strategy
Use temporary ID mapping tables (`#TableMap`) to track source->target ID mappings across steps, following the pattern in `LegacyStudentData.sql`.

## Migration Steps

| Step | Description | Tables |
|---|---|---|
| 0 | Update Tenant Id=1 with Organization data | Tenants |
| 1 | Create temp mapping tables | #DistrictMap, #AcademicYearMap, #ProgramMap, etc. |
| 2 | Migrate reference data | Levels, Genders, CollegeTypes, PreviousLevels, SubjectTypes, ExamTypes, Banks, Boards, Ethnicities |
| 3 | Migrate AcademicYears | AcademicYears |
| 4 | Migrate Faculties | Faculties (add TenantId) |
| 5 | Migrate Colleges | Colleges (with CollegeType, District FKs via mapping) |
| 6 | Migrate Programs | Programs (map LevelId, FacultyId) |
| 7 | Migrate CollegePrograms | CollegePrograms |
| 8 | Migrate Batches | Batches |
| 9 | Migrate SubjectCatalogs + SubjectOfferings | Split from SubjectDetail |
| 10 | Migrate StudentRegistrations | StudentRegistrations |
| 11 | Migrate StudentAdmissions | StudentAdmissions |
| 12 | Migrate ExamSchedules + ExamCenters | ExamSchedules, ExamCenters |
| 13 | Migrate ExamRegistrations | ExamRegistrations (248K rows) |
| 14 | Migrate StudentQualifications | StudentQualifications |
| 15 | Migrate ExamSubjectResults | ExamSubjectResults (1M+ rows, set-based) |
| 16 | Migrate ExamSlots | ExamSlots (from ExamScheduleDetail, 26 rows) |
| 17 | Verification | Row counts, FK orphan checks, spot checks |

## Important Notes

1. **Run AFTER C# seeders** - The script assumes Tenant Id=1, Provinces, Districts, LocalLevels, Countries already exist.
2. **Source is FWUEMIS** - All source queries use `[FWUEMIS].dbo.TableName`.
3. **Location data already seeded** - Districts/Provinces/LocalLevels/Countries are NOT migrated, only mapped.
4. **ExamSubjectAndMarksRegistration is the largest table** (1M+ rows) - Uses set-based INSERT for performance.
5. **ExamCenter is empty** in source (0 rows) - Centers will be created per ExamSchedule.
6. **ExamScheduleDetail has only 26 rows** - Minimal exam slot data available.
7. **StudentRegistration has rich data** - Includes father/mother names, SLC/HSS/Bachelor education that can be used for StudentGuardians and StudentQualifications.
