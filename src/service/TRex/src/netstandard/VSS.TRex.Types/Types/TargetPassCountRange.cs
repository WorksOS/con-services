using System;

namespace VSS.TRex.Types
{
    public struct TargetPassCountRange
    {
      public ushort Min;
      public ushort Max;

      public void SetMinMax(ushort min, ushort max)
      {
        if (max < min)
          throw new ArgumentException("Minimum value must be greater than or equal to minimum value.");
        Min = min;
        Max = max;
      }
    }
}
