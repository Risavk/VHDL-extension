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
        Linkage
    }

    public interface ISignal
    {
        string Name { get; set; }

        string Kind { get; set; }
    }
}
