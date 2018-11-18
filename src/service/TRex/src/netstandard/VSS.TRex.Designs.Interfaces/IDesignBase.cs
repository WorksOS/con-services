using System;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesignBase
  {
    int LockCount { get; }
    string FileName { get; set; }
    long DataModelID { get; set; }
    bool IsStale { get; set; }
    bool Locked { get; }
    DesignLoadResult LoadFromFile(string fileName);
    DesignLoadResult LoadFromStorage(Guid siteModelUid, string fileName, string localPath, bool loadIndices = false);
    void GetExtents(out double x1, out double y1, out double x2, out double y2);
    void GetHeightRange(out double z1, out double z2);

    bool InterpolateHeight(ref int Hint,
      double X, double Y,
      double Offset,
      out double Z);

    bool InterpolateHeights(float[,] Patch, // The receiver of the patch of elevations
      double OriginX, double OriginY,
      double CellSize,
      double Offset);

    bool ComputeFilterPatch(double StartStn, double EndStn, double LeftOffset, double RightOffset,
      SubGridTreeBitmapSubGridBits Mask,
      SubGridTreeBitmapSubGridBits Patch,
      double OriginX, double OriginY,
      double CellSize,
      double Offset);

    void WindLock();
    void UnWindLock();
    bool HasElevationDataForSubGridPatch(double X, double Y);
    bool HasElevationDataForSubGridPatch(uint SubGridX, uint SubgridY);
    bool HasFiltrationDataForSubGridPatch(double X, double Y);
    bool HasFiltrationDataForSubGridPatch(uint SubGridX, uint SubgridY);
    ISubGridTreeBitMask SubgridOverlayIndex();
    void AcquireExclusiveInterlock();
    void ReleaseExclusiveInterlock();
    XYZS[] ComputeProfile(XYZ[] profilePath, double cellSize);
  }
}
