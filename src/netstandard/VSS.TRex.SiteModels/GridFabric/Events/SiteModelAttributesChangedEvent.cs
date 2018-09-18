using System;
using VSS.TRex.SiteModels.Interfaces.Events;

namespace VSS.TRex.SiteModels.GridFabric.Events
{
    public class SiteModelAttributesChangedEvent : ISiteModelAttributesChangedEvent
    {
        public Guid SiteModelID { get; set; } = Guid.Empty;
    }
}
