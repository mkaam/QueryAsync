using CommandLine;

namespace QTAv2
{
    public class Options
    {
        public virtual bool Verbose { get; set; }
        public virtual string CsvFile { get; set; }
        public virtual string LogFile { get; set; }
        public virtual string DBName { get; set; }
        public virtual string ServerName { get; set; }
        public virtual string TableName { get; set; }
        public virtual string DBList { get; set; }
        public virtual string QueryFile { get; set; }
        public virtual bool TruncateTable { get; set; }
    }

    [Verb("ExportToTable", HelpText = "Query to All DB and Save result to SQL Table")]
    class ExportToTable : Options
    {
        [Option(HelpText = "Print process output to console")]
        public override bool Verbose { get; set; }

        [Option(HelpText = "Full path Logging file or use filename only, by default App rootpath will be used. eg: E:\\Interface\\Sampoerna\\QTAv2\\Logs\\LogAMI.txt or LogAMI.txt")]
        public override string LogFile { get; set; }

        [Option(Required = true, HelpText = "Database Name for save result. eg: Users")]
        public override string DBName { get; set; }

        [Option(Required = true, HelpText = "Server Name for save result. eg: PMIIDSUBISMS10")]
        public override string ServerName { get; set; }

        [Option(Required = true, HelpText = "Table Name for save result. eg: ExportTable")]
        public override string TableName { get; set; }

        [Option(Required = true, HelpText = "query filename, can use full path or use filename only, by default App rootpath will be used. eg: E:\\Interface\\Sampoerna\\QTAv2\\InputQuery\\CustomerReport.sql or CustomerReport.sql")]
        public override string QueryFile { get; set; }

        [Option(
            Default = "DBList.txt",
            HelpText = "Full path configuration file for Database Connection List. eg: E:\\QTAv2\\DBList.txt")]
        public override string DBList { get; set; }

        [Option(HelpText = "Truncate destination table before insert")]
        public override bool TruncateTable { get; set; }



    }

    [Verb("ExportToCSV", isDefault: true, HelpText = "Query to All DB and Save result to CSV file")]
    class ExportToCSV : Options
    {
        [Option(HelpText = "Print process output to console")]
        public override bool Verbose { get; set; }

        [Option(HelpText = "Full path Logging file or use filename only, by default App rootpath will be used. eg: E:\\Interface\\Sampoerna\\QTAv2\\Logs\\LogAMI.txt or LogAMI.txt")]
        public override string LogFile { get; set; }      

        [Option(Required = true, HelpText = "query filename, can use full path or use filename only, by default App rootpath will be used. eg: E:\\Interface\\Sampoerna\\QTAv2\\InputQuery\\CustomerReport.sql or CustomerReport.sql")]
        public override string QueryFile { get; set; }

        [Option(                        
            Default = "DBList.txt",
            HelpText = "Full path configuration file for Database Connection List. eg: E:\\QTAv2\\DBList.txt")]
        public override string DBList { get; set; }

        [Option(
            Required = true,
            HelpText = "Full path configuration file for Database Connection List. eg: E:\\QTAv2\\DBList.txt")]
        public override string CsvFile { get; set; }

    }

    [Verb("DML", HelpText = "Run Query as Data Manipulation Language (INSERT UPDATE DELETE)")]
    class DML : Options
    {
        [Option(HelpText = "Print process output to console")]
        public override bool Verbose { get; set; }

        [Option(HelpText = "Full path Logging file or use filename only, by default App rootpath will be used. eg: E:\\Interface\\Sampoerna\\QTAv2\\Logs\\LogAMI.txt or LogAMI.txt")]
        public override string LogFile { get; set; }

        [Option(Required = true, HelpText = "query filename, can use full path or use filename only, by default App rootpath will be used. eg: E:\\Interface\\Sampoerna\\QTAv2\\InputQuery\\CustomerReport.sql or CustomerReport.sql")]
        public override string QueryFile { get; set; }

        [Option(HelpText = "Optional. Full path configuration file for Database Connection List. eg: E:\\QTAv2\\DBList.txt")]
        public override string DBList { get; set; }

    }

}
