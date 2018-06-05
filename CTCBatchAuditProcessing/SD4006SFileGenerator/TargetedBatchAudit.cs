using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SD4006SFileGenerator
{
    public class TargetedBatchAudit
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseYrq">The cohort of students will include any enrolled in the given </param>
        /// <param name="outputFilePath"></param>
        public void GenerateSD4006SFile(string baseYrq, string outputFilePath)
        {
            DateTime today = DateTime.Now;
            //first quarter that is not summer. 

            var ctx = new SMSData.T200EXTRACTSDataContext();
            
            var q_current_yrq_not_summer =
              (from y in ctx.YRQ_Ms
               where
                  y.FIRST_DAY_YRQ.Value.CompareTo(today) <= 0
                  && !y.YRQ.Substring(3, 1).Equals("1") /* exclude summer */
               orderby y.FIRST_DAY_YRQ descending
               select y.YRQ).First();

            var q_cohort =
               from x in ctx.STU_YRQ_Ms
               where
                 x.STU_QTR_STAT.Equals('A')
                 && x.SQ_YRQ.CompareTo(baseYrq) >= 0
                 && !x.SQ_YRQ.Substring(3, 1).Equals("1") /* exclude summer */
               group x by x.SQ_SID into g_x
               let maxYrq = g_x.Max(z => z.SQ_YRQ)
               join sym in ctx.STU_YRQ_Ms
                  on new { A = g_x.Key, B = maxYrq } equals new { A = sym.SQ_SID, B = sym.SQ_YRQ }
               where !sym.STU_PRG_ENR.Equals("")
               join sd in ctx.STU_Ds
                  on g_x.Key equals sd.SID into j_sd
               from sd in j_sd.DefaultIfEmpty()
               select new
               {
                   SID = g_x.Key,
                   MaxYRQ = maxYrq,
                   EPC = sym.STU_PRG_ENR,
                   StudentName = sd.STU_NAME,
               };

            var q_quarter_list =
               from y in ctx.YRQ_Ms
               where
                  !y.YRQ.Substring(3, 1).Equals("1")
                  && !y.YRQ.Equals("Z999")
                  && (y.YRQ.CompareTo(q_current_yrq_not_summer) <= 0)
               orderby y.YRQ descending
               select y.YRQ;

            var q_epc_history =
              (from x in q_cohort
               join sym in ctx.STU_YRQ_Ms
                  on new { A = x.SID, B = x.EPC }
                  equals
                  new { A = sym.SQ_SID, B = sym.STU_PRG_ENR }
               where sym.STU_QTR_STAT.Equals("A")
               select new
               {
                   sym.SQ_SID,
                   sym.SQ_YRQ
               }).ToList();

            var q_epc_stu_group =
              (from x in q_epc_history
               group x.SQ_YRQ by x.SQ_SID into g_x
               select new
               {
                   SID = g_x.Key,
                   Quarters = g_x.OrderByDescending(z => z).ToList()
               }).ToDictionary(z => z.SID, z => z.Quarters);

            //q_epc_stu_group.Dump();


            string earliestYrq = null;
            List<AuditConfigRecord> acrList = new List<AuditConfigRecord>();
            AuditConfigRecord acrTemp = null;

            foreach (var c in q_cohort)
            {
                //if the latest enrollment is not the current quarter then use the current quarter. 
                if (!c.MaxYRQ.Equals(q_current_yrq_not_summer))
                {
                    earliestYrq = q_current_yrq_not_summer;
                }
                else
                {
                    //check the yrq timeline until there is no match in the student data. return earliest YRQ found. 
                    earliestYrq = null;
                    foreach (var yrq in q_quarter_list)
                    {
                        if (q_epc_stu_group[c.SID].Contains(yrq))
                        {
                            earliestYrq = yrq;
                        }
                        else
                        {
                            //found the first non-match stop processing this student. 
                            //(earliestYrq + ":" + c.SID).Dump();
                            break;
                        }
                    }
                }

                acrTemp = new AuditConfigRecord()
                {
                    AuditAYR = MapToAuditAYR(earliestYrq),
                    AuditEPC = c.EPC,
                    FullName = c.StudentName,
                    MostRecentYRQEnrolledInEPC = c.MaxYRQ,
                    SID = c.SID
                };
                acrList.Add(acrTemp);
            }

            

            int recordCharLength = 41;
            char[] buf = new char[recordCharLength];
            InitBuffer(buf);
            var sw = new System.IO.StreamWriter(outputFilePath, false, Encoding.ASCII);

            foreach (var acr in acrList)
            {
                InitBuffer(buf);
                AddSid(acr.SID, buf);
                AddName(acr.FullName, buf);
                AddEPC(acr.AuditEPC, buf);
                AddAYR(acr.AuditAYR, buf);
                sw.Write(buf);
            }

            sw.Close();
        }

        public class AuditConfigRecord
        {
            public string SID { get; set; }
            public string FullName { get; set; }
            public string AuditEPC { get; set; }
            ///Academic year code to use for audit template.
            public string AuditAYR { get; set; }
            public string MostRecentYRQEnrolledInEPC { get; set; }
        }

        public string MapToAuditAYR(string yrq)
        {
            string yrqPrefix = yrq.Substring(0, 3);

            switch (yrqPrefix)
            {
                case "B90":
                    return "1920";
                case "B89":
                    return "1819";
                case "B78":
                    return "1718";
                case "B67":
                    return "1617";
                case "B56":
                    return "1516";
                case "B45":
                    return "1415";
                case "B34":
                    return "1314";
                default:
                    return "";
            }
        }

        public void AddSid(string sid, char[] buf)
        {
            int index = 0;
            foreach (var c in sid)
            {
                buf[index++] = c;
            }
        }

        public void InitBuffer(char[] buf)
        {
            for (int i = 0; i < buf.Length - 1; i++)
            {
                buf[i] = ' ';
            }

            buf[buf.Length - 1] = '\u000A'; //record terminator


        }

        public void AddName(string name, char[] buf)
        {
            int index = 10;
            foreach (var c in name)
            {
                buf[index++] = c;
            }
        }

        public void AddEPC(string epc, char[] buf)
        {
            int index = 32;
            foreach (var c in epc)
            {
                buf[index++] = c;
            }
        }

        public void AddAYR(string ayr, char[] buf)
        {
            int index = 36;
            foreach (var c in ayr)
            {
                buf[index++] = c;
            }
        }


    }
}
