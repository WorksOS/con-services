﻿using System;

namespace VSS.TRex.Types
{
  [Obsolete("use PassCountRangeRecord")]
  public struct TargetPassCountRange
    {
      public ushort Min;
      public ushort Max;

      public void SetMinMax(ushort min, ushort max)
      {
        if (max < min)
          throw new ArgumentException("Maximum value must be greater than or equal to minimum value.");
        Min = min;
        Max = max;
      }
    }
}
