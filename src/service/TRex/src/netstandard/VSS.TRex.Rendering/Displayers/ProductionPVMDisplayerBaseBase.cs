using System;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  public abstract class ProductionPVMDisplayerBaseBase : IDisposable
  {
    public MapSurface MapView;

    public abstract void SetPalette(IPlanViewPalette palette);

    public abstract IPlanViewPalette GetPalette();

    public abstract bool RenderSubGrid(IClientLeafSubGrid clientSubGrid);

    #region IDisposable Support
    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          MapView?.Dispose();
          MapView = null;
        }

        disposedValue = true;
      }
    }

    // Override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~ProductionPVMDisplayerBase()
    // {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // Uncomment the following line if the finalizer is overridden above.
      // GC.SuppressFinalize(this);
    }
    #endregion
  }
}
