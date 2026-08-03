# Exam Fee Payment & Marksheet Flow

## 1. LOGIN
```
Student Login (Email / Password)
       |
       v
```

## 2. DASHBOARD -> Exam Forms
```
GET /Students/StudentDashboard/ExamForms
  For each ExamSchedule:
    +-- HasExistingPayment? -> Paid
    +-- No payment -> pending + fee
       |
       v
```

## 3. PAY EXAM FEE PAGE
```
GET /Students/StudentDashboard/PayExamFee?examScheduleId=X

  +--- GetFailedSubjectOfferingIdsAsync(userId, semesterId)
  |     StudentAdmission -> SemesterEnrollment -> ExamRegistrations
  |                                                  |
  |                                           ExamSubjectResults
  |                                           (GroupBy SubjectOfferingId)
  |                                                  |
  |                                           Latest per subject
  |                                                  |
  |                                     GradeLetter == F or NG?
  |                                                  |
  |          <--- return List<SubjectOfferingId> ----+
  |
  +--- failedIds.Count == 0               |    failedIds.Count > 0
       REGULAR                             |    NON-REGULAR
       Green banner                        |    Amber banner
       Compulsory subjects auto-selected   |    Only failed subjects pre-selected
       (disabled)                          |    (can modify)
       |                                   |
       +-------+---------------------------+
               |
     Student selects subjects (checkboxes)
               |
     Fee = ExamFee + Sum(PracticalFee per selected subject)
```

## 4. PAYMENT INITIATION
```
Direct (ProcessPayment):
  POST selectedSubjectIds, amount, paymentMethod
  +-- CreatePaymentRequestLog (saves SelectedSubjectIds)
  +-- CreateExamRegistrationAsync(scheduleId, userId, amount, subjectIds)
        +-- ExamRegistration record
        +-- ExamSubjectResult per subject

eSewa (async):
  POST -> CreatePaymentRequestLog (saves SelectedSubjectIds)
       -> Redirect to eSewa gateway
       -> User pays on eSewa
       -> Callback -> HandlePostPaymentRegistration(logId)
            +-- Reads SelectedSubjectIds from PaymentLog
            +-- CreateExamRegistrationAsync(...)

Khalti (async): Same as eSewa flow
```

## 5. MARKSHEET PAGE
```
GET /Students/StudentDashboard/Marksheet

  +-- GetResultRecordsAsync(registrationNumber)
  |     Returns: SGPA, Result(Pass/Fail), exam info
  |
  +-- GetStudentExamRegistrationsAsync(userId)
        For each ExamRegistration:
          +-- GradeLetter=A+..D -> PASS
          +-- GradeLetter=F/NG  -> FAIL
          +-- GradeLetter=null  -> PENDING

  DISPLAY:
  [4 Passed] [1 Failed] [0 Pending]
  Subject       Code    T   P  Grade  Status
  Intro IT    CSIT111  45   -   A+    PASS
  C Prog      CSIT114  15   8   F     FAIL
  English I   CSIT115  12   -   F     FAIL
  -----------------------------------------
  SGPA: 2.50              Result: [Pass]
```

## 6. REGULAR vs NON-REGULAR
```
GetFailedSubjectOfferingIdsAsync(userId, newSemesterId)
  Looks at latest ExamSubjectResult per subject
  in the PREVIOUS semester enrollment

  GradeLetter = F or NG  -> Non-Regular (retake needed)
  GradeLetter = A+..D    -> Regular (passed)
```

## SEED DATA (for Testing)
```
REGULAR:                              NON-REGULAR:
science.student@fwu.edu.np            partial.student@fwu.edu.np
Ram Sharma                            Hari Partial
B.Sc. CSIT, No failed subjects        CSIT114(C Prog)=Grade F, CSIT115(English)=Grade F
All 5 compulsory auto-selected        Only CSIT114+115 pre-selected
ExamFee=Rs1500, PracticalFee=Rs200    (can add other subjects)
Both passwords: Admin@123
```

## KEY FILES
```
Controller:   Areas/Students/Controllers/StudentDashboardController.cs
Service:      Infrastructure/Services/StudentDashboardService.cs
Interface:    Application/Interfaces/IStudentDashboardService.cs
Fee View:     Areas/Students/Views/StudentDashboard/PayExamFee.cshtml
Marksheet:    Areas/Students/Views/StudentDashboard/Marksheet.cshtml
ViewModels:   ViewModels/StudentProfileViewModel.cs
Seeder:       Data/Seeders/WorkflowTestDataSeeder.cs
Migration:    Migrations/20260624001904_AddSelectedSubjectIdsToPaymentRequestLog.cs
```
