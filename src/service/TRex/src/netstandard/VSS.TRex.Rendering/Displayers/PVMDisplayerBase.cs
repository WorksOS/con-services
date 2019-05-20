using VSS.TRex.Common.Exceptions.Exceptions;
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

    protected void ThrowTRexClientLeafSubGridException()
    {
      throw new TRexClientLeafSubGridException($"Invalid ClientLeafSubGrid type: {nameof(SubGrid)}");
    }

    protected void ThrowTRexColorPaletteException()
    {
      throw new TRexColorPaletteException($"Invalid Palette type: {nameof(Palette)}");
    }

    public PVMDisplayerBase()
    {
    }
  }
}
