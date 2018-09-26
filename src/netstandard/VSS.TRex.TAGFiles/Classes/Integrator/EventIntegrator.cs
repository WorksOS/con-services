using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;

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
            /*
            // TODO: readd when design name events are supported
            EventDesignName: TEventDesignName;
            string DesignName;

                //  with Source.SiteModel as TICSiteModel do
                for (int I = 0; I < Source.DesignNameIDStateEvents.Count; I++)
                {
                    if (SiteModelDesignNames.GetDesignName(Source.DesignNameIDStateEvents[I].State, DesignName))
                    {
                        EventDesignName = Target.SiteModel.SiteModelDesignNames.AddDesignName(DesignName);
                        if (EventDesignName != null)
                        {
                            Source.DesignNameIDStateEvents[I].State = EventDesignName.ID;
                        }
                    }
                    else
                    {
                        Log.LogError("Failed to locate design name in the design change events list");
                        return;
                    }
                }

                // with Source.SiteModel as TICSiteModel do
                for (int I = 0; I < Source.MapResets.Count; I++)
                {
                    if (SiteModelDesignNames.GetDesignName((Source.MapResets[I] as TICEventMapReset).DesignNameID, DesignName))
                    {
                        EventDesignName = Target.SiteModel.SiteModelDesignNames.AddDesignName(DesignName);
                        if (EventDesignName != null)
                        {
                            (Source.MapResets[I] as TICEventMapReset).DesignNameID = EventDesignName.ID;
                        }
                    }
                    else
                    {
                        Log.LogError("Failed to locate design name in the map reset events list");
                        return;
                    }
                }
            */
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
                                           bool integratingIntoPersistentDataModel)
        {
            SourceLists = sourceLists;
            TargetLists = targetLists;
            IntegratingIntoPersistentDataModel = integratingIntoPersistentDataModel;

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
