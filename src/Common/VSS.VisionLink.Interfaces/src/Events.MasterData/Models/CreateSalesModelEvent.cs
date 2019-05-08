using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
    public class CreateSalesModelEvent : ISalesModelEvent
    {
        public string SalesModelCode { get; set; } // Required Field
        public string SalesModelDescription { get; set; } // Required Field
        public string SerialNumberPrefix { get; set; } // Required Field
        public long? StartRange { get; set; } // Required Field
        public long? EndRange { get; set; } // Required Field
        public Guid? IconUID { get; set; } // Required Field
        public Guid? ProductFamilyUID { get; set; } // Required Field
        public Guid SalesModelUID { get; set; } // Required Field
        public DateTime ActionUTC { get; set; } // Required Field
        public DateTime ReceivedUTC { get; set; }
    }
}