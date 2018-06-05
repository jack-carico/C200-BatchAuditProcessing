using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SD4006SFileGenerator
{
    class Program
    {

        static string Usage =
@"
Incorrect arguments provided.
======================================================================
SD4006SFileGenerator.exe <academicYear> <baseQuarter> <outputFilePath>

<academicYear>          #must be a four digit academic year designation
                        #as used in the degree audit system. e.g. 1213

<baseQuarter>           #The cohort produced will include all students 
                        #who enrolled in the baseQuarter forward. Must
                        #be a SMS encoded quarter. e.g. B231

<outputFilePath>        #full path and filename into which the results 
                        #will be written. Existing files will be 
                        #overwritten without warning. 
";

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine(Usage);
                return;
            }

            string daAcademicYear = args[0];
            string smsBaseQuarter = args[1];
            string outputFilePath = args[2];

            var tba = new TargetedBatchAudit();

            tba.GenerateSD4006SFile(smsBaseQuarter, outputFilePath);
        }

        static void DoOldFileGenProcess(string daAcademicYear, string smsBaseQuarter, string outputFilePath)
        {            
            int recordCharLength = 41;
            char[] buf = new char[recordCharLength];
            InitBuffer(buf);

            using (var ctx = new SMSData.T200EXTRACTSDataContext())
            {
                var q_enr =
                   from sym in ctx.STU_YRQ_Ms
                   join sd in ctx.STU_Ds
                     on sym.SQ_SID equals sd.SID
                   where
                      sym.SQ_YRQ.CompareTo(smsBaseQuarter) >= 0
                      && sym.STU_QTR_STAT.Equals('A')
                      && !sd.STU_PRG_ENR.Equals("") //do not include undeclared students.        
                   group sym by sym.SQ_SID into g_sid
                   join sd2 in ctx.STU_Ds
                     on g_sid.Key equals sd2.SID
                   select new
                   {
                       sd2.SID,
                       sd2.STU_NAME,
                       STU_PRG_ENR = sd2.STU_PRG_ENR,
                   };

                using (var sw = new System.IO.StreamWriter(outputFilePath, false, Encoding.ASCII))
                {
                    foreach (var item in q_enr)
                    {
                        InitBuffer(buf);
                        AddSid(item.SID, buf);
                        AddName(item.STU_NAME, buf);
                        AddEPC(item.STU_PRG_ENR, buf);
                        AddAYR(daAcademicYear, buf);
                        sw.Write(buf);
                    }
                    sw.Close();
                }
            }
        }

        static void AddSid(string sid, char[] buf)
        {
            int index = 0;
            foreach (var c in sid)
            {
                buf[index++] = c;
            }
        }

        static void InitBuffer(char[] buf)
        {
            for (int i = 0; i < buf.Length - 1; i++)
            {
                buf[i] = ' ';
            }

            buf[buf.Length - 1] = '\u000A'; //record terminator

            
        }

        static void AddName(string name, char[] buf)
        {
            int index = 10;
            foreach (var c in name)
            {
                buf[index++] = c;
            }
        }

        static void AddEPC(string epc, char[] buf)
        {
            int index = 32;
            foreach (var c in epc)
            {
                buf[index++] = c;
            }
        }

        static void AddAYR(string ayr, char[] buf)
        {
            int index = 36;
            foreach (var c in ayr)
            {
                buf[index++] = c;
            }
        }
    }
}
