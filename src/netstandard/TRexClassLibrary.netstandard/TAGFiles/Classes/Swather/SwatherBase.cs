using System;
using VSS.TRex.Cells;
using VSS.TRex.Events;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.Swather
{
    /// <summary>
    /// SwatherBase provides a base class for the process of computing swathing
    /// information.Swathing is the general term for analysing a machine's activities
    /// and contributing relevant records (cell passes, events etc) to the production
    /// server database.This class implements much of the infractruture relevant to
    ///swathing, but does not define the semantics of how the swathing is to be performed    
    /// </summary>
    public class SwatherBase
    {
        // SiteModel is the site model that the read data is being contributed to
        protected SiteModel SiteModel { get; set; }

        // Grid is the grid into which the cell passes are to be aggregated into prior
        // to final insertion into the site model proper
        protected ServerSubGridTree Grid { get; set; }

        // InternalSiteModelMachineIndex is a reference to the compaction machine that has collected the data being processed.
        //public long MachineID { get; set; }
        public short InternalSiteModelMachineIndex { get; set; }

        //      FMachineConnectionLevel : MachineLevelEnum;

        //MachineTargetValueChanges is a reference to an object that records all the
        // machine state events of interest that we encounter while processing the file
        protected ProductionEventLists MachineTargetValueChanges { get; set; }

        protected TAGProcessorBase Processor { get; set; }

        protected IServerLeafSubGrid LastInMemoryLeafSubGrid { get; set; }

        protected Fence InterpolationFence { get; set; }

        public void CommitCellPassToModel(uint cellX, uint cellY,
                                          double gridX, double gridY,
                                          CellPass processedCellPass)
        {
            // Arrange the subgrid that will house this cell pass.
            // This needs to happen if, and only if, we will actually add a cell
            // pass to the subgrid. The reason for this restriction is that we may
            // otherwise end up creating a new subgrid that never has any cell passes
            // added to it.

            // The grid we are populating is a in-memory grid (ie: not the actual subgrid database
            // for this data model). Changes will not need to be synchronised with the main
            // server interlock (ICServerModule.Server.AquireLock) and we may interact
            // directly with the subgrid tree being populated

            IServerLeafSubGrid SubGrid = Grid.ConstructPathToCell(cellX, cellY, SubGridPathConstructionType.CreateLeaf) as IServerLeafSubGrid;
            LastInMemoryLeafSubGrid = SubGrid;

            SubGrid.AllocateLeafFullPassStacks();

            // If the node is brand new (ie: it does not have any cell passes committed to it yet)
            // then create and select the default segment

            if (SubGrid.Directory.SegmentDirectory.Count == 0)
            {
                SubGrid.Cells.SelectSegment(DateTime.MinValue);
            }

            SubGrid.Dirty = true;

            // Find the location of the cell within the subgrid.
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

        public virtual bool PerformSwathing(SimpleTriangle HeightInterpolator1,
                                            SimpleTriangle HeightInterpolator2,
                                            SimpleTriangle TimeInterpolator1,
                                            SimpleTriangle TimeInterpolator2,
                                            bool HalfPas,
                                            PassType passType,
                                            MachineSide machineSide)
        {
            throw new NotImplementedException();
        }

        public bool BaseProductionDataSupportedByMachine => true; // Need to wire this into subscriptions
        public bool CompactionDataSupportedByMachine => true; // Need to wire this into subscriptions

        public SwatherBase(TAGProcessorBase processor,
                           ProductionEventLists machineTargetValueChanges,
                           SiteModel siteModel,
                           ServerSubGridTree grid,
            //long machineID,
            short internalSiteModelMachineIndex,
        //                         AMachineConnectionLevel : MachineLevelEnum;
                           Fence interpolationFence)
        {
            Processor = processor;
            MachineTargetValueChanges = machineTargetValueChanges;
            SiteModel = siteModel;
            Grid = grid;
            InterpolationFence = interpolationFence;
            //MachineID = machineID;
            
        }
    }
}
