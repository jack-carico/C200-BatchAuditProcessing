using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTCBAStorage.Impl
{
    internal class AuditStorage : IAuditStorage
    {
        private static IAuditStorage TheInstance;
        private const string TruncatedMessage = "removed by admin.";
        static AuditStorage()
        {
            TheInstance = new AuditStorage();
        }

        private readonly object SyncRoot;
        private AuditStorage()
        {
            this.SyncRoot = new object();
        }

        internal static IAuditStorage GetInterface()
        {
            return TheInstance;
        }


        #region IAuditStorage Members

        public void PurgeAllStoredAudits()
        {
            lock (SyncRoot)
            {
                using (var ctx = new Model.CTCBAStorageContext())
                {
                    ctx.Database.ExecuteSqlCommand(
                        "delete from UnusedCourses", new object[0]);
                    ctx.Database.ExecuteSqlCommand(
                        "delete from StudentAudits", new object[0]);
                    ctx.Database.ExecuteSqlCommand(
                        "DBCC CHECKIDENT (UnusedCourses, reseed, 0)",
                        new object[0]);
                    ctx.Database.ExecuteSqlCommand(
                        "DBCC CHECKIDENT (StudentAudits, reseed, 0)",
                        new object[0]);
                }
            }
        }

        public void MergeAudits(CTCBAParser.IBatchAudit[] auditsToMerge)
        {
            if (auditsToMerge == null || auditsToMerge.Length < 1) { return; } 

            lock (SyncRoot)
            {
                //get the min and max timestamp from the auditsToMerge set. 
                //use the min/max timestamp to retrieve records that might have a collision with the incoming data. 
                DateTime minTS, maxTS;
                DetermineMinMax(auditsToMerge, out minTS, out maxTS);

                using (var ctx = new Model.CTCBAStorageContext())
                {
                    var q_potential_colliders =
                        from x in ctx.StudentAudits
                        where x.AuditTimestamp <= maxTS && x.AuditTimestamp >= minTS
                        select new //only fields used for collision detection.
                        {
                            x.StudentId,
                            x.AuditTimestamp,
                            x.ProgramCode
                        };

                    //determine which audits to merge. 
                    var q_to_merge =
                        from a in auditsToMerge
                        join c in q_potential_colliders
                          on new { A = a.AuditTimestamp, B = a.ProgramCode, C = a.StudentID }
                          equals new { A = c.AuditTimestamp, B = c.ProgramCode, C = c.StudentId } into j_c
                        from c in j_c.DefaultIfEmpty()
                        select new
                        {
                            a,
                            Collided = c != null
                        };

                    Model.StudentAudit saEntity = null;
                    Model.UnusedCourse ucEntity = null;
                    Console.Write("Total audits in batch: ");
                    Console.WriteLine(auditsToMerge.Count().ToString());

                    var mergeBatch = q_to_merge.Where(z=>z.Collided == false);
                    Console.Write("Total audits to be merged: ");
                    Console.WriteLine(mergeBatch.Count().ToString());

                    //attempt to speedup saving to database. (Seems to work, JMC 2013-04-08)
                    ctx.Configuration.AutoDetectChangesEnabled = false;
                    ctx.Configuration.ValidateOnSaveEnabled = false;

                    foreach (var item in mergeBatch)
                    {
                        saEntity = new Model.StudentAudit()
                        {
                            AuditText = item.a.AuditText,
                            AuditTimestamp = item.a.AuditTimestamp,
                            CreditsApplied = item.a.CreditsApplied,
                            CreditsRequired = item.a.CreditsRequired,
                            DegreeTitle = item.a.DegreeTitle,
                            ProgramCode = item.a.ProgramCode,
                            StudentId = item.a.StudentID,
                            MyUnusedCourses = new List<Model.UnusedCourse>(),
                        };

                        foreach (var uc in item.a.UnusedCourses)
                        {
                            ucEntity = new Model.UnusedCourse()
                            {
                                CourseId = uc.CourseID,
                                CourseStatus = uc.CourseStatus,
                                CourseTitle = uc.CourseTitle,
                                CTCYrq = uc.CTCYrq,
                                EnrolledCredits = uc.EnrolledCredits,
                                LetterGrade = uc.LetterGrade,
                                QuarterLabel = uc.QuarterLabel,
                            };
                            saEntity.MyUnusedCourses.Add(ucEntity);
                        }

                        ctx.StudentAudits.Add(saEntity);
                    }
                    ctx.SaveChanges();
                }

            }
        }

        private void DetermineMinMax(CTCBAParser.IBatchAudit[] auditsToMerge, out DateTime minTS, out DateTime maxTS)
        {
            minTS = auditsToMerge.Min(z => z.AuditTimestamp).Value;
            maxTS = auditsToMerge.Max(z => z.AuditTimestamp).Value;
        }

        public void PruneAuditText()
        {
            lock (SyncRoot)
            {
                using (var ctx = new LINQSQL.CTCBAStorageDataContext(Properties.Settings.Default.ProductionStorageDatabase)
                                 {
                                     CommandTimeout = 1200
                                 })
                {
                    
                    string auditFiller = ".";

                    //get record IDs of the stored audits which are not the most recent and group by student. 
                    var q =
                       from a in ctx.Z_StudentAudits
                       where a.AuditText.Length > 1
                       join b in ctx.v_MostRecentAudits
                          on a.RecordId equals b.RecordId into j_b
                       from b in j_b.DefaultIfEmpty()
                       where b == null
                       select new
                       {
                           a.RecordId,
                           a.StudentId
                       };

                    var q2 =
                       from x in q.ToArray()
                       group x.RecordId by x.StudentId into g_x
                       select new
                       {
                           SID = g_x.Key,
                           RecordIds = g_x.ToList(),
                       };

                    foreach (var item in q2)
                    {
                        var q_update =
                           from sa in ctx.Z_StudentAudits
                           where item.RecordIds.Contains(sa.RecordId)
                           select sa;

                        foreach (var rec in q_update)
                        {
                            rec.AuditText = auditFiller;
                        }
                        ctx.SubmitChanges();
                    }
                }
            }
        }

        /* public void PruneAuditText()
        {
            string query =
@"
update StudentAudits
set StudentAudits.AuditText = '.'
FROM [StudentAudits] as [t2]
left JOIN
(
    SELECT 1 as [test], MAX([t0].[AuditTimestamp]) AS [MaxAuditTimestamp], [t0].[StudentId], [t0].[ProgramCode]
    FROM [StudentAudits] AS [t0]
    GROUP BY [t0].[StudentId], [t0].[ProgramCode]
) AS [t1]
ON t2.[AuditTimestamp] = t1.[MaxAuditTimestamp] AND t2.[StudentId] = t1.[StudentId] AND t2.[ProgramCode] = t1.[ProgramCode]
where t2.AuditText <> '.' AND t1.test IS NULL  
";
            lock (SyncRoot)
            {
                using (var ctx = new Model.CTCBAStorageContext())
                {
                    ctx.MyObjectContext.ObjectContext.CommandTimeout =
                        Properties.Settings.Default.PruneOperationConnectionTimeoutSeconds;                    
                    ctx.Database.ExecuteSqlCommand(query);
                }
            }
        } */

        #endregion
    }
}
