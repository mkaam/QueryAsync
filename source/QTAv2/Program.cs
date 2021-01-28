using CommandLine;
using NLog;
using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using CsvHelper;
using System.Globalization;
using System.Diagnostics;

namespace QTAv2
{
    class Program
    {
        //private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();        
        private static Logger logger;
        private static Stopwatch _watch;

        static void Main(string[] args)
        {
            logger = new Logger("log");
            var parser = new Parser(config => {config.CaseSensitive = false;} );

            parser.ParseArguments<ExportToCSV, ExportToTable>(args)
                .WithParsed<ExportToCSV>(s => RunExportToCSV(s))
                .WithParsed<ExportToTable>(s => RunExportToTable(s))
                .WithNotParsed(errors => HandleParseError(errors));

            _watch.Stop();
            logger.Debug($"Application Finished. Elapsed time: {_watch.ElapsedMilliseconds}ms");

        }


        static void LoggerConfigure(Options opts)
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile");
            if (opts.LogFile != "")
            {
                if (Path.GetFileName(opts.LogFile) == opts.LogFile)                
                    logfile.FileName = $"logs/{opts.LogFile}";
                else 
                    logfile.FileName = $"{opts.LogFile}";
            }
            else              
                logfile.FileName = $"logs/{DateTime.Now.ToString("yyyyMMdd")}.log";
            logfile.MaxArchiveFiles = 60;
            logfile.ArchiveAboveSize = 10240000;

            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            if (opts.Verbose)
                config.AddRule(LogLevel.Trace, LogLevel.Fatal, logconsole);
            else
                config.AddRule(LogLevel.Error, LogLevel.Fatal, logconsole);

            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
            
            CsvLayout layout = new CsvLayout();
            layout.Delimiter = CsvColumnDelimiterMode.Comma;
            layout.Quoting = CsvQuotingMode.Auto;
            layout.Columns.Add(new CsvColumn("Start Time", "${longdate}"));
            layout.Columns.Add(new CsvColumn("Elapsed Time", "${elapsed-time}"));
            layout.Columns.Add(new CsvColumn("Machine Name", "${machinename}"));
            layout.Columns.Add(new CsvColumn("Login", "${windows-identity}"));
            layout.Columns.Add(new CsvColumn("Level", "${uppercase:${level}}"));
            layout.Columns.Add(new CsvColumn("Message", "${message}"));
            layout.Columns.Add(new CsvColumn("Exception", "${exception:format=toString}"));
            logfile.Layout = layout;

            CsvLayout ConsoleLayout = new CsvLayout();
            ConsoleLayout.Delimiter = CsvColumnDelimiterMode.Tab;
            ConsoleLayout.Columns.Add(new CsvColumn("Start Time", "${longdate}"));
            ConsoleLayout.Columns.Add(new CsvColumn("Message", "${message}"));
            ConsoleLayout.Columns.Add(new CsvColumn("Exception", "${exception:format=toString}"));
            logconsole.Layout = ConsoleLayout;

            // Apply config           
            NLog.LogManager.Configuration = config;
        }

        static int RunExportToCSV(Options opts)
        {
            var exitCode = 0;
            
            LoggerConfigure(opts);

            _watch = new Stopwatch();
            _watch.Start();
            logger.Debug("Application Start");

            logger.Info("Mode : Export to CSV, ");
            logger.Info($"Query File : {opts.QueryFile}");
            logger.Info($"Log File : {opts.LogFile}");
            logger.Info($"CSV File : {opts.CsvFile}");

            if (!File.Exists(opts.QueryFile)) logger.Debug($"Query file not found : {opts.QueryFile}");
            if (!File.Exists(opts.DBList)) logger.Debug($"DBList not found : {opts.DBList}");

            if (File.Exists(opts.QueryFile) && 
                File.Exists(opts.DBList)
                )
            {
                string QueryStr ="";
                
                using (var sr = new StreamReader(opts.QueryFile, true))                
                    QueryStr = sr.ReadToEnd();
                
                IEnumerable<string> ConnectionSrings = File.ReadLines(opts.DBList, Encoding.Default);
                List<string> TmpFiles = new List<string>();
                Parallel.ForEach(ConnectionSrings, (connstr) =>
               {
                   //logger.Debug($"Export Start... : {connstr}");
                   var TmpFile = $"Temp\\{Path.GetFileName(opts.CsvFile)}-{Guid.NewGuid()}";
                   TmpFiles.Add(TmpFile);
                   using (SqlManager sqlman = new SqlManager(connstr, logger))
                   {
                       try
                       {
                           sqlman.SqlToCsv(QueryStr, TmpFile);
                           logger.Debug($"Export Success : {connstr}");
                       }
                       catch (Exception ex)
                       {
                           exitCode = -1;
                           logger.Error($"Export Failed : {connstr}", ex);
                       }

                   }

               });
                CombineFiles(TmpFiles, opts.CsvFile);
                DeleteFiles(TmpFiles);
            }

            return exitCode;
        }
        static int RunExportToTable(Options opts)
        {
            var exitCode = 0;
            LoggerConfigure(opts);

            return exitCode;
        }

        static int RunImportFromCsv(Options opts)
        {

            return 0;
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {

            if (errs.Any(x => x is HelpRequestedError || x is VersionRequestedError))
                Console.WriteLine(errs);
        }

        static void CombineFiles(IEnumerable<string> SourceFiles, string OutputFile)
        {
            const int chunkSize = 2 * 1024; // 2KB            
            using (var output = File.Create(OutputFile))
            {
                foreach (var file in SourceFiles)
                {
                    if (File.Exists(file)) {
                        using (var input = File.OpenRead(file))
                        {
                            var buffer = new byte[chunkSize];
                            int bytesRead;
                            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                output.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
            }
        }

        static void DeleteFiles(IEnumerable<string> SourceFiles)
        {
            Parallel.ForEach(SourceFiles, (s) =>
            {
                if (File.Exists(s))
                {
                    try { File.Delete(s); } catch { }
                    
                }
            });
       
        }

    }
}
