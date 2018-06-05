USE [CTCBAStorage]
GO

/****** Object:  View [dbo].[v_MostRecentAudit]    Script Date: 5/29/2014 8:56:38 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[v_MostRecentAudit]
AS
SELECT     t0.RecordId, t0.StudentId, t0.AuditTimestamp, t0.ProgramCode, t0.CreditsRequired, t0.CreditsApplied, t0.DegreeTitle
FROM         dbo.StudentAudits AS t0 INNER JOIN
                          (SELECT     MAX(AuditTimestamp) AS value, StudentId, ProgramCode
                            FROM          dbo.StudentAudits AS t1
                            GROUP BY StudentId, ProgramCode) AS t2 ON t0.StudentId = t2.StudentId AND t0.ProgramCode = t2.ProgramCode AND t0.AuditTimestamp = t2.value

GO




