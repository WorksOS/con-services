using System;
using System.Collections;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.TAGFiles.Classes.Integrator
{
    /// <summary>
    /// Provides business logic driving integration of event lists derived from TAG file processing
    /// into event lists in a site model into the event lists of another site model
    /// </summary>
    public class EventIntegrator
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        private IProductionEventLists SourceLists;
        private IProductionEventLists TargetLists;
        private bool IntegratingIntoPersistentDataModel;
        private ISiteModel TargetSiteModel;

        public EventIntegrator()
        {
        }

        public EventIntegrator(IProductionEventLists sourceLists,
                               IProductionEventLists targetLists,
                               bool integratingIntoPersistentDataModel) : this()
        {
            SourceLists = sourceLists;
            TargetLists = targetLists;
            IntegratingIntoPersistentDataModel = integratingIntoPersistentDataModel;
        }

        private void IntegrateMachineDesignEventNames()
        {
          // ensure that 1 copy of the machineDesignName exists in the targetSiteModels List,
          //    and we reflect THAT Id in the source list
          for (int I = 0; I < SourceLists.MachineDesignNameIDStateEvents.Count(); I++)
          {
            int machineDesignId;
            SourceLists.MachineDesignNameIDStateEvents.GetStateAtIndex(I, out DateTime dateTime, out machineDesignId);
            if (machineDesignId > -1)
            {
              string machineDesignName = TargetSiteModel.SiteModelMachineDesigns[machineDesignId];

              if (machineDesignName != null)
              {
                SourceLists.MachineDesignNameIDStateEvents.PutValueAtDate(dateTime, machineDesignId);
              }
            }
            else
            {
              Log.LogError($"Failed to locate machine design name at dateTime: {dateTime} in the design change events list");
            }
          }
        }

        // IntegrateList takes a list of machine events and merges them into the machine event list.
        // Note: This method assumes that the methods being merged into the new list
        // are machine events only, and do not include custom events.
        private void IntegrateList(IProductionEvents source, IProductionEvents target)
        {
            if (source.Count() == 0)
                return;

            if (source.Count() > 1)
                source.Sort();

            target.CopyEventsFrom(source);
            target.Collate(TargetLists);
        }

        private void PerformListIntegration(IProductionEvents source, IProductionEvents target)
        {
            IntegrateList(source, target);
        }

        public void IntegrateMachineEvents(IProductionEventLists sourceLists,
                                           IProductionEventLists targetLists,
                                           bool integratingIntoPersistentDataModel,
                                           ISiteModel targetSiteModel)
        {
            SourceLists = sourceLists;
            TargetLists = targetLists;
            IntegratingIntoPersistentDataModel = integratingIntoPersistentDataModel;
            TargetSiteModel = targetSiteModel;
            IntegrateMachineEvents();
        }

        /// <summary>
        /// Integrate together all the events lists for a machine between the source and target lists of machine events
        /// </summary>
        public void IntegrateMachineEvents()
        {
            IntegrateMachineDesignEventNames();

            IProductionEvents SourceStartEndRecordedDataList = SourceLists.StartEndRecordedDataEvents;

            // Always integrate the machine recorded data start/stop events first, as collation
            // of the other events depends on collation of these events
            PerformListIntegration(SourceStartEndRecordedDataList, TargetLists.StartEndRecordedDataEvents); 

            var sourceEventLists = SourceLists.GetEventLists();
            var targetEventLists = TargetLists.GetEventLists();

            // Integrate all remaining event lists and collate them wrt the machine start/stop recording events
            foreach (var evt in Enum.GetValues(typeof(ProductionEventType)))
            {
                IProductionEvents SourceList = sourceEventLists[(int)evt];

                if (SourceList != null && SourceList != SourceStartEndRecordedDataList && SourceList.Count() > 0)
                {
                    // The source event list is always an in-memory list. The target event list
                    // will be an in-memory list unless IntegratingIntoPersistentDataModel is true,
                    // in which case the source events are being integrated into the data model events
                    // list present in the persistent store.

                    IProductionEvents TargetList = targetEventLists[(int)evt] ?? TargetLists.GetEventList(SourceList.EventListType);

                    if (IntegratingIntoPersistentDataModel && TargetList == null)
                        Log.LogError($"Event list {evt} not available in IntegrateMachineEvents");

                    if (TargetList != null)
                        PerformListIntegration(SourceList, TargetList);
                }
            }
        }
    }
}
