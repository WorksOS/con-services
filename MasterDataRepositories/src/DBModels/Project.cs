using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace Repositories.DBModels
{
  public class Project
  {
    public string ProjectUID { get; set; }

    // legacy ProjectID in Gen2 is a bigint. However Raptor can't handle one, and we're unlikely to need to get that big.
    public int LegacyProjectID { get; set; }
    public string Name { get; set; }
    public ProjectType ProjectType { get; set; }
    public bool IsDeleted { get; set; }

    public string ProjectTimeZone { get; set; }
    public string LandfillTimeZone { get; set; }



    public DateTime LastActionedUTC { get; set; }

    // start and end are actually only date with no time component. However C# has no date-only.
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; } // = DateTime.MaxValue.Date;


    //These properties are associations from link tables
    public string CustomerUID { get; set; }

    // legacy CustomerID in Gen2 is a bigint. Unlike LegacyProjectID, this is passed around as a long. I don't know why.
    public long LegacyCustomerID { get; set; }
    public string SubscriptionUID { get; set; }
    //These 2 properties used for authentication stuff

    public DateTime? SubscriptionEndDate { get; set; }
    public string GeometryWKT { get; set; }

    public int ServiceTypeID { get; set; } = 0;
    public DateTime? SubscriptionStartDate { get; set; }

    public override bool Equals(object obj)
    {
      var otherProject = obj as Project;
      if (otherProject == null) return false;
      return otherProject.ProjectUID == this.ProjectUID
            && otherProject.LegacyProjectID == this.LegacyProjectID
            && otherProject.Name == this.Name
            && otherProject.ProjectType == this.ProjectType
            && otherProject.ProjectTimeZone == this.ProjectTimeZone
            && otherProject.LandfillTimeZone == this.LandfillTimeZone
            && otherProject.StartDate == this.StartDate
            && otherProject.EndDate == this.EndDate
            && otherProject.LastActionedUTC == this.LastActionedUTC
            && otherProject.GeometryWKT == this.GeometryWKT
            ;
    }
    public override int GetHashCode() { return 0; }
  }
}