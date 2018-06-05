using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CTCBAStorage.Model
{
    public class StudentAudit
    {
        [Key]
        public int RecordId { get; set; }
        [StringLength(20)]
        public string StudentId { get; set; }
        public DateTime? AuditTimestamp { get; set; }
        [StringLength(10)]
        public string ProgramCode { get; set; }
        public decimal? CreditsRequired { get; set; }
        public decimal? CreditsApplied { get; set; }
        [StringLength(200)]
        public string DegreeTitle { get; set; }
        public string AuditText { get; set; }

        //related entities.
        public virtual List<UnusedCourse> MyUnusedCourses { get; set; }
    }
}
