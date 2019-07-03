using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.CellDatum.Executors;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.GridFabric.ComputeFuncs;

namespace VSS.TRex.CellDatum.GridFabric.ComputeFuncs
{
  public class CellPassesRequestComputeFunc_ApplicationService : BaseComputeFunc, IComputeFunc<CellPassesRequestArgument_ApplicationService, CellPassesResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CellPassesRequestComputeFunc_ApplicationService>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public CellPassesRequestComputeFunc_ApplicationService()
    {
    }

    public CellPassesResponse Invoke(CellPassesRequestArgument_ApplicationService arg)
    {
      Log.LogInformation($"In {nameof(CellPassesRequestComputeFunc_ApplicationService)}.Invoke()");

      try
      {
        var request = new CellPassesComputeFuncExecutor_ApplicationService();

        Log.LogInformation($"Executing {nameof(CellPassesRequestComputeFunc_ApplicationService)}.Execute()");

        return request.Execute(arg);
      }
      finally
      {
        Log.LogInformation($"Exiting {nameof(CellPassesRequestComputeFunc_ApplicationService)}.Invoke()");
      } 

    }
  }
}
