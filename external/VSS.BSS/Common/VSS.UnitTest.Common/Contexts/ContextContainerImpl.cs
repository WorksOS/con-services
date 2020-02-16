using System;
using System.Diagnostics;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.Contexts
{
  public class ContextContainerImpl : IContextContainer
  {
    private INH_OP _opContext;
    private INH_OP _rawContext;

    public INH_OP OpContext 
    {
      get 
      {
        if (_opContext == null)
        {
          Debug.WriteLine("ContextContainer.OpContext Created");
          _opContext = ObjectContextFactory.NewNHContext<INH_OP>();
        }
        return _opContext;
      }
    }
    public INH_OP RawContext 
    {
      get 
      {
        if (_rawContext == null)
        {
          Debug.WriteLine("ContextContainer.RawContext Created");
          _rawContext = ObjectContextFactory.NewNHContext<INH_OP>();
        }
        return _rawContext;
      }
    }
    

    public void Dispose() 
    {
      Debug.WriteLine("ContextContainer Dispose Called");

      if (_opContext != null) 
      {
        Debug.WriteLine("ContextContainer.OpContext Disposed");
        _opContext.Dispose();
        _opContext = null;
      }

      if (_rawContext != null) 
      {
        Debug.WriteLine("ContextContainer.RawContext Disposed");
        _rawContext.Dispose();
        _rawContext = null;
      }

      GC.SuppressFinalize(this);
    }
  }
}