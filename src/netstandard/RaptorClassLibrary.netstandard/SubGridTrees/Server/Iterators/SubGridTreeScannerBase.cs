using System.Diagnostics;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.SubGridTrees.Server.Iterators
{
    public abstract class SubGridTreeScannerBase
    {
        private bool scanActive;
        public bool Aborted { get; set; }
        private int firstScanLevel = -1;

        protected virtual bool GetProceedWithScannedData() => true;

        public ISubGridTree Grid { get; set; }
        public BoundingIntegerExtent2D Extent;
        public int ScanLevel { get; set; }
        public bool RequestRepresentativeGrids { get; set; }

        public LeafSubgridRequestType RequestType { get; set; } = LeafSubgridRequestType.FullFromServer;

        public bool ScanActive { get { return scanActive; } }
        public int FirstScanLevel { get { return firstScanLevel; } }

        public bool ProceedWithScannedData { get { return GetProceedWithScannedData(); } }

        public virtual SubGridProcessNodeSubGridResult OnProcessNodeSubgrid(ISubGrid subGrid, SubGridTreeBitmapSubGridBits existenceMap) => SubGridProcessNodeSubGridResult.OK;

        public virtual bool OnProcessLeafSubgrid(ISubGrid subGrid) => true;
        public virtual bool OnProcessLeafSubgridAddress(SubGridCellAddress cellAddress)
        {
            Debug.Assert(false, "TICSubGridTreeScannerBase.OnProcessLeafSubgridAddress should be overriden in descendant classes");
            return false;
        }

        public SubGridTreeScannerBase()
        {
        }

        public SubGridTreeScannerBase(ISubGridTree grid,
            BoundingIntegerExtent2D extent,
            int scanLevel,
            bool requestRepresentativeGrids) : this()
        {
            Grid = grid;
            Extent = extent;
            ScanLevel = scanLevel;
            RequestRepresentativeGrids = requestRepresentativeGrids;
        }

        public SubGridTreeScannerBase(ISubGridTree grid,
            BoundingWorldExtent3D extent,
            int scanLevel,
            bool requestRepresentativeGrids) : this(grid, new BoundingIntegerExtent2D(), scanLevel, requestRepresentativeGrids)
        {
            grid.CalculateRegionGridCoverage(extent, out Extent);
        }

        public virtual void OnStartScan(ISubGrid subGrid)
        {
            Debug.Assert(ScanLevel > 0, "Scan level must be > 0 in scanner");

            scanActive = true;
            Aborted = false;
            firstScanLevel = subGrid.Level;
        }

        public virtual void OnFinishedScan() => scanActive = false;

        public void Abort()
        {
            Aborted = true;
            scanActive = false;
        }
    }
}
