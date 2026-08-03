# FWU Exam Management System - Class Diagram

## High-Level Class Diagram

```mermaid
classDiagram
    class ITenantScoped {
        <<interface>>
        +TenantId
    }

    class IAuditable {
        <<interface>>
        +CreatedBy
        +CreatedDate
        +UpdatedBy
        +UpdatedDate
    }

    class Tenant {
        +Id : Guid
        +Name : string
        +OfficeCode : string
        +ContactNumber
        +Address
        +Email
        +LogoPath
        +TenantType : TenantType
        +IsActive : bool
    }

    class AppUser {
        +Id : Guid
        +FullName : string
        +Email : string
        +ProfilePath
        +SignaturePath
        +Designation
        +IsActive : bool
        +FacultyId : Guid?
        +CollegeId : Guid?
    }

    class StudentRegistration {
        +Id : Guid
        +TenantId : Guid
        +RegistrationNumber : string
        +FirstName : string
        +MiddleName
        +LastName
        +NepaliName
        +ContactNumber
        +Email : string
        +DateOfBirthBS : string
        +DateOfBirthAD : DateTime
        +GenderId
        +BloodGroup
        +Nationality
        +Religion
        +EthnicityId
        +LevelId
        +DepartmentId
        +FacultyId
        +CollegeId
        +ProgramId
        +AcademicYearId
        +StudentCategoryId
        +PermanentAddressId
        +CurrentAddressId
        +IsActive : bool
        +EntranceRollNumber
        +IsRegistrationNumberGenerated : bool
    }

    class ExamSchedule {
        +Id : Guid
        +TenantId : Guid
        +ExamScheduleName : string
        +ExamScheduleCode
        +StartDateBs : string
        +EndDateBs : string
        +StartTime
        +EndTime
        +PublishedDate
        +ExtendedDate
        +ExtendedDateCharge
        +CollegeApprovalDate
        +AdmissionCardReleaseDate
        +AcademicYearId
        +ProgramId
        +SemesterId
        +ExamTypeId
        +LevelId
    }

    class EntranceExamApplication {
        +Id : Guid
        +TenantId : Guid
        +AcademicYearId
        +CollegeId
        +ProgramId
        +FirstName
        +LastName
        +Email
        +ContactNumber
        +Status : ApplicationStatus
        +CreatedAt
    }

    class College {
        +Id : Guid
        +Code : string
        +Name : string
        +CollegeNameNepali
        +ShortName
        +FacultyId
        +CollegeTypeId
        +AddressId
        +CollegeProfileId
        +IsExamCenterOnly : bool
        +IsActive : bool
    }

    class Faculty {
        +Id : Guid
        +Name : string
        +OfficeCode : string
        +ContactNumber
        +Address
        +Email
        +LogoPath
        +TenantId : Guid
    }

    class Program {
        +Id : Guid
        +ProgramCode : string
        +ProgramName : string
        +ShortName
        +Duration
        +LevelId
        +DepartmentId
        +BoardId
    }

    class AcademicYear {
        +Id : Guid
        +AcademicYearCode : string
        +AcademicYearName : string
        +AcademicYearNameNepali
        +IsRunning : bool
    }

    class ExamCenter {
        +Id : Guid
        +ExamScheduleId
        +CollegeId
        +Code
    }

    class ExamRegistration {
        +Id : Guid
        +ExamScheduleId
        +ExamCenterId
        +AcademicYearId
        +CollegeId
        +ExamRollNumber
        +FeeEnclosed
        +Status : RegistrationStatus
        +ProgramsId
        +ApplicationVoucherId
    }

    class ExamSubjectResult {
        +Id : Guid
        +TenantId : Guid
        +ExamRegistrationId
        +ExamTypeId
        +SubjectOfferingId
        +ExamScheduleId
        +FullMarksTheory
        +PassMarksTheory
        +ObtainedTheory
        +FullMarksPractical
        +PassMarksPractical
        +ObtainedPractical
        +FullMarksInternal
        +PassMarksInternal
        +ObtainedInternal
        +GradeLetter
        +IsSubmitted : bool
    }

    class PaymentRequestLog {
        +Id : Guid
        +InvoiceNumber : string
        +Amount : decimal
        +PaymentTypeId
        +StudentRegistrationId
        +ExamScheduleId
        +CollegeId
        +TransactionId
        +FullRequestContent
    }

    class Address {
        +Id : Guid
        +LocalLevelId
        +WardNumber
        +HouseNumber
        +ToleStreet
        +FullAddress
        +AddressType : AddressType
    }

    class District {
        +Id : Guid
        +ProvinceId
        +Code
        +Name
        +NameNepali
    }

    class Province {
        +Id : Guid
        +Code
        +Name
        +NameNepali
    }

    class Level {
        +Id : Guid
        +LevelCode
        +LevelName
        +LevelDisplayOrder
    }

    class SubjectCatalog {
        +Id : Guid
        +SubjectCode : string (unique)
        +SubjectName : string
        +ShortName
        +CreditHours
        +SubjectTypeId
    }

    class SubjectOffering {
        +Id : Guid
        +TenantId : Guid
        +SubjectCatalogId
        +ProgramId
        +SemesterId
        +IsCompulsory
        +HasTheory
        +HasPractical
        +HasInternal
        +FullMarksTheory
        +PassMarksTheory
        +FullMarksPractical
        +PassMarksPractical
        +FullMarksInternal
        +PassMarksInternal
    }

    class Semester {
        +Id : Guid
        +Number
        +Year
        +Name
        +Code
        +StartDate
        +EndDate
        +AcademicYearId
    }

    class StudentAdmission {
        +Id : Guid
        +TenantId : Guid
        +ProgramsId
        +CollegeId
        +AdmissionDate
        +IsCompleted : bool
        +CollegeRollNumber
        +AppUserId
        +HasFeeExemption : bool
    }

    class GradingScheme {
        +Id : Guid
        +Name
        +ProgramId
        +AcademicYearId
    }

    class GradeDefinition {
        +Id : Guid
        +GradingSchemeId
        +GradeLetter : string
        +MinPercentage
        +MaxPercentage
        +GradePoint
        +IsPass : bool
    }

    class ESewaConfiguration {
        +Id : Guid
        +MerchantCode
        +SecretKey
        +SuccessUrl
        +FailureUrl
        +IsActive
    }

    class KhaltiConfiguration {
        +Id : Guid
        +PublicKey
        +SecretKey
        +IsActive
    }

    Tenant --> "*" ITenantScoped : filters via global query filter

    StudentRegistration ..|> ITenantScoped
    StudentRegistration ..|> IAuditable
    ExamSchedule ..|> ITenantScoped
    EntranceExamApplication ..|> ITenantScoped
    EntranceExamApplication ..|> IAuditable
    ExamSubjectResult ..|> ITenantScoped
    ExamSubjectResult ..|> IAuditable
    SubjectOffering ..|> ITenantScoped
    StudentAdmission ..|> ITenantScoped
    StudentAdmission ..|> IAuditable

    AppUser ..|> IAuditable

    Province "1" --> "*" District
    District "1" --> "*" Address
    Address "1" --> "1" StudentRegistration : PermanentAddress
    Address "1" --> "1" StudentRegistration : CurrentAddress
    Address "1" --> "1" College

    Level "1" --> "*" Program
    Program "1" --> "*" SubjectOffering
    Program "1" --> "*" GradingScheme
    Program "1" --> "*" ExamSchedule

    Faculty "1" --> "*" College
    Faculty "1" --> "*" AppUser

    College "1" --> "*" StudentRegistration
    College "1" --> "*" ExamCenter
    College "1" --> "*" ExamRegistration
    College "1" --> "*" StudentAdmission
    College "1" --> "*" EntranceExamApplication
    College "1" --> "1" CollegeType

    AcademicYear "1" --> "*" StudentRegistration
    AcademicYear "1" --> "*" ExamSchedule
    AcademicYear "1" --> "*" Semester

    ExamSchedule "1" --> "*" ExamCenter
    ExamSchedule "1" --> "*" ExamRegistration
    ExamSchedule "1" --> "*" ExamSubjectResult
    ExamSchedule "1" --> "*" ExamFee
    ExamSchedule "1" --> "*" PaymentRequestLog

    StudentRegistration "1" --> "*" PaymentRequestLog
    StudentRegistration "1" --> "*" StudentAdmission
    StudentRegistration "1" --> "*" EntranceExamApplication

    Semester "1" --> "*" SubjectOffering
    Semester "1" --> "*" ExamSchedule

    SubjectCatalog "1" --> "*" SubjectOffering
    SubjectType "1" --> "*" SubjectCatalog

    GradingScheme "1" --> "*" GradeDefinition

    ESewaConfiguration --> PaymentType
    KhaltiConfiguration --> PaymentType
    PaymentRequestLog --> PaymentType
    PaymentRequestLog --> StudentRegistration
    PaymentRequestLog --> ExamSchedule
```

## Layer Architecture

```mermaid
classDiagram
    class Domain {
        <<FWU.Exam.Management.Domain>>
        +Entities/
        +Enums/
        +Interfaces/
    }
    class Application {
        <<FWU.Exam.Management.Application>>
        +DTOs/
        +Interfaces/ (Service Interfaces)
    }
    class Infrastructure {
        <<FWU.Exam.Management.Infrastructure>>
        +Data/AppDbContext
        +Data/Models/AppUser
        +Services/
        +Interceptor/
    }
    class Web {
        <<FWU.Exam.Management.Web>>
        +Controllers/
        +Areas/
        +Middleware/
        +ViewModels/
        +Views/
    }
    class Api {
        <<FWU.Exam.Management.Api>>
        (stub/empty)
    }

    Domain <-- Application : depends on
    Domain <-- Infrastructure : depends on
    Application <-- Infrastructure : depends on
    Application <-- Web : depends on
    Infrastructure <-- Web : depends on
    Domain <-- Api : depends on
    Application <-- Api : depends on
```

## Key Interfaces & Implementations

```mermaid
classDiagram
    class IStudentRegistrationService {
        <<interface>>
        +CreateStudentRegistrationAsync(dto)
        +UpdateStudentRegistrationAsync(id, dto)
        +GetAllStudentRegistrationsAsync()
        +GetPagedDataAsync(params)
        +GetSelectListDataAsync()
        +UpdateStatusAsync(id, status)
    }

    class StudentRegistrationService {
        +CreateStudentRegistrationAsync(dto)
        +UpdateStudentRegistrationAsync(id, dto)
        +GetAllStudentRegistrationsAsync()
        +GetPagedDataAsync(params)
        +GetSelectListDataAsync()
        +UpdateStatusAsync(id, status)
        -EnsureStudentAppUserAsync(email, name, facultyId, collegeId, dob)
    }

    class IEntranceExamApplicationService {
        <<interface>>
        +SubmitApplicationAsync(dto)
        +ReviewApplicationAsync(id, status)
        +GetPagedApplicationsAsync(params)
        +GetSelectListsAsync()
    }

    class EntranceExamApplicationService {
        +SubmitApplicationAsync(dto)
        +ReviewApplicationAsync(id, status)
        +GetPagedApplicationsAsync(params)
        +GetSelectListsAsync()
    }

    class IStudentDashboardService {
        <<interface>>
        +GetStudentRegistrationByEmailAsync(email)
        +GetExamSchedulesForStudentAsync(email)
        +GetExamFeeForScheduleAsync(scheduleId)
        +CreatePaymentRequestLogAsync(dto)
        +GetFailedSubjectOfferingIdsAsync(email, scheduleId)
        +GetResultRecordsAsync(email, scheduleId)
    }

    class StudentDashboardService {
        +GetStudentRegistrationByEmailAsync(email)
        +GetExamSchedulesForStudentAsync(email)
        +GetExamFeeForScheduleAsync(scheduleId)
        +CreatePaymentRequestLogAsync(dto)
        +GetFailedSubjectOfferingIdsAsync(email, scheduleId)
        +GetResultRecordsAsync(email, scheduleId)
    }

    class IESewaService {
        <<interface>>
        +GeneratePaymentFormData(amount, invoice)
        +VerifyTransactionAsync(refId)
        +VerifyResponseSignature(data)
    }

    class ESewaService {
        +GeneratePaymentFormData(amount, invoice)
        +VerifyTransactionAsync(refId)
        +VerifyResponseSignature(data)
    }

    class IKhaltiService {
        <<interface>>
        +InitiatePaymentAsync(amount, invoice, returnUrl)
        +LookupPaymentAsync(transactionId)
    }

    class KhaltiService {
        +InitiatePaymentAsync(amount, invoice, returnUrl)
        +LookupPaymentAsync(transactionId)
    }

    class ITenantContext {
        <<interface>>
        +CurrentTenant : Tenant
        +TenantId : Guid
    }

    class TenantContext {
        +CurrentTenant : Tenant
        +TenantId : Guid
    }

    class IAuditUserProvider {
        <<interface>>
        +GetCurrentUserName() : string
    }

    class HttpContextAuditUserProvider {
        +GetCurrentUserName() : string
    }

    class IFacultyResolver {
        <<interface>>
        +ResolveFacultyAsync(hostname) : Faculty
    }

    class FacultyResolver {
        +ResolveFacultyAsync(hostname) : Faculty
    }

    IStudentRegistrationService <|.. StudentRegistrationService
    IEntranceExamApplicationService <|.. EntranceExamApplicationService
    IStudentDashboardService <|.. StudentDashboardService
    IESewaService <|.. ESewaService
    IKhaltiService <|.. KhaltiService
    ITenantContext <|.. TenantContext
    IAuditUserProvider <|.. HttpContextAuditUserProvider
    IFacultyResolver <|.. FacultyResolver
```

## DbContext & Interceptors

```mermaid
classDiagram
    class AppDbContext {
        +DbSet~Tenant~ Tenants
        +DbSet~StudentRegistration~ StudentRegistrations
        +DbSet~ExamSchedule~ ExamSchedules
        +DbSet~EntranceExamApplication~ EntranceExamApplications
        +DbSet~College~ Colleges
        +DbSet~Faculty~ Faculties
        +DbSet~Program~ Programs
        +DbSet~SubjectCatalog~ SubjectCatalogs
        +DbSet~SubjectOffering~ SubjectOfferings
        +DbSet~PaymentRequestLog~ PaymentRequestLogs
        +DbSet~StudentAdmission~ StudentAdmissions
        +DbSet~ExamSubjectResult~ ExamSubjectResults
        +DbSet~ResultRecord~ ResultRecords (view)
        +50+ more DbSets...
        +OnModelCreating(ModelBuilder)
        +ConfigureTenantScopedEntity(ModelBuilder)
        +ConfigureAuditableEntity(ModelBuilder)
    }

    class TenantSaveChangesInterceptor {
        +TenantId : Guid
        +SavingChangesAsync(DbContextEventData, InterceptionResult)
        -SetTenantId(entries)
    }

    class AuditableSaveChangesInterceptor {
        +SavingChangesAsync(DbContextEventData, InterceptionResult)
        -SetAuditableFields(entries)
    }

    AppDbContext --> TenantSaveChangesInterceptor
    AppDbContext --> AuditableSaveChangesInterceptor
```
