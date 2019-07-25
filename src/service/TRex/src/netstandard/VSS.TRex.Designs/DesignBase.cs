﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs
{
  public abstract class DesignBase : IDesignBase
  {
    private int FLockCount;

    //      function GetMemorySizeInKB: Integer; Virtual; Abstract;

    public int LockCount => FLockCount; 

    public string FileName { get; set; } = "";

    //      property MemorySizeInKB : Integer read GetMemorySizeInKB;

    public Guid DataModelID { get; set; }

    protected DesignBase()
    {
    }

    /// <summary>
    /// Indicates if the design exists but is in the process of being loaded. This is used to control
    /// multiple concurrent requests to a design that is not yet loaded.
    /// </summary>
    public bool IsLoading { get; set; } = false;

    public abstract DesignLoadResult LoadFromFile(string fileName, bool saveIndexFiles = true);

    public abstract Task<DesignLoadResult> LoadFromStorage(Guid siteModelUid, string fileName, string localPath,
      bool loadIndices = false);

    public abstract void GetExtents(out double x1, out double y1, out double x2, out double y2);

    public abstract void GetHeightRange(out double z1, out double z2);

    public abstract bool InterpolateHeight(ref int Hint,
      double X, double Y,
      double Offset,
      out double Z);

    public abstract bool InterpolateHeights(float[,] Patch, // The receiver of the patch of elevations
      double OriginX, double OriginY,
      double CellSize,
      double Offset);

    // ComputeFilterPatch computes a bit set representing which cells in the
    // sub grid will be selected within the filter (i.e. the design forms a mask
    // over the production data where the cells 'under' the design are considered
    // to be in the filtered set. The Mask parameter allows the caller to restrict
    // the set of cells in the sub grid to be filtered, allowing additional spatial
    // filtering operations to be applied prior to this filtering step.
    public abstract bool ComputeFilterPatch(double StartStn, double EndStn, double LeftOffset, double RightOffset,
      SubGridTreeBitmapSubGridBits Mask,
      SubGridTreeBitmapSubGridBits Patch,
      double OriginX, double OriginY,
      double CellSize,
      double Offset);

    public void WindLock() => Interlocked.Increment(ref FLockCount);

    public void UnWindLock() => Interlocked.Decrement(ref FLockCount);

    public bool IsStale { get; set; }

    public bool Locked => FLockCount > 0;

    public abstract bool HasElevationDataForSubGridPatch(double X, double Y);

    public abstract bool HasElevationDataForSubGridPatch(int SubGridX, int SubGridY);

    public abstract bool HasFiltrationDataForSubGridPatch(double X, double Y);

    public abstract bool HasFiltrationDataForSubGridPatch(int SubGridX, int SubGridY);

    public virtual ISubGridTreeBitMask SubGridOverlayIndex() => null;

    public void AcquireExclusiveInterlock() => Monitor.Enter(this);
    public void ReleaseExclusiveInterlock() => Monitor.Exit(this);

    public abstract List<XYZS> ComputeProfile(XYZ[] profilePath, double cellSize);

    public abstract List<Fence> GetBoundary();
  }
}
