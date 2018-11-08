using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Repositories.DBModels
{
    public class Customer
    {
        public string Name { get; set; }
        public CustomerType CustomerType { get; set; }
        public string CustomerUID { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime LastActionedUTC { get; set; }

        public override bool Equals(object obj)
        {
            var otherProject = obj as Customer;
            if (otherProject == null) return false;
            return otherProject.CustomerUID == CustomerUID
                   && otherProject.Name == Name
                   && otherProject.CustomerType == CustomerType
                   && otherProject.IsDeleted == IsDeleted
                   && otherProject.LastActionedUTC == LastActionedUTC
                ;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}