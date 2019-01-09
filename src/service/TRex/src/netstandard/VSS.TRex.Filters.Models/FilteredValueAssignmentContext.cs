using System;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Filters.Models
{
    /// <summary>
    /// FilteredValueAssignmentContext provides a context for collecting filtered values from production data in the 
    /// process of applying spatial, temporal and attribute filtering constraints.
    /// </summary>
    public class FilteredValueAssignmentContext
    {
        /// <summary>
        /// Local structure used to define the probe point positions for design lookups during filtering
        /// </summary>
        public struct ProbePoint
        {
            public float XOffset, YOffset;

            public ProbePoint(float xOffset, float yOffset)
            {
                XOffset = xOffset;
                YOffset = yOffset;
            }

            public void SetOffsets(float xOffset, float yOffset)
            {
                XOffset = xOffset;
                YOffset = yOffset;
            }

          public void SetOffsets(double xOffset, double yOffset)
          {
            XOffset = (float)xOffset;
            YOffset = (float)yOffset;
          }
        }

    public FilteredSinglePassInfo FilteredValue;

        public FilteredSinglePassInfo PreviousFilteredValue;

        public object /*IProfileCell*/ CellProfile { get; set; }

        // TODO     LiftBuildSettings: TICLiftBuildSettings;

        /// <summary>
        /// ProbePositions is used to store the real world positions used to probe into
        /// cells when computing floating type sieved bit masks during subgrid querying.
        /// The positions are stored as offsets from the real world origin of the subgrid
        /// As many queries do not need access to probe positions the array is created
        /// only if InitialiseProbePositions() is called
        /// </summary>
        public ProbePoint[,] ProbePositions { get; } = new ProbePoint[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

        /// <summary>
        /// Constructor...
        /// </summary>
        public FilteredValueAssignmentContext()
        {
            FilteredValue.Clear();
            PreviousFilteredValue.Clear();
            CellProfile = null;
//          todo  LiftBuildSettings = Nil;
        }
    }
}
