using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.SubGridTrees;

namespace VSS.Velociraptor.DesignProfiling
{
    public abstract class DesignBase
    {
        private int FLockCount = 0;

//      TICSubGridCellPassData_HeightPtr_Temp = TICClientSubGridTreeLeaf_Base<TICCellHeight>.TCellData;
//      TICSubGridCellPassData_HeightPtr = ^TICSubGridCellPassData_HeightPtr_Temp;

//      function GetMemorySizeInKB: Integer; Virtual; Abstract;

        public int LockCount { get { return FLockCount; } }

        public string FileName { get; set; } = "";

        //      property MemorySizeInKB : Integer read GetMemorySizeInKB;

        public long DataModelID { get; set; } = -1;

        public DesignBase()
        {
        }

        public abstract DesignLoadResult LoadFromFile(string FileName);

        public abstract void GetExtents(out double x1, out double y1, out double x2, out double y2);

        public abstract void GetHeightRange(out double z1, out double z2);

        public abstract bool InterpolateHeight(object DesignSearchContext,
                                   ref object Hint,
                                   double X, double Y,
                                   double Offset,
                                   out double Z);

        public abstract bool InterpolateHeights(object DesignSearchContext,
                                   float[,] Patch, // [TICSubGridCellPassData_HeightPtr] The receiver of the patch of elevations
                                   double OriginX, double OriginY,
                                   double CellSize,
                                   double Offset);

        // ComputeFilterPatch computes a bit set representing which cells in the
        // subgrid will be selected within the filter (i.e. the design forms a mask
        // over the production data where the cells 'under' the design are considered
        // to be in the filtered set. The Mask parameter allows the caller to restrict
        // the set of cells in the subgrid to be filtered, allowing additional spatial
        // filtering operations to be applied prior to this filtering step.
        public abstract bool ComputeFilterPatch(object DesignSearchContext,
                                  double StartStn, double EndStn, double LeftOffset, double RightOffset,
                                  SubGridTreeBitmapSubGridBits Mask,
                                  ref SubGridTreeBitmapSubGridBits Patch,
                                  double OriginX, double OriginY,
                                  double CellSize,
                                  DesignDescriptor DesignDescriptor);

        public void WindLock() => System.Threading.Interlocked.Increment(ref FLockCount);

        public void UnWindLock() => System.Threading.Interlocked.Decrement(ref FLockCount);

        public bool IsStale { get; set; }

        public bool Locked => FLockCount > 0;

        public abstract object CreateAccessContext();

        public abstract bool HasElevationDataForSubGridPatch(double X, double Y);

        public abstract bool HasElevationDataForSubGridPatch(uint SubGridX, uint SubgridY);

        public abstract bool HasFiltrationDataForSubGridPatch(double X, double Y);

        public abstract bool HasFiltrationDataForSubGridPatch(uint SubGridX, uint SubgridY);

        public virtual SubGridTreeBitMask SubgridOverlayIndex() => null;

        public void AcquireExclusiveInterlock() => Monitor.Enter(this);
        public void ReleaseExclusiveInterlock() => Monitor.Exit(this);

//function GetDesignLoadResultEnumName(LoadResult: TDesignLoadResult): String;
//begin
//  Result := GetEnumName(TypeInfo(TDesignLoadResult), Integer(LoadResult))
//end;
    }
}
