using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            TestPruneMethod(new DateTime(2013, 2, 1));
        }


        static void Test1(string[] args)
        {
            string endOfAuditMarker =
                @"Community and Technical Colleges of Washington State    All rights reserved. (c)";

            string testFile =
                @"P:\Reports\TechServices\sd4103b\sd4103b-2013-04-01.txt";


            var parser = CTCBAParser.Factory.GetBatchParser();

            System.IO.StreamReader sr = new System.IO.StreamReader(testFile);
            string line;
            StringBuilder buf = new StringBuilder();

            //strip form-feed characters. 
            line = sr.ReadLine();
            while (line != null)
            {
                line = line.Replace("\f", "");
                buf.AppendLine(line);
                line = sr.ReadLine();
            }


            string auditBatch = buf.ToString();
            sr.Close();

            var results = parser.ParseAuditBatch(auditBatch, endOfAuditMarker);

            var q_check_results =
                from x in results
                where !x.AuditTimestamp.HasValue
                select x;
            Console.WriteLine(q_check_results.Count().ToString());
            var storage = CTCBAStorage.Factory.GetAuditStorage();
            //storage.PurgeAllStoredAudits();
            storage.MergeAudits(results.ToArray());

            Console.Write(results.Last().AuditText);
        }

        static void TestPruneMethod(DateTime cutoffDate)
        {
            CTCBAStorage.Factory.GetAuditStorage().PruneAuditText();
        }
    }
}
