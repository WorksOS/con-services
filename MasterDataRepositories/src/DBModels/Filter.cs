using System;

namespace VSS.MasterData.Repositories.DBModels
{
  public class Filter
  {
    public string CustomerUid { get; set; }
    public string UserUid { get; set; }
    public string ProjectUid { get; set; }
    public string FilterUid { get; set; }

    public string Name { get; set; }
    public string FilterJson { get; set; }
    public bool IsDeleted { get; set; } 
    public DateTime LastActionedUtc { get; set; }

    public override bool Equals(object obj)
    {
      var otherProject = obj as Filter;
      if (otherProject == null) return false;
      return otherProject.FilterUid == FilterUid
             && otherProject.CustomerUid == CustomerUid
             && otherProject.Name == Name
             && otherProject.FilterJson == FilterJson
             && otherProject.IsDeleted == IsDeleted
             && otherProject.LastActionedUtc == LastActionedUtc
        ;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}