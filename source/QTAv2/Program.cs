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
using CommandLine.Text;
using CsvHelper.Configuration;

namespace QTAv2
{
    class Program
    {
        //private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();        
        private static Logger logger;
        private static Stopwatch _watch;
        private static string ExePath;
        private static string RootPath;
        private static string QueryPath;
        private static string ConfigPath;
        private static string LogPath;
        private static string CsvPath;
        private static bool ParserError=false;

        static void Main(string[] args)
        {   

            //var proccount = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 2.0));
            ExePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            RootPath = Path.Combine(ExePath,"..");
            QueryPath = Path.Combine(RootPath, "Query");
            LogPath = Path.Combine(RootPath, "Logs");
            ConfigPath = Path.Combine(RootPath, "Config");
            CsvPath = Path.Combine(RootPath, "Csv");


            logger = new Logger("log");
            //var parser = CommandLine.Parser.Default;
            //parser.Settings.CaseSensitive = false;
            var parser = new Parser(config =>
                {
                    config.IgnoreUnknownArguments = false;
                    config.CaseSensitive = false;
                    config.AutoHelp = true;
                    config.AutoVersion = true;
                    config.HelpWriter = Console.Error;
                });


            var result = parser.ParseArguments<ExportToCSV, ExportToTable>(args)
                .WithParsed<ExportToCSV>(s => RunExportToCSV(s))
                .WithParsed<ExportToTable>(s => RunExportToTable(s))
                .WithNotParsed(errors => HandleParseError(errors));

            if (!ParserError)
            {
                _watch.Stop();
                logger.Debug($"Application Finished. Elapsed time: {_watch.ElapsedMilliseconds}ms");
            }

#if DEBUG
            Console.WriteLine("Press enter to close...");
            Console.ReadLine();
#endif


        }


        static void LoggerConfigure(Options opts)
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile");
            if (opts.LogFile != null)
            {
                if (Path.GetFileName(opts.LogFile) == opts.LogFile)                
                    logfile.FileName = $"{Path.Combine(Path.Combine(RootPath,"Logs"),opts.LogFile)}";
                else 
                    logfile.FileName = $"{opts.LogFile}";
            }
            else
                logfile.FileName = $"{Path.Combine(Path.Combine(RootPath, "Logs"), $"{DateTime.Now.ToString("yyyyMMdd")}.log")}";            

            logfile.MaxArchiveFiles = 60;
            logfile.ArchiveAboveSize = 10240000;

            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            if (opts.Verbose)
                config.AddRule(LogLevel.Trace, LogLevel.Fatal, logconsole);
            else
                config.AddRule(LogLevel.Error, LogLevel.Fatal, logconsole);

            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
            
            // design layout for file log rotation
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

            // design layout for console log rotation
            SimpleLayout ConsoleLayout = new SimpleLayout("${longdate}:${message}\n${exception}");
            logconsole.Layout = ConsoleLayout;

            // Apply config           
            NLog.LogManager.Configuration = config;
        }

        static void PathConfigure(Options opts)
        {
            // DBList
            if (opts.DBList != null && Path.GetFileName(opts.DBList) == opts.DBList)
            {
                opts.DBList = $"{Path.Combine(ConfigPath,opts.DBList)}";
            }
            // Logs
            if (opts.LogFile != null && Path.GetFileName(opts.LogFile) == opts.LogFile)
            {
                opts.LogFile = $"{Path.Combine(LogPath, opts.LogFile)}";
            }
            // Query
            if (opts.QueryFile != null && Path.GetFileName(opts.QueryFile) == opts.QueryFile)
            {
                opts.QueryFile = $"{Path.Combine(QueryPath, opts.QueryFile)}";
            }
            // Query
            if (opts.CsvFile != null && Path.GetFileName(opts.CsvFile) == opts.CsvFile)
            {
                opts.CsvFile = $"{Path.Combine(CsvPath, opts.CsvFile)}";
            }

        }

        static int RunExportToCSV(Options opts)
        {
            var exitCode = 0;

            PathConfigure(opts);
            LoggerConfigure(opts);            

            _watch = new Stopwatch();
            _watch.Start();
            logger.Debug("Application Start");

            logger.Info("Mode : Export to CSV, ");
            logger.Info($"Query File : {opts.QueryFile}");
            logger.Info($"Log File : {opts.LogFile}");
            logger.Info($"CSV File : {opts.CsvFile}");
            

            if (!Directory.Exists(Path.Combine(ExePath, "Temp"))) Directory.CreateDirectory(Path.Combine(ExePath,"Temp"));

            if (!File.Exists(opts.QueryFile)) logger.Debug($"Query file not found : {opts.QueryFile}");
            if (!File.Exists(opts.DBList)) logger.Debug($"DBList not found : {opts.DBList}");

            if (File.Exists(opts.QueryFile) && 
                File.Exists(opts.DBList)
                )
            {
                CsvConfiguration csvconf;                
                if (opts.NoQuote)
                {
                    csvconf = new CsvConfiguration(CultureInfo.InvariantCulture,
                      delimiter: opts.Delimiter,
                      shouldQuote: (field, context) => { return false; }
                      );
                }
                else
                {
                    csvconf = new CsvConfiguration(CultureInfo.InvariantCulture,
                      delimiter: opts.Delimiter
                      );
                }

                if (opts.ForceQuote)
                {
                    csvconf = new CsvConfiguration(CultureInfo.InvariantCulture,
                      delimiter: opts.Delimiter,
                      shouldQuote: (field, context) => { return true; }                      
                      );
                }
                else
                {
                    csvconf = new CsvConfiguration(CultureInfo.InvariantCulture,
                      delimiter: opts.Delimiter
                      );
                }
  

                string QueryStr ="";
                
                using (var sr = new StreamReader(opts.QueryFile, true))                
                    QueryStr = sr.ReadToEnd();
                
                IEnumerable<string> tmp_ConnectionSrings = File.ReadLines(opts.DBList, Encoding.Default);
                List<string> ConnectionSrings = new List<string>();

                if (opts.DBListFilters.Count() > 0)
                    ConnectionSrings = EnumFiltered(tmp_ConnectionSrings, opts.DBListFilters);
                else
                    ConnectionSrings = tmp_ConnectionSrings.ToList();

                List<string> TmpFiles = new List<string>();
                string FileId = Guid.NewGuid().ToString();
                var HeaderFile = Path.Combine(ExePath,$"Temp\\{Path.GetFileName(opts.CsvFile)}_Header-{FileId}");
                int RowCount = 0;
                foreach (string connstr in ConnectionSrings)
                {
                    using (SqlManager sqlman = new SqlManager(connstr, logger))
                    {
                        try
                        {
                            var RowCountx = sqlman.SqlToCsvHeaderOnly(QueryStr, HeaderFile, csvconf);
                            RowCount += RowCountx;
                        }
                        catch (Exception ex)
                        {
                            exitCode = -1;
                            logger.Error($"Export Failed : {connstr}", ex);
                        }

                    }
                    break;

                };

                if (RowCount > 0)
                {
                    TmpFiles.Add(HeaderFile);

                    Parallel.ForEach(ConnectionSrings, (connstr) =>
                   {
                       //logger.Debug($"Export Start... : {connstr}");
                       var TmpFile = Path.Combine(ExePath, $"Temp\\{Path.GetFileName(opts.CsvFile)}_Detail-{FileId}");
                       TmpFiles.Add(TmpFile);
                       using (SqlManager sqlman = new SqlManager(connstr, logger))
                       {
                           try
                           {
                               sqlman.SqlToCsv(QueryStr, TmpFile, csvconf);
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

            }

            return exitCode;
        }

        static void TruncateTable(Options opts)
        {
            var DestConnStr = $"Data Source={opts.ServerName};Initial Catalog={opts.DBName};Integrated Security=True;Connection Timeout=3600;";
            using (SqlManager sqlman = new SqlManager(DestConnStr, logger))
            {
                try
                {
                    sqlman.TruncateTable(opts.TableName);
                }
                catch (SqlException ex)
                {
                    logger.Error($"Truncate Table {opts.TableName} Failed", ex);
                }                
            }
                
        }

        static int RunExportToTable(Options opts)
        {
            var exitCode = 0;

            PathConfigure(opts);
            LoggerConfigure(opts);

            _watch = new Stopwatch();
            _watch.Start();
            logger.Debug("Application Start");

            logger.Info("Mode : Export to Table, ");
            logger.Info($"Query File : {opts.QueryFile}");
            logger.Info($"Log File : {opts.LogFile}");
            logger.Info($"Server : {opts.ServerName}");
            logger.Info($"Database : {opts.DBName}");
            logger.Info($"Table : {opts.TableName}");

            if (!File.Exists(opts.QueryFile)) logger.Debug($"Query file not found : {opts.QueryFile}");
            if (!File.Exists(opts.DBList)) logger.Debug($"DBList not found : {opts.DBList}");

            if (File.Exists(opts.QueryFile) &&
                File.Exists(opts.DBList)
                )
            {
                string QueryStr = "";

                using (var sr = new StreamReader(opts.QueryFile, true))
                    QueryStr = sr.ReadToEnd();

                IEnumerable<string> ConnectionSrings = File.ReadLines(opts.DBList, Encoding.Default);
                List<string> TmpFiles = new List<string>();

                if (opts.TruncateTable) { TruncateTable(opts); }

                Parallel.ForEach(ConnectionSrings, (connstr) =>
                {                    
                    using (SqlManager sqlman = new SqlManager(connstr, logger))
                    {
                        try
                        {
                            var DestConnStr = $"Data Source={opts.ServerName};Initial Catalog={opts.DBName};Integrated Security=True;Connection Timeout=3600;";

                            sqlman.SqlToTable(QueryStr, DestConnStr, opts.TableName);
                            logger.Debug($"Export Success : {connstr}");
                        }
                        catch (Exception ex)
                        {
                            exitCode = -1;
                            logger.Error($"Export Failed : {connstr}", ex);
                        }

                    }

                });        
            }

            return exitCode;
        }

        static int RunImportFromCsv(Options opts)
        {

            return 0;
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            ParserError = true;

            if (errs.Any(x => x is HelpRequestedError || x is VersionRequestedError)) {                
            }
            else
                Console.WriteLine("Parameter unknown, please check the documentation or use parameter '--help' for more information");


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

        static List<string> EnumFiltered(IEnumerable<string> lines, IEnumerable<string> filters)
        {
        
            return filters.AsParallel()
                   .SelectMany(searchPattern =>
                          lines.Where(x => x.Contains(searchPattern))
                          ).ToList();

        }

        static bool test1()
        {
            return false;
        }

        static bool test2()
        {
            return true;
        }

    }
}
