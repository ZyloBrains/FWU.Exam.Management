BEGIN TRANSACTION;
CREATE TABLE [ExamScheduleCollegeApprovals] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [ExamScheduleId] int NOT NULL,
    [CollegeId] int NOT NULL,
    [Status] int NOT NULL,
    [RequestedApprovalDate] datetime2 NULL,
    [ApprovedDate] datetime2 NULL,
    [RejectedDate] datetime2 NULL,
    [ProposedDate] datetime2 NULL,
    [Remarks] nvarchar(500) NULL,
    [ApprovedByUserId] nvarchar(450) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_ExamScheduleCollegeApprovals] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExamScheduleCollegeApprovals_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamScheduleCollegeApprovals_ExamSchedules_ExamScheduleId] FOREIGN KEY ([ExamScheduleId]) REFERENCES [ExamSchedules] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamScheduleCollegeApprovals_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamScheduleCollegeApprovals_Users_ApprovedByUserId] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_ExamScheduleCollegeApprovals_ApprovedByUserId] ON [ExamScheduleCollegeApprovals] ([ApprovedByUserId]);

CREATE INDEX [IX_ExamScheduleCollegeApprovals_CollegeId] ON [ExamScheduleCollegeApprovals] ([CollegeId]);

CREATE UNIQUE INDEX [IX_ExamScheduleCollegeApprovals_ExamScheduleId_CollegeId] ON [ExamScheduleCollegeApprovals] ([ExamScheduleId], [CollegeId]);

CREATE INDEX [IX_ExamScheduleCollegeApprovals_TenantId] ON [ExamScheduleCollegeApprovals] ([TenantId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260807090550_AddExamScheduleCollegeApproval', N'10.0.7');

COMMIT;
GO

