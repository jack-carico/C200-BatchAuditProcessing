using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace CTCBAStorage.Model
{
    internal class CTCBAStorageContext : DbContext
    {
        public CTCBAStorageContext()
            : base(Properties.Settings.Default.CTCBAStorageConnectionString)
        {
            
        }

        public System.Data.Entity.Infrastructure.IObjectContextAdapter MyObjectContext
        {
            get { return this; }    
        }

        public DbSet<StudentAudit> StudentAudits { get; set; }
        public DbSet<UnusedCourse> UnusedCourses { get; set; }
    }
}
