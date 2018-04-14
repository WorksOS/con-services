using System;
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
    [Serializable]
    public class ProductionEventChangeList<T> : List<T>, IProductionEventChangeList<T> where T : ProductionEventChangeBase, new()
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public long SiteModelID { get; set; }
        public long MachineID { get; set; }

        [NonSerialized]
        public ProductionEventChanges Container;

        // FLastUpdateTimeUTC records the time at which this event change list was last updated
        // in the persistent store
        public DateTime LastUpdateTimeUTC { get; set; } = DateTime.MinValue;

        public ProductionEventType EventListType { get; } = ProductionEventType.Unknown;

        private bool eventsListIsOutOfDate;

        public ProductionEventChangeList()
        {
        }

        public ProductionEventChangeList(long machineID, long siteModelID,
                                         ProductionEventType eventListType) : this()
        {
            eventsListIsOutOfDate = true;

            MachineID = machineID;
            SiteModelID = siteModelID;
            EventListType = eventListType;

            // Machines created with the max machine ID are treated as transient and never
            // stored in or loaded from the FS file. 
            // LoadedFromPersistentStore = machineID == kICMachineIDMaxValue;
        }

        public ProductionEventChangeList(ProductionEventChanges container,
                                         long machineID, long siteModelID,
                                         ProductionEventType eventListType) : this(machineID, siteModelID, eventListType)
        {
            Container = container;
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
        /// <returns>The event instance that was added to the list</returns>
        public T PutValueAtDate(DateTime dateTime)
        {
            T newEvent = new T()
            {
                Date = dateTime
            };
            return PutValueAtDate(newEvent);
        }

        /// <summary>
        /// Adds the given event into the list.  If the event is a duplicate of an existing event the 
        /// passed event will be ignored and the existing duplicate event will be returned, otherwise 
        /// passed event will be returned.
        /// The method returns the event instance that was added to the list
        /// </summary>
        /// <param name="Event"></param>
        /// <returns>The event instance that was added to the list</returns>
        public T PutValueAtDate(T Event)
        {
            int EventIndex = Count;

            bool ExistingEventFound = Find(Event, out EventIndex);

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
                        if (Event.Type == this[EventIndex].Type)
                        {
                            // If we've got a machine event overriding a machine event or a custom event overriding a custom event
                            // then delete the existing event.
                            if (this[EventIndex].IsCustomEvent == Event.IsCustomEvent)
                            {
                                if (!Event.IsCustomEvent)
                                {
                                    if (Event.Type != ProductionEventType.StartRecordedData && Event.Type != ProductionEventType.EndRecordedData)
                                    {
                                        // Ignore the event and return the found item
                                        return this[EventIndex];
                                    }
                                }
                                else
                                {
                                    // TODO add when logging available
                                    // SIGLogMessage.Publish(Self, Format('Deleting custom machine event: %s', [Items[EventIndex].ToText]), slmcDebug);
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
                    }

                    ExistingEventFound = Find(Event, out EventIndex);
                }
                while (!CorrectInsertLocationIdentified);
            }

            Insert(EventIndex, Event);

            return Event;
        }

        public object PutValueAtDate(object Event) => PutValueAtDate((T)Event);

        public void SetContainer(object container)
        {
            Container = (ProductionEventChanges) container;
        }

        //    procedure ReadFromStream(const Stream: TStream); virtual;
        //    procedure WriteToStream(const Stream: TStream); virtual;
        //    function SaveToFile : Boolean; overload; Virtual;

        // The LoadFromFile & SaveToFile methods that take a filename argument are not for
        // use by the ProductionServer proper, but provide access for other utilites
        // that just want to load the file in isolation
        //    function LoadFromFile(const FileName : TFileName) : Boolean; overload;
        //    function SaveToFile(const FileName : TFileName) : Boolean; overload;

        public virtual void Collate()
        {
            ProductionEventChangeBase StartEvent = null;
            ProductionEventChangeBase EndEvent = null;

            int FirstIdx = 0;
            int SecondIdx = 1;
            bool NeedsNullRemoval = false;

            // We only want to collate items generally if they fall between a pair of Start/EndRecordedData events.
            // The TICEventStartEndRecordedDataChangeList.Collate method overrides this one to collate those
            // Start/EndRecordedData events slightly differently.
            // All other FContainer.FEventStartEndRecordedData should use this method.
            // This method also relies on the fact that the FContainer.FEventStartEndRecordedData instance should
            // have been correctly collated BEFORE any of the other FContainer event lists are
            // collated; this is currently achieved by the fact that TICProductionEventChanges.SaveToFile saves
            // the FEventStartEndRecordedData list first, indirectly invoking Collate on that list first, before
            // saving the rest of the event lists.
            while (SecondIdx < Count)
            {
                if ((StartEvent == null) || (EndEvent == null) ||
                     !Range.InRange(this[FirstIdx].Date, StartEvent.Date, EndEvent.Date))
                {
                    if (!Container.StartEndRecordedDataEvents.FindStartEventPairAtTime(this[FirstIdx].Date, out StartEvent, out EndEvent))
                    {
                        FirstIdx = SecondIdx; 
                        SecondIdx = FirstIdx + 1;

                        continue;
                    }
                }

                if (this[FirstIdx].Equals(this[SecondIdx]) &&
                   Range.InRange(this[FirstIdx].Date, StartEvent.Date, EndEvent.Date) &&
                   Range.InRange(this[SecondIdx].Date, StartEvent.Date, EndEvent.Date))
                {
                    NeedsNullRemoval = true;
                    this[SecondIdx] = null;
                }
                else
                {
                    FirstIdx = SecondIdx;
                }

                SecondIdx++;
            }

            if (NeedsNullRemoval)
            {
                RemoveAll(x => x == null);
            }
        }

        //    property EventChangeDataSize: Int64 read FEventChangeDataSize;
        //    property EventFileName : TFileName read GetEventFileName;
        //    procedure DumpToText(const FileName: TFileName;
        //                         const IncludeFileNameHeader : Boolean;
        //                         const NumberEvents : Boolean;
        //                         const IncludeFilenameInDump : Boolean);

        public int IndexOfClosestEventPriorToDate(DateTime eventDate)
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

            return LastIndex;
        }

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
        private void Write(BinaryWriter writer)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(writer.BaseStream, this);
        }

        /// <summary>
        /// Reads a binary serialisation of the content of the list
        /// </summary>
        /// <param name="reader"></param>
        private static IProductionEventChangeList<T> Read(BinaryReader reader)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (ProductionEventChangeList<T>)formatter.Deserialize(reader.BaseStream);
        }

        public void SaveToStream(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                Write(writer);
            }
        }

        public string EventChangeListPersistantFileName() => $"{MachineID}-Events-{EventListType}-Summary.evt";

        public void SaveToStore(IStorageProxy storageProxy)
        {
            using (MemoryStream MS = new MemoryStream())
            {
                SaveToStream(MS);

                storageProxy.WriteStreamToPersistentStoreDirect(SiteModelID, EventChangeListPersistantFileName(), FileSystemStreamType.Events, MS);
            }
        }

        public IProductionEventChangeList<T> LoadFromStore(IStorageProxy storageProxy)
        {
            storageProxy.ReadStreamFromPersistentStoreDirect(SiteModelID, EventChangeListPersistantFileName(), FileSystemStreamType.Events, out MemoryStream MS);

            if (MS != null)
            {
                MS.Position = 0;

                using (var reader = new BinaryReader(MS, Encoding.UTF8, true))
                {
                    IProductionEventChangeList<T> Result = Read(reader);
                    return Result ?? this;
                }
            }

            return this;
        }
    }

    [Serializable]
    public class ProductionEventChangeList<T, V> : ProductionEventChangeList<T> where T : ProductionEventChangeBase<V>, new()
    {
        public ProductionEventChangeList()
        {
        }

        public ProductionEventChangeList(ProductionEventChanges container,
                                         long machineID, long siteModelID,
                                         ProductionEventType eventListType) : base(container, machineID, siteModelID, eventListType)
        {
        }

        /// <summary>
        /// Adds an event of type T with the given date into the list, with a state value given by V.
        /// If the event is a duplicate of an existing event the passed event will be ignored and 
        /// the existing duplicate event will be returned, otherwise passed event will be returned.
        /// The method returns the event instance that was added to the list
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="value"></param>
        /// <returns>The event instance that was added to the list</returns>
        public virtual T PutValueAtDate(DateTime dateTime, V value)
        {
            T newEvent = new T
            {
                Date = dateTime,
                State = value
            };
            return PutValueAtDate(newEvent);
        }

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
}
