using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTCBAParser.Impl
{
    internal class UnusedCourse : IUnusedCourse
    {
        #region IUnusedCourse Members

        public string CourseStatus
        {
            get;
            internal set;
        }

        public string CourseID
        {
            get;
            internal set;
        }

        public string CourseTitle
        {
            get;
            internal set;
        }

        public decimal EnrolledCredits
        {
            get;
            internal set;
        }

        public string LetterGrade
        {
            get;
            internal set;
        }

        public string QuarterLabel
        {
            get;
            internal set;
        }

        public string CTCYrq
        {
            get;
            internal set;
        }

        #endregion
    }
}
