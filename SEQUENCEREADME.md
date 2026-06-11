# FWU Exam Management System - Sequence Diagrams

## 1. Multi-Tenant Request Flow

```mermaid
sequenceDiagram
    actor User
    participant Browser
    participant TenantResolutionMiddleware
    participant AppDbContext
    participant TenantContext
    participant Controller
    participant Service
    participant DB as Database

    User->>Browser: Navigate /tenant/{officeCode}/...
    Browser->>TenantResolutionMiddleware: HTTP Request
    TenantResolutionMiddleware->>TenantResolutionMiddleware: Extract tenant code from URL path
    TenantResolutionMiddleware->>AppDbContext: Query Tenant by OfficeCode
    AppDbContext->>DB: SELECT * FROM Tenants WHERE OfficeCode=@code
    DB-->>AppDbContext: Tenant entity
    AppDbContext-->>TenantResolutionMiddleware: Tenant
    TenantResolutionMiddleware->>TenantContext: Set CurrentTenant
    TenantResolutionMiddleware->>Browser: Continue pipeline
    Browser->>Controller: Route to action
    Controller->>Service: Call business logic
    Service->>DB: EF Core query (TenantId auto-filtered)
    DB-->>Service: Scoped results
    Service-->>Controller: Return data
    Controller-->>Browser: Render View
```

## 2. Student Registration Flow

```mermaid
sequenceDiagram
    actor Admin as Faculty/College Admin
    participant Web as StudentRegistrationsController
    participant Service as StudentRegistrationService
    participant AppDbContext
    participant DB as Database
    participant UserManager as UserManager~AppUser~

    Admin->>Web: GET /Students/Registrations/Create
    Web->>Service: GetSelectListDataAsync()
    Service->>DB: Load cascading selects (Province->District->LocalLevel, Level->Faculty->College->Program)
    DB-->>Service: Select lists
    Service-->>Web: ViewModel with dropdowns
    Web-->>Admin: Form (HTML)

    Admin->>Web: POST with form data
    Web->>Service: CreateStudentRegistrationAsync()
    Service->>AppDbContext: BeginTransactionAsync()
    Service->>AppDbContext: Create Address entity
    AppDbContext->>DB: INSERT Address
    Service->>AppDbContext: Create StudentRegistration
    AppDbContext->>DB: INSERT StudentRegistration
    Service->>Service: EnsureStudentAppUserAsync()
    Service->>UserManager: FindByEmailAsync(email)
    alt User exists
        UserManager-->>Service: AppUser
        Service->>UserManager: UpdateAsync (sync FullName/Faculty/College)
    else User not found
        Service->>UserManager: CreateAsync(AppUser, dobPassword)
        Service->>UserManager: AddToRoleAsync("Student")
        Service->>UserManager: ConfirmEmailAsync (auto)
    end
    Service->>AppDbContext: CommitTransactionAsync()
    Service-->>Web: Success result
    Web-->>Admin: Redirect to Index
```

## 3. Student Login Flow

```mermaid
sequenceDiagram
    actor Student
    participant LoginPage as /Identity/Account/Login
    participant SignInManager
    participant UserManager
    participant AppDbContext
    participant TenantContext
    participant DB as Database

    Student->>LoginPage: Navigate /tenant/{code}/Identity/Account/Login
    LoginPage-->>Student: Login form

    Student->>LoginPage: POST Email + Password (DateOfBirthBS)
    LoginPage->>SignInManager: PasswordSignInAsync(email, password, ...)
    SignInManager->>UserManager: FindByEmailAsync(email)
    UserManager->>AppDbContext: Query AppUser
    AppDbContext->>DB: SELECT User
    DB-->>AppDbContext: AppUser
    AppDbContext-->>UserManager: AppUser
    UserManager-->>SignInManager: AppUser
    SignInManager->>SignInManager: Verify hashed password (DOB relaxed rules)
    SignInManager-->>LoginPage: Success

    LoginPage->>LoginPage: Check user roles
    alt Has "Student" role
        LoginPage-->>Student: Redirect to /Dashboard
    else Has other roles
        LoginPage-->>Student: Redirect to area-specific dashboard
    end
```

## 4. Entrance Exam Application (Public) + Admin Review

```mermaid
sequenceDiagram
    actor Applicant as Public User
    actor Admin as FacultyAdmin
    participant EntranceController
    participant Service as EntranceExamApplicationService
    participant AppDbContext
    participant DB as Database

    Applicant->>EntranceController: GET /Exams/Entrance/Apply
    EntranceController->>Service: GetSelectListsAsync()
    Service->>DB: Load AcademicYears, Colleges, Programs
    DB-->>Service: Select options
    Service-->>EntranceController: SelectListsDto
    EntranceController-->>Applicant: Application Form

    Applicant->>EntranceController: POST form data
    EntranceController->>Service: SubmitApplicationAsync(dto)
    Service->>AppDbContext: Create EntranceExamApplication(Status=Submitted)
    AppDbContext->>DB: INSERT
    Service-->>EntranceController: Application ID
    EntranceController-->>Applicant: Success page

    Admin->>EntranceController: GET /Exams/Entrance (admin list)
    EntranceController->>Service: GetPagedApplicationsAsync()
    Service->>DB: SELECT applications
    DB-->>Service: Paged results
    Service-->>EntranceController: Application list
    EntranceController-->>Admin: Table with actions

    Admin->>EntranceController: POST MarkUnderReview / Approve / Reject
    EntranceController->>Service: ReviewApplicationAsync(id, status)
    Service->>AppDbContext: Update Status
    AppDbContext->>DB: UPDATE
    Service-->>EntranceController: Success
    EntranceController-->>Admin: Redirect to Index
```

## 5. Exam Registration & eSewa Payment Flow

```mermaid
sequenceDiagram
    actor Student
    participant Dashboard as StudentDashboardController
    participant Service as StudentDashboardService
    participant ESewaService
    participant AppDbContext
    participant DB as Database
    participant eSewaAPI as eSewa Gateway

    Student->>Dashboard: GET ExamForms
    Dashboard->>Service: GetExamSchedulesForStudentAsync(email)
    Service->>DB: Query StudentRegistration by email
    Service->>DB: Query ExamSchedules for student's program
    DB-->>Service: Available schedules
    Service-->>Dashboard: Exam schedule list
    Dashboard-->>Student: Available exam forms

    Student->>Dashboard: POST PayExamFee (scheduleId)
    Dashboard->>Service: GetExamFeeForScheduleAsync(scheduleId)
    Service->>DB: Load ExamFees
    DB-->>Service: Fee amounts
    Service-->>Dashboard: Fee details

    Dashboard->>Service: CreatePaymentRequestLogAsync()
    Service->>AppDbContext: INSERT PaymentRequestLog (Pending)
    Service-->>Dashboard: Invoice number

    alt eSewa Payment
        Dashboard->>ESewaService: GeneratePaymentFormData(amount, invoice)
        ESewaService-->>Dashboard: Form fields + signature
        Dashboard-->>Student: Auto-submit POST form to eSewa
        Student->>eSewaAPI: Redirect to eSewa payment page
        eSewaAPI-->>Student: eSewa UI
        Student->>eSewaAPI: Complete payment
        eSewaAPI->>Dashboard: GET callback?refId=xxx&oid=invoice
        Dashboard->>ESewaService: VerifyTransactionAsync(refId)
        ESewaService->>eSewaAPI: Transaction status lookup
        eSewaAPI-->>ESewaService: Verified
        ESewaService-->>Dashboard: Verification result
        Dashboard->>Service: Update payment status
        Service->>AppDbContext: UPDATE PaymentRequestLog (Success/Completed)
        Dashboard-->>Student: Success page
    end
```
