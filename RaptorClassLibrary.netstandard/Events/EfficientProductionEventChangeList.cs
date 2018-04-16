using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using log4net;
using VSS.VisionLink.Raptor.Events.Interfaces;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.Events
{
    /// <summary>
    /// ProductionEvents implements a generic event list without using class instances for each event
    /// </summary>
    /// <typeparam name="V"></typeparam>
    [Serializable]
    public class ProductionEvents<V> : IProductionEvents
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Serializable]
        public struct Event
        {
            /// <summary>
            /// Flag constant indicating this event is a customer event
            /// </summary>
            private const int kCustomEventBitFlag = 0;

            /// <summary>
            /// Defines whether this event is a custom event, ie: an event that was not recorded by a machine but which has been 
            /// inserted as a part of a nother process such as to override values recorded by the machine that were incorrect 
            /// (eg: design or material lift number)
            /// </summary>
            public bool IsCustomEvent
            {
                get => BitFlagHelper.IsBitOn(Flags, kCustomEventBitFlag);
                set => BitFlagHelper.SetBit(ref Flags, kCustomEventBitFlag, value);
            }

            /// <summary>
            /// The date/time at which this event occurred.
            /// </summary>
            public DateTime Date { set; get; }

            /// <summary>
            /// State defines the value of the generic event type. whose type is defined by the V generic type.
            /// It is assigned the default value for the type. Make sure all enumerated and other types specify an
            /// appropriate default (or null) value
            /// </summary>
            public V State { get; set; }

            public byte Flags;

            public bool EquivalentTo(Event other) => (!IsCustomEvent && !other.IsCustomEvent) && EqualityComparer<V>.Default.Equals(State, other.State);
        }

        /// <summary>
        /// The Site Model to which these events relate
        /// </summary>
        public long SiteModelID { get; set; }

        /// <summary>
        /// The machine to which these events relate
        /// </summary>
        public long MachineID { get; set; }

        /// <summary>
        /// The container of event changes lists for a machine in a project that this event list is a member of
        /// </summary>
        [NonSerialized]
        public ProductionEventLists Container;

        [NonSerialized]
        private DateTime lastUpdateTimeUTC = DateTime.MinValue;

        /// <summary>
        /// Records the time at which this event change list was last updated in the persistent store
        /// </summary>
        public DateTime LastUpdateTimeUTC { get => lastUpdateTimeUTC; set => lastUpdateTimeUTC = value; }

        [NonSerialized]
        private Action<BinaryWriter, V> serialiseStateOut;
        public Action<BinaryWriter, V> SerialiseStateOut
        {
            get => serialiseStateOut;
            set => serialiseStateOut = value;
        }

        [NonSerialized]
        private Func<BinaryReader, V> serialiseStateIn;
        public Func<BinaryReader, V> SerialiseStateIn
        {
            get => serialiseStateIn;
            set => serialiseStateIn = value;
        }

        /// <summary>
        /// The event type this list stores
        /// </summary>
        public ProductionEventType EventListType { get; } = ProductionEventType.Unknown;

        [NonSerialized]
        private bool eventsListIsOutOfDate;

        public List<Event> Events = new List<Event>();

        /// <summary>
        /// Default no-args constructor
        /// </summary>
        public ProductionEvents()
        {

        }

        public ProductionEvents(ProductionEventLists container,
            long machineID, long siteModelID,
            ProductionEventType eventListType)
        {
            MachineID = machineID;
            SiteModelID = siteModelID;
            EventListType = eventListType;
            Container = container;

            // Machines created with the max machine ID are treated as transient and never
            // stored in or loaded from the FS file. 
            // LoadedFromPersistentStore = machineID == kICMachineIDMaxValue;
        }

        public ProductionEvents(ProductionEventLists container,
            long machineID, long siteModelID,
            ProductionEventType eventListType,
            Action<BinaryWriter, V> serialiseStateOut,
            Func<BinaryReader, V> serialiseStateIn)
        {
            MachineID = machineID;
            SiteModelID = siteModelID;
            EventListType = eventListType;
            Container = container;

            SerialiseStateIn = serialiseStateIn;
            SerialiseStateOut = serialiseStateOut;
        }

        // Compare performs a date based comparison between the event identified
        // by <Item> and the date held in <Value>
        public bool Find(DateTime findDate, out int index)
        {
            int L = 0;
            int H = Events.Count - 1;

            while (L <= H)
            {
                int I = (L + H) >> 1;
                int C = Events[I].Date.CompareTo(findDate);

                if (C < 0)
                {
                    L = I + 1;
                }
                else
                {
                    H = I - 1;
                    if (C == 0)
                    {
                        index = I;
                        return true;
                    }
                }
            }

            index = L;
            return false;
        }

        public virtual bool Find(Event value, out int index)
        {
            return Find(value.Date, out index);
        }

        //    function Compare(Item : Pointer; const Value : Variant) : Integer; overload; override;
        //    function Compare(Item1, Item2 : Pointer) : Integer; overload; override;
        //    Function Compare_LongWord(const Item : Pointer;
        //                              const Value : LongWord) : Integer; override;
        //    Function Compare_DateTime(const Item : Pointer;
        //                              const Value : TDateTime) : Integer; override;
        //    function GetEventFilename: TFilename; virtual;
        //    property FileMajorVersion: Byte read FMajorVersion;
        //    property FileMinorVersion: Byte read FMinorVersion;
        //    function UpgradeEventListFile(const FileStream : TStream;
        //                                  const InternalStream: TMemoryStream;
        //                                  const FileMajorVersion, FileMinorVersion: Integer): Boolean; virtual;

        protected void InvalidateEventList() => eventsListIsOutOfDate = true;

        public bool EventsListIsOutOfDate() => eventsListIsOutOfDate;

        // protected bool LoadedFromPersistentStore = false;

        /// <summary>
        /// Adds an event of type T with the given date into the list. If the event is a duplicate
        /// of an existing event the passed event will be ignored and the existing duplicate event 
        /// will be returned, otherwise passed event will be returned. 
        /// The method returns the event instance that was added to the list
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="value"></param>
        /// <returns>The event instance that was added to the list</returns>
        public virtual Event PutValueAtDate(DateTime dateTime, V value)
        {
            return PutValueAtDate(new Event
            {
                Date = dateTime,
                State = value,
            });
        }

        /// <summary>
        /// Adds the given event into the list.  If the event is a duplicate of an existing event the 
        /// passed event will be ignored and the existing duplicate event will be returned, otherwise 
        /// passed event will be returned.
        /// The method returns the event instance that was added to the list
        /// </summary>
        /// <param name="Event"></param>
        /// <returns>The event instance that was added to the list</returns>
        public virtual Event PutValueAtDate(Event Event)
        {
            bool ExistingEventFound = Find(Event.Date, out int EventIndex);

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
                        // If we've got a machine event overriding a machine event or a custom event overriding a custom event
                        // then delete the existing event.
                        if (Events[EventIndex].IsCustomEvent == Event.IsCustomEvent)
                        {
                            if (!Event.IsCustomEvent)
                                return Events[EventIndex];

                            // TODO add when logging available
                            // SIGLogMessage.Publish(Self, Format('Deleting custom machine event: %s', [Items[EventIndex].ToText]), slmcDebug);
                            Events.RemoveAt(EventIndex);
                        }
                        else if (Event.IsCustomEvent)
                        {
                            // If we've got a custom event with the same date as a machine event
                            // then "bump" the custom event's date by a milli-second to ensure it's
                            // after the machine event.

                            Event.Date = Event.Date.AddMilliseconds(1);
                            CorrectInsertLocationIdentified = false;
                        }
                    }

                    ExistingEventFound = Find(Event, out EventIndex);
                }
                while (!CorrectInsertLocationIdentified);
            }

            Events.Insert(EventIndex, Event);

            return Event;
        }

        // public T PutValueAtDate(T Event) => PutValueAtDate(Event);

        /// <summary>
        /// Inserts an event into the appropriate location (given by the eventDate proprty of the event) 
        /// </summary>
        /// <param name="Event"></param>
        /// <returns></returns>
        public object PutValueAtDate(object Event) => PutValueAtDate((Event)Event);

        /// <summary>
        /// Sets the state of the owning 'container' of event list (the production event changes) that this list is a member of
        /// </summary>
        /// <param name="container"></param>
        public void SetContainer(object container)
        {
            Container = (ProductionEventLists)container;
        }

        //    procedure ReadFromStream(const Stream: TStream); virtual;
        //    procedure WriteToStream(const Stream: TStream); virtual;
        //    function SaveToFile : Boolean; overload; Virtual;

        // The LoadFromFile & SaveToFile methods that take a filename argument are not for
        // use by the ProductionServer proper, but provide access for other utilites
        // that just want to load the file in isolation
        //    function LoadFromFile(const FileName : TFileName) : Boolean; overload;
        //    function SaveToFile(const FileName : TFileName) : Boolean; overload;

        /// <summary>
        /// Collates a series of events within an event list by aggregating consecutive events where there is no change
        /// in the underlying event state
        /// </summary>
        public virtual void Collate()
        {
            bool HaveStartEndEventPair = false;

            ProductionEvents<ProductionEventType>.Event StartEvent = new ProductionEvents<ProductionEventType>.Event();
            ProductionEvents<ProductionEventType>.Event EndEvent = new ProductionEvents<ProductionEventType>.Event();

            int FirstIdx = 0;
            int SecondIdx = 1;

            // We only want to collate items generally if they fall between a pair of Start/EndRecordedData events.
            // The EventStartEndRecordedDataChangeList.Collate method overrides this one to collate those
            // Start/EndRecordedData events slightly differently.
            // All other Container.EventStartEndRecordedData should use this method.
            // This method also relies on the fact that the Container.FEventStartEndRecordedData instance should
            // have been correctly collated BEFORE any of the other Container event lists are
            // collated; this is currently achieved by the fact that ProductionEventChanges.SaveToFile saves
            // the EventStartEndRecordedData list first, indirectly invoking Collate on that list first, before
            // saving the rest of the event lists.
            while (SecondIdx < Events.Count)
            {
                if (!HaveStartEndEventPair ||
                     !Range.InRange(Events[FirstIdx].Date, StartEvent.Date, EndEvent.Date))
                {
                    if (!Container.StartEndRecordedDataEvents.FindStartEventPairAtTime(Events[FirstIdx].Date, out StartEvent, out EndEvent))
                    {
                        FirstIdx = SecondIdx;
                        SecondIdx = FirstIdx + 1;

                        continue;
                    }

                    HaveStartEndEventPair = true;
                }

                if (Events[FirstIdx].EquivalentTo(Events[SecondIdx]) &&
                   Range.InRange(Events[FirstIdx].Date, StartEvent.Date, EndEvent.Date) &&
                   Range.InRange(Events[SecondIdx].Date, StartEvent.Date, EndEvent.Date))
                {
                    Events.RemoveAt(SecondIdx);
                }
                else
                {
                    FirstIdx = SecondIdx;
                }

                SecondIdx++;
            }
        }

        //    property EventChangeDataSize: Int64 read FEventChangeDataSize;
        //    property EventFileName : TFileName read GetEventFileName;
        //    procedure DumpToText(const FileName: TFileName;
        //                         const IncludeFileNameHeader : Boolean;
        //                         const NumberEvents : Boolean;
        //                         const IncludeFilenameInDump : Boolean);

        /// <summary>
        /// Determines the index of the event whose date immediately precedes the given eventData
        /// </summary>
        /// <param name="eventDate"></param>
        /// <returns></returns>
        public int IndexOfClosestEventPriorToDate(DateTime eventDate)
        {
            if (Events.Count == 0 || (Events.Count > 0 && Events[0].Date > eventDate))
                return -1;

            bool FindResult = Find(eventDate, out int LastIndex);

            // We're looking for the event prior to the requested date.
            // If we didn't find an exact match for requested date, then
            // LastIndex will be the event subsequent to the requested date,
            // so subtract one from LastIndex to give us the event prior
            if ((!FindResult) && (LastIndex > 0))
                LastIndex--;

            return LastIndex;
        }

        /// <summary>
        /// Determines the index of the event whose date immediately follows the given eventData
        /// </summary>
        /// <param name="eventDate"></param>
        /// <returns></returns>
        public int IndexOfClosestEventSubsequentToDate(DateTime eventDate)
        {
            if (Events.Count == 0 || (Events.Count > 0 && Events[Events.Count - 1].Date < eventDate))
                return -1;

            Find(eventDate, out int LastIndex);

            return LastIndex;
        }

        // Merges Start/End events into an event list to enable easy navigation for things like the timeline
        // procedure AddStartEndEvents(StartStopEvents: TICProductionEventChangeList);

        // Function CalculateInMemorySize : Integer; Virtual;
        // Function InMemorySize : Integer; InLine;
        // Procedure EnsureEventListLoaded; Inline;
        // Procedure MarkEventListAsInMemoryOnly; Inline;
        // Procedure AcquireSharedReadInterlock; Inline;
        // Procedure ReleaseSharedReadInterlock; Inline;
        // Procedure AcquireExclusiveWriteInterlock; Inline;
        // Procedure ReleaseExclusiveWriteInterlock; Inline;

        /// <summary>
        /// Writes a binary serialisation of the content of the list
        /// </summary>
        /// <param name="writer"></param>
        public void Write(BinaryWriter writer)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(writer.BaseStream, this);
        }

        /// <summary>
        /// Reads a binary serialisation of the content of the list
        /// </summary>
        /// <param name="reader"></param>
        public static ProductionEvents<V> Read(BinaryReader reader)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (ProductionEvents<V>)formatter.Deserialize(reader.BaseStream);
        }

        /// <summary>
        /// Serialises the contents of the event list using a binary writer
        /// </summary>
        /// <param name="stream"></param>
        public void SaveToStream(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                Write(writer);
            }
        }

        public string EventChangeListPersistantFileName() => $"{MachineID}-Events-{EventListType}-Summary.evt";

        /// <summary>
        /// Serialises the events and stores the serialised represented in the persistent store
        /// </summary>
        /// <param name="storageProxy"></param>
        public void SaveToStore(IStorageProxy storageProxy)
        {
            // Do a trial serialisation using pure binary writer custom serialisation for comparison with the 
            // .Net serialisation approach
            using (MemoryStream MS = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(MS))
                {
                    foreach (var e in Events)
                    {
                        writer.Write(e.Date.ToBinary());
                        writer.Write(e.Flags);
                        SerialiseStateOut(writer, e.State);
                    }

                    storageProxy.WriteStreamToPersistentStoreDirect(SiteModelID, EventChangeListPersistantFileName() + ".BinaryWriter", FileSystemStreamType.Events, MS);
                }
            }

            using (MemoryStream MS = new MemoryStream())
            {
                SaveToStream(MS);

                storageProxy.WriteStreamToPersistentStoreDirect(SiteModelID, EventChangeListPersistantFileName(), FileSystemStreamType.Events, MS);
            }
        }

        /// <summary>
        /// Loads the event list by requesting it's serialised representation from the persistent store and 
        /// deserialising it into the event list
        /// </summary>
        /// <param name="storageProxy"></param>
        /// <returns></returns>
        public ProductionEvents<V> LoadFromStore(IStorageProxy storageProxy)
        {
            storageProxy.ReadStreamFromPersistentStoreDirect(SiteModelID, EventChangeListPersistantFileName(), FileSystemStreamType.Events, out MemoryStream MS);

            if (MS != null)
            {
                MS.Position = 0;

                using (var reader = new BinaryReader(MS, Encoding.UTF8, true))
                {
                    ProductionEvents<V> Result = Read(reader);

                    if (Result != null)
                    {
                        // Copy the seriualisation lambdas into the new instance as these are not serialised into the persistent store
                        Result.SerialiseStateOut = SerialiseStateOut;
                        Result.SerialiseStateIn = SerialiseStateIn;
                    }
                    return Result ?? this;
                }
            }

            return this;
        }

        /// <summary>
        /// Locates and returns the event occurring at or immediately prior to the given eventDate
        /// </summary>
        /// <param name="eventDate"></param>
        /// <param name="stateChangeIndex"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public virtual V GetValueAtDate(DateTime eventDate, out int stateChangeIndex, V defaultValue = default(V))
        {
            if (Events.Count == 0)
            {
                stateChangeIndex = -1;
                return defaultValue;
            }

            if (!Find(eventDate, out stateChangeIndex))
            {
                stateChangeIndex--;
            }

            if (stateChangeIndex >= 0)
            {
                Event StateChange = Events[stateChangeIndex];

                if (StateChange.Date <= eventDate)
                {
                    return StateChange.State;
                }
            }

            return defaultValue;
        }

        public void Sort() => Events.Sort((a, b) => a.Date.CompareTo(b.Date));

        public int Count() => Events.Count;

        public Event Last() => Events[Events.Count - 1];

        public IList EventsList() => Events;
    }

    //*************************************************************************************************************************************
/*
    /// <summary>
    /// EfficientProductionEventChangeList implements a generic event list without using class instances for each event
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="V"></typeparam>
    [Serializable]
    public class EfficientProductionEventChangeList<T, V> : List<T>, IEfficientProductionEventChangeList<T, V> where T : struct, IEfficientProductionEventChangeBase<V>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The Site Model to which these events relate
        /// </summary>
        public long SiteModelID { get; set; }

        /// <summary>
        /// The machine to which these events relate
        /// </summary>
        public long MachineID { get; set; }

        /// <summary>
        /// The container of event changes lists for a machine in a project that this event list is a member of
        /// </summary>
        [NonSerialized]
        public EfficientProductionEventChanges Container;

        [NonSerialized]
        private DateTime lastUpdateTimeUTC = DateTime.MinValue;

        /// <summary>
        /// Records the time at which this event change list was last updated in the persistent store
        /// </summary>
        public DateTime LastUpdateTimeUTC { get => lastUpdateTimeUTC; set => lastUpdateTimeUTC = value; }

        [NonSerialized]
        private Action<BinaryWriter, V> serialiseStateOut;
        public Action<BinaryWriter, V> SerialiseStateOut
        {
            get => serialiseStateOut;
            set => serialiseStateOut = value;
        }

        [NonSerialized]
        private Func<BinaryReader, V> serialiseStateIn;
        public Func<BinaryReader, V> SerialiseStateIn
        {
            get => serialiseStateIn;
            set => serialiseStateIn = value;
        }

        /// <summary>
        /// The event type this list stores
        /// </summary>
        public ProductionEventType EventListType { get; } = ProductionEventType.Unknown;

        [NonSerialized]
        private bool eventsListIsOutOfDate;

        public EfficientProductionEventChangeList()
        {
        }

        public EfficientProductionEventChangeList(EfficientProductionEventChanges container,
                                         long machineID, long siteModelID,
                                         ProductionEventType eventListType)
        {
            MachineID = machineID;
            SiteModelID = siteModelID;
            EventListType = eventListType;
            Container = container;

            // Machines created with the max machine ID are treated as transient and never
            // stored in or loaded from the FS file. 
            // LoadedFromPersistentStore = machineID == kICMachineIDMaxValue;
        }

        public EfficientProductionEventChangeList(EfficientProductionEventChanges container,
            long machineID, long siteModelID,
            ProductionEventType eventListType,
            Action<BinaryWriter, V> serialiseStateOut,
            Func<BinaryReader, V> serialiseStateIn)
        {
            MachineID = machineID;
            SiteModelID = siteModelID;
            EventListType = eventListType;
            Container = container;

            SerialiseStateIn = serialiseStateIn;
            SerialiseStateOut = serialiseStateOut;

            // Machines created with the max machine ID are treated as transient and never
            // stored in or loaded from the FS file. 
            // LoadedFromPersistentStore = machineID == kICMachineIDMaxValue;
        }

        // Compare performs a date based comparison between the event identified
        // by <Item> and the date held in <Value>
        public bool Find(Func<T, int> Comparer, out int index)
        {
            int L = 0;
            int H = Count - 1;

            while (L <= H)
            {
                int I = (L + H) >> 1;
                int C = Comparer(this[I]);

                if (C < 0)
                {
                    L = I + 1;
                }
                else
                {
                    H = I - 1;
                    if (C == 0)
                    {
                        index = I;
                        return true;
                    }
                }
            }

            index = L;
            return false;
        }

        public bool Find(DateTime value, out int index)
        {
            return Find(item => DateTime.Compare(item.Date, value), out index);
        }

        public virtual bool Find(T value, out int index)
        {
            return Find(value.Date, out index);
        }

        //    function Compare(Item : Pointer; const Value : Variant) : Integer; overload; override;
        //    function Compare(Item1, Item2 : Pointer) : Integer; overload; override;
        //    Function Compare_LongWord(const Item : Pointer;
        //                              const Value : LongWord) : Integer; override;
        //    Function Compare_DateTime(const Item : Pointer;
        //                              const Value : TDateTime) : Integer; override;
        //    function GetEventFilename: TFilename; virtual;
        //    property FileMajorVersion: Byte read FMajorVersion;
        //    property FileMinorVersion: Byte read FMinorVersion;
        //    function UpgradeEventListFile(const FileStream : TStream;
        //                                  const InternalStream: TMemoryStream;
        //                                  const FileMajorVersion, FileMinorVersion: Integer): Boolean; virtual;

        protected void InvalidateEventList() => eventsListIsOutOfDate = true;

        public bool EventsListIsOutOfDate() => eventsListIsOutOfDate;

        // protected bool LoadedFromPersistentStore = false;

        /// <summary>
        /// Adds an event of type T with the given date into the list. If the event is a duplicate
        /// of an existing event the passed event will be ignored and the existing duplicate event 
        /// will be returned, otherwise passed event will be returned. 
        /// The method returns the event instance that was added to the list
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="value"></param>
        /// <returns>The event instance that was added to the list</returns>
        public virtual T PutValueAtDate(DateTime dateTime, V value)
        {
            return PutValueAtDate(new T
            {
                Date = dateTime,
                State = value,
            });
        }

        /// <summary>
        /// Adds the given event into the list.  If the event is a duplicate of an existing event the 
        /// passed event will be ignored and the existing duplicate event will be returned, otherwise 
        /// passed event will be returned.
        /// The method returns the event instance that was added to the list
        /// </summary>
        /// <param name="Event"></param>
        /// <returns>The event instance that was added to the list</returns>
        public virtual T PutValueAtDate(T Event)
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
                        // If we've got a machine event overriding a machine event or a custom event overriding a custom event
                        // then delete the existing event.
                        if (this[EventIndex].IsCustomEvent == Event.IsCustomEvent)
                        {
                            if (!Event.IsCustomEvent)
                                return this[EventIndex];

                            // TODO add when logging available
                            // SIGLogMessage.Publish(Self, Format('Deleting custom machine event: %s', [Items[EventIndex].ToText]), slmcDebug);
                            RemoveAt(EventIndex);
                        }
                        else if (Event.IsCustomEvent)
                        {
                            // If we've got a custom event with the same date as a machine event
                            // then "bump" the custom event's date by a milli-second to ensure it's
                            // after the machine event.

                            Event.Date = Event.Date.AddMilliseconds(1);
                            CorrectInsertLocationIdentified = false;
                        }
                    }

                    ExistingEventFound = Find(Event, out EventIndex);
                }
                while (!CorrectInsertLocationIdentified);
            }

            Insert(EventIndex, Event);

            return Event;
        }

        // public T PutValueAtDate(T Event) => PutValueAtDate(Event);

        /// <summary>
        /// Inserts an event into the appropriate location (given by the eventDate proprty of the event) 
        /// </summary>
        /// <param name="Event"></param>
        /// <returns></returns>
        public object PutValueAtDate(object Event) => PutValueAtDate((T)Event);

        /// <summary>
        /// Sets the state of the owning 'container' of event list (the production event changes) that this list is a member of
        /// </summary>
        /// <param name="container"></param>
        public void SetContainer(object container)
        {
            Container = (EfficientProductionEventChanges) container;
        }

        //    procedure ReadFromStream(const Stream: TStream); virtual;
        //    procedure WriteToStream(const Stream: TStream); virtual;
        //    function SaveToFile : Boolean; overload; Virtual;

        // The LoadFromFile & SaveToFile methods that take a filename argument are not for
        // use by the ProductionServer proper, but provide access for other utilites
        // that just want to load the file in isolation
        //    function LoadFromFile(const FileName : TFileName) : Boolean; overload;
        //    function SaveToFile(const FileName : TFileName) : Boolean; overload;

        /// <summary>
        /// Collates a series of events within an event list by aggregating consecutive events where there is no change
        /// in the underlying event state
        /// </summary>
        public virtual void Collate()
        {
            bool HaveStartEndEventPair = false;

            EfficientProductionEventChangeBase<ProductionEventType> StartEvent = new EfficientProductionEventChangeBase<ProductionEventType>();
            EfficientProductionEventChangeBase<ProductionEventType> EndEvent = new EfficientProductionEventChangeBase<ProductionEventType>();

            int FirstIdx = 0;
            int SecondIdx = 1;

            // We only want to collate items generally if they fall between a pair of Start/EndRecordedData events.
            // The EventStartEndRecordedDataChangeList.Collate method overrides this one to collate those
            // Start/EndRecordedData events slightly differently.
            // All other Container.EventStartEndRecordedData should use this method.
            // This method also relies on the fact that the Container.FEventStartEndRecordedData instance should
            // have been correctly collated BEFORE any of the other Container event lists are
            // collated; this is currently achieved by the fact that ProductionEventChanges.SaveToFile saves
            // the EventStartEndRecordedData list first, indirectly invoking Collate on that list first, before
            // saving the rest of the event lists.
            while (SecondIdx < Count)
            {
                if (!HaveStartEndEventPair ||
                     !Range.InRange(this[FirstIdx].Date, StartEvent.Date, EndEvent.Date))
                {
                    if (!Container.StartEndRecordedDataEvents.FindStartEventPairAtTime(this[FirstIdx].Date, out StartEvent, out EndEvent))
                    {
                        FirstIdx = SecondIdx; 
                        SecondIdx = FirstIdx + 1;

                        continue;
                    }

                    HaveStartEndEventPair = true;
                }

                if (this[FirstIdx].EquivalentTo(this[SecondIdx]) &&
                   Range.InRange(this[FirstIdx].Date, StartEvent.Date, EndEvent.Date) &&
                   Range.InRange(this[SecondIdx].Date, StartEvent.Date, EndEvent.Date))
                {
                    RemoveAt(SecondIdx);
                }
                else
                {
                    FirstIdx = SecondIdx;
                }

                SecondIdx++;
            }
        }

        //    property EventChangeDataSize: Int64 read FEventChangeDataSize;
        //    property EventFileName : TFileName read GetEventFileName;
        //    procedure DumpToText(const FileName: TFileName;
        //                         const IncludeFileNameHeader : Boolean;
        //                         const NumberEvents : Boolean;
        //                         const IncludeFilenameInDump : Boolean);

        /// <summary>
        /// Determines the index of the event whose date immediately precedes the given eventData
        /// </summary>
        /// <param name="eventDate"></param>
        /// <returns></returns>
        public int IndexOfClosestEventPriorToDate(DateTime eventDate)
        {
            if ((Count == 0) || ((Count > 0) && (this[0].Date > eventDate)))
                return -1;

            bool FindResult = Find(eventDate, out int LastIndex);

            // We're looking for the event prior to the requested date.
            // If we didn't find an exact match for requested date, then
            // LastIndex will be the event subsequent to the requested date,
            // so subtract one from LastIndex to give us the event prior
            if ((!FindResult) && (LastIndex > 0))
                LastIndex--;

            return LastIndex;
        }

        /// <summary>
        /// Determines the index of the event whose date immediately follows the given eventData
        /// </summary>
        /// <param name="eventDate"></param>
        /// <returns></returns>
        public int IndexOfClosestEventSubsequentToDate(DateTime eventDate)
        {
            if ((Count == 0) || ((Count > 0) && (this[Count - 1].Date < eventDate)))
                return -1;

            Find(eventDate, out int LastIndex);

            return LastIndex;
        }

        // class function CreateListOfType(const EventType: TICProductionEventType; const AMachineID: TICMachineID): TICProductionEventChangeList;

        // Merges Start/End events into an event list to enable easy navigation for things like the timeline
        // procedure AddStartEndEvents(StartStopEvents: TICProductionEventChangeList);

        // Function CalculateInMemorySize : Integer; Virtual;
        // Function InMemorySize : Integer; InLine;
        // Procedure EnsureEventListLoaded; Inline;
        // Procedure MarkEventListAsInMemoryOnly; Inline;
        // Procedure AcquireSharedReadInterlock; Inline;
        // Procedure ReleaseSharedReadInterlock; Inline;
        // Procedure AcquireExclusiveWriteInterlock; Inline;
        // Procedure ReleaseExclusiveWriteInterlock; Inline;

        /// <summary>
        /// Writes a binary serialisation of the content of the list
        /// </summary>
        /// <param name="writer"></param>
        public void Write(BinaryWriter writer)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(writer.BaseStream, this);
        }

        /// <summary>
        /// Reads a binary serialisation of the content of the list
        /// </summary>
        /// <param name="reader"></param>
        public static IEfficientProductionEventChangeList<T, V> Read(BinaryReader reader)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (EfficientProductionEventChangeList<T, V>)formatter.Deserialize(reader.BaseStream);
        }

        /// <summary>
        /// Serialises the contents of the event list using a binary writer
        /// </summary>
        /// <param name="stream"></param>
        public void SaveToStream(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                Write(writer);
            }
        }

        public string EventChangeListPersistantFileName() => $"{MachineID}-Events-{EventListType}-Summary.evt";

        /// <summary>
        /// Serialises the events and stores the serialised represented in the persistent store
        /// </summary>
        /// <param name="storageProxy"></param>
        public void SaveToStore(IStorageProxy storageProxy)
        {
            // Do a trial serialisation using pure binary writer custom serialisation for comparison with the 
            // .Net serialisation approach
            using (MemoryStream MS = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(MS))
                {
                    foreach (var e in this)
                    {
                        writer.Write(e.Date.ToBinary());
                        writer.Write(e.Flags);
                        SerialiseStateOut(writer, e.State);
                    }

                    storageProxy.WriteStreamToPersistentStoreDirect(SiteModelID, EventChangeListPersistantFileName() + ".BinaryWriter", FileSystemStreamType.Events, MS);
                }
            }

            using (MemoryStream MS = new MemoryStream())
            {
                SaveToStream(MS);

                storageProxy.WriteStreamToPersistentStoreDirect(SiteModelID, EventChangeListPersistantFileName(), FileSystemStreamType.Events, MS);
            }
        }

        /// <summary>
        /// Loads the event list by requesting it's serialised representation from the persistent store and 
        /// deserialising it into the event list
        /// </summary>
        /// <param name="storageProxy"></param>
        /// <returns></returns>
        public IEfficientProductionEventChangeList<T, V> LoadFromStore(IStorageProxy storageProxy)
        {
            storageProxy.ReadStreamFromPersistentStoreDirect(SiteModelID, EventChangeListPersistantFileName(), FileSystemStreamType.Events, out MemoryStream MS);

            if (MS != null)
            {
                MS.Position = 0;

                using (var reader = new BinaryReader(MS, Encoding.UTF8, true))
                {
                    IEfficientProductionEventChangeList<T, V> Result = Read(reader);

                    if (Result != null)
                    {
                        // Copy the seriualisation lambdas into the new instance as these are not serialised into the persistent store
                        Result.SerialiseStateOut = SerialiseStateOut;
                        Result.SerialiseStateIn = SerialiseStateIn;
                    }
                    return Result ?? this;
                }
            }

            return this;
        }

        /// <summary>
        /// Locates and returns the event occurring at or immediately prior to the given eventDate
        /// </summary>
        /// <param name="eventDate"></param>
        /// <param name="stateChangeIndex"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public virtual V GetValueAtDate(DateTime eventDate, out int stateChangeIndex, V defaultValue = default(V))
        {
            if (Count == 0)
            {
                stateChangeIndex = -1;
                return defaultValue;
            }

            if (!Find(eventDate, out stateChangeIndex))
            {
                stateChangeIndex--;
            }

            if (stateChangeIndex >= 0)
            {
                T StateChange = this[stateChangeIndex];

                if (StateChange.Date <= eventDate)
                {
                    return StateChange.State;
                }
            }

            return defaultValue;
        }
    }
*/

}
