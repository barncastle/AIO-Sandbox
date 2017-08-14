using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Interfaces
{
    public interface IOpcodes
    {
        Opcodes this[uint id] { get; }
        uint this[Opcodes opcode] { get; }
        bool OpcodeExists(uint opcode);
    }
}
