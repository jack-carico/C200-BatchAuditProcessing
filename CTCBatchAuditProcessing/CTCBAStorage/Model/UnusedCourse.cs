using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CTCBAStorage.Model
{
    public class UnusedCourse
    {
        [Key]
        public int RecordId { get; set; }

        [StringLength(1)]
        public string CourseStatus { get; set; }        
        [StringLength(10)]
        public string CourseId { get; set; }
        [StringLength(200)]
        public string CourseTitle { get; set; }
        public decimal EnrolledCredits { get; set; }
        [StringLength(2)]
        public string LetterGrade { get; set; }
        [StringLength(8)]
        public string QuarterLabel { get; set; }
        [StringLength(4)]
        public string CTCYrq { get; set; }
    }
}
