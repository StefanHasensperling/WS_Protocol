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
        public string HostName { get; set; }
        public int Port { get; set; } = 5000;
        public bool IsConnected
        {
            get
            {
                if (tcpClient == null) return false;
                return tcpClient.Connected;
            }
        }

        private TimeSpan _timeout = TimeSpan.FromSeconds(5);
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

        public const int WS_MessgeFrameLength = 8;
        internal object SyncRoot = new object();

        public WS_TcpClient(string hostName)
        {
            this.HostName=hostName;
        }

        public WS_TcpClient(string hostName, int port)
        {
            this.HostName=hostName;
            this.Port=port;
        }

        public void Connect()
        {
            if (tcpClient != null) Disconnect();

            tcpClient = new TcpClient();

            var res =  tcpClient.BeginConnect(HostName, Port, (a) =>  {}, null);

            if (!res.AsyncWaitHandle.WaitOne((int)Timeout.TotalMilliseconds)) throw new TimeoutException();
            tcpClient.EndConnect(res);  //this will throw the exception if there was one that was not an Timeout

            stream = tcpClient.GetStream();

            if (SendAutomaticHeartBeat) StartHeartBeattimer();
        }

        public void Disconnect() 
        { 
            if (tcpClient == null) return;
            StopHeartBeattimer();
            tcpClient.Close();
            tcpClient = null;
            stream = null;
        }

        internal void SendSingleMessgeFrame(byte[] data)
        {
            if (data.Length != WS_MessgeFrameLength) throw new WS_ProtocolException("Each message frame must be exactly 8 bytes long");

            stream.Write(data, 0, WS_MessgeFrameLength);
            stream.Flush();
        }

        internal void SendSingleMessgeFrame(byte[] data, int offset)
        {
            if (data.Length < WS_MessgeFrameLength + offset) throw new WS_ProtocolException("Each message frame must be exactly 8 bytes long");

            stream.Write(data, offset, WS_MessgeFrameLength);
            stream.Flush();
        }


        internal byte[] ReceiveSingleMessgeFrame()
        {
            var data = new byte[8];
            stream.Read(data, 0, WS_MessgeFrameLength);
            return data;
        }

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

        public void NoOp()
        {
            NoOpSingleValueMessage.Execute(this);
        }

        private byte[] ReadSingleValue(uint TagId)
        {
            return ReadSingleValueMessage.Execute(this, TagId);
        }

        public int ReadSingleValueAsInt(uint TagId)
        {
            return BitConverter.ToInt32(ReadSingleValue(TagId), 0);
        }

        public float ReadSingleValueAsReal(uint TagId)
        {
            return BitConverter.ToSingle(ReadSingleValue(TagId), 0);
        }

        public string ReadSingleString(uint TagId)
        {
            return ReadSingleStringMessage.Execute(this, TagId);
        }

        public void WriteSingleValue(uint TagId, int Value)
        {
            WriteSingleValueMessage.Execute(this, TagId, Value);    
        }

        public void WriteSingleValue(uint TagId, double Value)
        {
            WriteSingleValueMessage.Execute(this, TagId, Value);
        }

        public void WriteSingleString(uint TagId, string Value)
        {
            WriteSingleStringMessage.Execute(this, TagId, Value);
        }

        #region HeartBeat
        /// <summary>
        /// If true the connection automatically sends periodic "NOp" messages that act as heart beat messages to check if the server is still alive
        /// </summary>
        public bool SendAutomaticHeartBeat { get; set; } = true;
        public TimeSpan HeartBeatInterval { get; set; } = TimeSpan.FromSeconds(20);
        private System.Timers.Timer _heartbeatTimer;
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
                    StopHeartBeattimer();
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
