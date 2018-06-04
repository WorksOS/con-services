using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.Types
{
    public struct TargetPassCountRange
    {
      public ushort Min;
      public ushort Max;

      public void SetMinMax(ushort min, ushort max)
      {
        Min = min;
        Max = max;
      }
    }
}
