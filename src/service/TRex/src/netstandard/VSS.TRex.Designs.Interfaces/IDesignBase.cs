using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    Guid ProjectUid { get; set; }
    bool IsStale { get; set; }
    bool Locked { get; }
    DesignLoadResult LoadFromFile(string fileName, bool saveIndexFiles = true);
    Task<DesignLoadResult> LoadFromStorage(Guid siteModelUid, string fileName, string localPath,
      bool loadIndices = false);
    bool RemoveFromStorage(Guid siteModelUid, string fileName);

    void GetExtents(out double x1, out double y1, out double x2, out double y2);
    void GetHeightRange(out double z1, out double z2);

    bool InterpolateHeight(ref int hint,
      double x, double y,
      double offset,
      out double z);

    bool InterpolateHeights(float[,] patch, // The receiver of the patch of elevations
      double originX, double originY,
      double cellSize,
      double offset);

    bool ComputeFilterPatch(double startStn, double endStn, double leftOffset, double rightOffset,
      SubGridTreeBitmapSubGridBits mask,
      SubGridTreeBitmapSubGridBits patch,
      double originX, double originY,
      double cellSize,
      double offset);

    void WindLock();
    void UnWindLock();
    bool HasElevationDataForSubGridPatch(double x, double y);
    bool HasElevationDataForSubGridPatch(int subGridX, int subGridY);
    bool HasFiltrationDataForSubGridPatch(double x, double y);
    bool HasFiltrationDataForSubGridPatch(int subGridX, int subGridY);
    ISubGridTreeBitMask SubGridOverlayIndex();
    void AcquireExclusiveInterlock();
    void ReleaseExclusiveInterlock();
    List<XYZS> ComputeProfile(XYZ[] profilePath, double cellSize);
    List<Fence> GetBoundary();
    bool IsLoading { get; set; }
  }
}
