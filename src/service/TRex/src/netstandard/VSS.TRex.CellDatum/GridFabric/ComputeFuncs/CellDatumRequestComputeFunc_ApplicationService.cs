using System.Reflection;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.CellDatum.Executors;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.GridFabric.ComputeFuncs;

namespace VSS.TRex.CellDatum.GridFabric.ComputeFuncs
{
  /// <summary>
  /// This compute func operates in the context of an application server that reaches out to the compute cluster to 
  /// perform subgrid processing.
  /// </summary>
  public class CellDatumRequestComputeFunc_ApplicationService : BaseComputeFunc, IComputeFunc<CellDatumRequestArgument_ApplicationService, CellDatumResponse_ApplicationService>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CellDatumRequestComputeFunc_ApplicationService>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public CellDatumRequestComputeFunc_ApplicationService()
    {
    }

    public CellDatumResponse_ApplicationService Invoke(CellDatumRequestArgument_ApplicationService arg)
    {
      Log.LogInformation("In CellDatumRequestComputeFunc_ApplicationService.Invoke()");

      try
      {
        var request = new CellDatumComputeFuncExecutor_ApplicationService();

        Log.LogInformation("Executing CellDatumRequestComputeFunc_ApplicationService.ExecuteAsync()");

        return request.ExecuteAsync(arg).WaitAndUnwrapException();
      }
      finally
      {
        Log.LogInformation("Exiting CellDatumRequestComputeFunc_ApplicationService.Invoke()");
      }
    }
  }
}
