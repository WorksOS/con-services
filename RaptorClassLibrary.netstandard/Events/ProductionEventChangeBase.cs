using System;
using System.Collections.Generic;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.Events
{
    /// <summary>
    /// Base class to represent all event types in a Raptor data model.
    /// </summary>
    [Serializable]
    public class ProductionEventChangeBase : IComparable<ProductionEventChangeBase>
    {
        /// <summary>
        /// Flag constant indicating this event is a customer event
        /// </summary>
        private const int kCustomEventBitFlag = 0;

//        private const int kSpatialDataRemovedForEventBitFlag = 1;

            /// <summary>
            /// Storage for event flags (such as Custom event)
            /// </summary>
        private byte flags;

        /// <summary>
        /// Most events don't care about their type. Typically only those event lists that include 'start' and 'end'
        /// time based variations of events need to distinguish between them. 
        /// Event types that do need to distinguish should override this method. By default, an event will advertise
        /// 'Unknown' for the event type
        /// </summary>
        /// <returns></returns>
        protected virtual ProductionEventType GetEventType()
        {
            return ProductionEventType.Unknown;
        }

        /// <summary>
        /// The 'Type' of event, such as machine start or stop. See GetEventType for further informtion.
        /// </summary>
        public ProductionEventType Type { get { return GetEventType(); } }

        /// <summary>
        /// The date/time at which this event occurred.
        /// </summary>
        public DateTime Date { get; set; } = DateTime.MinValue;

        // The base reading and writing of streams is not virtual to
        // provide more control of streaming during event upgrades
        //    procedure BaseWriteToStream(const Stream: TStream);
        //    procedure BaseReadFromStream(const Stream: TStream);
        //    procedure BaseReadFromStream_1p0(const Stream: TStream);
        //    procedure DoWriteToStream(const Stream: TStream); virtual;
        //    procedure DoReadFromStream(const Stream: TStream); virtual;
        //    procedure ReadFromStream(const Stream: TStream);
        //    procedure WriteToStream(const Stream: TStream);

        /// <summary>
        /// EquivalentTo defines equivalency between Self and Source defined
        /// as the state of the Self event being the same as the Source event,
        /// but the time of occurrence need not be.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public virtual bool EquivalentTo(ProductionEventChangeBase source)
        {
            return !IsCustomEvent && !source.IsCustomEvent;
        }

        /// <summary>
        /// Base no-arg constructor. 
        /// </summary>
        public ProductionEventChangeBase()
        {
            IsCustomEvent = false;
        }

        /// <summary>
        /// Constructor taking the date/time the event occurred.
        /// </summary>
        /// <param name="dateTime"></param>
        public ProductionEventChangeBase(DateTime dateTime) : this()
        {
            Date = dateTime;
        }

        /// <summary>
        /// Defines whether this event is a custom event, ie: an event that was not recorded by a machine but which has been 
        /// inserted as a part of a nother process such as to override values recorded by the machine that were incorrect 
        /// (eg: design or material lift number)
        /// </summary>
        public bool IsCustomEvent
        {
            get { return BitFlagHelper.IsBitOn(flags, kCustomEventBitFlag); }
            set { BitFlagHelper.SetBit(ref flags, kCustomEventBitFlag, value); }
        }

//        public bool SpatialDataRemovedForEvent
//        {
//            get { return BitFlagHelper.IsBitOn(flags, kSpatialDataRemovedForEventBitFlag); }
//            set { BitFlagHelper.SetBit(ref flags, kSpatialDataRemovedForEventBitFlag, true); }
//        }

            /// <summary>
            /// Assigns the content of another event to this event
            /// </summary>
            /// <param name="source"></param>
        public void Assign(ProductionEventChangeBase source)
        {
            Date = source.Date;
            flags = source.flags;
        }

        /// <summary>
        /// Denotes if this event has a spatial location associated with it. THe base class always responds with false
        /// </summary>
        /// <returns></returns>
        public virtual bool HasSpatialLocation() => false;

        /// <summary>
        /// Base method for requesting the spatial location of the machine at the time the event was recorded.
        /// The base method simply returns null, descendent events override this method as apporopriate.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public virtual void GetLocation(out double X, out double Y)
        {
            X = Consts.NullDouble;
            Y = Consts.NullDouble;
        }

        /// <summary>
        /// Provides the base comparer between two events for an event. Base events define only a date/time, this comparer
        /// uses that as the ordering predicate
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo(ProductionEventChangeBase other)
        {
            return Date.CompareTo(other.Date);
        }
    }

    /// <summary>
    /// A generic version of the base class for production events in Raptor data models. The type V defines a type
    /// that represents a new state of the event at the date/time represented by the base class
    /// </summary>
    /// <typeparam name="V"></typeparam>
    [Serializable]
    public class ProductionEventChangeBase<V> : ProductionEventChangeBase, IComparable<ProductionEventChangeBase<V>>
    {
        /// <summary>
        /// State defines the value of the generic event type. whose type is defined by the V generic type.
        /// It is assigned the default value for the type. Make sure all enumerated and other types specify an
        /// appropriate default (or null) value
        /// </summary>
        public V State { get; set; } // = default(V);

        /// <summary>
        /// Provides the base comparer between two events for a generic event. Base events define only a date/time, this comparer
        /// uses that as the ordering predicate
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo(ProductionEventChangeBase<V> other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// EquivalentTo defines equivalency between Self and Source defined
        /// as the state of the Self event being the same as the Source event,
        /// but the time of occurrence need not be.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public virtual bool EquivalentTo(ProductionEventChangeBase<V> source)
        {
          return base.EquivalentTo(source) && EqualityComparer<V>.Default.Equals(State, source.State);
        }
    }
}
