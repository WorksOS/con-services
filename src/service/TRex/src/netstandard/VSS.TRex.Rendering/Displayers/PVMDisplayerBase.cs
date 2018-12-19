using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  public class PVMDisplayerBase : ProductionPVMDisplayerBase
  {
    protected DisplayMode DisplayMode = DisplayMode.Height;

    public IPlanViewPalette Palette { get; set; }

    //        private DisplayPaletteBase palette = null;
    //        public DisplayPaletteBase Palette { get { return palette; } set { SetPalette(value); } }
    //        private virtual void SetPalette(DisplayPaletteBase value) => Palette = value;

    protected PVMDisplayerBase()
    {
    }

    protected PVMDisplayerBase(DisplayMode displayMode)
    {
      DisplayMode = displayMode;
    }

    protected PVMDisplayerBase(DisplayMode displayMode, IPlanViewPalette palette)
    {
      DisplayMode = displayMode;
      Palette = palette;
    }

    ///// <summary>
    ///// Renders CCA summary data as tiles. 
    ///// </summary>
    ///// <param name="subGrid"></param>
    ///// <returns></returns>
    //protected override bool DoRenderSubGrid<T>(ISubGrid subGrid)
    //{
    //  if (subGrid is T grid)
    //  {
    //    SubGrid = (ISubGrid) grid;
    //    return base.DoRenderSubGrid<T>(SubGrid);
    //  }

    //  return false;
    //}
  }
}
