using System;
using System.Diagnostics;

namespace VSS.UnitTest.Common.Contexts 
{
  public class ContextContainer
  {
    [ThreadStatic] private static IContextContainer _contextContainer;

    public static IContextContainer Current 
    {
      get
      {
        Debug.WriteLine("ContextContainer Called");

        if(_contextContainer == null)
        {
          _contextContainer = new ContextContainerImpl();
          Debug.WriteLine("ContextContainer Created");
        }
        return _contextContainer;
      }
    }
  }
}
