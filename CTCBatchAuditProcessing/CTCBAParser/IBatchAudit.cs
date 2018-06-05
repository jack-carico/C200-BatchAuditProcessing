using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTCBAParser
{
    public interface IBatchAudit
    {
        string StudentID { get; }
        DateTime? AuditTimestamp { get; }
        string ProgramCode { get; }
        decimal? CreditsRequired { get; }
        decimal? CreditsApplied { get; }
        string DegreeTitle { get; }
        string AuditText { get; }

        IUnusedCourse[] UnusedCourses { get; }

    }
}
