using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Types
{
    /// <summary>
    ///  The types of data held in an FS file: Spatial Directory and segmetn information and event and event directory
    /// </summary>
    public enum FileSystemSpatialStreamType
    {
        SubGridSegment,
        SubGridDirectory,
        Events,
        EventDirectory
    }
}
