using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
    public class DeleteSalesModelEvent : ISalesModelEvent
    {
        public Guid SalesModelUID { get; set; } // Required Field
        public DateTime ActionUTC { get; set; } // Required Field
        public DateTime ReceivedUTC { get; set; }
    }
}