using System;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DataContext : IDisposable
  {
    private INH_OP _opContext;
    public INH_OP OP
    {
      get { return _opContext ?? (_opContext = ObjectContextFactory.NewNHContext<INH_OP>()); }
    }

    
    public void Dispose()
    {
      if (_opContext != null)
      {
        _opContext.Dispose();
        _opContext = null;
      }

      GC.SuppressFinalize(this);
    }
  }
}