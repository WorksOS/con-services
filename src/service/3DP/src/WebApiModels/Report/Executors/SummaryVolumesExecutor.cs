using System.Collections.Generic;
using System.Threading.Tasks;
using CCSS.Productivity3D.Service.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Extensions;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  public class SummaryVolumesExecutor : TbcExecutorHelper
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryVolumesExecutor()
    {
      ProcessErrorCodes();
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<SummaryVolumesRequest>(item);

        var baseFilter = request.BaseFilter;
        var topFilter = request.TopFilter;
        await PairUpAssetIdentifiers(request.ProjectUid.Value, baseFilter, topFilter);
        await PairUpImportedFileIdentifiers(request.ProjectUid.Value, filter1: baseFilter, filter2: topFilter);

        var designDescriptors = new List<DesignDescriptor>();
        designDescriptors.Add(request.BaseDesignDescriptor);
        designDescriptors.Add(request.TopDesignDescriptor);
        await PairUpImportedFileIdentifiers(request.ProjectUid.Value, designDescriptors);

        if (request.VolumeCalcType == VolumesType.Between2Filters)
        {
          if (!request.ExplicitFilters)
          {
            (baseFilter, topFilter) = FilterUtilities.AdjustFilterToFilter(request.BaseFilter, request.TopFilter);
          }
        }
        else
        {
          (baseFilter, topFilter) =FilterUtilities.ReconcileTopFilterAndVolumeComputationMode(baseFilter, topFilter, request.VolumeCalcType);
        }

        var summaryVolumesRequest = new SummaryVolumesDataRequest(
            request.ProjectUid,
            baseFilter,
            topFilter,
            request.BaseDesignDescriptor.FileUid,
            request.BaseDesignDescriptor.Offset,
            request.TopDesignDescriptor.FileUid,
            request.TopDesignDescriptor.Offset,
            request.VolumeCalcType);
        log.LogDebug($"{nameof(SummaryVolumesExecutor)} trexRequest {JsonConvert.SerializeObject(summaryVolumesRequest)}");
        return await trexCompactionDataProxy.SendDataPostRequest<SummaryVolumesResult, SummaryVolumesDataRequest>(summaryVolumesRequest, "/volumes/summary", customHeaders);
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
