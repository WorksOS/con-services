using System;
using System.Collections.Generic;
using System.Diagnostics;
using VSS.TRex.Common;

namespace VSS.TRex.Designs.SVL
{
  public class NamedGuidanceIDComparer : IComparer<TNFFNamedGuidanceID>
  {
    public int Compare(TNFFNamedGuidanceID x, TNFFNamedGuidanceID y)
    {
      double CalcNamedGuidanceIDCompareOffset(TNFFNamedGuidanceID NamedGuidanceID)
      {
        const double BatterAlignmentOffset  = 1E10;
        const double DitchAlignmentOffset = 1E9;
        const double HingeAlignmentOffset = 1E8;

        double Result = 0.0;

        // Return a modified Offset value for a NamedGuidanceID, fudged for NamedGuidanceIDs
        // that are flagged as being Hinge, Ditch or Batter.
        switch (NamedGuidanceID.GuidanceAlignmentType)
        {
          case NFFGuidanceAlignmentType.gtMasterAlignment:
         case NFFGuidanceAlignmentType.gtSubAlignment:
            return NamedGuidanceID.StartOffset;
         case NFFGuidanceAlignmentType.gtHinge:
            return HingeAlignmentOffset * Math.Sign(NamedGuidanceID.StartOffset);
         case NFFGuidanceAlignmentType.gtDitch:
            return DitchAlignmentOffset * Math.Sign(NamedGuidanceID.StartOffset);
         case NFFGuidanceAlignmentType.gtBatter:
            return BatterAlignmentOffset * Math.Sign(NamedGuidanceID.StartOffset);
          default:
            Debug.Assert(false, "Unknown guidance alignment type");
            return Consts.NullDouble;
        }
      }

      Debug.Assert(x.StartOffset != Consts.NullDouble && y.StartOffset != Consts.NullDouble);

      return CalcNamedGuidanceIDCompareOffset(x).CompareTo(CalcNamedGuidanceIDCompareOffset(y));
    }
  }
}
