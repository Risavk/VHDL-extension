using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHDL_Extension.Types
{
    class Architecture
    {
        public string Name { get; set; } //Name is not optional
        public List<ArchitectureSignal> Signals { get; set; } = new List<ArchitectureSignal>();
        public List<Process> Processes { get; set; } //All the processes contained in the Architecture
    }
}
