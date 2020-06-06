using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.CellDatum.Executors;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.GridFabric.ComputeFuncs;

namespace VSS.TRex.CellDatum.GridFabric.ComputeFuncs
{
  public class CellPassesRequestComputeFunc_ApplicationService : BaseComputeFunc, IComputeFunc<CellPassesRequestArgument_ApplicationService, CellPassesResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<CellPassesRequestComputeFunc_ApplicationService>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public CellPassesRequestComputeFunc_ApplicationService()
    {
    }

    public CellPassesResponse Invoke(CellPassesRequestArgument_ApplicationService arg)
    {
      _log.LogInformation($"In {nameof(CellPassesRequestComputeFunc_ApplicationService)}.Invoke()");

      try
      {
        var request = new CellPassesComputeFuncExecutor_ApplicationService();

        _log.LogInformation($"Executing {nameof(CellPassesRequestComputeFunc_ApplicationService)}.ExecuteAsync()");

        return request.ExecuteAsync(arg).WaitAndUnwrapException();
      }
      finally
      {
        _log.LogInformation($"Exiting {nameof(CellPassesRequestComputeFunc_ApplicationService)}.Invoke()");
      } 

    }
  }
}
