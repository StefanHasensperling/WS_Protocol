using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WS_Protocol.Server;

namespace WS_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {

                //This will pipe all Trace messages to the Console
                Trace.Listeners.Add(new ConsoleTraceListener());

                Console.WriteLine("MLogics Chile Ltda. Weihenstepahn Test-Server");
                Console.WriteLine("Starting Weihenstephan Server on Port 5000, on all IP Interfaces");

                //Define the Server and its available Tags
                var WsServer = new WS_TcpServer(5000);

                //add some normal Read write Tags
                WsServer.Tags.Add(new ServerTag() { DataType = WS_Protocol.Ws_DataTypes.Integer, TagId = 30, IntValue = 130 });
                WsServer.Tags.Add(new ServerTag() { DataType = WS_Protocol.Ws_DataTypes.Integer, TagId = 190, IntValue = 300 });
                WsServer.Tags.Add(new ServerTag() { DataType = WS_Protocol.Ws_DataTypes.String, TagId = 30, StringValue = "test" });
                WsServer.Tags.Add(new ServerTag() { DataType = WS_Protocol.Ws_DataTypes.String, TagId = 31, StringValue = "" });

                //Add an custom Tag that always returns Random integer
                WsServer.Tags.Add(new RandomIntegerTag() { DataType = WS_Protocol.Ws_DataTypes.Integer, TagId = 200, IntValue = 300 });

                Console.WriteLine("Listening for Clients...");
                WsServer.Start();
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
