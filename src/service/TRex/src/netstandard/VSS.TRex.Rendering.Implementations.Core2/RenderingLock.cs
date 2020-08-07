using System;

namespace VSS.TRex.Rendering.Implementations.Core2
{
  public static class RenderingLock
  {
    public static object Lock = new object();
  }
}
