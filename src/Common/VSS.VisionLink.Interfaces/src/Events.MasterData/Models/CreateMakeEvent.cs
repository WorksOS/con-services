using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
    public class CreateMakeEvent : IMakeEvent
    {
        public string MakeCode { get; set; } // Required Field
        public string MakeDesc { get; set; } // Required Field
        public Guid MakeUID { get; set; } // Required Field
        public DateTime ActionUTC { get; set; } // Required Field
        public DateTime ReceivedUTC { get; set; }
    }
}