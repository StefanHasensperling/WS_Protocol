using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_Protocol.Client
{
    internal class ReadSingleStringMessage
    {
        public static string Execute(WS_TcpClient client, uint TagId)
        {
            //Request Message Frame Layout
            //0, 1                Command
            //2, 3,               Tag ID
            //4, 5, 6, 7          Padding to "0"
            var SendBuffer = new byte[8];
            var MemStream = new MemoryStream(SendBuffer) { Position = 0 };
            var BinWriter = new BinaryWriter(MemStream);

            BinWriter.Write((UInt16)Ws_Commands.WS_CMD_READ_STRING);
            BinWriter.Write((UInt16)TagId);
            BinWriter.Write((UInt32)0); //add Padding, 4 bytes

            //Exchange message frames
            byte[] res;
            lock (client.SyncRoot)
            {
                client.SendSingleMessgeFrame(SendBuffer);
                res = client.ReceiveSingleMessgeFrame();


                //Response Message Frame Layout of First Frame
                //0, 1                Return Code
                //2, 3,               Tag ID
                //4, 5, 6, 7          Length of String in Unicode Characters

                MemStream = new MemoryStream(res) { Position = 0 };
                var BinReader = new BinaryReader(MemStream);

                var RetCode = BinReader.ReadInt16();
                var RetTagId = BinReader.ReadUInt16();
                var RetStringLength = BinReader.ReadUInt32();

                //check Return Code
                //if there is an error, the server will not send any further Message frames, so we can safely throw here
                client.CheckAndThrowReturnCode(RetCode);

                //The String Length is given in Unicode Characters, which take two byte each.
                var RetByteLengh = RetStringLength *2;

                //Calculate how many message frames we will receive
                var RetMsgFramesStillToGo = (int)Math.Ceiling((double)RetByteLengh / (double)WS_TcpClient.WS_MessgeFrameLength);

                //Response Message Frame Layout of subsequent Frames
                //0, 1, 2, 3, 4, 5, 6, 7               All string data in Unicode

                //this will be our buffer for all Message payload bytes
                //we do it this way, because this way we ensure the bytes[] will always be an multiple of the WS Message frame length
                //This basically takes care of the "Padding" of the last message that needs to be done for the last message to reach 8 byte of length
                var RetStringBytes = new byte[RetMsgFramesStillToGo *  WS_TcpClient.WS_MessgeFrameLength]; 

                for (int i = 0; i < RetMsgFramesStillToGo; i++) 
                {
                    res = client.ReceiveSingleMessgeFrame();
                    res.CopyTo(RetStringBytes, i * WS_TcpClient.WS_MessgeFrameLength);
                }

                //check the TagID we received, is the one we asked for
                //we have to check this after receiving all Message frames, otherwise we could leave message frames on the stream
                if (RetTagId != TagId) throw new WS_ProtocolException("the response TagId did not match up with the requested TagId");

                //it is important to note that due to the simplistic implementation of the server in the Plc, it may return 
                //Garbage padding data after the length of the string, in the last message. We have to "Trim" these, otherwise we read garbage
                //Response Message Frame Layout of Last Message Frame
                //0, 1                           string data in Unicode
                //2, 3, 4, 5, 6, 7               garbage data
                return Encoding.Unicode.GetString(RetStringBytes, 0, (int)RetByteLengh);
            }
        }
    }
}