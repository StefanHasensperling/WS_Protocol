using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WS_Protocol.Client
{
    /// <summary>
    /// Represents an Weihenstephan Standard Protocol TCP client
    /// </summary>
    public class WS_TcpClient
    {
        /// <summary>
        /// The IP-Address of the Weihenstephan server to connect to
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// The Port number of the Server to connect to. Usually it is 5000 or 50000 but may vary depending on the server. 
        /// </summary>
        public int Port { get; set; } = 5000;

        /// <summary>
        /// True if the Client is currently connected to the server
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (tcpClient == null) return false;
                return tcpClient.Connected;
            }
        }

        private TimeSpan _timeout = TimeSpan.FromSeconds(5);

        /// <summary>
        /// The Timeout for connecting and receiving responses from the server
        /// </summary>
        public TimeSpan Timeout 
        {
            get { return _timeout; }
            set
            {
                _timeout = value;
                if (stream != null)
                {
                    stream.ReadTimeout = (int)Timeout.TotalMilliseconds;
                    stream.WriteTimeout = (int)Timeout.TotalMilliseconds;
                }
            }
        }

        private TcpClient tcpClient;
        private NetworkStream stream;

        /// <summary>
        /// Each message in an Weihenstephaner Standard communication always has 8 bytes in length. 
        /// if more data is exchanged, for example for strings, then multiple messages are sent, but each message must be an multiple of 8 bytes in length. 
        /// If an message is less than 8 bytes, it must be padded with 0 to reach 8 bytes length. For string messages the last message must be padded to reach 
        /// an multiple of 8 bytes. 
        /// </summary>
        public const int WS_MessgeFrameLength = 8;

        /// <summary>
        /// This is used to lock around the stream, for some basic thread safety
        /// </summary>
        internal object SyncRoot = new object();

        /// <summary>
        /// Creates an new Weihenstephaner Standard Protocol Tcp client
        /// </summary>
        /// <param name="hostName">the IP-Address of the Server to connect to. This is the address of the Machine you want to read data from</param>
        public WS_TcpClient(string hostName)
        {
            this.HostName=hostName;
        }

        /// <summary>
        /// Creates an new Weihenstephaner Standard Protocol Tcp client
        /// </summary>
        /// <param name="hostName">the IP-Address of the Server to connect to. This is the address of the Machine you want to read data from</param>
        /// <param name="port">The Tcp Port of the server. Usually it is 5000 or 50000, but may vary depending on the manufacturer of the machine you want to read data from</param>
        public WS_TcpClient(string hostName, int port)
        {
            this.HostName=hostName;
            this.Port=port;
        }

        /// <summary>
        /// Connect to the Weihenstephaner Standard Protocol Server
        /// </summary>
        /// <exception cref="TimeoutException">is Thrown if the client could not connect to the server</exception>
        public void Connect()
        {
            if (tcpClient != null) Disconnect();

            tcpClient = new TcpClient();

            //We use "BeginConnect" here, so we can define our own Timeout on the WaitOne method. 
            var res =  tcpClient.BeginConnect(HostName, Port, (a) =>  {}, null);
            if (!res.AsyncWaitHandle.WaitOne((int)Timeout.TotalMilliseconds)) throw new TimeoutException(); //it took to long for the server to respond, or it is not even there
            tcpClient.EndConnect(res);  //this will throw the exception if there was an Error which was not an Timeout

            //we Connected, so we can set the client up for data exchange
            stream = tcpClient.GetStream();

            if (SendAutomaticHeartBeat) StartHeartBeattimer();
        }

        /// <summary>
        /// Disconnect from the Server
        /// </summary>
        public void Disconnect() 
        { 
            if (tcpClient == null) return;
            StopHeartBeattimer();
            tcpClient.Close();
            tcpClient = null;
            stream = null;
        }

        /// <summary>
        /// This is used to send an single 8 byte message frame to the server
        /// </summary>
        /// <param name="data">The data to be sent. Must be an 8 byte array</param>
        /// <exception cref="WS_ProtocolException"></exception>
        internal void SendSingleMessgeFrame(byte[] data)
        {
            if (data.Length != WS_MessgeFrameLength) throw new WS_ProtocolException("Each message frame must be exactly 8 bytes long");

            stream.Write(data, 0, WS_MessgeFrameLength);
            stream.Flush();
        }

        /// <summary>
        /// This is used to send an single 8 byte message frame to the server
        /// </summary>
        /// <param name="data">The data to be sent. must be offset + 8 bytes long</param>
        /// <param name="offset">The offset in the data to send the 8 bytes</param>
        /// <exception cref="WS_ProtocolException"></exception>
        internal void SendSingleMessgeFrame(byte[] data, int offset)
        {
            if (data.Length < WS_MessgeFrameLength + offset) throw new WS_ProtocolException("Each message frame must be exactly 8 bytes long");

            stream.Write(data, offset, WS_MessgeFrameLength);
            stream.Flush();
        }

        /// <summary>
        /// Reads an single 8 byte frame from the stream. Blocks until 8 byte are available, or an timeout happens. 
        /// </summary>
        /// <returns></returns>
        internal byte[] ReceiveSingleMessgeFrame()
        {
            var data = new byte[8];
            stream.Read(data, 0, WS_MessgeFrameLength);
            return data;
        }

        /// <summary>
        /// Checks and translate common error codes to more meaningful exceptions
        /// </summary>
        /// <param name="RetCode"></param>
        /// <exception cref="WS_ProtocolException"></exception>
        internal void CheckAndThrowReturnCode(int RetCode)
        {
            switch ((Ws_Errors)RetCode)
            {
                case Ws_Errors.WS_ERR_SUCCESSFUL:   //We are good, no error code
                    break;

                case Ws_Errors.WS_ERR_WRITE_NOT_SUCCESSFUL:
                    throw new WS_ProtocolException("The value could not be written");
                case Ws_Errors.WS_ERR_MEMORY_OVERFLOW:
                    throw new WS_ProtocolException("There was an Memory overflow");
                case Ws_Errors.WS_ERR_UNKNOWN_CMD:
                    throw new WS_ProtocolException("The requested command is unknown");
                case Ws_Errors.WS_ERR_UNAUTHORIZED_ACCESS:
                    throw new WS_ProtocolException("The value could not be written because it is read only");
                case Ws_Errors.WS_ERR_SERVER_OVERLOAD:
                    throw new WS_ProtocolException("The server is currently overloaded, and can not serve the request");
                case Ws_Errors.WS_ERR_IMPLAUSIBLE_ARGUMENT:
                    throw new WS_ProtocolException("The given TagId is not available on the server");
                case Ws_Errors.WS_ERR_IMPLAUSIBLE_LIST:
                    throw new WS_ProtocolException("An list given in the request is not plausible");
                default:
                    throw new WS_ProtocolException(string.Format("The server responded with the error code '{0}'", RetCode.ToString()));
            }
        }

        /// <summary>
        /// Checks if the server is still alive and responding. otherwise does no action on the server, other than make him respond
        /// </summary>
        public void NoOp()
        {
            NoOpSingleValueMessage.Execute(this);
        }

        /// <summary>
        /// Reads an single Value from the server. Since the value can either be an Integer or an Real, only the raw result bytes are returned and must be converted
        /// to the appropriate data type
        /// </summary>
        /// <param name="TagId">The TagID of the data to read. Please check "WS_TagNumbers.xls" for some known TagIds</param>
        /// <returns>an 4 Byte array of the returned data</returns>
        private byte[] ReadSingleValue(uint TagId)
        {
            return ReadSingleValueMessage.Execute(this, TagId);
        }

        /// <summary>
        /// Reads an single Value from the server as an Integer number. 
        /// </summary>
        /// <param name="TagId">The TagID of the data to read. Please check "WS_TagNumbers.xls" for some known TagIds</param>
        /// <returns>The value of the requested TagId</returns>
        public int ReadSingleValueAsInt(uint TagId)
        {
            return BitConverter.ToInt32(ReadSingleValue(TagId), 0);
        }

        /// <summary>
        /// Reads an single Value from the server as an Floating point number. 
        /// </summary>
        /// <param name="TagId">The TagID of the data to read. Please check "WS_TagNumbers.xls" for some known TagIds</param>
        /// <returns>The value of the requested TagId</returns>
        public float ReadSingleValueAsReal(uint TagId)
        {
            return BitConverter.ToSingle(ReadSingleValue(TagId), 0);
        }

        /// <summary>
        /// Reads an single Value from the server as an String. 
        /// </summary>
        /// <param name="TagId">The TagID of the data to read. Please check "WS_TagNumbers.xls" for some known TagIds</param>
        /// <returns>The value of the requested TagId</returns>
        public string ReadSingleString(uint TagId)
        {
            return ReadSingleStringMessage.Execute(this, TagId);
        }

        /// <summary>
        /// Writes an single Value to the server as an Integer number. 
        /// </summary>
        /// <param name="TagId">The TagID of the data to write. Please check "WS_TagNumbers.xls" for some known TagIds</param>
        /// <param name="Value">The number to write to the TagId</param>
        public void WriteSingleValue(uint TagId, int Value)
        {
            WriteSingleValueMessage.Execute(this, TagId, Value);    
        }

        /// <summary>
        /// Writes an single Value to the server as an Floating point number. 
        /// </summary>
        /// <param name="TagId">The TagID of the data to write. Please check "WS_TagNumbers.xls" for some known TagIds</param>
        /// <param name="Value">The number to write to the TagId</param>
        public void WriteSingleValue(uint TagId, double Value)
        {
            WriteSingleValueMessage.Execute(this, TagId, Value);
        }

        /// <summary>
        /// Writes an single Value to the server as an String
        /// </summary>
        /// <param name="TagId">The TagID of the data to write. Please check "WS_TagNumbers.xls" for some known TagIds</param>
        /// <param name="Value">The Text to write to the TagId</param>
        public void WriteSingleString(uint TagId, string Value)
        {
            WriteSingleStringMessage.Execute(this, TagId, Value);
        }

        #region Async
        /// <summary>
        /// Connect to the Weihenstephaner Standard Protocol Server
        /// </summary>
        /// <exception cref="TimeoutException">is Thrown if the client could not connect to the server</exception>
        public Task ConnectAsync()
        {
            return Task.Run(Connect);
        }

        /// <summary>
        /// Disconnect from the Server
        /// </summary>
        public Task DisconnectAsync()
        {
            return Task.Run(Disconnect);
        }

        /// <summary>
        /// Checks if the server is still alive and responding. otherwise does no action on the server, other than make him respond
        /// </summary>
        public Task NoOpAsync()
        {
            return Task.Run(() => { NoOp(); });
        }

        /// <summary>
        /// Reads an single Value from the server as an Integer number. 
        /// </summary>
        /// <param name="TagId">The TagID of the data to read. Please check "WS_TagNumbers.xls" for some known TagIds</param>
        /// <returns>The value of the requested TagId</returns>
        public Task<int> ReadSingleValueAsIntAsync(uint TagId)
        {
            return Task.Run(() => { return ReadSingleValueAsInt(TagId); });
        }

        /// <summary>
        /// Reads an single Value from the server as an Floating point number. 
        /// </summary>
        /// <param name="TagId">The TagID of the data to read. Please check "WS_TagNumbers.xls" for some known TagIds</param>
        /// <returns>The value of the requested TagId</returns>
        public Task<float> ReadSingleValueAsRealAsync(uint TagId)
        {
            return Task.Run(() => { return ReadSingleValueAsReal(TagId); });
        }

        /// <summary>
        /// Reads an single Value from the server as an String. 
        /// </summary>
        /// <param name="TagId">The TagID of the data to read. Please check "WS_TagNumbers.xls" for some known TagIds</param>
        /// <returns>The value of the requested TagId</returns>
        public Task<string> ReadSingleStringAsync(uint TagId)
        {
            return Task.Run(() => { return ReadSingleString(TagId); });
        }

        /// <summary>
        /// Writes an single Value to the server as an Integer number. 
        /// </summary>
        /// <param name="TagId">The TagID of the data to write. Please check "WS_TagNumbers.xls" for some known TagIds</param>
        /// <param name="Value">The number to write to the TagId</param>
        public Task WriteSingleValueAsync(uint TagId, int Value)
        {
            return Task.Run(() => { WriteSingleValue(TagId, Value); });
        }

        /// <summary>
        /// Writes an single Value to the server as an Floating point number. 
        /// </summary>
        /// <param name="TagId">The TagID of the data to write. Please check "WS_TagNumbers.xls" for some known TagIds</param>
        /// <param name="Value">The number to write to the TagId</param>
        public Task WriteSingleValueAsync(uint TagId, double Value)
        {
            return Task.Run(() => { WriteSingleValue(TagId, Value); });
        }

        /// <summary>
        /// Writes an single Value to the server as an String
        /// </summary>
        /// <param name="TagId">The TagID of the data to write. Please check "WS_TagNumbers.xls" for some known TagIds</param>
        /// <param name="Value">The Text to write to the TagId</param>
        public Task WriteSingleStringAsync(uint TagId, string Value)
        {
            return Task.Run(() => { WriteSingleString(TagId, Value); });
        }
        #endregion

        #region HeartBeat
        /// <summary>
        /// If true the connection automatically sends periodic "NOp" messages that act as heart beat messages to check if the server is still alive
        /// </summary>
        public bool SendAutomaticHeartBeat { get; set; } = true;

        /// <summary>
        /// The interval in which the heart beat is being sent to check if the server is still alive
        /// </summary>
        public TimeSpan HeartBeatInterval { get; set; } = TimeSpan.FromSeconds(20);
        private System.Timers.Timer _heartbeatTimer;

        /// <summary>
        /// Will be invoked if the server did not respond correctly to an heart beat message. After receiving this event, you need to reconnect again. 
        /// </summary>
        public event EventHandler ConnectionBroken;

        private void StartHeartBeattimer()
        {
            if (_heartbeatTimer != null) StopHeartBeattimer();

            _heartbeatTimer = new System.Timers.Timer(HeartBeatInterval.TotalMilliseconds);
            _heartbeatTimer.Elapsed += (a, b) =>
            {
                try
                {
                    _heartbeatTimer.Stop();
                    NoOp();
                    _heartbeatTimer.Start();
                }
                catch (Exception)
                {
                    ConnectionBroken?.Invoke(this, EventArgs.Empty);
                    Disconnect();
                }
            };
            _heartbeatTimer.Start();
        }

        private void StopHeartBeattimer()
        {
            if (_heartbeatTimer == null) return;
            _heartbeatTimer.Stop();
            _heartbeatTimer = null;
        }

        #endregion
    }
}
