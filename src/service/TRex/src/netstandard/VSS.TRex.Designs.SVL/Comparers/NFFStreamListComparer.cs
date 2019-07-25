using System;
using System.Collections.Generic;

namespace VSS.TRex.Designs.SVL.Comparers
{
  public class NFFStreamListComparer : IComparer<TNFFStreamInfo>
  {
    public int Compare(TNFFStreamInfo x, TNFFStreamInfo y)
    {
      return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
    }
  }
}
