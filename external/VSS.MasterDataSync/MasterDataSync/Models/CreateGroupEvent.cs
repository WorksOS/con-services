using System;
using System.Collections.Generic;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
    public class CreateGroupEvent : IGroupEvent
    {
        public string GroupName { get; set; }
        public Guid CustomerUID { get; set; }
        public Guid UserUID { get; set; }
        public List<Guid?> AssetUID { get; set; }
        public Guid GroupUID { get; set; }
        public DateTime ActionUTC { get; set; }
    }
}