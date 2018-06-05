using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTCBAParser.Impl
{
    internal class BatchAudit : IBatchAudit
    {
        internal BatchAudit()
        {
            _unusedCourses = new IUnusedCourse[0];
        }

        #region IBatchAudit Members

        public string StudentID
        {
            get;
            internal set;
        }

        public DateTime? AuditTimestamp
        {
            get;
            internal set;
        }

        public string ProgramCode
        {
            get;
            internal set;
        }

        public decimal? CreditsRequired
        {
            get;
            internal set;
        }

        public decimal? CreditsApplied
        {
            get;
            internal set;
        }

        public string DegreeTitle
        {
            get;
            internal set;
        }

        public string AuditText
        {
            get;
            internal set;
        }


        private IUnusedCourse[] _unusedCourses;
        public IUnusedCourse[] UnusedCourses
        {
            get
            {
                return _unusedCourses.ToArray();
            }
            internal set
            {
                _unusedCourses = value;
            }
        }

        #endregion
    }
}
