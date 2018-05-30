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
    }

    public interface ISignal
    {
        string Name { get; set; }
        
    }
}
