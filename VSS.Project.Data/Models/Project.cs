using System;

namespace VSS.Project.Data.Models
{
  public class Project
  {
    public DateTime lastActionedUtc { get; set; }
    public int projectId { get; set; }
    public string name { get; set; }
    public string projectTimeZone { get; set; }
    public string landfillTimeZone { get; set; }
    public DateTime retrievalStartedAt { get; set; }
    public string projectUid { get; set; }
    public string customerUid { get; set; }
    public string subscriptionUid { get; set; }
    public DateTime projectEndDate { get; set; }
    public DateTime projectStartDate { get; set; }
    public ProjectType projectType { get; set; }
    //These 2 properties used for authentication stuff
    public bool isDeleted { get; set; }
    public DateTime subEndDate { get; set; } 

  
    public override bool Equals(object obj)
    {
      var otherProject = obj as Project;
      if (otherProject == null) return false;
      return otherProject.projectId == projectId && otherProject.name == name && otherProject.projectTimeZone == projectTimeZone
             && otherProject.projectUid == projectUid;
    }
    public override int GetHashCode() { return 0; }
  }
}