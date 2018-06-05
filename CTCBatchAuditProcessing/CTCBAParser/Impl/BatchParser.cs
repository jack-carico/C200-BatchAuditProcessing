using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTCBAParser.Impl
{
    internal class BatchParser : IBatchParser
    {
        private System.Text.RegularExpressions.Regex sidRegex;
        private System.Text.RegularExpressions.Regex appliedCreditsRegex;
        private System.Text.RegularExpressions.Regex requiredCreditsRegex;
        private System.Text.RegularExpressions.Regex degreeTitleRegex;
        private System.Text.RegularExpressions.Regex programCodeRegex;
        private System.Text.RegularExpressions.Regex timestampRegex;


        private BatchParser()
        {
            sidRegex = new System.Text.RegularExpressions.Regex(
                @"Student ID:\s*(?<sid>\w{9})");
            appliedCreditsRegex = new System.Text.RegularExpressions.Regex(
                @"Credits applied:\s*(?<cr>\d+(\.\d+)?)");
            requiredCreditsRegex = new System.Text.RegularExpressions.Regex(
                @"Credits required:\s*(?<cr>\d+(\.\d+)?)");
            degreeTitleRegex = new System.Text.RegularExpressions.Regex(
                @"\*{80}\r\n(?<title>.+)\r\n(?<ayr>.+)\r\n(\s*)\r\n");
            programCodeRegex = new System.Text.RegularExpressions.Regex(
                @"\[EPC:(?<epc>.+)\]");

            timestampRegex = new System.Text.RegularExpressions.Regex(
                @"(?<dayname>Sunday|Monday|Tuesday|Wednesday|Thursday|Friday|Saturday)"
                + @",\s+(?<month>January|February|March|April|May|June|July|August|September|October|November|December)"
                + @"\s+(?<day>\d\d)"
                + @",\s+(?<year>\d\d\d\d)"
                + @"\s+(?<hour>\d+):(?<minute>\d\d):(?<second>\d\d)"
                + @"\s+(?<ampm>\w\w)");

        }
        
        internal static IBatchParser GetInterface()
        {
            return new BatchParser();
        }

        private List<string> BreakIntoSingleAudits(string batchText, string endOfAuditMarker)
        {
            List<string> results = new List<string>();
            System.IO.StringReader sr = new System.IO.StringReader(batchText);
            StringBuilder sb = new StringBuilder();
            string line;
            do
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    sb.Append(line);
                    sb.Append(Environment.NewLine);
                    if (line.Contains(endOfAuditMarker))
                    {
                        results.Add(sb.ToString());
                        sb.Length = 0;
                    }
                }
            } while (line != null);
            return results;
        }

        private List<string> BreakIntoSingleAudits(System.IO.Stream textStream, string endOfAuditMarker)
        {
            System.IO.StreamReader sr;
            using (sr = new System.IO.StreamReader(textStream))
            {
                List<string> results = new List<string>();
                StringBuilder sb = new StringBuilder();
                string line;
                do
                {
                    line = sr.ReadLine();
                    if (line != null)
                    {
                        sb.Append(line);
                        sb.Append(Environment.NewLine);
                        if (line.Contains(endOfAuditMarker))
                        {
                            results.Add(sb.ToString());
                            sb.Length = 0;
                        }
                    }
                } while (line != null);
                return results;
            }
        }

        private bool TryExtractAuditDetails(
            string auditText,
            out string studentSid,
            out decimal? creditsRequired,
            out decimal? creditsApplied,
            out string degreeTitle,
            out string programCode,
            out DateTime? auditTimestamp,
            out IUnusedCourse[] unusedCourses)
        {
            studentSid = null;
            creditsApplied = null;
            creditsRequired = null;
            degreeTitle = null;
            programCode = null;
            auditTimestamp = null;
            unusedCourses = null;

            if (auditText == null) { return false; }                        
            var match = sidRegex.Match(auditText);
            if (match.Success)
            {
                studentSid = match.Groups["sid"].Value;
            }
            else { return false; }

            match = appliedCreditsRegex.Match(auditText);                        
            if (match.Success)
            {                
                creditsApplied = decimal.Parse(match.Groups["cr"].Value);                
            }
            else { return false; }

            match = requiredCreditsRegex.Match(auditText);
            if (match.Success)
            {
                creditsRequired = decimal.Parse(match.Groups["cr"].Value);
            }
            else { return false; }

            match = degreeTitleRegex.Match(auditText);
            if (match.Success)
            {
                degreeTitle =
                    match.Groups["title"].Value.Trim()
                    + " ("
                    + match.Groups["ayr"].Value.Trim()
                    + ")";
            }
            else { degreeTitle = "unable to parse degree title."; }

            match = programCodeRegex.Match(degreeTitle);
            if (match.Success)
            {
                programCode = match.Groups["epc"].Value;
            }

            string auditFragment = 
                auditText.Substring(0, 200)
                    .Replace("\f", "")
                    .Replace(Environment.NewLine, " ");
            match = timestampRegex.Match(auditFragment);
            if (match.Success)
            {
                auditTimestamp = ParseTimestamp(match);
            }

            unusedCourses = ExtractUnusedCourses(auditText).ToArray();

            return true;
        }


        private System.Text.StringBuilder sbTimestamp = new StringBuilder();
        private DateTime? ParseTimestamp(System.Text.RegularExpressions.Match match)
        {
            string month = ConvertMonthName(match.Groups["month"].Value).ToString();
            string day = match.Groups["day"].Value;
            string year = match.Groups["year"].Value;
            string hour = match.Groups["hour"].Value;
            string minute = match.Groups["minute"].Value;
            string second = match.Groups["second"].Value;
            string ampm = match.Groups["ampm"].Value;
            
            sbTimestamp.Clear();
            sbTimestamp.Append(month)
                .Append("/")
                .Append(day)
                .Append("/")
                .Append(year)
                .Append(" ")
                .Append(hour)
                .Append(":")
                .Append(minute)
                .Append(":")
                .Append(second)
                .Append(" ")
                .Append(ampm);

            DateTime result;
            if (DateTime.TryParse(sbTimestamp.ToString(), out result))
            {
                return result;
            }
            else
            {
                return null;
            }

        }

        private int ConvertMonthName(string monthName)
        {
            //convert monthName to month number string.
            switch (monthName)
            {
                case "January": return 1;
                case "February": return 2;
                case "March": return 3;
                case "April": return 4;
                case "May": return 5;
                case "June": return 6;
                case "July": return 7;
                case "August": return 8;
                case "September": return 9;
                case "October": return 10;
                case "November": return 11;
                case "December": return 12;
                default: return 0;
            }
        }

        private const string UnusedCoursesBeginMarker =
            @"Unused Courses                            | KEY                                |";
        private const string UnusedCoursesEndMarker =
            @"--------------------------------------------------------------------------------";

        private List<IUnusedCourse> ExtractUnusedCourses(string auditText)
        {
            List<IUnusedCourse> results = new List<IUnusedCourse>();

            System.IO.StringReader sr = new System.IO.StringReader(auditText);
            
            string line = null;
            do
            {
                line = sr.ReadLine();
            }
            while (line != null && !line.Contains(UnusedCoursesBeginMarker));

            if (line == null) { return results; }
            //otherwise read past next 5 lines.
            for (int i = 0; i < 5; i++) { sr.ReadLine(); }

            //start reading in unused course records. 
            List<string> unusedCourseLines = new List<string>();
            do
            {
                line = sr.ReadLine();
                if (line == null) { break; }
                if (line.Contains(UnusedCoursesEndMarker)) { break; }
                else
                {
                    unusedCourseLines.Add(line);
                }
            }
            while (true);

            //TODO parse unused course lines. 
            return ParseUnusedCourseLines(unusedCourseLines.ToArray());

            
        }

        private const int MaxUnusedCourseLineLength = 90;
        private const int MinUnusedCourseLineLength = 70;
        private List<IUnusedCourse> ParseUnusedCourseLines(string[] lines)
        {
            List<IUnusedCourse> results = new List<IUnusedCourse>();

            string rawStatus;
            string rawCourseId;
            string rawCourseTitle;
            string rawCredits;
            string rawLetterGrade;
            string rawQuarterLabel;
            string yrq;
            UnusedCourse uc = null;
            string checkStr = null;
            foreach (var line in lines)
            {
                if (line.Length < MinUnusedCourseLineLength || line.Length > MaxUnusedCourseLineLength)
                {
                    continue;
                }
                                
                var checkChar = line[4];
                if (!checkChar.Equals('['))
                {
                    continue; //skip this line.
                }

                //check for WV code (wavier?)
                if (line.Length >= 74)
                {
                    checkStr = line.Substring(72, 2);
                    if (checkStr.Equals("WV"))
                    {
                        continue; //course information is incomplete in this instance. 
                    }
                }

                rawStatus = line.Substring(5, 1);
                rawCourseId = line.Substring(9, 10);
                rawCourseTitle = line.Substring(22, 27);
                rawCredits = line.Substring(49, 3);
                rawLetterGrade = line.Substring(55, 2);
                rawQuarterLabel = line.Substring(62, 8);
                yrq = ConvertToCTCYrq(rawQuarterLabel);

                uc = new UnusedCourse()
                {
                    CourseID = rawCourseId.Trim(),
                    CourseStatus = rawStatus,
                    CourseTitle = rawCourseTitle.Trim(),
                    CTCYrq = yrq,
                    EnrolledCredits = decimal.Parse(rawCredits),
                    LetterGrade = rawLetterGrade.Trim(),
                    QuarterLabel = rawQuarterLabel.Trim(),
                };
                results.Add(uc);
            }

            return results;
        }

        private readonly SmsDataUtilities.IYrqEncoding YrqEncoder =
            SmsDataUtilities.InterfaceFactory.GetYrqEncodingInterface();

        private string ConvertToCTCYrq(string rawQuarterLabel)
        {
            try
            {
                string qtr = rawQuarterLabel.Substring(0, 3);
                int year = int.Parse(rawQuarterLabel.Substring(4, 4));
                string yrq = null;
                switch (qtr)
                {
                    case "Fal":
                        YrqEncoder.TryEncodeYrq(year, SmsDataUtilities.YrqQuarter.Fall, out yrq);
                        break;
                    case "Win":
                        YrqEncoder.TryEncodeYrq(year-1, SmsDataUtilities.YrqQuarter.Winter, out yrq);
                        break;
                    case "Spr":
                        YrqEncoder.TryEncodeYrq(year-1, SmsDataUtilities.YrqQuarter.Spring, out yrq);
                        break;
                    case "Sum":
                        YrqEncoder.TryEncodeYrq(year, SmsDataUtilities.YrqQuarter.Summer, out yrq);
                        break;
                    default:
                        throw new Exception("Unknown quarter identifier.");
                }

                return yrq;

            }
            catch (Exception ex)
            {
                return "ERR";
            }
        }


        #region IBatchParser Members

        public List<IBatchAudit> ParseAuditBatch(string batchText, string endOfAuditMarker)
        {
            if (batchText == null || endOfAuditMarker == null)
            {
                return null;
            }

            var rawAudits = BreakIntoSingleAudits(batchText, endOfAuditMarker);
            string studentSid;
            decimal? creditsRequired;
            decimal? creditsApplied;
            string degreeTitle;
            string programCode;
            DateTime? auditTimestamp;
            IUnusedCourse[] unusedCourses;


            List<IBatchAudit> results =
                new List<IBatchAudit>();
            IBatchAudit result;
            foreach (var audit in rawAudits)
            {
                if (TryExtractAuditDetails(audit, out studentSid, out creditsRequired, out creditsApplied, out degreeTitle, out programCode, out auditTimestamp, out unusedCourses)) 
                {

                    result = new BatchAudit()
                    {
                        CreditsApplied = creditsApplied,
                        CreditsRequired = creditsRequired,
                        DegreeTitle = degreeTitle,
                        AuditText = audit,
                        StudentID = studentSid,
                        ProgramCode = programCode,
                        AuditTimestamp = auditTimestamp,
                        UnusedCourses = unusedCourses,
                    };
                    results.Add(result);
                }
            }

            return results;
        }

        public List<IBatchAudit> ParseAuditBatch(System.IO.Stream textStream, string endOfAuditMarker)
        {
            var rawAudits = BreakIntoSingleAudits(textStream, endOfAuditMarker);
            string studentSid;
            decimal? creditsRequired;
            decimal? creditsApplied;
            string degreeTitle;
            string programCode;
            DateTime? auditTimestamp;
            IUnusedCourse[] unusedCourses;

            List<IBatchAudit> results =
                new List<IBatchAudit>();
            IBatchAudit result;
            foreach (var audit in rawAudits)
            {
                if (TryExtractAuditDetails(audit, out studentSid, out creditsRequired, out creditsApplied, out degreeTitle, out programCode, out auditTimestamp, out unusedCourses))
                {
                    result = new BatchAudit()
                    {
                        CreditsApplied = creditsApplied,
                        CreditsRequired = creditsRequired,
                        DegreeTitle = degreeTitle,
                        AuditText = audit,
                        StudentID = studentSid,
                        ProgramCode = programCode,
                        AuditTimestamp = auditTimestamp,
                        UnusedCourses = unusedCourses,
                    };
                    results.Add(result);
                }
            }
            return results;
        }

        #endregion
    }
}
