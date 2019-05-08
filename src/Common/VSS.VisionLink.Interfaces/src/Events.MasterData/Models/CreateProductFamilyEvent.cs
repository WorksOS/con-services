using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
    public class CreateProductFamilyEvent : IProductFamilyEvent
    {
        public string ProductFamilyName { get; set; } // Required Field
        public string ProductFamilyDesc { get; set; } // Required Field
        public Guid ProductFamilyUID { get; set; } // Required Field
        public DateTime ActionUTC { get; set; } // Required Field
        public DateTime ReceivedUTC { get; set; }
    }
}