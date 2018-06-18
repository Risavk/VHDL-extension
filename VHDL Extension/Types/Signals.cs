using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHDL_Extension.Types
{
    public class PortSignal : ISignal
    {
        public string Name { get; set; }
        public string Kind { get; set; }

        public PortDirection Direction { get; set; }

        public static PortDirection GetDirection(string direction)
        {
            switch (direction)
            {
                case "in":
                    return PortDirection.In;
                case "out":
                    return PortDirection.Out;
                case "inout":
                    return PortDirection.Inout;
                case "buffer":
                    return PortDirection.Buffer;
                case "linkage":
                    return PortDirection.Linkage;
                default:
                    return PortDirection.UNEXPECTED;
            }
        }
    }

    public class ArchitectureSignal : ISignal
    {
        public string Name { get; set; }
        public string Kind { get; set; }
    }

    public enum PortDirection
    {
        In,
        Out,
        Inout,
        Buffer,
        Linkage,
        UNEXPECTED
    }

    public interface ISignal
    {
        string Name { get; set; }

        string Kind { get; set; }
    }
}
