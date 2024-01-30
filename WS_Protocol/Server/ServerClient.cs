using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WS_Protocol.Client;

namespace WS_Protocol.Server
{
    internal class ServerClient
    {
        public TcpClient Client;
        public WS_TcpServer Server;

        public ServerClient(TcpClient client, WS_TcpServer server)
        {
            Client=client;
            Server=server;

            lock (Server.Clients)
            {
                Server.Clients.Add(this);
            }

            ProcessRequests();
        }

        private void ProcessRequests()
        {
            try
            {
                Trace.WriteLine("New client connected, start processing Requests");

                while (true)
                {
                    var Stream = Client.GetStream();
                    var RecBuffer = new byte[8];
                    var ReadBytes = Stream.Read(RecBuffer, 0, 8);

                    //the socket was closed
                    if (ReadBytes != 8) //Each WS message has exactly 8 Bytes, or multiple of 8
                    {
                        Trace.WriteLine("Client disconnected");
                        return;
                    }

                    var MemStream = new MemoryStream(RecBuffer);
                    var BinReader = new BinaryReader(MemStream);
                    var CommandId = (UInt16)BinReader.ReadInt16();

                    switch (CommandId)
                    {
                        case (UInt16)Ws_Commands.WS_CMD_READ_SVALUE:
                            ProcessReadSingleValue(Stream, RecBuffer);
                            break;

                        case (UInt16)Ws_Commands.WS_CMD_WRITE_SVALUE:
                            ProcessWriteSingleValue(Stream, RecBuffer);
                            break;

                        case (UInt16)Ws_Commands.WS_CMD_READ_STRING:
                            ProcessReadStringValue(Stream, RecBuffer);
                            break;


                        case (UInt16)Ws_Commands.WS_CMD_WRITE_STRING:
                            ProcessWriteStringValue(Stream, RecBuffer);
                            break;

                        case (UInt16)Ws_Commands.WS_CMD_NOOP:
                            ProcessNoOp(Stream, RecBuffer);
                            break;

                        default:
                            Trace.WriteLine("Received unknown or unsupported Command");
                            SendErrorResponse(Stream, Ws_Errors.WS_ERR_UNKNOWN_CMD);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception thrown: {0}", ex.Message));
                Disconect();
            }
        }

        private void ProcessReadSingleValue(NetworkStream stream, byte[] RequestMsg)
        {
            var RespBuffer = new byte[8];
            var RespMemStream = new MemoryStream(RespBuffer);
            var ReqMemStream = new MemoryStream(RequestMsg);
            var BinReader = new BinaryReader(ReqMemStream);
            var BinWriter = new BinaryWriter(RespMemStream);

            //Request Message Frame Layout
            //0, 1                Command
            //2, 3,               Tag ID
            //4, 5, 6, 7          always 0, for Write commands it would have the value to be written
            var CmdId = BinReader.ReadInt16();
            var TagId = BinReader.ReadUInt16();
            var Padding = BinReader.ReadUInt32();

            //first we try to find the requested Tag
            var Tag = Server.FindTagByTagId(TagId);
            if (Tag == null)
            {
                Trace.WriteLine(string.Format("Requested unknown Tag with ID: {0}", TagId));
                SendErrorResponse(stream, Ws_Errors.WS_ERR_IMPLAUSIBLE_ARGUMENT);
                return;
            }

            //Check Tag Access rights
            if (Tag.Access != Ws_DataAccess.ReadOnly && Tag.Access != Ws_DataAccess.ReadWrite)
            {
                SendErrorResponse(stream, Ws_Errors.WS_ERR_UNAUTHORIZED_ACCESS);
                return;
            }

            //Response Message Frame Layout
            //0, 1                Return Code
            //2, 3,               Tag ID
            //4, 5, 6, 7          The value of the requested Tag
            BinWriter.Write((Int16)Ws_Errors.WS_ERR_SUCCESSFUL); 
            BinWriter.Write(TagId);

            //Write value depending on data type
            switch (Tag.DataType)
            {
                case Ws_DataTypes.Integer:
                    BinWriter.Write(Tag.IntValue);
                    break;
                case Ws_DataTypes.Floatingpoint:
                    BinWriter.Write(Tag.RealValue);
                    break;
                case Ws_DataTypes.String:       //this does not make sense, the client should call readStringValue!
                    Trace.WriteLine("Client requested an String value from ReadSingleValue, but should have called ReadStringValue");
                    BinWriter.Write(0);
                    break;
            }

            stream.Write(RespBuffer, 0, RespBuffer.Length);
            stream.Flush();
        }

        private void ProcessReadStringValue(NetworkStream stream, byte[] RequestMsg)
        {
            var RespBuffer = new byte[8];
            var RespMemStream = new MemoryStream(RespBuffer);
            var ReqMemStream = new MemoryStream(RequestMsg);
            var BinReader = new BinaryReader(ReqMemStream);
            var BinWriter = new BinaryWriter(RespMemStream);

            //Request Message Frame Layout
            //0, 1                Command
            //2, 3,               Tag ID
            //4, 5, 6, 7          always 0, for Write commands it would have the value to be written
            var CmdId = BinReader.ReadInt16();
            var TagId = BinReader.ReadUInt16();
            var Padding = BinReader.ReadUInt32();

            //first we try to find the requested Tag
            var Tag = Server.FindTagByTagId(TagId);
            if (Tag == null)
            {
                Trace.WriteLine(string.Format("Requested unknown Tag with ID: {0}", TagId));
                SendErrorResponse(stream, Ws_Errors.WS_ERR_IMPLAUSIBLE_ARGUMENT);
                return;
            }

            //Check Tag Access rights
            if (Tag.Access != Ws_DataAccess.ReadOnly && Tag.Access != Ws_DataAccess.ReadWrite)
            {
                SendErrorResponse(stream, Ws_Errors.WS_ERR_UNAUTHORIZED_ACCESS);
                return;
            }

            //Response Message Frame Layout of First Frame
            //0, 1                Return Code
            //2, 3,               Tag ID
            //4, 5, 6, 7          Length of String in Unicode Characters
            BinWriter.Write((Int16)Ws_Errors.WS_ERR_SUCCESSFUL);
            BinWriter.Write(TagId);
            BinWriter.Write(Tag.StringValue.Length);
            stream.Write(RespBuffer, 0, RespBuffer.Length);
            stream.Flush();

            //Response Message Frame Layout of subsequent Frames
            //0, 1, 2, 3, 4, 5, 6, 7               All string data in Unicode
            var StringBytes = Encoding.Unicode.GetBytes(Tag.StringValue);
            var RetByteLengh = StringBytes.Length;

            //Calculate how many message frames we will receive
            var RespMsgFramesStillToGo = (int)Math.Ceiling((double)RetByteLengh / (double)WS_TcpClient.WS_MessgeFrameLength);

            //this will be our buffer for all Message payload bytes
            //we do it this way, because this way we ensure the bytes[] will always be an multiple of the WS Message frame length
            //This basically takes care of the "Padding" of the last message that needs to be done for the last message to reach 8 byte of length
            var RespStringBytes = new byte[RespMsgFramesStillToGo *  WS_TcpClient.WS_MessgeFrameLength]; //this will be our buffer for all Message payload bytes
            StringBytes.CopyTo(RespStringBytes, 0);

            var offset = 0;
            while (offset < StringBytes.Length)
            {
                RespBuffer = new byte[8];
                RespMemStream = new MemoryStream(RespBuffer);

                RespMemStream.Write(RespStringBytes, offset, WS_TcpClient.WS_MessgeFrameLength);
                stream.Write(RespBuffer, 0, RespBuffer.Length);
                stream.Flush();

                offset += 8;
            }
        }

        private void ProcessWriteStringValue(NetworkStream stream, byte[] RequestMsg)
        {
            var RespBuffer = new byte[8];
            var RespMemStream = new MemoryStream(RespBuffer);
            var ReqMemStream = new MemoryStream(RequestMsg);
            var BinReader = new BinaryReader(ReqMemStream);
            var BinWriter = new BinaryWriter(RespMemStream);

            //Request Message Frame Layout
            //0, 1                Command
            //2, 3,               Tag ID
            //4, 5, 6, 7          Length of string, which ultimately determines how many frames we will send
            var CmdId = BinReader.ReadInt16();
            var TagId = BinReader.ReadUInt16();
            var StringLength = BinReader.ReadUInt32();

            //first we try to find the requested Tag
            var Tag = Server.FindTagByTagId(TagId);
            if (Tag == null)
            {
                Trace.WriteLine(string.Format("Requested unknown Tag with ID: {0}", TagId));
                SendErrorResponse(stream, Ws_Errors.WS_ERR_IMPLAUSIBLE_ARGUMENT);
                return;
            }

            //Check Tag Access rights
            if (Tag.Access != Ws_DataAccess.WriteOnly && Tag.Access != Ws_DataAccess.ReadWrite)
            {
                SendErrorResponse(stream, Ws_Errors.WS_ERR_UNAUTHORIZED_ACCESS);
                return;
            }

            //Calculate how many message frames we will receive
            var RetByteLengh = StringLength *2; //The String Length is given in Unicode Characters, which take two byte each.
            var RetMsgFramesStillToGo = (int)Math.Ceiling((double)RetByteLengh / (double)WS_TcpClient.WS_MessgeFrameLength);

            //this will be our buffer for all Message payload bytes
            //we do it this way, because this way we ensure the bytes[] will always be an multiple of the WS Message frame length
            //This basically takes care of the "Padding" of the last message that needs to be done for the last message to reach 8 byte of length
            var RetStringBytes = new byte[RetMsgFramesStillToGo *  WS_TcpClient.WS_MessgeFrameLength]; //this will be our buffer for all Message payload bytes

            //Response Message Frame Layout of subsequent Frames
            //0, 1, 2, 3, 4, 5, 6, 7               All string data in Unicode

            //Response Message Frame Layout of last Frames
            //we ensure this by always using an fresh buffer
            //0, 1, 2, 3, 4, 5              All string data in Unicode
            //6, 7                          Possible Garbage Data      
            for (int i = 0; i < RetMsgFramesStillToGo; i++)
            {
                var res = new byte[WS_TcpClient.WS_MessgeFrameLength];
                stream.Read(res, 0, WS_TcpClient.WS_MessgeFrameLength) ;
                res.CopyTo(RetStringBytes, i * WS_TcpClient.WS_MessgeFrameLength);

            }

            Tag.StringValue = Encoding.Unicode.GetString(RetStringBytes, 0, (int)RetByteLengh);

            //Prepare Response
            BinWriter.Write((Int16)Ws_Errors.WS_ERR_SUCCESSFUL);
            BinWriter.Write((Int16)TagId);
            BinWriter.Write((Int32)0);
            stream.Write(RespBuffer, 0, RespBuffer.Length);
            stream.Flush();
        }

        private void ProcessWriteSingleValue(NetworkStream stream, byte[] RequestMsg)
        {
            var RespBuffer = new byte[8];
            var RespMemStream = new MemoryStream(RespBuffer);
            var ReqMemStream = new MemoryStream(RequestMsg);
            var BinReader = new BinaryReader(ReqMemStream);
            var BinWriter = new BinaryWriter(RespMemStream);

            //Request Message Frame Layout
            //0, 1                Command
            //2, 3,               Tag ID
            //4, 5, 6, 7          value to be written
            var CmdId = BinReader.ReadInt16();
            var TagId = BinReader.ReadUInt16();
            var Value = new byte[4];
            BinReader.Read(Value, 0, 4);

            //first we try to find the requested Tag
            var Tag = Server.FindTagByTagId(TagId);
            if (Tag == null)
            {
                Trace.WriteLine(string.Format("Requested unknown Tag with ID: {0}", TagId));
                SendErrorResponse(stream, Ws_Errors.WS_ERR_IMPLAUSIBLE_ARGUMENT);
                return;
            }

            //Check Tag Access rights
            if (Tag.Access != Ws_DataAccess.WriteOnly && Tag.Access != Ws_DataAccess.ReadWrite)
            {
                SendErrorResponse(stream, Ws_Errors.WS_ERR_UNAUTHORIZED_ACCESS);
                return;
            }

            //Write value to Tag
            //Write value depending on data type
            try
            {
                switch (Tag.DataType)
                {
                    case Ws_DataTypes.Integer:
                        Tag.IntValue = BitConverter.ToInt32(Value, 0);
                        break;
                    case Ws_DataTypes.Floatingpoint:
                        Tag.RealValue = BitConverter.ToSingle(Value, 0);
                        break;
                    case Ws_DataTypes.String:       //this does not make sense, the client should call readStringValue!
                        Trace.WriteLine("Client wrote an String value from WriteSingleValue, but should have called WriteStringValue");
                        SendErrorResponse(stream, Ws_Errors.WS_ERR_WRITE_NOT_SUCCESSFUL);
                        return;
                }
            }
            catch (Exception ex) 
            {
                Trace.WriteLine(String.Format("Failed while writing value to Tag {0} with error {1}", TagId, ex.Message));
                SendErrorResponse(stream, Ws_Errors.WS_ERR_WRITE_NOT_SUCCESSFUL);
                return;
            }

            //Response Message Frame Layout
            //0, 1                Return Code
            //2, 3                Tag ID that was written
            //4, 5, 6, 7          Padding to "0"
            BinWriter.Write((Int16)Ws_Errors.WS_ERR_SUCCESSFUL);
            BinWriter.Write(TagId);
            BinWriter.Write((Int32)0);  //Padding

            stream.Write(RespBuffer, 0, RespBuffer.Length);
            stream.Flush();
        }

        private void ProcessNoOp(NetworkStream stream, byte[] RequestMsg)
        {
            var RespBuffer = new byte[8];
            var RespMemStream = new MemoryStream(RespBuffer);
            var BinWriter = new BinaryWriter(RespMemStream);

            //the NoOp response must be
            //0, 1          0xffff or -1
            //2,3,4,5,6,7   All padded to 0
            BinWriter.Write((short)-1);
            BinWriter.Write((short)0);
            BinWriter.Write((int)0);

            //Send Response
            stream.Write(RespBuffer, 0, RespBuffer.Length);
            stream.Flush();
        }

        private void SendErrorResponse(NetworkStream stream, Ws_Errors Error)
        {
            var RespBuffer = new byte[8];
            var RespMemStream = new MemoryStream(RespBuffer);
            var BinWriter = new BinaryWriter(RespMemStream);

            //Build Error Response
            //0, 1          //Error Code
            //2,3,4,5,6,7   //Padded with 0

            BinWriter.Write((Int16)Error);
            BinWriter.Write((Int16)0); 
            BinWriter.Write((Int32)0);

            stream.Write(RespBuffer, 0, RespBuffer.Length);
            stream.Flush();
        }

        private void Disconect()
        {
            Trace.WriteLine("Server Side Client Disconnection");

            Client.Close();
            lock (Server.Clients)
            {
                Server.Clients.Remove(this);
            }
        }

    }
}