using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace WS_Protocol.Server
{
    public class WS_TcpServer
    {
        public ConcurrentBag<ServerTag> Tags = new ConcurrentBag<ServerTag>();
        public int Port { get; }
        private TcpListener TcpListener;

        internal List<ServerClient> Clients =  new List<ServerClient>(); 

        public WS_TcpServer(int port)
        {  
            Port = port; 
        }

        public void Start()
        {
            TcpListener = new TcpListener(IPAddress.Any, Port);
            TcpListener.Start();

            try
            {
                while (true) 
                {
                    var TcpClient = TcpListener.AcceptTcpClient();
                    var ServerClient = new ServerClient(TcpClient, this);
                }
            }
            catch (Exception) 
            {
                throw; //so we can make an breakpoint here, if we need to.
            }

        }

        public ServerTag FindTagByTagId(uint TagId)
        {
            return Tags.Where((a)=>a.TagId == TagId).FirstOrDefault();
        }
    }
}
