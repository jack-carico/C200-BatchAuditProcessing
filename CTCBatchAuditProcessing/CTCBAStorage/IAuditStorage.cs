using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTCBAStorage
{
    public interface IAuditStorage
    {
        void PurgeAllStoredAudits();
        void MergeAudits(CTCBAParser.IBatchAudit[] auditsToMerge);
        /// <summary>
        /// Truncates the full AuditText filed of stored audits which are older than the cutoff date. 
        /// The summary details are left intact.
        /// </summary>
        /// <param name="cutoffDate"></param>
        void PruneAuditText();

    }
}
