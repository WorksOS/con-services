using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Interfaces.Events.Telematics.Context;
using VSS.VisionLink.Interfaces.Events.Telematics.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.Telematics.Machine
{
    public class FaultCodeEvent : IAnnotatedEvent
    {
        public AssetDetail Asset { get; set; }
        public DeviceDetail Device { get; set; }
        public OwnerDetail Owner { get; set; }
        public TimestampDetail Timestamp { get; set; }
        public TracingMetadataDetail TracingMetadata { get; set; }

        public string CodeIdentifier { get; set; }
        public string CodeDescription { get; set; }
        public string CodeSeverity { get; set; }        
        public string CodeSource { get; set; }
    }
}
