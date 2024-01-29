using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_Protocol
{
    public class WS_ProtocolException: ApplicationException
    {
        public WS_ProtocolException(string message): base(message) { }
        public WS_ProtocolException(string message, Exception innerException) : base(message, innerException) { }

    }
}
