using System;
using VSS.VisionLink.Raptor.Events.Interfaces;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.Events
{
    /// <summary>
    /// Base class to represent all event types in a Raptor data model.
    /// </summary>
    [Serializable]
    public struct EfficientProductionEventChangeBase<V> : IEfficientProductionEventChangeBase<V>, IComparable<EfficientProductionEventChangeBase<V>>
    {
        /// <summary>
        /// Flag constant indicating this event is a customer event
        /// </summary>
        private const int kCustomEventBitFlag = 0;

        /// <summary>
        /// Storage for event flags (such as Custom event)
        /// </summary>
        private byte flags;

        /// <summary>
        /// The date/time at which this event occurred.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// State defines the value of the generic event type. whose type is defined by the V generic type.
        /// It is assigned the default value for the type. Make sure all enumerated and other types specify an
        /// appropriate default (or null) value
        /// </summary>
        public V State { get; set; }

        /// <summary>
        /// Constructor taking the date/time the event occurred.
        /// </summary>
        /// <param name="dateTime"></param>
        public EfficientProductionEventChangeBase(DateTime dateTime) : this()
        {
            Date = dateTime;
            IsCustomEvent = false;
        }

        /// <summary>
        /// Defines whether this event is a custom event, ie: an event that was not recorded by a machine but which has been 
        /// inserted as a part of a nother process such as to override values recorded by the machine that were incorrect 
        /// (eg: design or material lift number)
        /// </summary>
        public bool IsCustomEvent
        {
            get => BitFlagHelper.IsBitOn(flags, kCustomEventBitFlag);
            set => BitFlagHelper.SetBit(ref flags, kCustomEventBitFlag, value);
        }

        /// <summary>
        /// Assigns the content of another event to this event
        /// </summary>
        /// <param name="source"></param>
        public void Assign(EfficientProductionEventChangeBase<V> source)
        {
            Date = source.Date;
            flags = source.flags;
        }

        /// <summary>
        /// Provides the base comparer between two events for a generic event. Base events define only a date/time, this comparer
        /// uses that as the ordering predicate
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(EfficientProductionEventChangeBase<V> other) => Date.CompareTo(other.Date);

        /// <summary>
        /// EquivalentTo defines equivalency between Self and Source defined
        /// as the state of the Self event being the same as the Source event,
        /// but the time of occurrence need not be.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool EquivalentTo(EfficientProductionEventChangeBase<V> source) => !IsCustomEvent && !source.IsCustomEvent;
    }
}
