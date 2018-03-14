using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Storage
{
    /// <summary>
    /// Denotes if a data store has mutable (read-write) or immutable (write-new, readonly) semantics
    /// </summary>
    public enum StorageMutability
    {
        Mutable,
        Immutable
    }
}
