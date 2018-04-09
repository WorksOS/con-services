using System.Diagnostics;
using System.Linq;
using VSS.VisionLink.Raptor.Events;
using VSS.VisionLink.Raptor.Events.Interfaces;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.Integrator
{
    public class EventIntegrator
    {
        private EfficientProductionEventChanges Source;
        private EfficientProductionEventChanges Target;
        private bool RemoveFromSource;
        private bool IntegratingIntoPersistentDataModel;

        public EventIntegrator()
        {
        }

        public EventIntegrator(EfficientProductionEventChanges Source,
            EfficientProductionEventChanges Target,
                               bool RemoveFromSource,
                               bool IntegratingIntoPersistentDataModel) : this()
        {
            this.Source = Source;
            this.Target = Target;
            this.RemoveFromSource = RemoveFromSource;
            this.IntegratingIntoPersistentDataModel = IntegratingIntoPersistentDataModel;
        }

        private void IntegrateMachineDesignEventNames()
        {
            /*
                        // TODO: readd when design name events are supported
                        EventDesignName: TEventDesignName;
                        string DesignName;

                        if (IntegratingIntoPersistentDataModel)
                            Source.DesignNameIDStateEvents.AcquireExclusiveWriteInterlock;

                        try
                        {
                            //                  with Source.SiteModel as TICSiteModel do
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
                                    // TODO readd when loggin available
                                    //SIGLogMessage.PublishNoODS(Nil, 'Failed to locate design name in the design change events list', slmcAssert);
                                    return;
                                }
                            }
                        }
                        finally
                        {
                            if IntegratingIntoPersistentDataModel then
                                Source.DesignNameIDStateEvents.ReleaseExclusiveWriteInterlock;
                        }

                        if (IntegratingIntoPersistentDataModel)
                        {
                            Source.MapResets.AcquireExclusiveWriteInterlock;
                        }

                        try
                        {
                            //                with Source.SiteModel as TICSiteModel do
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
                                    // TODO readd when loggin available
                                    //SIGLogMessage.PublishNoODS(Nil, 'Failed to locate design name in the map reset events list', slmcAssert);
                                    return;
                                }
                            }
                        }
                        finally
                        {
                            if (IntegratingIntoPersistentDataModel)
                            {
                                Source.MapResets.ReleaseExclusiveWriteInterlock;
                            }
                        }
            */
        }

        // IntegrateList takes a list of machine events and merges them into the machine event list.
        // Note: This method assumes that the methods being merged into the new list
        // are machine events only, and do not include custom events.
        //        private void IntegrateList(ProductionEventChangeList<ProductionEventChangeBase> Source,
        //                                   ProductionEventChangeList<ProductionEventChangeBase> Target)
        private void IntegrateList(IProductionEventChangeList source,
                                   IProductionEventChangeList target)
        {
            if (source.Count == 0)
            {
                return;
            }

            if (source.Count > 1)
            {
                source.Sort();
            }

            if (IntegratingIntoPersistentDataModel)
            {
                // TODO revisit when sitemodel locking semantics are defined
                //Target.AcquireExclusiveWriteInterlock();
            }

            try
            {
                for (int I = 0; I < source.Count; I++)
                {

                    // If the numbers of events being added here become significant, then
                    // it may be worth using an event merge process similar to the one done
                    // in cell pass integration
                    if (target.PutValueAtDate(source[I]) != source[I])
                    {
                        if (!RemoveFromSource)
                        {
                            // TODO readd when logging available
                            //SIGLogMessage.PublishNoODS(null, 'AddAtCorrectTimePoint destroyed an event when RemoveFromSource was false', slmcAssert);
                            return;
                        }

                        source[I] = null;
                    }
                }

                target.Collate();
            }
            finally
            {
                if (IntegratingIntoPersistentDataModel)
                {
                    // TODO revisit when sitemodel locking semantics are defined
                    // Target.ReleaseExclusiveWriteInterlock;
                }
            }
        }


        //        private void PerformListIntegration(ProductionEventChangeList<ProductionEventChangeBase> Source,
        //                                          ProductionEventChangeList<ProductionEventChangeBase> Target)
        private void PerformListIntegration(IProductionEventChangeList source,
                                            IProductionEventChangeList target)
        {
            IntegrateList(source, target);
            
            if (RemoveFromSource)
            {
                source.Clear();
            }
        }

        public void IntegrateMachineEvents(EfficientProductionEventChanges source,
                                           EfficientProductionEventChanges target,
                                           bool removeFromSource,
                                           bool integratingIntoPersistentDataModel)
        {
            Source = source;
            Target = target;
            RemoveFromSource = removeFromSource;
            IntegratingIntoPersistentDataModel = integratingIntoPersistentDataModel;

            IntegrateMachineEvents();
        }

        public void IntegrateMachineEvents()
        {
            IProductionEventChangeList SourceList;
            IProductionEventChangeList TargetList;
            IProductionEventChangeList SourceStartEndRecordedDataList;

            //            ProductionEventChangeList<ProductionEventChangeBase> SourceList;
            //            ProductionEventChangeList<ProductionEventChangeBase> TargetList;
            //            ProductionEventChangeList<ProductionEventChangeBase<ProductionEventType>, ProductionEventType> SourceStartEndRecordedDataList;

            IntegrateMachineDesignEventNames();

            SourceStartEndRecordedDataList = Source.StartEndRecordedDataEvents; // EventStartEndRecordedData;

            // Always integrate the machine recorded data start/stop events first, as collation
            // of the other events depends on collation of these events
            PerformListIntegration(SourceStartEndRecordedDataList, Target.StartEndRecordedDataEvents); // EventStartEndRecordedData);

            var sourceEventLists = Source.GetEventLists();
            var targetEventLists = Target.GetEventLists();

            // Integrate all remaining event lists and collate them wrt the machine start/stop recording events
            for (int I = 0; I < sourceEventLists.Length; I++)
            {
                SourceList = sourceEventLists[I];

                if (SourceList != null)
                {
                    if (SourceList != SourceStartEndRecordedDataList && SourceList.Count > 0)
                    {
                        // The source event list is always an in-memory list. The target event list
                        // will be an in-memory list unless IntegratingIntoPersistentDataModel is true,
                        // in which case the source events are being integrated into the data model events
                        // list present in the persistent store. In this instance the target event list
                        // must be read from the persistent store (or be in a cached location) prior
                        // to the integration of the source events

                        TargetList = targetEventLists[I];
                        if (TargetList != null)
                        {
                            // TODO revisit if target lists become lazy loaded
                            //TargetList.EnsureEventListLoaded();
                            Debug.Assert(false, "Target list not loaded");
                        }

                        if (IntegratingIntoPersistentDataModel && TargetList == null)
                        {
                            // Read the events list from the persistent store.
                            // OR... Just read the whole lot to start with
                            // OR... Don't bother as the caller will have sorted it all out.

                            // TODO add when logging available
                            //SIGLogMessage.PublishNoODS(Nil, Format('Event list %d not available in IntegrateMachineEvents', [I]), slmcError);
                        }

                        if (TargetList != null)
                        {
                            PerformListIntegration(SourceList, TargetList);
                        }
                        else
                        {
                            // TODO add when logging available
                            // SIGLogMessage.PublishNoODS(Nil, Format('Event list %d not available in IntegrateMachineEvents', [I]), slmcError);
                        }
                    }
                }
            }
        }
    }
}
