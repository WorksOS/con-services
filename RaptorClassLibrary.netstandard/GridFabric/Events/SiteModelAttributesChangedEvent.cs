using System;

namespace VSS.VisionLink.Raptor.GridFabric.Events
{
    public class SiteModelAttributesChangedEvent
    {
        public Guid SiteModelID { get; set; } = Guid.Empty;
    }
}
