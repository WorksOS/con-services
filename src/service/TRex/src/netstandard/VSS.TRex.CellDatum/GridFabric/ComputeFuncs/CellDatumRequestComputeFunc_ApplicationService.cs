using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.Productivity3D.Models.ResultHandling;
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
    private static readonly ILogger _log = Logging.Logger.CreateLogger<CellDatumRequestComputeFunc_ApplicationService>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public CellDatumRequestComputeFunc_ApplicationService()
    {
    }

    public CellDatumResponse_ApplicationService Invoke(CellDatumRequestArgument_ApplicationService arg)
    {
      _log.LogInformation("In CellDatumRequestComputeFunc_ApplicationService.Invoke()");

      try
      {
        var request = new CellDatumComputeFuncExecutor_ApplicationService();

        _log.LogInformation("Executing CellDatumRequestComputeFunc_ApplicationService.ExecuteAsync()");

        return request.ExecuteAsync(arg).WaitAndUnwrapException();
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception in cell datum application service compute func");
        return new CellDatumResponse_ApplicationService { ReturnCode = CellDatumReturnCode.UnexpectedError };
      }
      finally
      {
        _log.LogInformation("Exiting CellDatumRequestComputeFunc_ApplicationService.Invoke()");
      } 
    }
  }
}
