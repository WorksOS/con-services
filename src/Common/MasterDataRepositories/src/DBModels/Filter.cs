using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Repositories.DBModels
{
  public class Filter
  {
    public string FilterUid { get; set; }
    public string CustomerUid { get; set; }
    public string ProjectUid { get; set; }

    // UserId will contain either UserUID (Guid) or ApplicationID (string)
    public string UserId { get; set; }
    public string Name { get; set; }
    public string FilterJson { get; set; }
    public FilterType FilterType { get; set; }
    public bool IsDeleted { get; set; } 
    public DateTime LastActionedUtc { get; set; }

    public override bool Equals(object obj)
    {
      var otherFilter = obj as Filter;
      if (otherFilter == null) return false;
      return otherFilter.FilterUid == FilterUid
             && otherFilter.CustomerUid == CustomerUid
             && otherFilter.ProjectUid == ProjectUid
             && otherFilter.UserId == UserId
             && otherFilter.Name == Name
             && otherFilter.FilterJson == FilterJson
             && otherFilter.FilterType == FilterType
             && otherFilter.IsDeleted == IsDeleted
             && otherFilter.LastActionedUtc == LastActionedUtc
        ;
    }

    public override int GetHashCode()
    {
      return 0;
    }

    public override string ToString()
    {
      return
        $"Filter Uid: {FilterUid}  CustomerUid: {CustomerUid} ProjectUid: {ProjectUid} UserId: {UserId} Name: {Name} " +
        $"FilterType: {FilterType} IsDeleted: {IsDeleted} LastActionedUtc: {LastActionedUtc} FilterJson {FilterJson}";
    }
  }
}