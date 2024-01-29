using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_Protocol
{
    internal enum Ws_Errors: UInt16
    {
        WS_ERR_SUCCESSFUL = 0x0,

        /// <summary>
        /// WS error code: Writing not successful
        /// </summary>
        WS_ERR_WRITE_NOT_SUCCESSFUL =	0x8888,

        /// <summary>
        /// WS error code: Memory overflow
        /// </summary>
        WS_ERR_MEMORY_OVERFLOW =	0x9999,

        /// <summary>
        /// WS error code: Unknown command
        /// </summary>
        WS_ERR_UNKNOWN_CMD =	0xAAAA,

        /// <summary>
        /// WS error code: Unauthorized access
        /// </summary>
        WS_ERR_UNAUTHORIZED_ACCESS =	0xBBBB,

        /// <summary>
        /// WS error code: Server overload
        /// </summary>
        WS_ERR_SERVER_OVERLOAD = 0xCCCC,

        /// <summary>
        /// WS error code: Implausible argument
        /// </summary>
        WS_ERR_IMPLAUSIBLE_ARGUMENT =	0xDDDD,

        /// <summary>
        /// WS error code: Implausible list
        /// </summary>
        WS_ERR_IMPLAUSIBLE_LIST =	0xEEEE,

        /// <summary>
        /// WS error code: Implausible list
        /// </summary>
        WS_ERR_ALIVE = 0xFFFF
    }


    internal enum Ws_Commands : UInt16
    {
        WS_CMD_NOOP = 1,
        WS_CMD_READ_SVALUE = 2,
        WS_CMD_WRITE_SVALUE = 3,
        WS_CMD_READ_LIST = 4,
        WS_CMD_WRITE_LIST = 5,
        WS_CMD_READ_STRING = 8,
        WS_CMD_WRITE_STRING = 9
    }

    public enum Ws_DataTypes
    {
        Integer,
        Floatingpoint,
        String
    }

    public enum Ws_DataAccess
    {
        ReadOnly,
        WriteOnly,
        ReadWrite
    }
}

