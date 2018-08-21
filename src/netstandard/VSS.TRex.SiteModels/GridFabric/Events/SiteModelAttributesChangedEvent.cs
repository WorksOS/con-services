using System;

namespace VSS.TRex.SiteModels.GridFabric.Events
{
    public class SiteModelAttributesChangedEvent
    {
        public Guid SiteModelID { get; set; } = Guid.Empty;
    }
}
