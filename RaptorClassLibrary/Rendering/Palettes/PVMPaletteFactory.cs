using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Rendering.Palettes.Interfaces;

namespace VSS.VisionLink.Raptor.Rendering.Palettes
{
    /// <summary>
    /// A factory responsible for determining the appropriate plan view map colour palette to use
    /// when rendering plan view tiles (Web Map Service tiles)
    /// </summary>
    public static class PVMPaletteFactory
    {
        public static IPlanViewPalette GetPallete()
        {
            return null;
        }
    }
}
