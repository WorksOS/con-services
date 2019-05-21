using System;
using VSS.TRex.Common.Exceptions.Exceptions;
using VSS.TRex.Rendering.Palettes.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  public abstract class PVMDisplayerBase : ProductionPVMDisplayerBase
  {
    private IPlanViewPalette _palette;

    protected virtual void SetPalette(IPlanViewPalette value)
    {
      _palette = value;
    }

    public IPlanViewPalette Palette
    {
      get => _palette;
      set => SetPalette(value);
    }

    /// <summary>
    /// Casts input object to type T for use with child executors.
    /// </summary>
    protected void CastRequestObjectTo<T>(object item, Action action) where T : class
    {
      if (!(item is T request))
        action();
    }

    /// <summary>
    ///  Enables a displayer to advertise is it capable of rendering cell information in strips.
    /// </summary>
    /// <returns></returns>
    protected override bool SupportsCellStripRendering() => true;

    protected void ThrowTRexClientLeafSubGridTypeCastException<T>() where T : class
    {
      throw new TRexClientLeafSubGridTypeCastException(SubGrid.GetType().Name, typeof(T).Name);
    }

    protected void ThrowTRexColorPaletteTypeCastException<T>() where T : class
    {
      throw new TRexColorPaletteTypeCastException(_palette.GetType().Name, typeof(T).Name);
    }

    public PVMDisplayerBase()
    {
    }
  }
}
