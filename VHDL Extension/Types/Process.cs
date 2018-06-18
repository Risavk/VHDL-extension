using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHDL_Extension.Types
{
    class Process
    {
        public string Name { get; set; } //Process Name. Is optional
        public List<Variable> Variables { get; set; } = new List<Variable>();
    }

    class Variable
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string InitialValue { get; set; }
    }
}
