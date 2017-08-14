using System;

namespace VSS.Productivity3D.Scheduler.WebAPI
{
  public class Filter
  {
    public string FilterUid { get; set; }
    public string CustomerUid { get; set; }
    public string ProjectUid { get; set; }
    public string UserUid { get; set; }

    public string Name { get; set; }
    public string FilterJson { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime LastActionedUtc { get; set; }

    public override bool Equals(object obj)
    {
      var otherFilter = obj as Filter;
      if (otherFilter == null) return false;
      return otherFilter.FilterUid == FilterUid
             && otherFilter.CustomerUid == CustomerUid
             && otherFilter.ProjectUid == ProjectUid
             && otherFilter.UserUid == UserUid
             && otherFilter.Name == Name
             && otherFilter.FilterJson == FilterJson
             && otherFilter.IsDeleted == IsDeleted
             && otherFilter.LastActionedUtc == LastActionedUtc
        ;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}