using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Types
{
    /// <summary>
    ///  The types of data held in an FS file: Spatial Directory and segment information, events and ProductionDataXML
    /// </summary>
    public enum FileSystemStreamType
    {
        SubGridSegment,
        SubGridDirectory,
        Events,
        ProductionDataXML
    }
}
