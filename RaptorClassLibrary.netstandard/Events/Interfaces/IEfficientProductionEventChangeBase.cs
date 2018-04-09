using System;
using System.Collections.Generic;
using System.Text;
using VSS.VisionLink.Raptor.Events;

namespace VSS.VisionLink.Raptor.Events.Interfaces
{
    public interface IEfficientProductionEventChangeBase<V>
    {
        /// <summary>
        /// Defines whether this event is a custom event, ie: an event that was not recorded by a machine but which has been 
        /// inserted as a part of a nother process such as to override values recorded by the machine that were incorrect 
        /// (eg: design or material lift number)
        /// </summary>
        bool IsCustomEvent { get; set; }

        /// <summary>
        /// The 'Type' of event, such as machine start or stop. See GetEventType for further informtion.
        /// </summary>
        ProductionEventType Type { set; get; }

        /// <summary>
        /// The date/time at which this event occurred.
        /// </summary>
        DateTime Date { set; get; }

        /// <summary>
        /// State defines the value of the generic event type. whose type is defined by the V generic type.
        /// It is assigned the default value for the type. Make sure all enumerated and other types specify an
        /// appropriate default (or null) value
        /// </summary>
        V State { get; set; } // = default(V);
    }
}
