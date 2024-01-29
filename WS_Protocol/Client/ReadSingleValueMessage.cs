using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_Protocol.Client
{
    internal class ReadSingleValueMessage
    {
        public static byte[] Execute(WS_TcpClient client, uint TagId)
        {
            //Request Message Frame Layout
            //0, 1                Command
            //2, 3,               Tag ID
            //4, 5, 6, 7          always 0, for Write commands it would have the value to be written
            var SendBuffer = new byte[8];
            var MemStream = new MemoryStream(SendBuffer) { Position = 0 };
            var BinWriter = new BinaryWriter(MemStream);

            BinWriter.Write((UInt16)Ws_Commands.WS_CMD_READ_SVALUE);
            BinWriter.Write((UInt16)TagId);
            BinWriter.Write((UInt32)0); //add Padding, 4 bytes

            //Exchange message frames
            byte[] res;
            lock (client.SyncRoot)
            {
                client.SendSingleMessgeFrame(SendBuffer);
                res = client.ReceiveSingleMessgeFrame();
            }

            //Response Message Frame Layout
            //0, 1                Return Code
            //2, 3,               Tag ID
            //4, 5, 6, 7          The value of the requested Tag

            MemStream = new MemoryStream(res) { Position = 0 };
            var BinReader = new BinaryReader(MemStream);

            var RetCode = BinReader.ReadInt16();
            var RetTagId = BinReader.ReadUInt16();
            var RetValue = new byte[4];
            BinReader.Read(RetValue, 0, 4);

            //check Return Code
            client.CheckAndThrowReturnCode(RetCode);

            //check the TagID we received, is the one we asked for
            if (RetTagId != TagId) throw new WS_ProtocolException("the response TagId did not match up with the requested TagId");

            //Return our Payload, which will be 4 bytes
            return RetValue;
        }
    }
}