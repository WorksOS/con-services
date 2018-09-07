using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Events.Interfaces;

namespace VSS.TRex.Events
{
    /// <summary>
    /// Implements an event list containing events that detail when a machine started recording production data, and when it stopped
    /// recording production data.
    /// </summary>
    [Serializable]
    public class StartEndProductionEvents : ProductionEvents<ProductionEventType>, IStartEndProductionEvents
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        public StartEndProductionEvents()
        {}

        public StartEndProductionEvents(long machineID, Guid siteModelID,
            ProductionEventType eventListType,
            Action<BinaryWriter, ProductionEventType> serialiseStateOut,
            Func<BinaryReader, ProductionEventType> serialiseStateIn) : base(machineID, siteModelID,
            eventListType, serialiseStateOut, serialiseStateIn)
        {}

        public int IndexOfClosestEventPriorToDate(DateTime eventDate, ProductionEventType eventType)
        {
            if (Events.Count == 0 || (Events.Count > 0 && Events[0].Date > eventDate))
                return -1;

            bool FindResult = Find(eventDate, out int LastIndex);

            // We're looking for the event prior to the requested date.
            // If we didn't find an exact match for requested date, then
            // LastIndex will be the event subsequent to the requested date,
            // so subtract one from LastIndex to give us the event prior
            if (!FindResult && LastIndex > 0)
                LastIndex--;

            while (LastIndex > 0 && Events[LastIndex].State != eventType)
                LastIndex--;

            return LastIndex;
        }

        public int IndexOfClosestEventSubsequentToDate(DateTime eventDate, ProductionEventType eventType)
        {
            if (Events.Count == 0 || (Events.Count > 0 && Events[Events.Count - 1].Date < eventDate))
                return -1;

            Find(eventDate, out int LastIndex);

            while (LastIndex < Events.Count - 1 && Events[LastIndex].State != eventType)
                LastIndex++;

            return LastIndex;
        }

        /// <summary>
        /// Implements search semantics for paired events where it is important to locate bracketing pairs of
        /// start and stop evnets given a date/time
        /// </summary>
        /// <param name="eventDate"></param>
        /// <param name="StartEventDate"></param>
        /// <param name="EndEventDate"></param>
        /// <returns></returns>
        public bool FindStartEventPairAtTime(DateTime eventDate, out DateTime StartEventDate, out DateTime EndEventDate)
        {
            int StartIndex = IndexOfClosestEventPriorToDate(eventDate, ProductionEventType.StartEvent);

            if (StartIndex > -1 && StartIndex < Events.Count - 1)
            {
                StartEventDate = Events[StartIndex].Date;
                EndEventDate = Events[StartIndex + 1].Date;
                return true;
            }
            else
            {
                StartEventDate = default(DateTime);
                EndEventDate = default(DateTime);

                if (StartIndex == Events.Count - 1)
                {
                    Log.LogError(
                        $"FindStartEventPairAtTime located only one event (index:{StartIndex}) at search time {eventDate:6f} {eventDate:o}");
                }
            }

            return false;
        }

        /// <summary>
        /// Implements collation semantics for event lists that do not contain homogenous lists of events. Machine start/stop and 
        /// data recording start/end are examples
        /// </summary>
        public override void Collate(IProductionEventLists container)
        {
            // Note: Machine startup and shutdown start/end event lists are never collated
            if (EventListType == ProductionEventType.MachineStartupShutdown)
                return;

            // First, deal with any nested events
            // ie: Structures of the form <Start><Start><End><Start><End><End>
            // in these instances, all events bracketed by the double <start><end> events should be removed

            int I = 0;
            int NestingLevel = 0;
            while (I < Events.Count)
            {
                bool DecNestingLevel = false;

                if (Events[I].State == ProductionEventType.StartEvent)
                    NestingLevel++;
                else if (Events[I].State == ProductionEventType.EndEvent)
                        DecNestingLevel = true;
                    else
                        Debug.Assert(false, "Unknown event type in list");

                if (NestingLevel > 1)
                {
                    Events.RemoveAt(I);

                    // Decrement location to take into account the removal of the event
                    I--;
                }

                if (DecNestingLevel)
                    NestingLevel--;

                I++;
            }

            // Deal with collation of non-nested events
            I = 0;
            while (I < Events.Count - 1)
            {
                Debug.Assert(Events[I].Date <= Events[I + 1].Date, "Start/end recorded data events are out of order");

                if (Events[I].EquivalentTo(Events[I + 1]) &&
                    Events[I].State == ProductionEventType.StartEvent &&
                    Events[I + 1].State == ProductionEventType.EndEvent)
                {
                    //Don't collate this pair - it's a single epoch tag file
                    I++;
                }
                else if (Events[I].EquivalentTo(Events[I + 1]) && Events[I].State != Events[I + 1].State)
                {
                    Events.RemoveAt(I); // this[I] = null;
                    Events.RemoveAt(I); // this[I + 1] = null;

                    // Decrement location to take into account the removal of the event
                    I--;
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
        public override void PutValueAtDate(Event Event)
        {
            bool ExistingEventFound = Find(Event, out int EventIndex);

            if (ExistingEventFound)
            {
                Debug.Assert(Events[EventIndex].Date == Event.Date,
                    "Have determined two events are the same but that they have different dates!!!");

                // If we find an event with the same date then delete the existing one and replace it with the new one.
                bool CorrectInsertLocationIdentified;
                do
                {
                    CorrectInsertLocationIdentified = true;

                    if (ExistingEventFound)
                    {
                        // Check is start==start or end==end
                        if (Event.State == Events[EventIndex].State)
                        {
                            // If we've got a machine event overriding a machine event or a custom event overriding a custom event
                            // then delete the existing event.
                            if (Events[EventIndex].IsCustomEvent == Event.IsCustomEvent)
                            {
                                if (Event.IsCustomEvent)
                                {
                                    Log.LogDebug($"Deleting custom machine event: {Events[EventIndex]}");
                                    Events.RemoveAt(EventIndex);
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
                            if (Events[EventIndex].State == ProductionEventType.StartEvent &&
                                Event.State == ProductionEventType.EndEvent)
                            {
                                Event.Date = Event.Date.AddMilliseconds(1);
                                CorrectInsertLocationIdentified = false;
                            }
                        }
                    }

                    ExistingEventFound = Find(Event, out EventIndex);
                } while (!CorrectInsertLocationIdentified);
            }

            Events.Insert(EventIndex, Event);
        }

        /// <summary>
        /// Provides a Start-End event pair based comparator that takes into account both date of event
        /// and 'start' and 'end' natures of the events states.
        /// </summary>
        /// <param name="I1"></param>
        /// <param name="I2"></param>
        /// <returns></returns>
        protected override int Compare(Event I1, Event I2)
        {
            const int LessThanValue = -1;
            const int EqualsValue = 0;
            const int GreaterThanValue = 1;

            if (I1.State == I2.State || I1.Date != I2.Date)
                return base.Compare(I1, I2);

            if (I1.Date == I2.Date)
            {
                if (I1.State == ProductionEventType.StartEvent && I2.State == ProductionEventType.EndEvent)
                    return LessThanValue;

                if (I1.State == ProductionEventType.EndEvent && I2.State == ProductionEventType.StartEvent)
                    return GreaterThanValue;

                // Then they must be equal...
                return EqualsValue;
            }

            // Not sure why control would ever get here...
            return base.Compare(I1, I2);
        }  
  }
}
