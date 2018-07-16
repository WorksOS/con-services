using System;

namespace VSS.TRex.GridFabric.Events
{
    public class SiteModelAttributesChangedEvent
    {
        public Guid SiteModelID { get; set; } = Guid.Empty;
    }
}
