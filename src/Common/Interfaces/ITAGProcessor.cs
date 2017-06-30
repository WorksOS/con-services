using VSS.Velociraptor.PDSInterface.Client.TAGProcessor;

namespace VSS.Productivity3D.Common.Interfaces
{
  /// <summary>
  /// Interface for ITagProcessor
  /// </summary>
  public interface ITagProcessor
  {
    /// <summary>
    /// Tag processing Client
    /// </summary>
    /// <returns></returns>
    TAGProcessorClient ProjectDataServerTAGProcessorClient();
  }
}
