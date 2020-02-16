using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Services.MDM.Models
{
    public class CreateWorkDefinitionEvent
    {
        public Guid AssetUID { get; set; }
        public string WorkDefinitionType { get; set; }
        public int? SensorNumber { get; set; }
        public bool? StartIsOn { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
    public class UpdateWorkDefinitionEvent
    {
        public Guid AssetUID { get; set; }
        public string WorkDefinitionType { get; set; }
        public int? SensorNumber { get; set; }
        public bool? StartIsOn { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
