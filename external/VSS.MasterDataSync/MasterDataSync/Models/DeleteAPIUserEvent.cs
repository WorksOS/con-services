using System;

namespace VSS.Nighthawk.MasterDataSync.Models
{
    public class DeleteAPIUserEvent
    {
        public Guid Customeruid { get; set; }
        public string UserName { get; set; }
    }
}
