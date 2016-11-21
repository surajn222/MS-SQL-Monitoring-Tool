using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Collections;
using System.IO;
using System.Net.Mail;



namespace HealthcheckApplication
{
    class checkParam
    {
        //d = connection string, p = path, c= entire value from key-value pair
        public static String checkTotalMemory(string d, string p, string c, SqlConnection connsql)
        {
            //definitions
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;
 
            //query to get Target Memory
            string q = "SELECT cntr_value/1024 FROM sys.dm_os_performance_counters WHERE counter_name = 'Total Server Memory (KB)'";
            Double qd = 0.00;
            
            //Connection to Data Adapter, get data and store in 'a'
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    //Define a DataTable
                    DataTable t = new DataTable();
                    //From Data Adapter to Data Table
                    a.Fill(t);

                    //loop through each row in Data Table
                    foreach (DataRow row in t.Rows)
                    {
                        //store DateTime in 'text' string
                        text = text + DateTime.Now.ToString() + ";";

                        //loop through each column of the above row
                        foreach (DataColumn col in t.Columns)
                        {
                            //get each column Name and store that in data[start,column]
                            data[start, column] = row[col.ColumnName].ToString();
                            //append that to 'text' string
                            text = text + row[col.ColumnName].ToString() + ";";
                        }
                        text = text + Environment.NewLine;
                        //Take the Total Memory value from key-value pair
                        qd = Convert.ToDouble(text.Split(';')[1].ToString());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
           
            

            //definitions
            String content = String.Empty;

            //define file name and write to file
            String file = @p + "totalMemory.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
                string newContent = content + text;
                File.WriteAllText(file, newContent);
            }
                //end of write to file            
            
            
            //Comparing the current value with the threshold value
            Double basememory = Convert.ToDouble(c.Split(',')[4]);

            //Checking the condition and sending the email
            //qd = Set memory of the SQL Server, base memory = Current memory of the SQL Server
            //checks if the deviation of 'base memory'(Current memory) from the 'qd'(Set Memory) is >20% or <(-20%)
            if (((((qd - basememory) / basememory) * 100) > 20) || ((((qd - basememory) / basememory) * 100) < (-20)))
            {
                try
                {
                    //creates sting for emailing the data that was received in Data Table defined above
                    String subject = "UNUSUAL TOTAL MEMORY";
                    String emailtext = "CRITICALITY : HIGH <br >"
                                       + "Server IP Address: " + d.Split(';')[0].Split('=')[1]
                                       + "Total Memory: " + qd + "<br>";
                    //Creating an object to send email
                    ReportMailer mailer = new ReportMailer(0, 0, emailtext, subject);
                    //Send Email
                    mailer.SendMail();
                    Console.WriteLine("Mail Sent");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return text;
        }

        public static String checkTargetMemory(string d, string p, string c, SqlConnection connsql)
        {
            int retry = 3;
            trytotalmemory:
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;
           
            //Query
            string q = "SELECT cntr_value/1024 FROM sys.dm_os_performance_counters WHERE counter_name = 'Target Server Memory (KB)'";
            Double qd = 0.00;
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                Console.WriteLine("In");
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);
 
                    foreach (DataRow row in t.Rows)                   
                    {
                        text = text + DateTime.Now.ToString() + ";";

                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                            Console.WriteLine(row[col.ColumnName].ToString());

                            text =  text + row[col.ColumnName].ToString() + ";";
                        }
                        text = text + Environment.NewLine;
                        qd = Convert.ToDouble(text.Split(';')[1].ToString());
                        Console.WriteLine("Target Memory = " + qd);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            
            String content = String.Empty;
            String file = @p + "targetMemory.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
                string newContent = content + text;
                File.WriteAllText(file, newContent);
            }
            Double basememory = Convert.ToDouble(c.Split(',')[5]);

            if(((((qd - basememory) /basememory)*100)>20) || ((((qd - basememory) /basememory)*100)<(-20)))
            {

                try
                {
                    String subject = "UNUSUAL TARGET MEMORY";
                    String emailtext = "CRITICALITY : HIGH <br >"
                                       + "Server IP Address: " + d.Split(';')[0].Split('=')[1]
                                       + "Target Memory: " + qd + "<br>";
                    ////Console.WriteLine("Sending E-Mail at this time : {0}", DateTime.Now);
                    ReportMailer mailer = new ReportMailer(0, 0, emailtext, subject);
                    mailer.SendMail();
                    Console.WriteLine("Mail Sent");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                 }
            }
            return text;
        }

        //done
        public static String checkFullScans(string d, string p, string c, SqlConnection connsql)
        {
            //definitions
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

           

            //query
            string q = @" 
 Select (SELECT cntr_value FROM sys.dm_os_performance_counters where counter_name like 'Index%'),(SELECT cntr_value FROM sys.dm_os_performance_counters where counter_name like 'Index%'),(Select cast((SELECT cntr_value FROM sys.dm_os_performance_counters where counter_name like 'Index%') as float)/ cast((SELECT cntr_value FROM sys.dm_os_performance_counters WHERE counter_name = 'Full Scans/sec') as float))";            
            
            Double q2 = 0.00;

            //getting data
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);
                    foreach (DataRow row in t.Rows)
                    {
                        text = text + DateTime.Now.ToString() + ";";

                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                            text = text + row[col.ColumnName].ToString() + ";";
                        }
                        text = text + Environment.NewLine;
                        q2 = Convert.ToDouble(text.Split(';')[3].ToString());
                    }
                }
                catch (Exception e)
                {
                    ////Console.WriteLine(e.Message);
                }
            }
           
            

            //append to file
            String content = String.Empty;
            String file = @p + "fullScans.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
                string newContent = content + text;
                File.WriteAllText(file, newContent);
            }            
            //check condition and send email
            if ((q2) > 1000)
            {
                try
                {
                    String emailtext = "Full Page Scans High <br >" 
                                     + "IP Address" + d.Split(';')[0].Split('=')[1]
                                     + "Value: " + q2;
                    ReportMailer mailer = new ReportMailer(0, 0, emailtext);
                    mailer.SendMail();
                 }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return text;
        }

        //done
        public static String checkAveragelatchWaitTime(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            //Query
            string q = "SELECT cntr_value/1000 FROM sys.dm_os_performance_counters WHERE counter_name = 'Average Latch Wait Time (ms)'";
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);

                    foreach (DataRow row in t.Rows)
                    {
                        text = text + DateTime.Now.ToString() + ";";
                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                            text = text + row[col.ColumnName].ToString() + ";";
                        }
                        text = text + Environment.NewLine;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            

            String content = String.Empty;
            String file = @p + "averageLatchWaitTime.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
                string newContent = content + text;
                File.WriteAllText(file, newContent);
            }
            return text;
        }


        //done
        public static String checkLatchWaitTimeperQuery(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[100, 100];
            int start = 0;
            int column = 0;

            string q = @"select session_id, 
	status, 
	command,
	blocking_session_id,
	wait_type, 
	wait_time,
	last_wait_type,
	wait_resource
from sys.dm_exec_requests r
where r.session_id >= 50
and r.session_id <> @@spid and wait_time>300";

            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);
               
                    foreach (DataRow row in t.Rows)
                    {
                        text = text + DateTime.Now.ToString() + ";";
                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                            text = text + row[col.ColumnName].ToString() + ";";
                        }
                        text = text + Environment.NewLine;
                        Double thresholdvalue = Convert.ToDouble(text.Split(';')[5].ToString());

                        if (thresholdvalue > 300)
                        {
                            try
                            {
                                String subject = "HIGH LATCH WAIT TIME";
                                String emailtext = "Criticality : High <br>" 
                                                       + "IP Address" + d.Split(';')[0].Split('=')[1]                                    
                                                       + "Value:" + thresholdvalue;
                                ReportMailer mailer = new ReportMailer(0, 0, emailtext, subject);
                                mailer.SendMail();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            

            String content = String.Empty;
            String file = @p + "latchWaitTimePerQuery.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
                string newContent = content + text;
                File.WriteAllText(file, newContent);
            }
            return text;
        }

        //done
        public static String checkLockTimeperQuery(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[100, 100];
            int start = 0;
            int column = 0;

            //query
            string q = @"SELECT  req.session_id   
       ,ses.host_name
       ,DB_NAME(req.database_id) AS DB_NAME
       ,ses.login_name
       ,req.status
       ,req.command   
       ,req.total_elapsed_time / 1000.0 AS total_elapsed_time
       ,req.wait_type
       ,sqltext.text
FROM    sys.dm_exec_requests req
CROSS APPLY sys.dm_exec_sql_text(sql_handle) AS sqltext
JOIN    sys.dm_exec_sessions ses
        ON ses.session_id = req.session_id
WHERE req.wait_type IS NOT NULL and req.total_elapsed_time/1000.0 > '1800'";

            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);
                   
                    foreach (DataRow row in t.Rows)
                    {
                        text = text + DateTime.Now.ToString() + ";";

                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                   
                            text = text + row[col.ColumnName].ToString() + ";";
                        }
                        text = text + Environment.NewLine;
                   
                            try
                            {   
                                //email
                                String subject = "HIGH LOCK WAIT TIME";
                                String emailtext = "Criticality : High <br>"
                                                    + "IP Address" + d.Split(';')[0].Split('=')[1]
                                                    + "Value:"  + text.Split(';')[8]
                                                    + "Text: "   + text.Split(';')[9];
                                ReportMailer mailer = new ReportMailer(0, 0, emailtext, subject);
                                mailer.SendMail();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                    }
                }
                catch (Exception e)
                {
                    ////Console.WriteLine(e.Message);
                }
            }
            

            //write to file
            String content = String.Empty;
            String file = @p + "lockWaitTimePerQuery.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
                string newContent = content + text;
                File.WriteAllText(file, newContent);
            }
            return text;
        }

        //done
        public static String checkAverageLockWaitTime(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            string q = "SELECT instance_name,cntr_value FROM sys.dm_os_performance_counters WHERE counter_name = 'Average Wait Time (ms)'";
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);

                    foreach (DataRow row in t.Rows)
                    {
                        text = text + DateTime.Now.ToString() + ";";
                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                            text = text + row[col.ColumnName].ToString() + ";";
                        }
                        text = text + Environment.NewLine;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            

            String content = String.Empty;
            String file = @p + "averageLockWaitTime.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
                string newContent = content + text;
                File.WriteAllText(file, newContent);
            }
            return text;
        }
        
        //done
        public static String checkActiveTransactionsPerSecond(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[100, 100];
            int start = 0;
            int column = 0;

            string q = @"SELECT object_name,instance_name,cntr_value
    FROM sys.dm_os_performance_counters 
    WHERE counter_name = 'transactions/sec' 
        AND object_name = 'SQLServer:Databases'";
            
            
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);
                    String check = String.Empty;
                    foreach (DataRow row in t.Rows)
                    {
                        text = text + DateTime.Now.ToString() + ";";
                        
                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                            check = check + row[col.ColumnName].ToString() + ";";
                            text = text + row[col.ColumnName].ToString() + ";";
                        }
                        text = text + Environment.NewLine;
                        Double thresholdvalue = Convert.ToDouble(check.Split(';')[2].ToString());

                        if(thresholdvalue > 10000000000)
                        {
                            try 
                            {
                            String subject = "High Acive Transactions per second";
                            String emailtext = "Criticality : Low <br>" +
                                                "Server IP Address:     " + (d.Split(';')[0]).Split('=')[1] + "<br />" +
                                                "Database Name: " + check.Split(';')[1].ToString()
                                                + "Value:" + thresholdvalue;
                             ReportMailer mailer = new ReportMailer(0, 0, emailtext, subject);
                             mailer.SendMail();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            
          
            String content = String.Empty;
            String file = @p + "activeTransactions.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
                string newContent = content + text;
                File.WriteAllText(file, newContent);
            }
            return text;
        }

        //done
        public static String checkNumberOfActiveTransactions(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[100, 100];
            int start = 0;
            int column = 0;

            string q = "Select COUNT(*) from sys.dm_tran_active_transactions";

            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);

                    foreach (DataRow row in t.Rows)
                    {
                        text = text + DateTime.Now.ToString() + ";";
                        String check = String.Empty;
                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                            text = text + row[col.ColumnName].ToString() + ";";
                            check = check + row[col.ColumnName].ToString() + ";";
                        }
                        text = text + Environment.NewLine;
                        Double thresholdvalue = Convert.ToDouble(text.Split(';')[1].ToString());
                        if (thresholdvalue > 1000)
                        {
                            try
                            {
                                String subject = "High Number of Active Transactions";
                                String emailtext = "Criticality : High <br>"  
                                                  + "IP Address" + d.Split(';')[0].Split('=')[1] 
                                                    +"Value:" + thresholdvalue;
                                ReportMailer mailer = new ReportMailer(0, 0, emailtext, subject);
                                mailer.SendMail();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            

            String content = String.Empty;
            String file = @p + "numberOfActiveTransactions.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
                string newContent = content + text;
                File.WriteAllText(file, newContent);
            }
            return text;
        }

        //done
        public static String checkNumberOfActiveConnections(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            string q = @"SELECT DB_NAME(dbid) as DBName,COUNT(dbid) as NumberOfConnections,loginame as LoginName FROM sys.sysprocesses WHERE dbid > 0 GROUP BY dbid, loginame";
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);
                    foreach (DataRow row in t.Rows)
                    {
                        text = text + DateTime.Now.ToString() + ";";
                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                            text = text + row[col.ColumnName].ToString() + ";";
                        }
                        text = text + Environment.NewLine;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            

            String content = String.Empty;
            String file = @p + "numberOfActiveConnections.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
                string newContent = content + text;
                File.WriteAllText(file, newContent);

            } return text;
        }

        //done
        public static String checklongRunningQueries(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            string q = @"SELECT text,CAST((total_elapsed_time/3600000.00) AS FLOAT) FROM sys.dm_exec_requests CROSS APPLY sys.dm_exec_sql_text(sql_handle) where total_elapsed_time > 7200000";
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);
            
                    if (t.Rows.Count > 0)
                    {
                        foreach (DataRow row in t.Rows)
                        {
                            String datatext = string.Empty;
                            datatext = DateTime.Now.ToString() + "|";

                            foreach (DataColumn col in t.Columns)
                            {
                                data[start, column] = row[col.ColumnName].ToString();
            
                                datatext = datatext + row[col.ColumnName].ToString() + "|";

                            }

                            try
                            {
                                String[] param = datatext.Split('|');
                                String subject = "Long Running Query Alert";
                                String emailtext = "Long Running Query Alert <br />" +
                                                "Server IP Address:     " + (d.Split(';')[0]).Split('=')[1] + "<br />" +
                                                 "Time:      " + param[0] + "<br />" +
                                                 "Query:       " + param[1] + "<br />" +
                                                 "Time In Hours: " + param[2] + " hours <br />";
                                ReportMailer mailer = new ReportMailer(0, 0, emailtext, subject);
                                mailer.SendMail();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                            text = datatext + Environment.NewLine;
                        }

                    String content = String.Empty;
                    String file = @p + "longRunningQueries.txt";
                    if (File.Exists(file))
                    {
                        content = File.ReadAllText(file);
                        string newContent = content + text;
                        File.WriteAllText(file, newContent);
                    }
                   }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                
                return text;
            }
        }


        //This method is no longer used. Physical Memory is logged with WMI
        public static String checkPhysicalMemory(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            string q = @"SELECT ((available_physical_memory_kb/1024.00)/(total_physical_memory_kb/1024.00)*100) FROM sys.dm_os_sys_memory where ((available_physical_memory_kb/1024.00)/(total_physical_memory_kb/1024.00)*100)< 5.00";
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);

                    if (t.Rows.Count > 0)
                    {
                        foreach (DataRow row in t.Rows)
                        {
                            text = text + DateTime.Now.ToString() + ";";

                            foreach (DataColumn col in t.Columns)
                            {
                                data[start, column] = row[col.ColumnName].ToString();
                                text = text + row[col.ColumnName].ToString() + ";";
                            }
                            text = text + Environment.NewLine;
                        }

                    String content = String.Empty;
                    String file = @p + "physicalMemory.txt";
                    if (File.Exists(file))
                    {
                        content = File.ReadAllText(file);
                        string newContent = content + text;
                        File.WriteAllText(file, newContent);
                    }
                    //try
                    //{
                    //    ////Console.WriteLine("Sending E-Mail at this time : {0}", DateTime.Now);
                    //    String emailtext = "Physical Memory On Server Remaining < 5%" + "<br />" + d + "<br />" + text + " % " + "<br />";
                    //    ReportMailer mailer = new ReportMailer(0, 0, emailtext);
                    //    mailer.SendMail();
                    //    ////Console.WriteLine("Mail Sent");
                    //    //Console.ReadLine();
                    //}
                    //catch (Exception e)
                    //{
                    //    ////Console.WriteLine(e.Message);
                    //    //Console.ReadLine();
                    //}
                }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                
                return text;
            }
        }

        //done
        public static String checkBlockingQueries(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            string q = @"SELECT  session_id ,blocking_session_id ,wait_time ,wait_type ,last_wait_type ,wait_resource ,transaction_isolation_level ,lock_timeout FROM sys.dm_exec_requests WHERE blocking_session_id <> 0";
            
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);

                    foreach (DataRow row in t.Rows)
                    {
                        String datatext = String.Empty;
                        text = text + DateTime.Now.ToString() + ";";
                         datatext = datatext + DateTime.Now.ToString() + ";";
                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                            ////Console.WriteLine(row[col.ColumnName].ToString());

                            text = text + row[col.ColumnName].ToString() + ";";
                            datatext = datatext + row[col.ColumnName].ToString() + ";";
                        }
                        text = text + Environment.NewLine;

                        Double threshold = Convert.ToDouble(datatext.Split(';')[3].ToString());

                        if ((threshold / 1000) > 300)
                        {
                            try
                            {
                                Console.WriteLine("Sending E-Mail for Blocking Queries : {0}", DateTime.Now);
                                String subject = "Blocking Queries";
                                String emailtext = "Criticality : Low" + "<br />"
                                    + "Server IP Address " + d.Split(';')[0].Split('=')[1] + "<br />"
                                    + "Session Id : " + datatext.Split(';')[1] + "<br />"
                                     + "Blocking Session Id : " + datatext.Split(';')[2] + "<br />"
                                      + "Wait Time : " + datatext.Split(';')[3] + "<br />";

                                ReportMailer mailer = new ReportMailer(0, 0, emailtext, subject);
                                mailer.SendMail();
                                ////Console.WriteLine("Mail Sent");
                                //Console.ReadLine();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Here");
                                Console.WriteLine(e.Message);
                                //Console.ReadLine();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            

            String content = String.Empty;
            String file = @p + "blockingQueries.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
                string newContent = content + text;
                File.WriteAllText(file, newContent);
            }
            return text;
        }

        //This method is no longer used. Average Disk Read is logged with WMI
        public static String checkAverageDiskRead(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            string q = "SELECT * FROM sys.dm_io_virtual_file_stats(- 1, - 1) stat";

            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);

                    foreach (DataRow row in t.Rows)
                    {
                        text = text + DateTime.Now.ToString() + ";";

                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                            text = text + row[col.ColumnName].ToString() + ";";
                        }
                        text = text + Environment.NewLine;

                        String[] param = text.Split(';'); 
                        //try
                        //{
                        //    String emailtext = "Blocking Queries <br />" 
                        //                        + "IP Address: " + d.Split(';')[0].Split('=')[1] + "<br />"
                        //                        + "Time: " + param[0] + "<br />"
                        //                         + "Blocking Session: " + param[2] + "<br />"
                        //                         + "Wait Time: " + param[3] + "<br />";

                        //    ////Console.WriteLine("Sending E-Mail at this time : {0}", DateTime.Now);
                        //    ReportMailer mailer = new ReportMailer(0, 0, emailtext);
                        //    mailer.SendMail();
                        //    ////Console.WriteLine("Mail Sent");
                        //    //Console.ReadLine();
                        //}
                        //catch (Exception e)
                        //{
                        //    //Console.WriteLine(e.Message);
                        //    //Console.ReadLine();
                        //}



                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            

            String content = String.Empty;
            String file = @p + "averageDiskRead.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
                string newContent = content + text;
                File.WriteAllText(file, newContent);

            }
            return text;

        }

        //This method is no longer used. Average Disk Write is logged with WMI
        public static String checkAverageDiskWrite(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            string q = "SELECT * FROM sys.dm_io_virtual_file_stats(- 1, - 1) stat";
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);
                    ////Console.WriteLine();

                    foreach (DataRow row in t.Rows)
                    {
                        text = text + DateTime.Now.ToString() + ";";
                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                            text = text + row[col.ColumnName].ToString() + ";";
                        }
                        text = text + Environment.NewLine;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            

            String content = String.Empty;
            String file = @p + "averageDiskWrite.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
            string newContent = content + text;
            File.WriteAllText(file, newContent);
            }
            return text;
        }

        //This method is no longer used. Physical Memory is logged with WMI
        public static String logPhysicalMemory(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            string q = @"SELECT ((available_physical_memory_kb/1024.00)/(total_physical_memory_kb/1024.00)*100) FROM sys.dm_os_sys_memory";
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);

                    if (t.Rows.Count > 0)
                    {
                        foreach (DataRow row in t.Rows)
                        {
                            text = text + DateTime.Now.ToString() + ";";

                            foreach (DataColumn col in t.Columns)
                            {
                                data[start, column] = row[col.ColumnName].ToString();
                                text = text + row[col.ColumnName].ToString() + ";";
                            }
                            text = text + Environment.NewLine;
                        }
                    }


                    

                    String content = String.Empty;
                    String file = @p + "logphysicalMemory.txt";
                    if (File.Exists(file))
                    {
                        content = File.ReadAllText(file);
                        string newContent = content + text;
                        File.WriteAllText(file, newContent);
                }
                    //try
                    //{
                    //    ////Console.WriteLine("Sending E-Mail at this time : {0}", DateTime.Now);
                    //    ReportMailer mailer = new ReportMailer(0, 0, text);
                    //    mailer.SendMail();
                    //    ////Console.WriteLine("Mail Sent");
                    //    //Console.ReadLine();
                    //}
                    //catch (Exception e)
                    //{
                    //    ////Console.WriteLine(e.Message);
                    //    //Console.ReadLine();
                    //}

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return text;
            }
        }

        //done
        public static String checkUnusualJobDuration(string d, string p, string c, SqlConnection connsql)
        {
            //definitions
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            //query
            string q = @"

;WITH temp1 (jobname, average )as
(
Select JobName , AVG(Duration2) as average 
 from (
select 
 j.name as 'JobName',
 run_date,
 run_time,
 msdb.dbo.agent_datetime(run_date, run_time) as 'RunDateTime',
 run_duration,

 
 
CASE len(h.run_duration)
WHEN 1 THEN h.run_duration
WHEN 2 THEN h.run_duration

WHEN 3 THEN cast(
(Left(right(h.run_duration,3),1) * 60)
+ right(h.run_duration,2) as int)

WHEN 4 THEN cast( 
(Left(right(h.run_duration,4),2) * 60)
+ right(h.run_duration,2) as int)

WHEN 5 THEN cast( 
+ Left(right(h.run_duration,5),1) * 60 * 60
+ Left(right(h.run_duration,4),2) * 60
+ right(h.run_duration,2) as int)

WHEN 6 THEN cast(Left(right(h.run_duration,6),2) 
+Left(right(h.run_duration,4),2) 
+ right(h.run_duration,2) as int)
END as 'Duration2',

 
 --AVG(run_duration) as AVERAGE,
 --run_duration,
 --((run_duration/10000*3600 + (run_duration/100)%100*60 + run_duration%100 + 31 ) / 60)   as 'RunDurationMinutes'
row_number() over(partition by j.name order by msdb.dbo.agent_datetime(run_date, run_time) desc) as rn
From msdb.dbo.sysjobs j 
INNER JOIN msdb.dbo.sysjobhistory h 
 ON j.job_id = h.job_id 
 
where j.enabled = 1 and step_id='0') 
as a 
where rn <=10

group by JobName 
--group by j.name
),
 temp2 ( job_id, job_name, job_execution_date,current_executed_step_id , step_name, timeelapsed) as
(

SELECT
    ja.job_id,
    j.name AS job_name,
    ja.start_execution_date,      
    ISNULL(last_executed_step_id,0)+1 AS current_executed_step_id,
    Js.step_name,
    DATEDIFF(SECOND, ja.start_execution_date, GETDATE()) as timeelapsed
    
FROM msdb.dbo.sysjobactivity ja 
LEFT JOIN msdb.dbo.sysjobhistory jh 
    ON ja.job_history_id = jh.instance_id
JOIN msdb.dbo.sysjobs j 
ON ja.job_id = j.job_id
JOIN msdb.dbo.sysjobsteps js
    ON ja.job_id = js.job_id
    AND ISNULL(ja.last_executed_step_id,0)+1 = js.step_id
WHERE ja.session_id = (SELECT TOP 1 session_id FROM msdb.dbo.syssessions ORDER BY agent_start_date DESC)
AND start_execution_date is not null
AND stop_execution_date is null

)

Select job_name, temp2.job_execution_date,average ,timeelapsed,((cast((timeelapsed - average)as float)/average)  *100) as percentage from temp1 inner join temp2 on temp1.jobname = temp2.job_name 



 where (( (timeelapsed-average)>=1800  and (average)>=0 and (average)<=1800  ) 
 
 or ((((timeelapsed-average)/CAST(average as float))*100) >= 100 and (average)>1800 and (average)<=7200  ) 
 
or  ((((timeelapsed-average)/CAST(average as float))*100) >= 50 and (average)>7200   ) )
 
 and average != 0"
                ;

            //connect and get data
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);
                    if (t.Rows.Count > 0)
                    {
                        foreach (DataRow row in t.Rows)
                        {
                            String datatext = String.Empty;
                            datatext = DateTime.Now.ToString() + "|";

                            foreach (DataColumn col in t.Columns)
                            {
                                data[start, column] = row[col.ColumnName].ToString();
                                datatext = datatext + row[col.ColumnName].ToString() + "|";
                            }

                            try
                            {   
                                //email
                                String[] param = datatext.Split('|');
                                String subject = "UNUSUAL JOB DURATION";
                                String emailtext = "Criticality : High <br />" + "IP Address: " + d.Split(';')[0].Split('=')[1] + "<br />"
                                                                                 + "Time: " + param[0] + "<br />"
                                                                                 + "Name: " + param[1] + "<br />"
                                                                                 + "Job Start Time (yyyymmdd hhmmss): " + param[2] + "<br />"
                                                                                  + "Average in seconds :" + param[3] + "<br />"
                                                                                  + "Current run duration in seconds : " + param[4] + "<br />"
                                                                                  + "Excess in percentage: " + param[5] + "<br />";
                                                                                  

                                ReportMailer mailer = new ReportMailer(0, 0, emailtext, subject);
                                mailer.SendMail();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }

                             text = datatext + Environment.NewLine;
                        }
                        //write to file
                        String content = String.Empty;
                        String file = @p + "unusualJobDuration.txt";
                        if (File.Exists(file))
                        {
                            content = File.ReadAllText(file);

                            string newContent = content + text;
                            File.WriteAllText(file, newContent);
                        }
                     }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                //connection close
                
                return text;
            }
        }

        //done
        public static String checkSeverity(string d, string p, string c, SqlConnection connsql)
        {
            //definitions
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            //query
            string q = @"

        set nocount on 
        Declare @CurrentDate datetime
        Declare @DatelessHour datetime
        Set @CurrentDate = getdate()
        Set @DatelessHour = DATEADD(mi,-5,@CurrentDate)
        
        --create table #el90 (LogDate datetime, ProcessInfo varchar(20), log_text varchar(7000))
        --insert #el90 (LogDate, ProcessInfo, log_text) EXEC xp_ReadErrorLog 0, 1, NULL, NULL, @DatelessHour, @CurrentDate;
        
        create table #el80 (LogDate datetime, ProcessInfo varchar(20), log_text varchar(7000))
        insert #el80 (LogDate, ProcessInfo, log_text) EXEC xp_ReadErrorLog 0, 1, 'Severity: 16', NULL, @DatelessHour, @CurrentDate;
        insert #el80 (LogDate, ProcessInfo, log_text) EXEC xp_ReadErrorLog 0, 1, 'Severity: 17', NULL, @DatelessHour, @CurrentDate;
        insert #el80 (LogDate, ProcessInfo, log_text) EXEC xp_ReadErrorLog 0, 1, 'Severity: 18', NULL, @DatelessHour, @CurrentDate;
        insert #el80 (LogDate, ProcessInfo, log_text) EXEC xp_ReadErrorLog 0, 1, 'Severity: 19', NULL, @DatelessHour, @CurrentDate;
        insert #el80 (LogDate, ProcessInfo, log_text) EXEC xp_ReadErrorLog 0, 1, 'Severity: 20', NULL, @DatelessHour, @CurrentDate;
        insert #el80 (LogDate, ProcessInfo, log_text) EXEC xp_ReadErrorLog 0, 1, 'Severity: 21', NULL, @DatelessHour, @CurrentDate;
        insert #el80 (LogDate, ProcessInfo, log_text) EXEC xp_ReadErrorLog 0, 1, 'Severity: 22', NULL, @DatelessHour, @CurrentDate;
        insert #el80 (LogDate, ProcessInfo, log_text) EXEC xp_ReadErrorLog 0, 1, 'Severity: 23', NULL, @DatelessHour, @CurrentDate;
	    insert #el80 (LogDate, ProcessInfo, log_text) EXEC xp_ReadErrorLog 0, 1, 'Severity: 24', NULL, @DatelessHour, @CurrentDate;
	    insert #el80 (LogDate, ProcessInfo, log_text) EXEC xp_ReadErrorLog 0, 1, 'Severity: 25', NULL, @DatelessHour, @CurrentDate;
        insert #el80 (LogDate, ProcessInfo, log_text) EXEC xp_ReadErrorLog 0, 1, 'Incorrect checksum', NULL, @DatelessHour, @CurrentDate;
        insert #el80 (LogDate, ProcessInfo, log_text) EXEC xp_ReadErrorLog 0, 1, 'I/O requests', NULL, @DatelessHour, @CurrentDate;
        
        
        Select * from #el80 order by ProcessInfo
        --Select * from #el90 where ProcessInfo in (Select distinct ProcessInfo from #el80) order by ProcessInfo
        
        --drop table #el90;
        drop table #el80;
        
        
       
        ";

            //connect and get data
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);
                    if (t.Rows.Count > 0)
                    {
                        foreach (DataRow row in t.Rows)
                        {
                            string datatext = "";
                            datatext = datatext + DateTime.Now.ToString() + ";";
                            foreach (DataColumn col in t.Columns)
                            {
                                data[start, column] = row[col.ColumnName].ToString();
                                datatext = datatext + row[col.ColumnName].ToString() + ";";
                            }
                            text = datatext + Environment.NewLine;
                            Console.WriteLine(text);
                            try
                            {
                                String subject = "SEVERITY ERROR";
                                String emailtext = "CRITICALITY : HIGH <br>"
                                                + "IP Address :  " + d.Split(';')[0].Split('=')[1] + "<br>"
                                                + "Time:" + datatext.Split(';')[0] + "<br>"
                                                + "Error Log Time: " + datatext.Split(';')[1] + "<br>"
                                                + " Processor Info :" + datatext.Split(';')[2] + "<br>"
                                                + "Error Log Text: " + datatext.Split(';')[3] + "<br>";
                                ReportMailer mailer = new ReportMailer(0, 0, emailtext, subject);
                                mailer.SendMail();
  
                                String content = String.Empty;
                                String file = @p + "severity.txt";
                                if (File.Exists(file))
                                {
                                    content = File.ReadAllText(file);

                                    string newContent = content + text;
                                    File.WriteAllText(file, newContent);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                    }
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return text;
            }
        }

        //This method is no longer used. Not required
        public static String checkerror825(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            //Query
            string q = @"EXEC xp_ReadErrorLog 0, 1, 'incorrect checksum';";
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);
                    ////Console.WriteLine();


                    if (t.Rows.Count > 0)
                    {
                        foreach (DataRow row in t.Rows)
                        {
                            text = text + DateTime.Now.ToString() + ";";
                            foreach (DataColumn col in t.Columns)
                            {
                                data[start, column] = row[col.ColumnName].ToString();
                                text = text + row[col.ColumnName].ToString() + ";";
                            }
                            text = text + Environment.NewLine;
                        }
                    }


                    

                    String content = String.Empty;
                    String file = @p + "error825.txt";
                    if (File.Exists(file))
                    {
                        content = File.ReadAllText(file);
                        string newContent = content + text;
                        File.WriteAllText(file, newContent);
                    }
                    //try
                    //{
                    //    ////Console.WriteLine("Sending E-Mail at this time : {0}", DateTime.Now);
                    //    ReportMailer mailer = new ReportMailer(0, 0, text);
                    //    mailer.SendMail();
                    //    ////Console.WriteLine("Mail Sent");
                    //    //Console.ReadLine();
                    //}
                    //catch (Exception e)
                    //{
                    //    ////Console.WriteLine(e.Message);
                    //    //Console.ReadLine();
                    //}

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                return text;
            }
        }

        //This method is no longer used. This is logged with WMI
        public static String checkProcessorUtilization(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            string q = @"
DECLARE @ts_now bigint = (SELECT cpu_ticks/(cpu_ticks/ms_ticks)FROM sys.dm_os_sys_info);

SELECT TOP(144) SQLProcessUtilization AS [SQL Server Process CPU Utilization],
               SystemIdle AS [System Idle Process],
               100 - SystemIdle - SQLProcessUtilization AS [Other Process CPU Utilization],
               DATEADD(ms, 1 * (@ts_now - [timestamp]), GETDATE()) AS [Event Time]
				FROM (
                  SELECT record.value('(./Record/@id)[1]', 'int') AS record_id,
                                                record.value('(./Record/SchedulerMonitorEvent/SystemHealth/SystemIdle)[1]', 'int')
                                                AS [SystemIdle],
                                                record.value('(./Record/SchedulerMonitorEvent/SystemHealth/ProcessUtilization)[1]',
                                                'int')
                                                AS [SQLProcessUtilization], [timestamp]
												FROM (
                                                SELECT [timestamp], CONVERT(xml, record) AS [record]
                                                FROM sys.dm_os_ring_buffers
                                                WHERE ring_buffer_type = N'RING_BUFFER_SCHEDULER_MONITOR'
                                                AND record LIKE N'%<SystemHealth>%') AS x
                  ) AS y
ORDER BY [Event Time] DESC OPTION (RECOMPILE);";
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);

                    if (t.Rows.Count > 0)
                    {
                        foreach (DataRow row in t.Rows)
                        {
                            text = text + DateTime.Now.ToString() + ";";

                            foreach (DataColumn col in t.Columns)
                            {
                                data[start, column] = row[col.ColumnName].ToString();
                                text = text + row[col.ColumnName].ToString() + ";";
                            }
                            text = text + Environment.NewLine;
                        }
                    }


                    

                    String content = String.Empty;
                    String file = @p + "processorUtilizations.txt";
                    if (File.Exists(file))
                    {
                        content = File.ReadAllText(file);
                    string newContent = content + text;
                    File.WriteAllText(file, newContent);
                }
                    //try
                    //{
                    //    ////Console.WriteLine("Sending E-Mail at this time : {0}", DateTime.Now);
                    //    ReportMailer mailer = new ReportMailer(0, 0, text);
                    //    mailer.SendMail();
                    //    ////Console.WriteLine("Mail Sent");
                    //    //Console.ReadLine();
                    //}
                    //catch (Exception e)
                    //{
                    //    ////Console.WriteLine(e.Message);
                    //    //Console.ReadLine();
                    //}

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                return text;
            }
        }
   
        //done
        public static String checkDeadLocks(string d, string p, string c, SqlConnection connsql)
        {
            //definitions
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            //query
            string q = @"
SELECT Blocker.text, BlockedReqs.wait_time, Conns.client_net_address
FROM sys.dm_exec_connections AS Conns
INNER JOIN sys.dm_exec_requests AS BlockedReqs
    ON Conns.session_id = BlockedReqs.blocking_session_id
INNER JOIN sys.dm_os_waiting_tasks AS w
    ON BlockedReqs.session_id = w.session_id
CROSS APPLY sys.dm_exec_sql_text(Conns.most_recent_sql_handle) AS Blocker


";
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);

                    String datatext = String.Empty;
                    if (t.Rows.Count > 0)
                    {
                        foreach (DataRow row in t.Rows)
                        {
                            text = text + DateTime.Now.ToString() + ";";
                            datatext = datatext + DateTime.Now.ToString() + ";";
                            foreach (DataColumn col in t.Columns)
                            {
                                data[start, column] = row[col.ColumnName].ToString();
                                text = text + row[col.ColumnName].ToString() + ";";
                                datatext = datatext + row[col.ColumnName].ToString() + ";";
                            }
                            text = text + Environment.NewLine;
                  
                   
                    //check
                     Double threshold = Convert.ToDouble(datatext.Split(';')[2]);             
                    if((threshold/1000) >300)
                    {
                    try
                    {
                        String subject = "Deadlocks";
                        String emailtext = "Criticality : Medium " + "<br />"
                                          + "IP Address :  " + d.Split(';')[0].Split('=')[1]  + "<br />"
                                          + "Query :     "  + text + "<br />";
                        ReportMailer mailer = new ReportMailer(0, 0, emailtext, subject);
                        mailer.SendMail();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    }
                  }
                }
                    
                    //write to file
                     String content = String.Empty;
                    String file = @p + "deadLocks.txt";
                    if (File.Exists(file))
                    {
                        content = File.ReadAllText(file);
                 
                    string newContent = content + text;
                    File.WriteAllText(file, newContent);
                }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                
                return text;
            }
        }
        

        //this method is no longer used. Not requireds
        public static String checkDiskLatencyCritical(string d, string p, string c, SqlConnection connsql)
       {
       //definitions
       string[] connList = new string[3];
       string text = "";
       String[,] data = new String[10, 10];
       int start = 0;
       int column = 0;

       //this is old query
       //string q = "SELECT database_id,file_id,io_stall_read_ms / NULLIF(num_of_reads, 0) AS 'AVG READ Time (Transfer/msec)',io_stall_write_ms / NULLIF(num_of_writes, 0) AS 'AVG WRITE Time (Transfer/msec)' FROM sys.dm_io_virtual_file_stats(- 1, - 1) stat where io_stall_read_ms / NULLIF(num_of_reads, 0)> 1000 or io_stall_write_ms / NULLIF(num_of_reads, 0) > 1000";
       
       //this is the udpated query 
       string q = "SELECT database_id,file_id,io_stall_read_ms / NULLIF(num_of_reads, 0) AS 'AVG READ Time (Transfer/msec)',io_stall_write_ms / NULLIF(num_of_writes, 0) AS 'AVG WRITE Time (Transfer/msec)' FROM sys.dm_io_virtual_file_stats(- 1, - 1) stat";


       using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
       {
           try
           {
               DataTable t = new DataTable();
               a.Fill(t);

               foreach (DataRow row in t.Rows)
               {
                   text = text + DateTime.Now.ToString() + ";";
                   foreach (DataColumn col in t.Columns)
                   {
                       data[start, column] = row[col.ColumnName].ToString();
                       text = text + row[col.ColumnName].ToString() + ";";
                   }
                   text = text + Environment.NewLine;
               }
           }
           catch (Exception e)
           {
               Console.WriteLine(e.Message);
           }
       }
       

        //append to file
       String content = String.Empty;
       String file = @p + "diskLatencyCritical.txt";
       if (File.Exists(file))
       {
           content = File.ReadAllText(file);

           string newContent = content + text;
           File.WriteAllText(file, newContent);
       }
       return text;
       }


        //Required for additional analysis
        public static String buffermanager(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            string q = @"SELECT * FROM sys.dm_os_performance_counters WHERE [object_name] LIKE '%Buffer Manager%'";
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);
                    foreach (DataRow row in t.Rows)
                    {
                        text = text + DateTime.Now.ToString() + ";";
                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                            text = text + row[col.ColumnName].ToString().Trim() + ";";
                        }
                        text = text + Environment.NewLine;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            

            String content = String.Empty;
            String file = @p + "buffermanager.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);

                string newContent = content + text;
                File.WriteAllText(file, newContent);
            }
            return text;
        }



        //Required for additional analysis
        public static String bufferpoolUsedPerDatabase(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            string q = @"

DECLARE @total_buffer INT;

SELECT @total_buffer = cntr_value
FROM sys.dm_os_performance_counters 
WHERE RTRIM([object_name]) LIKE '%Buffer Manager'
AND counter_name = 'Database Pages';

;WITH src AS
(
SELECT 
database_id, db_buffer_pages = COUNT_BIG(*)
FROM sys.dm_os_buffer_descriptors
--WHERE database_id BETWEEN 5 AND 32766
GROUP BY database_id
)
SELECT
[db_name] = CASE [database_id] WHEN 32767 
THEN 'Resource DB' 
ELSE DB_NAME([database_id]) END,
db_buffer_pages,
db_buffer_MB = db_buffer_pages / 128,
db_buffer_percent = CONVERT(DECIMAL(6,3), 
db_buffer_pages * 100.0 / @total_buffer)
FROM src
ORDER BY db_buffer_MB DESC; 


";
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);
                    foreach (DataRow row in t.Rows)
                    {
                        text = text + DateTime.Now.ToString() + ";";
                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                            text = text + row[col.ColumnName].ToString().Trim() + ";";
                        }
                        text = text + Environment.NewLine;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            

            String content = String.Empty;
            String file = @p + "bufferpoolUsedPerDatabase.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
         
            string newContent = content + text;
            File.WriteAllText(file, newContent);
        }

            return text;
        }



        //Required for additional analysis
        public static String bufferpoolUsedPerObject(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            string q = @"

IF OBJECT_ID('TempDB..#BufferSummary') IS NOT NULL BEGIN
	DROP TABLE #BufferSummary
END

IF OBJECT_ID('TempDB..#BufferPool') IS NOT NULL BEGIN
	DROP TABLE #BufferPool
END

CREATE TABLE #BufferPool
(
	Cached_MB Int
	, Database_Name SysName
	, Schema_Name SysName NULL
	, Object_Name SysName NULL
	, Index_ID Int NULL
	, Index_Name SysName NULL
	, Used_MB Int NULL
	, Used_InRow_MB Int NULL
	, Row_Count BigInt NULL
)

SELECT Pages = COUNT(1)
	, allocation_unit_id
	, database_id
INTO #BufferSummary
FROM sys.dm_os_buffer_descriptors 
GROUP BY allocation_unit_id, database_id 
	
DECLARE @DateAdded SmallDateTime  
SELECT @DateAdded = GETDATE()  
  
DECLARE @SQL NVarChar(4000)  
SELECT @SQL = ' USE [?]  
INSERT INTO #BufferPool (
	Cached_MB 
	, Database_Name 
	, Schema_Name 
	, Object_Name 
	, Index_ID 
	, Index_Name 
	, Used_MB 
	, Used_InRow_MB 
	, Row_Count 
	)  
SELECT sum(bd.Pages)/128 
	, DB_Name(bd.database_id)
	, Schema_Name(o.schema_id)
	, o.name
	, p.index_id 
	, ix.Name
	, i.Used_MB
	, i.Used_InRow_MB
	, i.Row_Count     
FROM #BufferSummary AS bd 
	LEFT JOIN sys.allocation_units au ON bd.allocation_unit_id = au.allocation_unit_id
	LEFT JOIN sys.partitions p ON (au.container_id = p.hobt_id AND au.type in (1,3)) OR (au.container_id = p.partition_id and au.type = 2)
	LEFT JOIN (
		SELECT PS.object_id
			, PS.index_id 
			, Used_MB = SUM(PS.used_page_count) / 128 
			, Used_InRow_MB = SUM(PS.in_row_used_page_count) / 128
			, Used_LOB_MB = SUM(PS.lob_used_page_count) / 128
			, Reserved_MB = SUM(PS.reserved_page_count) / 128
			, Row_Count = SUM(row_count)
		FROM sys.dm_db_partition_stats PS
		GROUP BY PS.object_id
			, PS.index_id
	) i ON p.object_id = i.object_id AND p.index_id = i.index_id
	LEFT JOIN sys.indexes ix ON i.object_id = ix.object_id AND i.index_id = ix.index_id
	LEFT JOIN sys.objects o ON p.object_id = o.object_id
WHERE database_id = db_id()  
GROUP BY bd.database_id   
	, o.schema_id
	, o.name
	, p.index_id
	, ix.Name
	, i.Used_MB
	, i.Used_InRow_MB
	, i.Row_Count     
HAVING SUM(bd.pages) > 128  
ORDER BY 1 DESC;'  

EXEC sp_MSforeachdb @SQL

SELECT Cached_MB 
	, Pct_of_Cache = CAST(Cached_MB * 100.0 / SUM(Cached_MB) OVER () as Dec(20,3))
	, Pct_Index_in_Cache = CAST(Cached_MB * 100.0 / CASE Used_MB WHEN 0 THEN 0.001 ELSE Used_MB END as DEC(20,3))
	, Database_Name 
	, Schema_Name 
	, Object_Name 
	, Index_ID 
	, Index_Name 
	, Used_MB 
	, Used_InRow_MB 
	, Row_Count 
FROM #BufferPool 
ORDER BY Cached_MB DESC

";
            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);
                    foreach (DataRow row in t.Rows)
                    {
                        text = text + DateTime.Now.ToString() + ";";
                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                            text = text + row[col.ColumnName].ToString().Trim() + ";";
                        }
                        text = text + Environment.NewLine;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            

            String content = String.Empty;
            String file = @p + "bufferpoolUsedPerObject.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
         
            string newContent = content + text;
            File.WriteAllText(file, newContent);
        }
            return text;
        }



        public static String checkPendingMemoryGrants(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            string q = @"SELECT [cntr_value] FROM sys.dm_os_performance_counters WHERE 	[object_name] LIKE '%Memory Manager%' 	AND [counter_name] = 'Memory Grants Pending'";

            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);
                    foreach (DataRow row in t.Rows)
                    {
                        text = text + DateTime.Now.ToString() + ";";
                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                            text = text + row[col.ColumnName].ToString().Trim() + ";";
                        }
                        text = text + Environment.NewLine;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }


            String content = String.Empty;
            String file = @p + "PendingMemoryGrants.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
         
            string newContent = content + text;
            File.WriteAllText(file, newContent);
        }
            return text;
        }




        public static String checkBatchRequestsPerSecond(string d, string p, string c, SqlConnection connsql)
        {
            string[] connList = new string[3];
            string text = "";
            String[,] data = new String[10, 10];
            int start = 0;
            int column = 0;

            string q = @"SELECT cntr_value FROM sys.dm_os_performance_counters WHERE 	[object_name] LIKE '%Statistics%' 	AND [counter_name] like '%Batch Requests%'";

            using (SqlDataAdapter a = new SqlDataAdapter(@q, connsql))
            {
                try
                {
                    DataTable t = new DataTable();
                    a.Fill(t);
                    foreach (DataRow row in t.Rows)
                    {
                        text = text + DateTime.Now.ToString() + ";";
                        foreach (DataColumn col in t.Columns)
                        {
                            data[start, column] = row[col.ColumnName].ToString();
                            text = text + row[col.ColumnName].ToString().Trim() + ";";
                        }
                        text = text + Environment.NewLine;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }


            String content = String.Empty;
            String file = @p + "BatchRequestsPerSecond.txt";
            if (File.Exists(file))
            {
                content = File.ReadAllText(file);
         
            string newContent = content + text;
            File.WriteAllText(file, newContent);
        }
            return text;
        }


    }
}

            
