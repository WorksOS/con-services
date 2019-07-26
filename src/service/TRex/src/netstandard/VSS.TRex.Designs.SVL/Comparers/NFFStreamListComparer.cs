using System;
using System.Collections.Generic;

namespace VSS.TRex.Designs.SVL.Comparers
{
  public class NFFStreamListComparer : IComparer<NFFStreamInfo>
  {
    public int Compare(NFFStreamInfo x, NFFStreamInfo y)
    {
      return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
    }
  }
}
