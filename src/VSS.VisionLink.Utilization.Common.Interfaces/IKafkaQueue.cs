namespace VSS.VisionLink.Landfill.Common.Interfaces
{
  /// <summary>
  ///   Interface provides access to kafka queue provider
  /// </summary>
  public interface IKafkaQueue<out T>
  {
    T GetNextItem(out long offset);
  }
}