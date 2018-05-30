using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHDL_Extension.Types
{
    public class std_logic : IDatatype
    {
        public Types Type => Types.std_logic;
        public char[] Values => new[] {'U', 'X', '0', '1', 'Z', 'W', 'L', 'H', '-'};
    }

    public class std_logic_vector : IDatatype
    {
        public Types Type => Types.std_logic_vector;
        public char[] Values => new[] { 'U', 'X', '0', '1', 'Z', 'W', 'L', 'H', '-' };
        public int Length { get; set; }
    }

    public interface IDatatype
    {
        Types Type { get; }
        char[] Values { get; }
    }

    public enum Types
    {
        std_logic,
        std_logic_vector
    }
}
