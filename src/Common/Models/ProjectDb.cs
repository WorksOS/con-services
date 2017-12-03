using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace Common.Models
{
  public class ProjectDb
  {
    public DateTime LastActionedUTC { get; set; }
    public int ProjectID { get; set; }
    public string Name { get; set; }
    public string ProjectTimeZone { get; set; }
    public string LandfillTimeZone { get; set; }
    public string ProjectUID { get; set; }
    public DateTime ProjectEndDate { get; set; }
    public DateTime ProjectStartDate { get; set; }
    public ProjectType ProjectType { get; set; }
    //These properties are associations from link tables
    public string CustomerUID { get; set; }
    public long LegacyCustomerID { get; set; }
    public string SubscriptionUID { get; set; }
    //These 2 properties used for authentication stuff
    public bool IsDeleted { get; set; }
    public DateTime SubEndDate { get; set; } 

  
    public override bool Equals(object obj)
    {
      var otherProject = obj as ProjectDb;
      if (otherProject == null) return false;
      return otherProject.ProjectID == this.ProjectID && otherProject.Name == this.Name && otherProject.ProjectTimeZone == this.ProjectTimeZone
             && otherProject.ProjectUID == this.ProjectUID;
    }
    public override int GetHashCode() { return 0; }
  }
}