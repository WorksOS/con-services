using VSS.Velociraptor.PDSInterface.Client.TAGProcessor;

namespace VSS.Raptor.Service.Common.Interfaces
{
  /// <summary>
  /// Interface for ITAGProcessor
  /// </summary>
  public interface ITAGProcessor
  {
    /// <summary>
    /// Tag processing Client
    /// </summary>
    /// <returns></returns>
    TAGProcessorClient ProjectDataServerTAGProcessorClient();
  }
}
