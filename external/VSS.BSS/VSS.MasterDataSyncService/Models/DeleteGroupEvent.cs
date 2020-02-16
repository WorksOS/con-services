using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
    public class DeleteGroupEvent : IGroupEvent
    {
        public Guid UserUID { get; set; }
        public Guid GroupUID { get; set; }
        public DateTime ActionUTC { get; set; }
    }
}