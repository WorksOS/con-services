using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Repositories.DBModels
{
    public class CustomerTccOrg
    {
        public string Name { get; set; }
        public CustomerType CustomerType { get; set; }
        public string CustomerUID { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime LastActionedUTC { get; set; }

        public string TCCOrgID { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as CustomerTccOrg;
            if (other == null) return false;
            return other.CustomerUID == CustomerUID
                   && other.Name == Name
                   && other.CustomerType == CustomerType
                   && other.IsDeleted == IsDeleted
                   && other.LastActionedUTC == LastActionedUTC
                   && other.TCCOrgID == TCCOrgID
                ;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}