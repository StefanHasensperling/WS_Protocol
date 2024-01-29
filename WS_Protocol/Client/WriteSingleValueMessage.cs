using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_Protocol.Client
{
    internal class WriteSingleValueMessage
    {
        public static void Execute(WS_TcpClient client, uint TagId, int value)
        {
            Execute(client, TagId, BitConverter.GetBytes((Int32)value));
        }

        public static void Execute(WS_TcpClient client, uint TagId, double value)
        {
            Execute(client, TagId, BitConverter.GetBytes((Single)value));
        }

        private static void Execute(WS_TcpClient client, uint TagId, byte[] value)
        {
            //Request Message Frame Layout
            //0, 1                Command
            //2, 3,               Tag ID
            //4, 5, 6, 7          value to be written
            var SendBuffer = new byte[8];
            var MemStream = new MemoryStream(SendBuffer) { Position = 0 };
            var BinWriter = new BinaryWriter(MemStream);

            BinWriter.Write((UInt16)Ws_Commands.WS_CMD_WRITE_SVALUE);
            BinWriter.Write((UInt16)TagId);
            BinWriter.Write(value); 

            //Exchange message frames
            byte[] res;
            lock (client.SyncRoot)
            {
                client.SendSingleMessgeFrame(SendBuffer);
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
