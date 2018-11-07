using System;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.TAGFiles.Classes.Integrator
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
        public ISiteModel TargetSiteModel { get; set; }
        public Guid TargetSiteModelID { get; set; }

        // TargetMachine is the IC machine that has produced the compaction data
        // that is being used to populate the IC server grid database.
        // This instance of a machine is, however, a transient one constructed for
        // for purpose of processing the individual TAG file and is not the
        // persisted instance, or a reference to it.
        public IMachine TargetMachine { get; set; }
        public Guid TargetMachineID { get; set; }

        public IServerSubGridTree AggregatedCellPasses { get; set; }
        public IProductionEventLists AggregatedMachineEvents { get; set; }

        public int AggregatedCellPassCount { get; set; } 

        public DateTime StartProcessingTime { get; set; } = DateTime.MinValue;
    }
}
