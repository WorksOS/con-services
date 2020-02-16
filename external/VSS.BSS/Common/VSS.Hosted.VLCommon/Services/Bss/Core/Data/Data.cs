using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class Data
  {
    [ThreadStatic]
    private static DataContext _data;
    public static DataContext Context
    {
      get { return _data ?? (_data = new DataContext()); }
    }
  }
}