using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Rendering.Palettes.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  public class PVMDisplayerBase : ProductionPVMDisplayerBase
    {
        private DisplayMode DisplayMode = DisplayMode.Height;

        public IPlanViewPalette Palette { get; set; }

        //        private DisplayPaletteBase palette = null;
        //        public DisplayPaletteBase Palette { get { return palette; } set { SetPalette(value); } }
        //        private virtual void SetPalette(DisplayPaletteBase value) => Palette = value;

        public PVMDisplayerBase()
        {
        }

        public PVMDisplayerBase(DisplayMode displayMode) : this()
        {
            DisplayMode = displayMode;
        }

        public PVMDisplayerBase(DisplayMode displayMode, IPlanViewPalette palette) : this()
        {
            DisplayMode = displayMode;
            Palette = palette;
        }
    }
}
