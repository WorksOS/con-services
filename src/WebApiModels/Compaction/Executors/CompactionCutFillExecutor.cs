using System;
using System.Net;
using ASNodeDecls;
using SVOICOptionsDecls;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// Processes the request to get cut-fill details
  /// </summary>
  public class CompactionCutFillExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;

      CutFillDetailsRequest request = item as CutFillDetailsRequest;
      var filter = RaptorConverters.ConvertFilter(null, request.filter, request.projectId);
      var designDescriptor = RaptorConverters.DesignDescriptor(request.designDescriptor);
      var liftBuildSettings =
        RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmNone);
 
      TCutFillDetails cutFillDetails;

      bool success = raptorClient.GetCutFillDetails(request.projectId ?? -1,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(Guid.NewGuid(), 0, TASNodeCancellationDescriptorType.cdtCutfillDetailed),
        new TCutFillSettings
        {
          Offsets = request.CutFillTolerances,
          DesignDescriptor = designDescriptor
        },
        filter,
        liftBuildSettings,
        out cutFillDetails);

      if (success)
      {
        result = CompactionCutFillDetailedResult.CreateCutFillDetailedResult(cutFillDetails.Percents);
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          "Failed to get requested cut-fill details data"));
      }
    
      return result;
    }
  }
}
