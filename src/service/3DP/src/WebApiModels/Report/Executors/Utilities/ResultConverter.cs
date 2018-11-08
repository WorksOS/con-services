using ASNode.Volumes.RPC;
using BoundingExtents;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors.Utilities
{
  public class ResultConverter
  {
    /// <summary>
    /// Converts a <see cref="TASNodeSimpleVolumesResult"/> to a <see cref="SummaryVolumesResult"/> object.
    /// </summary>
    public static SummaryVolumesResult SimpleVolumesResultToSummaryVolumesResult(TASNodeSimpleVolumesResult result)
    {
      return SummaryVolumesResult.Create(
        ConvertBoundingExtentsToBoundingBox(result.BoundingExtents),
        result.Cut,
        result.Fill,
        result.TotalCoverageArea,
        result.CutArea,
        result.FillArea);
    }

    /// <summary>
    /// Converts a <see cref="T3DBoundingWorldExtent"/> to a <see cref="BoundingBox3DGrid"/> object.
    /// </summary>
    private static BoundingBox3DGrid ConvertBoundingExtentsToBoundingBox(T3DBoundingWorldExtent extents)
    {
      return BoundingBox3DGrid.CreatBoundingBox3DGrid(
        extents.MinX,
        extents.MinY,
        extents.MinZ,
        extents.MaxX,
        extents.MaxY,
        extents.MaxZ);
    }
  }
}
