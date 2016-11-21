using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Collections;
using System.Net.Mail;
using System.Threading;

namespace HealthcheckApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            //The name of the tool.
            Console.WriteLine("The Appmon Tool");

            int count = 0;

            //counting SV key value pairs of 'SV' from configuration file
            foreach (var item in ConfigurationManager.AppSettings.AllKeys)
            {
                if (item.StartsWith("SV"))
                {
                    count = count + 1;
                }
            }

            //array to store connection list
            string[] connList = new string[count];


            int arrcount = 0;

            //loop to get value of keys 'SV' from configuration file and store them in connList
            foreach (var item in ConfigurationManager.AppSettings.AllKeys)
            {
                if (item.StartsWith("SV"))
                {
                    string query = ConfigurationManager.AppSettings[item];
                    Console.WriteLine("Adding server list");
                    Console.WriteLine(query);
                    connList[arrcount] = query;
                    arrcount = arrcount + 1;
                }

            }

            foreach (var connect in connList)
            {

                Console.WriteLine(connect + Environment.NewLine);

                //split the connect(value part of key-value pair) string
                string[] sv = connect.Split(',');
                //sv[1] = IP address, sv[2]= database on the IP address
                string connectionstring = "Data Source=" + sv[1] + ";Initial Catalog=" + sv[2] + ";Integrated Security=sspi;";

                Console.WriteLine(connectionstring);

                //sv[3] = path of the log file
                String path = sv[3];

                Console.WriteLine(path);







                int retry = 10;
            trytotalmemory:
                SqlConnection connsql = new SqlConnection(@connectionstring);
                try
                {
                    Console.WriteLine("Trying to open");
                    connsql.Open();
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                    if (retry != 0)
                    {
                        //Console.WriteLine("Trying " + retry);
                        retry = retry - 1;
                        Thread.Sleep(10);
                        goto trytotalmemory;
                    }
                    else
                    {
                        try
                        {
                            ////Console.WriteLine("Sending E-Mail at this time : {0}", DateTime.Now);
                            String subject = "UNABLE TO CONNECT TO SERVER";
                            String emailtext = "CRITICALITY : HIGH:" + "<br />" + (@connectionstring.Split(';')[0]).Split('=')[1] + "<br />" + "Error: " + error.Message;
                            ReportMailer mailer = new ReportMailer(0, 0, emailtext, subject);
                            mailer.SendMail();
                        }
                        catch (Exception error2)
                        {
                            Console.WriteLine(error2.Message);
                        }
                    }
                }





                //calling each method
                checkParam.checkTargetMemory(@connectionstring, @path, @connect, connsql);
                checkParam.checkTotalMemory(@connectionstring, @path, @connect, connsql);
                checkParam.checkFullScans(@connectionstring, @path, @connect, connsql);
                checkParam.checkAveragelatchWaitTime(@connectionstring, @path, @connect, connsql);
                checkParam.checkAverageLockWaitTime(@connectionstring, @path, @connect, connsql);
                checkParam.checkActiveTransactionsPerSecond(@connectionstring, @path, @connect, connsql);
                checkParam.checkNumberOfActiveTransactions(@connectionstring, @path, @connect, connsql);
                checkParam.checkNumberOfActiveConnections(@connectionstring, @path, @connect, connsql);
                checkParam.checklongRunningQueries(@connectionstring, @path, @connect, connsql);
                checkParam.checkPhysicalMemory(@connectionstring, @path, @connect, connsql);
                checkParam.checkBlockingQueries(@connectionstring, @path, @connect, connsql);
                // checkParam.checkAverageDiskRead(@connectionstring, @path, @connect, connsql);
                // checkParam.checkAverageDiskWrite(@connectionstring, @path, @connect, connsql);
                checkParam.logPhysicalMemory(@connectionstring, @path, @connect, connsql);
                checkParam.checkSeverity(@connectionstring, @path, @connect, connsql);
                // checkParam.checkerror825(@connectionstring, @path, @connect, connsql);
                checkParam.checkDeadLocks(@connectionstring, @path, @connect, connsql);
                checkParam.checkDiskLatencyCritical(@connectionstring, @path, @connect, connsql);
                checkParam.checkUnusualJobDuration(@connectionstring, @path, @connect, connsql);
                checkParam.buffermanager(@connectionstring, @path, @connect, connsql);
                checkParam.bufferpoolUsedPerDatabase(@connectionstring, @path, @connect, connsql);
                checkParam.bufferpoolUsedPerObject(@connectionstring, @path, @connect, connsql);
                checkParam.checkPendingMemoryGrants(@connectionstring, @path, @connect, connsql);
                checkParam.checkBatchRequestsPerSecond(@connectionstring, @path, @connect, connsql);
                Console.WriteLine("Done with " + connectionstring + Environment.NewLine);

                connsql.Dispose();

            }

            Console.WriteLine("END");

        }
    }
}



