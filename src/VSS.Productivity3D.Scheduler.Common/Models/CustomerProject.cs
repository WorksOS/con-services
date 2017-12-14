namespace VSS.Productivity3D.Scheduler.Common.Models
{
  public class CustomerProject 
  {
    public long LegacyCustomerId { get; set; }
    public string CustomerUid { get; set; }
    public long LegacyProjectId { get; set; }
    public string ProjectUid { get; set; }

    public override bool Equals(object obj)
    {
      CustomerProject other = obj as CustomerProject;
      if (
        other?.LegacyProjectId != LegacyProjectId
        || other.ProjectUid == ProjectUid 
        || other.LegacyCustomerId != LegacyCustomerId
        || other.CustomerUid == CustomerUid
      )
        return false;
      return true;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}