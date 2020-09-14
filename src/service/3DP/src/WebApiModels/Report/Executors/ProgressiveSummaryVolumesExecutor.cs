using System;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// Summary volumes executor for use with API v2.
  /// </summary>
  public class ProgressiveSummaryVolumesExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public ProgressiveSummaryVolumesExecutor()
    {
      ProcessErrorCodes();
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<ProgressiveSummaryVolumesRequest>(item);
        var summaryVolumesRequest = new ProgressiveSummaryVolumesDataRequest(
          request.ProjectUid,
          request.Filter,
          request.BaseDesignDescriptor?.FileUid,
          request.BaseDesignDescriptor?.Offset,
          request.TopDesignDescriptor?.FileUid,
          request.TopDesignDescriptor?.Offset,
          request.VolumeCalcType,
          request.CutTolerance,
          request.FillTolerance,
          request.AdditionalSpatialFilter,
          request.StartDate,
          request.EndDate,
          request.IntervalSeconds);

        return await trexCompactionDataProxy.SendDataPostRequest<ProgressiveSummaryVolumesResult, ProgressiveSummaryVolumesDataRequest>(summaryVolumesRequest, "/volumes/summary/progressive", customHeaders);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    protected sealed override void ProcessErrorCodes()
    {
    }
  }
}
