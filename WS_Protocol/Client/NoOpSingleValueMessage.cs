using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace WS_Protocol.Client
{
    internal class NoOpSingleValueMessage
    {

        public static byte[] Execute(WS_TcpClient client)
        {
            //Request Message Frame Layout
            //0, 1                Command
            //2, 3,               Padding to "0"
            //4, 5, 6, 7          Padding to "0
            var SendBuffer = new byte[8];
            var MemStream = new MemoryStream(SendBuffer) { Position = 0 };
            var BinWriter = new BinaryWriter(MemStream);

            BinWriter.Write((UInt16)Ws_Commands.WS_CMD_NOOP);
            BinWriter.Write((UInt16)0);
            BinWriter.Write((UInt32)0); //add Padding, 4 bytes

            //Exchange message frames
            byte[] res;
            lock (client.SyncRoot)
            {
                client.SendSingleMessgeFrame(SendBuffer);
                res = client.ReceiveSingleMessgeFrame();
            }

            //Response Message Frame Layout
            //0, 1                Return Code will be "0xFFFF" for NoOp!
            //2, 3,               Must be "0"
            //4, 5, 6, 7          Must be "0"

            MemStream = new MemoryStream(res) { Position = 0 };
            var BinReader = new BinaryReader(MemStream);

            var RetCode = BinReader.ReadInt16();
            var Val1 = BinReader.ReadInt16();
            var Val2 = BinReader.ReadInt32();

            if (RetCode != -1 || Val1 != 0 || Val2 != 0) //Both of them are expected to be "0"
            {
                throw new WS_ProtocolException("The Server responded with an invalid response to an NoOp Message");
            }

            //Return our Payload, which will be 4 bytes
            return res.Skip(4).ToArray();
        }
    }
}