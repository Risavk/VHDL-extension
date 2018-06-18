using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHDL_Extension.Types
{
    class Entity
    {
        public string Name { get; set; }
        public Port Port { get; set; } = new Port();
        public Architecture Architecture { get; set; } = new Architecture(); //Containing Architecture
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        //TODO Add generic
    }
}
