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
    /// <summary>
    /// Represents an Weihenstephan Standard Protocol TCP server
    /// </summary>
    public class WS_TcpServer
    {
        /// <summary>
        /// An collection of all available tags on the server. these tag hold the configuration of the tags itself, and also their value
        /// </summary>
        public ConcurrentBag<ServerTag> Tags = new ConcurrentBag<ServerTag>();

        /// <summary>
        /// the port the server should be listening to. usually it is 5000 or 50000
        /// </summary>
        public int Port { get; }
        private TcpListener TcpListener;

        /// <summary>
        /// an internal list of connected clients
        /// </summary>
        internal List<ServerClient> Clients =  new List<ServerClient>();

        /// <summary>
        /// Creates an new Weihenstephan Standard Protocol TCP server.
        /// The server always listens to all IP Interfaces
        /// </summary>
        /// <param name="port">The port the server is listening on</param>
        public WS_TcpServer(int port)
        {  
            Port = port; 
        }

        /// <summary>
        /// Start the server. This is a blocking call
        /// </summary>
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

        /// <summary>
        /// Searches through all configured tags, and returns the first one that matches the TagID
        /// </summary>
        /// <param name="TagId">The TagId to search for</param>
        /// <returns></returns>
        public ServerTag FindTagByTagId(uint TagId)
        {
            return Tags.Where((a)=>a.TagId == TagId).FirstOrDefault();
        }
    }
}
