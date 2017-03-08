using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace Repositories.ExtendedModels
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
      return other.CustomerUID == this.CustomerUID           
            && other.Name == this.Name
            && other.CustomerType == this.CustomerType  
            && other.IsDeleted == this.IsDeleted          
            && other.LastActionedUTC == this.LastActionedUTC
            && other.TCCOrgID == this.TCCOrgID
            ;
    }
    public override int GetHashCode() { return 0; }

  }
}
