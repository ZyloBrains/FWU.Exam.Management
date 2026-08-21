-- ============================================================
-- Migration: AddIsSupplementaryToExamRegistrationAndSubjectResult
-- Date: 2026-08-20
-- Description: Adds IsSupplementary (bit, default false) column
--              to ExamRegistrations and ExamSubjectResults tables.
-- ============================================================

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('ExamRegistrations')
      AND name = 'IsSupplementary'
)
BEGIN
    ALTER TABLE [ExamRegistrations]
        ADD [IsSupplementary] BIT NOT NULL CONSTRAINT [DF_ExamRegistrations_IsSupplementary] DEFAULT 0;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('ExamSubjectResults')
      AND name = 'IsSupplementary'
)
BEGIN
    ALTER TABLE [ExamSubjectResults]
        ADD [IsSupplementary] BIT NOT NULL CONSTRAINT [DF_ExamSubjectResults_IsSupplementary] DEFAULT 0;
END
GO
