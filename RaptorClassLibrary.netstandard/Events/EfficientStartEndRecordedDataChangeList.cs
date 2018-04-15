using System;
using System.Diagnostics;
using System.Reflection;
using log4net;

namespace VSS.VisionLink.Raptor.Events
{
    /// <summary>
    /// Implements an event list containing events that detail when a machine started recording production data, and when it stopped
    /// recording production data.
    /// </summary>
    [Serializable]
    public class EfficientStartEndRecordedDataChangeList : EfficientSpecificProductionEventChangeList<EfficientProductionEventChangeBase<ProductionEventType>>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public EfficientStartEndRecordedDataChangeList()
        {
        }

        public EfficientStartEndRecordedDataChangeList(EfficientProductionEventChanges container,
                                              long machineID, long siteModelID,
                                              ProductionEventType eventListType) : base(container, machineID, siteModelID, eventListType)
        {
        }

        //    function GetEventFilename: TFilename; override;
        //    function Find(const Value : TICProductionEventChangeBase;
        //    var Index: Integer): Boolean; overload; override;
        //          protected Compare(Item1, Item2 : Pointer) : Integer; overload; override;

        public int IndexOfClosestEventPriorToDate(DateTime eventDate, ProductionEventType eventType)
        {
            if ((Count == 0) || ((Count > 0) && (this[0].Date > eventDate)))
                return -1;

            bool FindResult = Find(eventDate, out int LastIndex);

            // We're looking for the event prior to the requested date.
            // If we didn't find an exact match for requested date, then
            // LastIndex will be the event subsequent to the requested date,
            // so subtract one from LastIndex to give us the event prior
            if (!FindResult && LastIndex > 0)
                LastIndex--;

            while (LastIndex > 0 && this[LastIndex].State != eventType)
                LastIndex--;

            return LastIndex;
        }

        public int IndexOfClosestEventSubsequentToDate(DateTime eventDate, ProductionEventType eventType)
        {
            if ((Count == 0) || ((Count > 0) && (this[Count - 1].Date < eventDate)))
                return -1;

            Find(eventDate, out int LastIndex);

            while (LastIndex < Count - 1 && this[LastIndex].State != eventType)
                LastIndex++;

            return LastIndex;
        }

        /// <summary>
        /// Implements search semantics for paired events where it is important to locate bracketing pairs of
        /// start and stop evnets given a date/time
        /// </summary>
        /// <param name="eventDate"></param>
        /// <param name="StartEvent"></param>
        /// <param name="EndEvent"></param>
        /// <returns></returns>
        public bool FindStartEventPairAtTime(DateTime eventDate, out EfficientProductionEventChangeBase<ProductionEventType> StartEvent, out EfficientProductionEventChangeBase<ProductionEventType> EndEvent)
        {
            int StartIndex = IndexOfClosestEventPriorToDate(eventDate, ProductionEventType.StartRecordedData);

            if (StartIndex > -1 && StartIndex < Count - 1)
            {
                StartEvent = this[StartIndex];
                EndEvent = this[StartIndex + 1];
                return true;
            }
            else
            {
                StartEvent = default(EfficientProductionEventChangeBase<ProductionEventType>);
                EndEvent = default(EfficientProductionEventChangeBase<ProductionEventType>);

                if (StartIndex == Count - 1)
                {
                    Log.Error($"FindStartEventPairAtTime located only one event (index:{StartIndex}) at search time {eventDate:6f} {eventDate:o}");
                }
            }

            return false;
        }

        /// <summary>
        /// Implements collation semantics for event lists that do not contain homogenous lists of events. Machine start/stop and 
        /// data recording start/end are examples
        /// </summary>
        public override void Collate()
        {
            // First, deal with any nested events
            // ie: Structures of the form <Start><Start><End><Start<End><End>
            // in these instances, all events bracketed by the double <start><end> events should be removed

            int I = 0;
            int NestingLevel = 0;
            while (I < Count)
            {
                bool DecNestingLevel = false;

                if (this[I].State == ProductionEventType.StartRecordedData)
                    NestingLevel++;
                else
                {
                    if (this[I].State == ProductionEventType.EndRecordedData)
                        DecNestingLevel = true;
                    else
                        Debug.Assert(false, "Unknown event type in list");

                    if (NestingLevel > 1)
                        RemoveAt(I);

                    if (DecNestingLevel)
                        NestingLevel--;
                }

                I++;
            }

            // Deal with collation of non-nested events
            I = 0;
            while (I < Count - 1)
            {
                Debug.Assert(this[I].Date <= this[I + 1].Date, "Start/end recorded data events are out of order");

                if (this[I].EquivalentTo(this[I + 1]) &&
                    this[I].State == ProductionEventType.StartRecordedData &&
                    this[I + 1].State == ProductionEventType.EndRecordedData)
                {
                    //Don't collate this pair - it's a single epoch tag file
                }
                else if (this[I].EquivalentTo(this[I + 1]) && this[I].State != this[I + 1].State)
                {
                    RemoveAt(I); // this[I] = null;
                    RemoveAt(I); // this[I + 1] = null;
                }

                I++;
            }
        }

        /// <summary>
        /// Provides business logic for adding start/end production event types where events define contiguous ranges rather
        /// then singular state changes at points in time
        /// </summary>
        /// <param name="Event"></param>
        /// <returns></returns>
        public override EfficientProductionEventChangeBase<ProductionEventType> PutValueAtDate(EfficientProductionEventChangeBase<ProductionEventType> Event)
        {
            bool ExistingEventFound = Find(Event, out int EventIndex);

            if (ExistingEventFound)
            {
                Debug.Assert(this[EventIndex].Date == Event.Date,
                "Have determined two events are the same but that they have different dates!!!");

                // If we find an event with the same date then delete the existing one and replace it with the new one.
                bool CorrectInsertLocationIdentified;
                do
                {
                    CorrectInsertLocationIdentified = true;

                    if (ExistingEventFound)
                    {
                        // Check is start==start or end==end
                        if (Event.State == this[EventIndex].State)
                        {
                            // If we've got a machine event overriding a machine event or a custom event overriding a custom event
                            // then delete the existing event.
                            if (this[EventIndex].IsCustomEvent == Event.IsCustomEvent)
                            {
                                if (Event.IsCustomEvent)
                                {
                                    Log.Debug($"Deleting custom machine event: {this[EventIndex]}");
                                    RemoveAt(EventIndex);
                                }
                            }
                            else
                            {
                                if (Event.IsCustomEvent)
                                {
                                    // If we've got a custom event with the same date as a machine event
                                    // then "bump" the custom event's date by a milli-second to ensure it's
                                    // after the machine event.

                                    Event.Date = Event.Date.AddMilliseconds(1);
                                    CorrectInsertLocationIdentified = false;
                                }
                            }
                        }
                        else
                        {
                            // Ensure 'end' events are placed after 'start' events at the same time
                            if (this[EventIndex].State == ProductionEventType.StartRecordedData &&
                                Event.State == ProductionEventType.EndRecordedData)
                            {
                                Event.Date = Event.Date.AddMilliseconds(1);
                                CorrectInsertLocationIdentified = false;
                            }
                        }
                    }

                    ExistingEventFound = Find(Event, out EventIndex);
                }
                while (!CorrectInsertLocationIdentified);
            }

            Insert(EventIndex, Event);

            return Event;
        }
    }
}
