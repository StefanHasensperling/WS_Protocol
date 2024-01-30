
# WS Protocol Client and Server Library

This is an library that implements the Weihenstephan Standards Protocol (WS Protocol in short). You can find more information here: [WS Protocol - Weihenstephan-Standards](https://www.weihenstephan-standards.com/technical-information/communication-interface/ws-protocol/)
The Weihenstephaner Standards are used for data exchange between industrial beverage filling machines and Data Acquisition systems/Manufacturing Execution Systems.
This standard is particulary popular in European Manufacturers for "Carbonated Soft drink" and "Beer" filling equipment. 
The WS (Weihenstephaner Standard) defines certain "data points" (called Tags) that each machine can send to an requesting application, which holds production related data, so the connected application can calculate "Machine Efficiency", 
"Stand still times" and so on. It is basically meant to Standardize the data that each machine can hand out to MES applications to make it easier to integrate different manufacturers.  The "Tags" define data such as: "Machine Operating State", 
"Current Bottles per Hour", "Total Filled Bottles", "Total of Bad product", etc. 
The Weihenstephaner Standard also defines "Machine Profiles", which defines which machine type (Filling Machine, Shrink Wrapper, Palletizer, etc.) must have which Tags available. 

# What is the WS Protocol?
The WS Protocol is an binary TCP based protocol that is used to quickly and easily exchange data between Clients (the application) and Servers (the filling machines). It is an very simple Protocol similar, but not compatible with Modbus TCP.
The Weihenstephaner Standard primarily is focused on "OPC-UA", but this simple TCP protocol is being used for most machines, since it is simple and very easy to implement in the machines "Plc (Programmable Logic Controllers)".

# How does the WS Protocol work?
The Protocol uses an TCP connection usually on Port 5000 or 50000. It implements an very simple Request/Response mechanism, where you "ask" the machine for some "Tags" and it sends you the values back. 
The WS Protocol does not implement any Security mechanism whatsoever, it has no Encryption, no Authentication, no Request Throttling, Replay attack prevention, not even Message Integrity checks. 
It relies entirely on the TCP Protocols built in message integrity mechanisms. 

The data exchange works by sending and receiving "WS Message Frames". Each message frame is exactly 8 bytes long. So each data exchange happens in 8 byte quantum. 
If an Request or response Message is shorter that 8 bytes, it is padded with 0, to always be of 8 bytes length.
Some response messages are longer than 8 bytes, which means that you will receive multiple message frames. The first frame indicates the payload size. From that you have to calculate how many message frames (each 8 bytes long) you will need to receive. 
The remainder of the last message most of the time contains "Garbage" or "0". The "Garbage" data comes from the simplistic implementation in the Plc's that run the "Server" of this protocol.

# Message Types (Commands IDs)
There are the following message types:

|Value |Command |Description|
|--|--|--|
|1|No Op |Null Operation, does nothing on the server, but allows the client to check if the server is still alive and responding. Serves as an Heartbeat message
|2|Read Single Value |Reads an single Tags value either as 32 bit integer or 32 bit floating point value
|3|Write Single Value |Writes an single Tags value either as 32 bit integer or 32 bit floating point value
|4|Read List |Reads an Predefined list from the server. The list is predefined on the server. NOT IMPLEMENTED IN THIS LIBRARY!
|5|Write List |Writes an Predefined list from the server. The list is predefined on the server. NOT IMPLEMENTED IN THIS LIBRARY!
|8|Read String |Reads a single Tag as string value from the server
|9|Write String |Writes a single Tag as string value to the server

# Return Codes of Response Messages
|Value |Name| Description|
|--|--|--|
|0x0|WS_ERR_SUCCESSFUL |
|0x8888|WS_ERR_WRITE_NOT_SUCCESSFUL|WS error code: Writing not successful
|0x9999|WS_ERR_MEMORY_OVERFLOW |WS error code: Memory overflow
|0xAAAA|WS_ERR_UNKNOWN_CMD |WS error code: Unknown command or Not supported
|0xBBBB|WS_ERR_UNAUTHORIZED_ACCESS |WS error code: Unauthorized access. You tried to write an read only tag
|0xCCCC|WS_ERR_SERVER_OVERLOAD |WS error code: Server overload
|0xDDDD|WS_ERR_IMPLAUSIBLE_ARGUMENT |WS error code: Implausible argument. The Tag does not exist
|0xEEEE|WS_ERR_IMPLAUSIBLE_LIST |WS error code: Implausible list
|0xFFFF|WS_ERR_ALIVE |WS error code: Memory overflow

# Common Message Frames

## Read Single Value Request
|Byte |Data |Value|
|--|--|--|
|0| Command ID LSB |0x02
|1| Command ID MSB |0x00
|2| Tag ID LSB |xxx
|3| Tag ID MSB |xxx
|4| Padding, always 0 |0x00
|5| Padding, always 0 |0x00
|6| Padding, always 0 |0x00
|7| Padding, always 0 |0x00

## Read Single Value Response
|Byte |Data |Value|
|--|--|--|
|0| Return Code LSB |0x00
|1| Return Code MSB |0x00
|2| Tag ID LSB | xxx
|3| Tag ID MSB | xxx
|4| Value LSB | xxx
|5| Value | xxx
|6| Value | xxx
|7| Value MSB | xxx
The value must be converted to either an 32 bit integer or real, depending on the Tag that was requested. Unfortunately you have to know what type each TagIds is.

## Write Single Value Request
|Byte |Data |Value|
|--|--|--|
|0| Command ID LSB |0x03
|1| Command ID MSB |0x00
|2| Tag ID LSB |xxx
|3| Tag ID MSB |xxx
|4| Value LSB | xxx
|5| Value | xxx
|6| Value | xxx
|7| Value MSB | xxx

## Read Single Value Response
|Byte |Data |Value|
|--|--|--|
|0| Return Code LSB |0x00
|1| Return Code MSB |0x00
|2| Tag ID LSB | xxx
|3| Tag ID MSB | xxx
|4| Padding, always 0 |0x00
|5| Padding, always 0 |0x00
|6| Padding, always 0 |0x00
|7| Padding, always 0 |0x00
The value must be converted to either an 32 bit integer or real, depending on the Tag that was requested. Unfortunately you have to know what type each TagIds is.

# TagIds
The TagIds are mostly standardized by the Weihenstephaner Standard itself. But there are also a lot of "Manufacturer" specific tags that the different manufacturers use, which are not documented anywhere i have access to. 
In the source code you can check the file "WS_TagNumbers.xlsx" which lists an lot of Tags that i have found during Reverse Engineering.
Unfortunately there is no definitive list so you will have to poke around in the Manufacturer documentation.

If you find more Tags, please make a pull request, and I will be happy to add them 

# How did this Library come to be
The motivation for this library was that i needed to get some production statistics from Beverage Packaging Machines, that did not support other protocols. Since there where no libraries for the WS Protocol, I made one. 

This library was entirely created by "Reverse Engineering" of the communication Protocol between an existing filling machine and an MES application. 
I do not have access to the "Weihenstephaner Standard", so everything was done by Packet Capturing and Reverse Engineering of Plc programs. 
This means that this library "works" but is probably not fully "Weihenstephaner Standard" compliant. 
However, it allows applications to read and even write Tags from/to filling machines and thus allows you to read production data from these machines. 

# Testing
Currently testing is entirely done by running the Plc code of an filling machine in an "Virtual Soft PLC" (which is kind of an Virtual Machine for Programmable logic controllers), and connecting to this "Server". 
Currently I do not have access to filling machines, so I can not test it under Live conditions. 

# Current State of this Library
The library currently works for the basic functionality, namely Reading and writing single values and also reading and writing string values.
The more advanced functions such as "ReadList", "ReadConfig" etc. are currently not implemented, as i do not have an test-environment available that 
implement these functions with which y could test this library against.

The library basically "Works", although it is currently not thoroughly tested, and may have error in some corner-cases.

# Manufacturers that usually implement the Weihenstephaner-Standards
Usually the Standard is implemented by the "big" filling machine manufacturers from Europe. 
Be aware that not all machine actually implement the standard, since it is relatively new (as of 2024), so only machines from about 2018 onwards possibly have this standard. 
You can check by probing for the commonly used TCP Ports of an Plc of an filling machine. 

The ones that I encountered are:
 - KHS
 - Heuft
 - Krones

# Client example
Please look into the WS_Test or WS_TestClient project

```C#
var WSclient = new WS_TcpClient("127.0.0.1", 5000) ;
WSclient.Connect();
var TagValueOfTagID30 = WSclient.ReadSingleValueAsInt(30);
```

# Server example
Please look into the Server Sample project for information.
The basic idea is that you subclass the ServerTag which gives you the ability to overload the getter and setter of the respective properties, and gives you the ability to inject your own logic when an value is read or written. 

Here an example on how to set up an Server:
```C#
var WsServer = new WS_TcpServer(5000);

//add some normal Read write Tags
WsServer.Tags.Add(new ServerTag() { DataType = WS_Protocol.Ws_DataTypes.Integer, TagId = 30, IntValue = 130 });
WsServer.Tags.Add(new ServerTag() { DataType = WS_Protocol.Ws_DataTypes.Integer, TagId = 190, IntValue = 300 });
WsServer.Tags.Add(new ServerTag() { DataType = WS_Protocol.Ws_DataTypes.String, TagId = 30, StringValue = "test" });
WsServer.Tags.Add(new ServerTag() { DataType = WS_Protocol.Ws_DataTypes.String, TagId = 31, StringValue = "" });

//Add an custom Tag that always returns Random integer
WsServer.Tags.Add(new RandomIntegerTag() { DataType = WS_Protocol.Ws_DataTypes.Integer, TagId = 200, IntValue = 300 });

//Start up the server and accept incoming client requests
WsServer.Start();
```

Here an example of an Server Tag:
```C#
/// <summary>
/// You can Override Server Tags to implement custom Getter and Setter logic, in this case always get Random Numbers
/// </summary>
internal class RandomIntegerTag: ServerTag
{
	public override int IntValue 
	{ 
		get => new Random().Next(); 
		set => base.IntValue=value; 
	}
}
```

# Todo
 - Implement ReadList and WriteList: I have no idea how they work
 - Find out how these work
 - Find an example "PdaConfig.xml" to parse and extract TagIds from: I would really appreciate if somebody could send me one
 - Implement "ReadPdaConfig" commands
 - Implement "ReadMultiList" and "WriteMultiList" Commands: I have no idea what they are supposed to do
