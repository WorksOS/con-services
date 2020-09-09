using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.ResultHandling.Coords;

namespace VSS.Productivity3D.WebApi.Models.Coord.Executors
{
  /// <summary>
  /// Generic coordinate system definition file executor.
  /// </summary>
  public class CoordinateSystemExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CoordinateSystemExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Populates ContractExecutionStates with PDS error messages.
    /// </summary>
    /// 
    protected sealed override void ProcessErrorCodes()
    { }

    /// <summary>
    /// Sends a request to TRex Gateway client.
    /// </summary>
    protected virtual Task<CoordinateSystemSettings> SendRequestToTRexGatewayClient(object item)
    {
      return null;
    }

    /// <summary>
    /// Coordinate system definition file executor (Post/Get).
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        return await SendRequestToTRexGatewayClient(item);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }
  }
}
