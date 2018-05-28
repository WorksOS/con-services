using VSS.TRex.Filters;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface ICellLiftBuilder
  {
    bool Build(ProfileCell cell,
      // todo const LiftBuildSettings: TICLiftBuildSettings;
      IClientLeafSubGrid ClientGrid,
      FilteredValueAssignmentContext AssignmentContext,
      ISubGridSegmentCellPassIterator cellPassIterator,
      ref int filteredPassCountOfTopMostLayer,

      // FilteredHalfCellPassCountOfTopMostLayer tracks 'half cell passes'.
      // A half cell pass is recorded whan a Quattro four drum compactor drives over the ground.
      // Other machines, like single drum compactors, record two half cell pass counts to form a single cell pass.
      ref int FilteredHalfCellPassCountOfTopMostLayer);
  }
}
