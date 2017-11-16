using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Utilities.Interfaces
{
    /// <summary>
    /// Interface detailing 'from' and 'to' byte array serialisation semantics
    /// </summary>
    public interface IBinaryReaderWriter
    {
        void Read(BinaryReader reader);
        void Write(BinaryWriter writer);
        void Write(BinaryWriter writer, byte [] buffer);
    }
}
