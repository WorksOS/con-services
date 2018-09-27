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
        /// start and stop events given a date/time
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

            StartEventDate = default;
            EndEventDate = default;

            if (StartIndex == Events.Count - 1)
            {
                Log.LogError(
                    $"FindStartEventPairAtTime located only one event (index:{StartIndex}) at search time {eventDate:6f} {eventDate:o}");
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

          // Please leave...
          //{
          //  Log.LogInformation($"In: Before sort: Collating start/end events: event count = {Events.Count}");
          //  int count = 0;
          //  foreach (var evt in Events)
          //    Log.LogInformation($"{count++}: Date {evt.Date} -> {evt.State}");
          //}

            Sort();

          // Please leave...
          //{
          //  Log.LogInformation($"In: After sort: Collating start/end events: event count = {Events.Count}");
          //  int count = 0;
          //  foreach (var evt in Events)
          //   Log.LogInformation($"{count++}: Date {evt.Date} -> {evt.State}");      
          //}

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
                    // Log.LogInformation($"Removing due to nesting: {I}: Date {Events[I].Date} -> {Events[I].State}");
                    Events.RemoveAt(I);

                    // Decrement location to take into account the removal of the event
                    I--;
                }

                if (DecNestingLevel)
                    NestingLevel--;

                I++;
            }

            if (Events.Count == 2)
            {
                // Single start/stop events (from processing a single TAG file) never need collation
                return;
            }

            // Deal with collation of non-nested events. This means removing End/Start pairs occurring 
            // at the same point in time
            I = 0;
            while (I < Events.Count - 1)
            {
                // Introduce one second worth of slop into the start/stop event date comparisons to deal with 
                // jitter between the last time epoch in one TAG file and the first time epoch in the 
                // following TAG file. The second TAG file may even have a first epoch time before the last
                // epoch time of the previous TAG file.
                // Note: This means the list of events may be validly strictly out of date order.

                TimeSpan eventTimeDelta = Events[I].Date - Events[I + 1].Date; 
                bool eventTimesAreEqual = Math.Abs(eventTimeDelta.Ticks) < TimeSpan.TicksPerSecond;

                // Log.LogInformation($"Comparing at {I}->{I + 1}: {Events[I].Date}[{Events[I].Date.ToBinary()}] -> {Events[I + 1].Date}[{Events[I + 1].Date.ToBinary()}] (close enough?:{eventTimesAreEqual}, {Events[I].State} -> {Events[I + 1].State}");

                if (eventTimesAreEqual &&
                    Events[I].State == ProductionEventType.EndEvent &&
                    Events[I + 1].State == ProductionEventType.StartEvent)
                {
                    // Log.LogInformation($"Removing due end/start co-location: {I}: Date {Events[I].Date}");

                    // Remove the End/Start combo and reset the location to take into account the removal of the event
                    Events.RemoveAt(I);
                    Events.RemoveAt(I);
                    I--;
                }

                I++;
            }

          // Please leave...
          //{
          //  Log.LogInformation($"Out: Collating start/end events, event count = {Events.Count}");
          //  int count = 0;
          //  foreach (var evt in Events)
          //    Log.LogInformation($"{count++}: Date {evt.Date} -> {evt.State}");
          //}
        }

        /// <summary>
        /// Provides business logic for adding start/end production event types where events define contiguous ranges rather
        /// then singular state changes at points in time
        /// </summary>
        /// <param name="Event"></param>
        /// <returns></returns>
        public override void PutValueAtDate(Event Event)
        {
            // Note: The event being added may be a duplicate of an existing event. These are added to
            // permit correct collation of the events across nested spans created by reprocessing TAG file data
            Find(Event, out int EventIndex);
            Events.Insert(EventIndex, Event);
            EventsChanged = true;
        }

        /// <summary>
        /// Provides a Start-End event pair based comparator that takes into account both date of event
        /// and 'start' and 'end' natures of the events states.
        /// Note: If a start and an end event are at the same date, the end event is said to occur before the
        /// start event occurs so as not to get zero-length start-end event periods.
        /// </summary>
        /// <param name="I1"></param>
        /// <param name="I2"></param>
        /// <returns></returns>
        protected override int Compare(Event I1, Event I2)
        {
            const int LessThanValue = -1;
            const int EqualsValue = 0;
            const int GreaterThanValue = 1;

            // Introduce one second worth of slop into the start/stop event date comparisons to deal with 
            // jitter between the last time epoch in one TAG file and the first time epoch in the 
            // following TAG file. The second TAG file may even have a first epoch time before the last
            // epoch time of the previous TAG file.
            // Note: This means the list of events may be validly strictly out of date order.

            TimeSpan eventTimeDelta = I1.Date - I2.Date;
            bool eventTimesAreEqual = Math.Abs(eventTimeDelta.Ticks) <= TimeSpan.TicksPerSecond;

            if (!eventTimesAreEqual)
              return base.Compare(I1, I2);

            if (I1.State == ProductionEventType.StartEvent && I2.State == ProductionEventType.EndEvent)
              return GreaterThanValue;

            if (I1.State == ProductionEventType.EndEvent && I2.State == ProductionEventType.StartEvent)
              return LessThanValue;

            // Then they must be equal...
            return EqualsValue;
        }  
    }
}
