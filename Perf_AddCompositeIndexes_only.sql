BEGIN TRANSACTION;
DROP INDEX [IX_SemesterEnrollments_StudentAdmissionId] ON [SemesterEnrollments];

DROP INDEX [IX_PaymentRequestLogs_ExamScheduleId] ON [PaymentRequestLogs];

DROP INDEX [IX_ExamSubjectResults_ExamRegistrationId] ON [ExamSubjectResults];

DROP INDEX [IX_ExamSubjectResults_ExamScheduleId] ON [ExamSubjectResults];

DROP INDEX [IX_ExamRegistrations_ExamScheduleId] ON [ExamRegistrations];

DROP INDEX [IX_ExamRegistrations_SemesterEnrollmentId] ON [ExamRegistrations];

DROP INDEX [IX_CollegeAdminSubjectAssignments_SubjectOfferingId] ON [CollegeAdminSubjectAssignments];

CREATE INDEX [IX_SemesterEnrollments_StudentAdmissionId_EnrollmentStatus] ON [SemesterEnrollments] ([StudentAdmissionId], [EnrollmentStatus]);

CREATE INDEX [IX_SemesterEnrollments_StudentAdmissionId_SemesterInstanceId] ON [SemesterEnrollments] ([StudentAdmissionId], [SemesterInstanceId]);

CREATE INDEX [IX_PaymentRequestLogs_ExamScheduleId_StudentRegistrationId_PaymentRequestLogStatus] ON [PaymentRequestLogs] ([ExamScheduleId], [StudentRegistrationId], [PaymentRequestLogStatus]);

CREATE INDEX [IX_ExamSubjectResults_ExamRegistrationId_ExamScheduleId_IsActive] ON [ExamSubjectResults] ([ExamRegistrationId], [ExamScheduleId], [IsActive]) WHERE [ExamScheduleId] IS NOT NULL;

CREATE INDEX [IX_ExamSubjectResults_ExamScheduleId_SubjectOfferingId_IsActive] ON [ExamSubjectResults] ([ExamScheduleId], [SubjectOfferingId], [IsActive]) WHERE [ExamScheduleId] IS NOT NULL;

CREATE INDEX [IX_ExamRegistrations_ExamScheduleId_CollegeId_IsActive_Status] ON [ExamRegistrations] ([ExamScheduleId], [CollegeId], [IsActive], [Status]);

CREATE INDEX [IX_ExamRegistrations_ExamScheduleId_ProgramsId_IsActive] ON [ExamRegistrations] ([ExamScheduleId], [ProgramsId], [IsActive]);

CREATE INDEX [IX_ExamRegistrations_SemesterEnrollmentId] ON [ExamRegistrations] ([SemesterEnrollmentId]) WHERE [SemesterEnrollmentId] IS NOT NULL;

CREATE INDEX [IX_CollegeAdminSubjectAssignments_SubjectOfferingId_ExamScheduleId] ON [CollegeAdminSubjectAssignments] ([SubjectOfferingId], [ExamScheduleId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260915103739_Perf_AddCompositeIndexes', N'10.0.7');

COMMIT;
GO

