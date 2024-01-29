using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_Protocol.Client
{
    internal class WriteSingleStringMessage
    {
        public static void Execute(WS_TcpClient client, uint TagId, string value)
        {
            //Request Message Frame Layout
            //0, 1                Command
            //2, 3,               Tag ID
            //4, 5, 6, 7          Length of string, which ultimately determines how many frames we will send
            var SendBuffer = new byte[8];
            var MemStream = new MemoryStream(SendBuffer) { Position = 0 };
            var BinWriter = new BinaryWriter(MemStream);

            BinWriter.Write((UInt16)Ws_Commands.WS_CMD_WRITE_STRING);
            BinWriter.Write((UInt16)TagId);
            BinWriter.Write(value.Length); //Length of string

            //Exchange message frames
            byte[] res;
            lock (client.SyncRoot)
            {
                client.SendSingleMessgeFrame(SendBuffer);

                //now calculate how many payload bytes we will need to send
                var SendStringBytes = Encoding.Unicode.GetBytes(value);
                var SendByteLength = SendStringBytes.Length;
                var MsgFramesStillToGo = (int)Math.Ceiling((double)SendByteLength / (double)WS_TcpClient.WS_MessgeFrameLength);
                SendBuffer = new byte[MsgFramesStillToGo * WS_TcpClient.WS_MessgeFrameLength];

                SendStringBytes.CopyTo(SendBuffer, 0);

                for (int i = 0; i < MsgFramesStillToGo; i++)
                {
                    client.SendSingleMessgeFrame(SendBuffer, i * WS_TcpClient.WS_MessgeFrameLength);
                }

                //Receive Response
                res = client.ReceiveSingleMessgeFrame();
            }

            //Response Message Frame Layout
            //0, 1                Return Code
            //2, 3                Tag ID that was written
            //4, 5, 6, 7          Padding to "0"

            MemStream = new MemoryStream(res) { Position = 0 };
            var BinReader = new BinaryReader(MemStream);

            var RetCode = BinReader.ReadInt16();
            var RetTagId = BinReader.ReadUInt16();
            var Padding = BinReader.ReadInt32();

            //check Return Code
            client.CheckAndThrowReturnCode(RetCode);

            //check the TagID we received, is the one we asked for
            if (RetTagId != TagId) throw new WS_ProtocolException("the response TagId did not match up with the requested TagId");

        }
    }
}
