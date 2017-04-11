using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Types
{
    /// <summary>
    /// The layer detection mode to be used for layers analysis of cell passes within a cell
    /// </summary>
    public enum LayerMethod
    {
          Invalid,
          OffsetFromBench,
          OffsetFromDesign,
          OffsetFromProfile,
          MapReset,
          TagFileLayerNumber,
          AutoMapReset,
          Automatic
    }
}
