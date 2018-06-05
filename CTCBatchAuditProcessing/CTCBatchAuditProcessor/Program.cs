using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTCBatchAuditProcessor
{
    class Program
    {
        static string Usage = @"
CTCBatchAuditProcessor.exe USAGE
==============================================================================
Processes SBCTC batch audit text file output from the SD4103 series of jobs.
The batch files should be converted to text format and windows line endings 
CRLF are expected withing all files. 
==============================================================================
-P                      #Purge all stored audits. (resets storage.)

-M <audit file path>    #Processes and merges all audits found in the 
                        #batch audit file at path given by <audit file path>

-MF <merge list file>   #Processes and merges all audits found by reading
                        #the file found at <merge list file>. 
                        #The <merge list file> must contain one file path 
                        #per line only. 

-PR                     #Prunes stored audits by truncating the full 
                        #stored audit text. Audits are grouped by the 
                        #student's ID and the program code and for all 
                        #except the most recent will have the audit text
                        #replaced with a '.' and this vastly reduces the
                        #storage space taken in the database. The Unused Course 
                        #data and the audit summary data will be untouched.
";

        static void Main(string[] args)
        {
            string arg0 = null;
            string arg1 = null;
            Console.WriteLine("");
            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine(Usage);
                return;
            }

            
            arg0 = args[0].Trim();
            
            if (args.Length == 2)
            {
                arg1 = args[1].Trim();
            }

            switch (arg0)
            {
                case "-P":
                    DoPurgeCommand();
                    break;
                case "-M":
                    if (arg1 == null)
                    {
                        Console.WriteLine(Usage);
                        return;
                    }
                    DoSingleFileMerge(arg1);
                    break;
                case "-MF":
                    if (arg1 == null)
                    {
                        Console.WriteLine(Usage);
                        return;
                    }
                    DoMultipleFileMerge(arg1);
                    break;
                case "-PR":                    
                    DoPruneStoredAudits();
                    break;
                default:
                    Console.WriteLine(Usage);
                    break;

            }
        }

        private static void DoPruneStoredAudits()
        {
            Console.WriteLine("The pruneing procedure may take several minutes or longer if the pruning operation is not carried out after each upload.");
            CTCBAStorage.Factory.GetAuditStorage().PruneAuditText();
            Console.WriteLine("Prune operation completed: " + DateTime.Now.ToString());
        }

        private static void DoMultipleFileMerge(string mergeFilePath)
        {
            bool fileExists = System.IO.File.Exists(mergeFilePath) && !System.IO.Directory.Exists(mergeFilePath);
            if (!fileExists)
            {
                Console.WriteLine("Path provided is not a file or does not exist: " + mergeFilePath);
                return;
            }

            var srMergeFile = new System.IO.StreamReader(mergeFilePath);

            string auditFile = srMergeFile.ReadLine();
            while (auditFile != null)
            {                
                DoSingleFileMerge(auditFile);
                auditFile = srMergeFile.ReadLine();
            }

            srMergeFile.Close();
        }

        private static void DoSingleFileMerge(string filePath)
        {

            bool fileExists = System.IO.File.Exists(filePath) && !System.IO.Directory.Exists(filePath);
            if (!fileExists)
            {
                Console.WriteLine("Path provided is not a file or does not exist: " + filePath);
                return;
            }

            var bParser = CTCBAParser.Factory.GetBatchParser();
            var bStorage = CTCBAStorage.Factory.GetAuditStorage();
            Console.Write("Trying to merge file: ");
            Console.WriteLine(filePath);
            //try to read in the file. 
            var sReader = new System.IO.StreamReader(filePath);
            var batchResults = bParser.ParseAuditBatch(
                                 CleanTextStream(sReader),
                                 Properties.Settings.Default.EndOfAuditMarker);
            sReader.Close();
            bStorage.MergeAudits(batchResults.ToArray());

        }

        private static void DoPurgeCommand()
        {
            CTCBAStorage.Factory.GetAuditStorage().PurgeAllStoredAudits();
            Console.Write("All stored audits have been purged: ");
            Console.WriteLine(DateTime.Now.ToString());
        }

        private static StringBuilder Buf = new StringBuilder();
        private static string CleanTextStream(System.IO.StreamReader sr)
        {
            Buf.Clear();
            string line = sr.ReadLine();
            while (line != null)
            {
                line = line.Replace("\f", "");//get rid of form feeds.
                Buf.AppendLine(line);
                line = sr.ReadLine();
            }
            return Buf.ToString();
        }
    }
}
