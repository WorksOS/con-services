using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Rendering.Palettes.Interfaces;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Rendering.Displayers
{
    public class PVMDisplayerBase : ProductionPVMDisplayerBase
    {
        private DisplayMode DisplayMode = DisplayMode.Height;

        public IPlanViewPalette Palette { get; set; } = null;

        //        private DisplayPaletteBase palette = null;
        //        public DisplayPaletteBase Palette { get { return palette; } set { SetPalette(value); } }
        //        private virtual void SetPalette(DisplayPaletteBase value) => Palette = value;

        public PVMDisplayerBase() : base()
        {
        }

        public PVMDisplayerBase(DisplayMode displayMode) : this()
        {
            DisplayMode = displayMode;
        }
    }
}
