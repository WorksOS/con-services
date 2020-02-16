using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
    public class UpdateGroupEvent : IGroupEvent
    {
        public string GroupName { get; set; }
        public Guid UserUID { get; set; }
        public List<Guid> AssociatedAssetUID { get; set; }
        public List<Guid> DissociatedAssetUID { get; set; }
        public Guid GroupUID { get; set; }
        public DateTime ActionUTC { get; set; }
    }
}