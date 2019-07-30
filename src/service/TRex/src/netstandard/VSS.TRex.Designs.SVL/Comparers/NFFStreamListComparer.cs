using System;
using System.Collections.Generic;

namespace VSS.TRex.Designs.SVL.Comparers
{
  public class NFFStreamListComparer : IComparer<NFFStreamInfo>
  {
    public int Compare(NFFStreamInfo x, NFFStreamInfo y)
    {
      if (x == null && y == null) return 0;
      if (x == null) return -1;
      if (y == null) return 1;

      return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
    }
  }
}
