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
        public uint ScanLevel { get; set; }
        public bool RequestRepresentativeGrids { get; set; }

        public LeafSubgridRequestType RequestType { get; set; } = LeafSubgridRequestType.FullFromServer;

        public bool ScanActive => scanActive;
        public int FirstScanLevel => firstScanLevel; 

        public bool ProceedWithScannedData => GetProceedWithScannedData();

        public virtual SubGridProcessNodeSubGridResult OnProcessNodeSubgrid(ISubGrid subGrid, SubGridTreeBitmapSubGridBits existenceMap) => SubGridProcessNodeSubGridResult.OK;

        public abstract bool OnProcessLeafSubgrid(ISubGrid subGrid);
        public abstract bool OnProcessLeafSubgridAddress(SubGridCellAddress cellAddress);

        public SubGridTreeScannerBase()
        {
        }

        public SubGridTreeScannerBase(ISubGridTree grid,
            BoundingIntegerExtent2D extent,
            uint scanLevel,
            bool requestRepresentativeGrids) : this()
        {
            Grid = grid;
            Extent = extent;
            ScanLevel = scanLevel;
            RequestRepresentativeGrids = requestRepresentativeGrids;
        }

        public SubGridTreeScannerBase(ISubGridTree grid,
            BoundingWorldExtent3D extent,
            uint scanLevel,
            bool requestRepresentativeGrids) : this(grid, new BoundingIntegerExtent2D(), scanLevel, requestRepresentativeGrids)
        {
            grid.CalculateRegionGridCoverage(extent, out Extent);
        }

        public virtual void OnStartScan(ISubGrid subGrid)
        {
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
