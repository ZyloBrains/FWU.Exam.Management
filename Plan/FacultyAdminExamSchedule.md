# Faculty Admin - Exam Schedule Workflow

Shortest end-to-end guide for a Faculty Admin to create and manage exam schedules.

## 1. Access & Account

- Role **FacultyAdmin** already includes every exam-schedule permission
  (`Permissions.cs`): view, create, edit, delete, approval resubmit.
- Create the account via **Users -> Create** (Super Admin only) or add to
  `UserSeeder.cs`:

| Field     | Value                 |
| --------- | --------------------- |
| Email     | eeo@fwu.edu.np        |
| Password  | *t0kv%#lm#uXG4        |
| Role      | FacultyAdmin          |
| Faculty   | e.g. L001             |

## 2. URL (faculty tenant)

```
/Exams/ExamSchedules
```

## 3. Workflow (short)

1. **Create** schedule (Index -> Create): academic year, program, semester,
   exam type, dates (BS auto-converts to AD), times, fees.
   - Set **College Approval Date** to open a Pending approval row for every
     college that offers the program.
2. **Details** -> assign **Batch** + per-subject **Exam Center / date / time**,
   then **Save All Subjects**.
3. **Edit** to adjust dates if colleges reject with a proposed date.
4. **Resubmit for College Approval** (Details page, shown when a college
   rejected): Pending/Rejected colleges are asked again; approved stay approved.
5. Track status in **Details -> College Approval Status** (Pending / Approved /
   Rejected per college).

## 4. Permissions used

- `examschedules.view`, `.create`, `.edit`, `.delete`
- `examapproval.resubmit`

## 5. Related actions

- Export list: CSV / Excel / PDF from Index.
- View registrations & payment summary: Details -> "View All Registered Students".
