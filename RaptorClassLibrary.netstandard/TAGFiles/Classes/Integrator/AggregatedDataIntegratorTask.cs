using System;
using VSS.VisionLink.Raptor.Events;
using VSS.VisionLink.Raptor.Machines;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.SubGridTrees.Server;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.Integrator
{
    public class AggregatedDataIntegratorTask
    {
        // TargetSiteModel is the site model that the snippet processor is populating
        // with information from a compactor data file. The snippet processor will have
        // first determined which site model the data should be contributed to after
        // examining which machine produced the compaction data.
        // This instance of a site model is, however, a transient one constructed for
        // for purpose of processing the individual TAG file and is not the
        // persisted instance, or a reference to it.
        public SiteModel TargetSiteModel { get; set; }
        public long TargetSiteModelID { get; set; }

        // TargetMachine is the IC machine that has produced the compaction data
        // that is being used to populate the IC server grid database.
        // This instance of a machine is, however, a transient one constructed for
        // for purpose of processing the individual TAG file and is not the
        // persisted instance, or a reference to it.
        public Machine TargetMachine { get; set; }
        public long TargetMachineID { get; set; }

        public ServerSubGridTree AggregatedCellPasses { get; set; }
        public ProductionEventChanges AggregatedMachineEvents { get; set; }
        public SubGridTreeSubGridExistenceBitMask AggregateModifiedSubgrids { get; set; }

        //      Finalizer : TAggregationTaskFinalizer;

        public int AggregatedCellPassCount { get; set; } 

        public DateTime StartProcessingTime { get; set; } = DateTime.MinValue;

        // OnAdditionToFinalizationQueue destroys the aspects of the integration task
        // that are no longer required once it has been entered into the finalization queue.
        // This reduces memory stress while it is waiting for the embargo period to wind down.
        public void OnAdditionToFinalizationQueue()
        {
            AggregatedCellPasses = null;
            AggregatedMachineEvents = null;
            TargetMachine = null;
            TargetSiteModel = null;
        }

        public void SetAggregateModifiedSubgrids(ref SubGridTreeSubGridExistenceBitMask AAggregateModifiedSubgrids)
        {
            AggregateModifiedSubgrids = AAggregateModifiedSubgrids;

            // Take ownership of the modified subgrids information
            AAggregateModifiedSubgrids = null;
        }
    }
}
