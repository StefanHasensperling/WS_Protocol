using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_Server
{
    internal class ConsoleTraceListener : TraceListener
    {
        public override void Write(string message)
        {
            Console.Write(message);
        }

        public override void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}
