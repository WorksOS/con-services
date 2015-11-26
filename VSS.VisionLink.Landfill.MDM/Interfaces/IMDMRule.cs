namespace VSS.VisionLink.Landfill.MDM.Interfaces
{
  public interface IMDMRule<T>
  {
    T ExecuteRule(T incoming);
  }
}