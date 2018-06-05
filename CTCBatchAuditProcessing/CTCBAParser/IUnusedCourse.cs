using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTCBAParser
{
    public interface IUnusedCourse
    {
        string CourseStatus { get; }
        string CourseID { get; }
        string CourseTitle { get; }
        decimal EnrolledCredits { get; }
        string LetterGrade { get; }
        string QuarterLabel { get; }
        string CTCYrq { get; }

    }
}
