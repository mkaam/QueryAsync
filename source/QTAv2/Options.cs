using CommandLine;
using System.Collections.Generic;

namespace QTAv2
{
    public class Options
    {
        [Option(HelpText = "Print process output to console")]
        public virtual bool Verbose { get; set; }

        public virtual string CsvFile { get; set; }

        public virtual string LogFile { get; set; }

        public virtual string DBName { get; set; }
        
        public virtual string ServerName { get; set; }

        public virtual string TableName { get; set; }

        public virtual string DBList { get; set; }

        public virtual IEnumerable<string> DBListFilters { get; set; }

        public virtual string QueryFile { get; set; }

        public virtual bool TruncateTable { get; set; }

        public virtual string Delimiter { get; set; }

        public virtual bool NoQuote { get; set; }

        public virtual IEnumerable<string> CsvFileList { get; set; }

        public virtual bool ForceQuote { get; set; }

        public virtual bool NoHeader { get; set; }

        public virtual IEnumerable<string> QueryTextReplace { get; set; }
       
        public virtual bool HeaderInBody { get; set; }

    }

    [Verb("ExportToTable", HelpText = "Query to All DB and Save result to SQL Table")]
    class ExportToTable : Options
    {
        public override bool Verbose { get; set; }

        [Option('g', "LogFile", HelpText = "Full path Logging file or use filename only, by default App rootpath will be used. eg: E:\\Interface\\Sampoerna\\QTAv2\\Logs\\LogAMI.txt or LogAMI.txt")]
        public override string LogFile { get; set; }

        [Option('b', "DBName", Required = true, HelpText = "Database Name for save result. eg: Users")]
        public override string DBName { get; set; }

        [Option('s', "ServerName", Required = true, HelpText = "Server Name for save result. eg: PMIIDSUBISMS10")]
        public override string ServerName { get; set; }

        [Option('t', "TableName", Required = true, HelpText = "Table Name for save result. eg: ExportTable")]
        public override string TableName { get; set; }

        [Option('q', "QueryFile", Required = true, HelpText = "query filename, can use full path or use filename only, by default App rootpath will be used. eg: E:\\Interface\\Sampoerna\\QTAv2\\InputQuery\\CustomerReport.sql or CustomerReport.sql")]
        public override string QueryFile { get; set; }

        [Option('l', "DBList", Default = "DBList.txt", HelpText = "Full path configuration file for Database Connection List. eg: E:\\QTAv2\\DBList.txt")]
        public override string DBList { get; set; }

        [Option('c', "TruncateTable", HelpText = "Truncate destination table before insert")]
        public override bool TruncateTable { get; set; }

        [Option('u', "DBListFilters", HelpText = "filter connection string from DB List to be executed. Eg: AMI JKT")]
        public override IEnumerable<string> DBListFilters { get; set; }

        [Option('r', "QueryTextReplace", HelpText = "replace string on query, usually use for specific filter on query. eg: ParamAreaCode=JKT ParamSalesPoint=SSLI")]
        public override IEnumerable<string> QueryTextReplace { get; set; }

    }

    [Verb("ExportToCSV", isDefault: true, HelpText = "Query to All DB and Save result to CSV file")]
    class ExportToCSV : Options
    {        
        public override bool Verbose { get; set; }

        [Option('g', "LogFile", HelpText = "Full path Logging file or use filename only, by default App rootpath will be used. eg: E:\\Interface\\Sampoerna\\QTAv2\\Logs\\LogAMI.txt or LogAMI.txt")]
        public override string LogFile { get; set; }
        
        public override string QueryFile { get; set; }

        [Option('l', "DBList", Default = "DBList.txt", HelpText = "Full path configuration file for Database Connection List. eg: E:\\QTAv2\\DBList.txt")]
        public override string DBList { get; set; }

        [Option('u', "DBListFilters", HelpText = "filter connection string from DB List to be executed. Eg: AMI JKT")]
        public override IEnumerable<string> DBListFilters { get; set; }

        [Option('f', "CsvFile", Required = true, HelpText = "CSV File with full path. eg: E:\\QTAv2\\Output.csv")]
        public override string CsvFile { get; set; }

        [Option('d', "Delimiter", Default = ",", HelpText = "Delimiter CSV file. eg: ; , |")]
        public override string Delimiter { get; set; }

        [Option('n', "NoQuote", Default = false, HelpText = "Disable quote on every column")]
        public override bool NoQuote { get; set; }

        [Option('o', "ForceQuote", Default = false, HelpText = "Force use quote on every data")]
        public override bool ForceQuote { get; set; }

        [Option('p', "NoHeader", Default = false, HelpText = "No Header")]
        public override bool NoHeader { get; set; }

        public override IEnumerable<string> QueryTextReplace { get; set; }

        [Option('z', "HeaderInBody", Default = false, HelpText = "HeaderInBody")]
        public override bool HeaderInBody { get; set; }
    }

    [Verb("DML", HelpText = "Run Query as Data Manipulation Language (INSERT UPDATE DELETE)")]
    class DML : Options
    {
        [Option(HelpText = "Print process output to console")]
        public override bool Verbose { get; set; }

        [Option('g', "LogFile", HelpText = "Full path Logging file or use filename only, by default App rootpath will be used. eg: E:\\Interface\\Sampoerna\\QTAv2\\Logs\\LogAMI.txt or LogAMI.txt")]
        public override string LogFile { get; set; }

        public override string QueryFile { get; set; }

        public override string DBList { get; set; }

    }

    [Verb("ImportCSV", HelpText = "Import CSV file")]
    class ImportCSV : Options
    {
        [Option(HelpText = "Print process output to console")]
        public override bool Verbose { get; set; }

        [Option('g', "LogFile", HelpText = "Full path Logging file or use filename only, by default App rootpath will be used. eg: E:\\Interface\\Sampoerna\\QTAv2\\Logs\\LogAMI.txt or LogAMI.txt")]
        public override string LogFile { get; set; }

        [Option(Required = true, HelpText = "Full Path CSV File. Multiple CSV File support with comma separator. eh: C:\\A.csv,C:\\B.csv")]
        public override IEnumerable<string> CsvFileList { get; set; }

        [Option('b', "DBName", Required = true, HelpText = "Database Name for save result. eg: Users")]
        public override string DBName { get; set; }

        [Option('s', "ServerName", Required = true, HelpText = "Server Name for save result. eg: PMIIDSUBISMS10")]
        public override string ServerName { get; set; }

        [Option('t', "TableName", Required = true, HelpText = "Table Name for save result. eg: ExportTable")]
        public override string TableName { get; set; }

    }

}
