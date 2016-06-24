﻿using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Project.Data.Models
{
  public class Project
  {
    public DateTime LastActionedUTC { get; set; }
    public int ProjectID { get; set; }
    public string Name { get; set; }
    public string ProjectTimeZone { get; set; }
    public string LandfillTimeZone { get; set; }
    public string ProjectUID { get; set; }
    public string CustomerUID { get; set; }
    public long CustomerID { get; set; }
    public string SubscriptionUID { get; set; }
    public DateTime ProjectEndDate { get; set; }
    public DateTime ProjectStartDate { get; set; }
    public ProjectType ProjectType { get; set; }
    //These 2 properties used for authentication stuff
    public bool IsDeleted { get; set; }
    public DateTime SubEndDate { get; set; } 

  
    public override bool Equals(object obj)
    {
      var otherProject = obj as Project;
      if (otherProject == null) return false;
      return otherProject.ProjectID == this.ProjectID && otherProject.Name == this.Name && otherProject.ProjectTimeZone == this.ProjectTimeZone
             && otherProject.ProjectUID == this.ProjectUID;
    }
    public override int GetHashCode() { return 0; }
  }
}