using System;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.Swather
{
    /// <summary>
    /// SwatherBase provides a base class for the process of computing swathing
    /// information.Swathing is the general term for analyzing a machine's activities
    /// and contributing relevant records (cell passes, events etc) to the production
    /// server database.This class implements much of the infrastructure relevant to
    ///swathing, but does not define the semantics of how the swathing is to be performed    
    /// </summary>
    public abstract class SwatherBase
    {
        // SiteModel is the site model that the read data is being contributed to
        protected ISiteModel SiteModel;

        // Grid is the grid into which the cell passes are to be aggregated into prior
        // to final insertion into the site model proper
        protected IServerSubGridTree Grid; 

        //MachineTargetValueChanges is a reference to an object that records all the
        // machine state events of interest that we encounter while processing the file
        protected IProductionEventLists MachineTargetValueChanges;

        protected TAGProcessorBase Processor; 

        public Fence InterpolationFence;

        public void CommitCellPassToModel(int cellX, int cellY,
                                          double gridX, double gridY,
                                          CellPass processedCellPass)
        {
            // Arrange the sub grid that will house this cell pass.
            // This needs to happen if, and only if, we will actually add a cell
            // pass to the sub grid. The reason for this restriction is that we may
            // otherwise end up creating a new sub grid that never has any cell passes
            // added to it.

            // The grid we are populating is a in-memory grid (ie: not the actual sub grid database
            // for this data model). Changes will not need to be synchronized with the main
            // server interlock (ICServerModule.Server.AcquireLock) and we may interact
            // directly with the sub grid tree being populated

            var SubGrid = Grid.ConstructPathToCell(cellX, cellY, SubGridPathConstructionType.CreateLeaf) as IServerLeafSubGrid;

            SubGrid.AllocateLeafFullPassStacks();

            // If the node is brand new (ie: it does not have any cell passes committed to it yet)
            // then create and select the default segment

            if (SubGrid.Directory.SegmentDirectory.Count == 0)
            {
                SubGrid.Cells.SelectSegment(Consts.MIN_DATETIME_AS_UTC);
            }

            SubGrid.SetDirty();

            // Find the location of the cell within the sub grid.
            SubGrid.GetSubGridCellIndex(cellX, cellY, out byte SubGridCellX, out byte SubGridCellY);

            // Now add the pass to the cell information
            SubGrid.AddPass(SubGridCellX, SubGridCellY, processedCellPass);

            // Include the new point into the extents being maintained for
            // any proofing run being processed.
            Processor.ProofingRunExtent.Include(gridX, gridY, processedCellPass.Height);

            // Include the new point into the extents being maintained for
            // any design being processed.
            Processor.DesignExtent.Include(gridX, gridY, processedCellPass.Height);

            SiteModel.SiteModelExtent.Include(gridX, gridY, processedCellPass.Height);

            Processor.ProcessedCellPassesCount++;
        }

        public abstract bool PerformSwathing(SimpleTriangle HeightInterpolator1,
          SimpleTriangle HeightInterpolator2,
          SimpleTriangle TimeInterpolator1,
          SimpleTriangle TimeInterpolator2,
          bool HalfPas,
          PassType passType,
          MachineSide machineSide);


        public bool BaseProductionDataSupportedByMachine => true; // Todo: Need to wire this into subscriptions
        public bool CompactionDataSupportedByMachine => true; // Todo: Need to wire this into subscriptions

        public SwatherBase(TAGProcessorBase processor,
                           IProductionEventLists machineTargetValueChanges,
                           ISiteModel siteModel,
                           IServerSubGridTree grid,
                           Fence interpolationFence)
        {
            Processor = processor;
            MachineTargetValueChanges = machineTargetValueChanges;
            SiteModel = siteModel;
            Grid = grid;
            InterpolationFence = interpolationFence;           
        }
    }
}
