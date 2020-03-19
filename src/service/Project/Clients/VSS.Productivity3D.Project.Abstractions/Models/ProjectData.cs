using System;
using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models
{
  public class ProjectData : IMasterDataModel
  {
    public string ProjectUID { get; set; }

    // legacy ProjectID in Gen2 is a bigint. However Raptor can't handle one, and we're unlikely to need to get that big.
    public int ShortRaptorProjectId { get; set; }

    public ProjectType ProjectType { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }

    // IanaTimeZone
    public string ProjectTimeZone { get; set; }

    // This should really be named ProjectTimeZoneIana.
    //     It is required for all projects, not just landfill.
    //     ProjectTimeZone is in Windows StandardTime name,
    //         which the UI,and ProjectSvc limit to a known set (contained in PreferencesTimeZones.cs).
    public string ProjectTimeZoneIana { get; set; }

    // start and end are actually only date with no time component. However C# has no date-only.
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }


    public string CustomerUID { get; set; }

    // todoMaverick, what is this for?
    //// legacy CustomerID in Gen2 is a bigint. Unlike LegacyProjectID, this is passed around as a long. I don't know why.
    //public long LegacyCustomerID { get; set; }

    public string GeometryWKT { get; set; }

    public string CoordinateSystemFileName { get; set; }
    public DateTime? CoordinateSystemLastActionedUTC { get; set; }

    public bool IsArchived { get; set; }

    public override bool Equals(object obj)
    {
      if (!(obj is ProjectData otherProject))
      {
        return false;
      }

      return otherProject.ProjectUID == ProjectUID
        && otherProject.ShortRaptorProjectId == ShortRaptorProjectId
        && otherProject.ProjectType == ProjectType
        && otherProject.Name == Name
        && otherProject.Description == Description
        && otherProject.ProjectTimeZone == ProjectTimeZone
        && otherProject.ProjectTimeZoneIana == ProjectTimeZoneIana
        && otherProject.StartDate == StartDate
        && otherProject.EndDate == EndDate
        && otherProject.CustomerUID == CustomerUID
        // todoMaverick && otherProject.LegacyCustomerID == LegacyCustomerID
        && otherProject.GeometryWKT == GeometryWKT
        && otherProject.CoordinateSystemFileName == CoordinateSystemFileName
        && otherProject.CoordinateSystemLastActionedUTC == CoordinateSystemLastActionedUTC
        && otherProject.IsArchived == IsArchived;
    }

    public List<string> GetIdentifiers()
    {
      return new List<string>
      {
        CustomerUID,
        ProjectUID
      };
    }
  }
}
