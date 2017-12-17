using Apache.Ignite.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.GridFabric.Events
{
    public class SiteModelAttributesChangedEvent
    {
        public long SiteModelID { get; set; } = long.MinValue;
    }
}
