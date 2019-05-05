using VSS.TRex.Rendering.Palettes.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  public abstract class PVMDisplayerBase : ProductionPVMDisplayerBase
  {
    public IPlanViewPalette Palette { get; set; }

    //        private DisplayPaletteBase palette = null;
    //        public DisplayPaletteBase Palette { get { return palette; } set { SetPalette(value); } }
    //        private virtual void SetPalette(DisplayPaletteBase value) => Palette = value;

    /// <summary>
    ///  Enables a displayer to advertise is it capable of rendering cell information in strips.
    /// </summary>
    /// <returns></returns>
    protected override bool SupportsCellStripRendering() => true;

    public PVMDisplayerBase()
    {
    }
  }
}
