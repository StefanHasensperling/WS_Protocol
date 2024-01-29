using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_Protocol.Server
{
    public class ServerTag
    {
        public virtual uint TagId { get; set; }
        public virtual string Name { get; set; }
        public virtual Ws_DataTypes DataType { get; set; } = Ws_DataTypes.Integer;
        public virtual string StringValue { get; set; } = "";
        public virtual int IntValue { get; set; } = 0;
        public virtual double RealValue { get; set; } = 0.0;
        public virtual Ws_DataAccess Access { get; set; } = Ws_DataAccess.ReadWrite;
    }
}
