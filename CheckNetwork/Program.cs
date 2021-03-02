using System;
using System.IO;
using System.Text;
using System.Net.NetworkInformation;
using System.Configuration;
using System.Threading;


namespace CheckNetwork
{
    class Program
    {
        public static int Timeout { get; set; }
        public static int SleepTime { get; set; }        
        public static string HostNameOrAddress { get; set; }
        public static string OutputFilePath { get; set; }
        public static string ConsoleOutput { get; set; }        

        static void Main(string[] args)
        {
            try
            {
                Ping myPing = new Ping();

                // Create a buffer of 32 bytes of data to be transmitted.
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);

                // Set options for transmission:
                // The data can go through 64 gateways or routers
                // before it is destroyed, and the data packet
                // cannot be fragmented.
                PingOptions options = new PingOptions(64, true);

                CheckParameters();
                bool console = ConsoleOutput.ToLower().Equals("true");

                PrintLine($"{System.DateTime.Now} - Pinging: {HostNameOrAddress} - Timeout Threshold: {Timeout}ms\n\n", console);

                while (true)
                {
                    try
                    {
                        Thread.Sleep(SleepTime);

                        PingReply reply = myPing.Send(HostNameOrAddress, Timeout, buffer, options);
                        if (reply.Status == IPStatus.Success)
                        {
                            if (reply.RoundtripTime > 1000)
                                PrintLine($"RoundTrip time: {reply.RoundtripTime}ms", console);
                        }
                        else
                            PrintLine($"{System.DateTime.Now} - Status: {reply.Status}", console);
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                        if (ex.InnerException != null)
                            message += " -> " + ex.InnerException.Message;
                        PrintLine($"{System.DateTime.Now} - Exception! {message}", console);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message += " -> " + ex.InnerException.Message;
                PrintLine($"*** Main Function Exception! {message}", true);
            }
        }

        static void CheckParameters()
        {
            int timeout;
            if (!int.TryParse(ConfigurationManager.AppSettings["Timeout"], out timeout))
                throw new ApplicationException("TimeoutMillisecond is needed");
            Timeout = timeout;

            int sleepTime;
            if (!int.TryParse(ConfigurationManager.AppSettings["SleepTime"], out sleepTime))
                throw new ApplicationException("SleepTime is needed");
            SleepTime = sleepTime;            

            HostNameOrAddress = ConfigurationManager.AppSettings["HostNameOrAddress"];
            if (string.IsNullOrWhiteSpace(HostNameOrAddress))
                throw new ApplicationException("HostNameOrAddress is needed");

            ConsoleOutput = ConfigurationManager.AppSettings["ConsoleOutput"];
            if (string.IsNullOrWhiteSpace(ConsoleOutput))
                throw new ApplicationException("ConsoleOutput is needed");

            OutputFilePath = ConfigurationManager.AppSettings["OutputFilePath"];
            if (string.IsNullOrWhiteSpace(OutputFilePath))
                throw new ApplicationException("OutputFilePath is needed");

            if (File.Exists(OutputFilePath))
                File.Delete(OutputFilePath);

        }

        static public void PrintLine(string message, bool console = false)
        {
            using (StreamWriter sw = File.AppendText(OutputFilePath))
            {
                sw.WriteLine(message);
                sw.Flush();
                sw.Close();
            }

            if (console)
                Console.WriteLine(message);
        }

    }
}
