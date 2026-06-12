# ExamSchedule College Scoping - Options

## Problem
CollegeAdmin cannot see/create ExamSchedules scoped to their college.

## Current Architecture (Broken)

```
ExamSchedule (current - no CollegeId)
┌──────────────────┐
│ Id               │
│ ProgramId ───────┼──→ CollegePrograms (all colleges get all programs)
│ ...              │    └── CollegeId (DemoDataSeeder links ALL programs to ALL colleges)
└──────────────────┘     → Filter returns everything ── BUG
```

Because `DemoDataSeeder` links every program to every college via `CollegePrograms`, the `ProgramId`-based filtering in `ApplyScope()` returns ALL exam schedules regardless of which college they belong to. Also, when a CollegeAdmin creates an ExamSchedule, there is no `CollegeId` field to associate it with their college.

## Option A (Simple - No Migration)
Keep the current indirect filtering via `ProgramId → CollegePrograms`. Fix the seed data to only link relevant programs to each college. Limitation: CollegeAdmin-created ExamSchedules still cannot be scoped to their college since the entity has no `CollegeId`.

## Option B (Proper - Requires Migration) ✅ SELECTED
Add `CollegeId` to `ExamSchedule` entity and filter directly by it.

```
ExamSchedule (with CollegeId)
┌──────────────────┐
│ Id               │
│ CollegeId ───────┼──→ College (direct filter)
│ ProgramId        │      → Filter returns only this college's schedules ✓
│ ...              │
└──────────────────┘
```

### Changes Required
1. Add `CollegeId` (int?) and `College` navigation property to `ExamSchedule` entity
2. Update `ApplyScope()` in `ExamScheduleService` to filter by `CollegeId` directly
3. Update `Create` action to auto-set `CollegeId` from current user for CollegeAdmin
4. Add EF Core migration
5. Update seeder to set `CollegeId` on existing ExamSchedules
