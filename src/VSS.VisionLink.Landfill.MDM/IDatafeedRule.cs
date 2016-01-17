namespace VSS.VisionLink.Utilization.DataFeed.Interfaces
{
  public interface IDatafeedRule<T>
  {
    T ExecuteRule(T incoming);
  }
}