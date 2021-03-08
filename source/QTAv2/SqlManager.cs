﻿using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;

namespace QTAv2
{
    class SqlManager : IDisposable
    {
        public SqlManager(Logger log)
        {
            Log = log;
        }
        public SqlManager(string ConnectionStr, Logger log)
        {
            ConnectionString = ConnectionStr;
            Log = log;
        }

        public void TruncateTable(string TableName)
        {
            using (SqlConnection sqlconn = new SqlConnection(ConnectionString))
            {
                sqlconn.Open();
                using (SqlCommand sqlcmd = new SqlCommand())
                {
                    sqlcmd.CommandTimeout = 3600;  //setting query timeout for 1 hour
                    sqlcmd.Connection = sqlconn;
                    sqlcmd.CommandText = $"truncate table {TableName}";

                    sqlcmd.ExecuteNonQuery();            

                }

                sqlconn.Close();
            }
        }

        public void SqlToCsv(string QueryString, string CsvFileName, CsvConfiguration csvConfig)
        {
            

            using (SqlConnection sqlconn = new SqlConnection(ConnectionString))
            {                
                sqlconn.Open();                
                using (SqlCommand sqlcmd = new SqlCommand())
                {
                    sqlcmd.CommandTimeout = 3600; //setting query timeout for 1 hour
                    sqlcmd.Connection = sqlconn;
                    sqlcmd.CommandText = QueryString;
                    
                    using (SqlDataAdapter sqlda = new SqlDataAdapter())
                    {
                        using (DataSet ds = new DataSet())
                        {                            
                            sqlda.SelectCommand = sqlcmd;
                            sqlda.Fill(ds);

                            using (StreamWriter sw = new StreamWriter(CsvFileName, true ))
                            {
                                using (var csv = new CsvHelper.CsvWriter(sw, csvConfig))
                                {                                    
                                    // Write row values
                                    foreach (DataRow row in ds.Tables[0].Rows)
                                    {
                                        for (var i = 0; i < ds.Tables[0].Columns.Count; i++)
                                        {
                                            csv.WriteField(row[i]);
                                        }
                                        csv.NextRecord();
                                    }
                                }
                            }
                        }
                    }

                }

                sqlconn.Close();
            }
        }

        public int SqlToCsvHeaderOnly(string QueryString, string CsvFileName)
        {
            int rowCount=0;

            bool FileIsExist;
            if (File.Exists(CsvFileName))
                FileIsExist = true;
            else FileIsExist = false;

            using (SqlConnection sqlconn = new SqlConnection(ConnectionString))
            {
                sqlconn.Open();
                using (SqlCommand sqlcmd = new SqlCommand())
                {
                    sqlcmd.CommandTimeout = 3600; //setting query timeout for 1 hour
                    sqlcmd.Connection = sqlconn;
                    sqlcmd.CommandText = QueryString;

                    using (SqlDataAdapter sqlda = new SqlDataAdapter())
                    {
                        using (DataSet ds = new DataSet())
                        {
                            sqlda.SelectCommand = sqlcmd;
                            sqlda.Fill(ds);
                            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                rowCount = ds.Tables[0].Rows.Count;

                                using (StreamWriter sw = new StreamWriter(CsvFileName, true))
                                {
                                    using (var csv = new CsvWriter(sw, CultureInfo.InvariantCulture))
                                    {
                                        if (!FileIsExist)
                                        {
                                            foreach (DataColumn column in ds.Tables[0].Columns)
                                            {
                                                csv.WriteField(column.ColumnName);
                                            }
                                            csv.NextRecord();
                                        }

                                    }
                                }
                            }

                        }
                    }

                }

                sqlconn.Close();
            }

            return rowCount;
        }

        public void SqlToTable(string QueryString, string DestinationConnectionString, string TableName, int BatchSize = 1000, int Timeout=3600)
        {

            using (SqlConnection sqlconn = new SqlConnection(ConnectionString))
            {
                sqlconn.Open();
                using (SqlCommand sqlcmd = new SqlCommand())
                {
                    sqlcmd.CommandTimeout = 3600;  //setting query timeout for 1 hour
                    sqlcmd.Connection = sqlconn;
                    sqlcmd.CommandText = QueryString;

                    using (SqlDataReader sqldr = sqlcmd.ExecuteReader())
                    {
                       
                        using (SqlBulkCopy bcp = new SqlBulkCopy(DestinationConnectionString))
                        {
                            bcp.BatchSize = BatchSize;
                            bcp.BulkCopyTimeout = Timeout;
                            bcp.DestinationTableName = $"{TableName}";
                            bcp.WriteToServer(sqldr);
                        }                        
                    }

                }

                sqlconn.Close();
            }
        }
        public void CsvToTable(string ServerName, string DbName, string TableName)
        {

        }

        public string ConnectionString { get; set; }

        public Logger Log { get;  }

        public void Dispose() { }
   
    }
}
