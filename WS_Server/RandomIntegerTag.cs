using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WS_Protocol.Server;

namespace WS_Server
{
    /// <summary>
    /// You can Override Server Tags to implement custom Getter and Setter logic
    /// </summary>
    internal class RandomIntegerTag: ServerTag
    {
        public override int IntValue 
        { 
            get => new Random().Next(); 
            set => base.IntValue=value; 
        }
    }
}
