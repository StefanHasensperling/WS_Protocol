using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WS_Protocol.Client;

namespace WS_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("MLogics Chile Ltda. Weihenstepahn Test-Client");
                Console.WriteLine("Testing Weihenstephan Protocol");
                //var WSclient = new WS_TcpClient("192.168.0.101", 5000);
                var WSclient = new WS_TcpClient("127.0.0.1", 5000) { SendAutomaticHeartBeat = false} ;

                Console.Write("using address: ");
                Console.Write(WSclient.HostName);
                Console.Write(":");
                Console.WriteLine(WSclient.Port);
                Console.WriteLine();

                Console.Write("Connecting...          ");
                WSclient.Connect();
                Console.WriteLine("Done");

                Console.Write("Requesting TagID= 30:  ");
                Console.WriteLine(WSclient.ReadSingleValueAsInt(30));

                Console.Write("Requesting TagID= 200: ");
                Console.WriteLine(WSclient.ReadSingleValueAsInt(200));

                Console.Write("Sending NoOp:          ");
                WSclient.NoOp();
                Console.WriteLine("Done");

                Console.Write("Writing TagID= 190:    ");
                WSclient.WriteSingleValue(190, 123);
                Console.WriteLine("Done");

                Console.Write("Requesting TagID= 30:  ");
                Console.WriteLine(WSclient.ReadSingleString(30));

                Console.Write("Writing TagID= 31:    ");
                WSclient.WriteSingleString(31, "test");
                Console.WriteLine("Done");

                Console.Write("Disconnecting...       ");
                WSclient.Disconnect();
                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine("Press any key to close");
                Console.ReadKey();
            }
        }
    }
}
