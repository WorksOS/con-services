#if NET_4_7 
using System;
using System.Threading.Tasks;
using SkuTester.DataModel;
using VSS.Hydrology.WebApi.Abstractions.Models;
using VSS.Hydrology.WebApi.Abstractions.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Hydrology.WebApi.Common.Executors
{
  /// <summary>
  /// Executor for ...
  /// </summary>
  public class PondingExecutor : RequestExecutorContainer
  {
    public PondingExecutor()
    {
      ProcessErrorCodes();
    }

    protected sealed override void ProcessErrorCodes()
    { }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as PondingRequest;

      // todo
      // 1) get boundary from Filter (designBoundary OR Geofence OR projectBoundary)
      // 2) get latestSurface from TRex using that boundary
      // 3) convert ttm to dxf mesh
      var useCase = Extract(request, string.Empty);
      //if (useCase == null)
      //  throw new ArgumentException("Unable to load surface configuration");

      return new PondingResult(String.Empty);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }

    private TestCase Extract(PondingRequest pondingRequest, string surfaceFileName)
    {
      var hydroRequest = new TestCase
      {
        Surface = surfaceFileName, // todo or wherever the local file is
        Resolution = pondingRequest.Resolution,
        IsXYZ = pondingRequest.IsXYZ,
        IsMetric = pondingRequest.IsMetric
      };
      return hydroRequest;
    }
  }
}
#endif
