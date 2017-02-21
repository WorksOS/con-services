using System;
using System.Net;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using SVOICFilterSettings;
using VLPDDecls;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Report.Models;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Report.Executors
{
  /// <summary>
  /// The executor which passes the detailed CMV request to Raptor
  /// </summary>
  public class DetailedCMVExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public DetailedCMVExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DetailedCMVExecutor()
    {
    }

    /// <summary>
    /// Processes the detailed CMV request by passing the request to Raptor and returning the result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a CMVDetailedResult if successful</returns>     
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
       ContractExecutionResult result = null;
        try
        {
          TCMVDetails cmvDetails;
          CMVRequest request = item as CMVRequest;
          TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId,
              request.overrideStartUTC, request.overrideEndUTC, request.overrideAssetIds);
          bool success = raptorClient.GetCMVDetails(request.projectId ?? -1,
              ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0,
                  TASNodeCancellationDescriptorType.cdtCMVDetailed),
              ConvertSettings(request.cmvSettings),
              raptorFilter,
              RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
              out cmvDetails);
          if (success)
          {
            result = ConvertResult(cmvDetails);
          }
          else
          {
            throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                "Failed to get requested CMV details data"));
          }
        }
        finally
        {
          //ContractExecutionStates.ClearDynamic();
        }
        return result;
    }

      protected override void ProcessErrorCodes()
      {
      }

    
      private CMVDetailedResult ConvertResult(TCMVDetails details)
      {
        return CMVDetailedResult.CreateCMVDetailedResult(details.Percents);
      }

      private TCMVSettings ConvertSettings(CMVSettings settings)
      {
        return new TCMVSettings
        {
          CMVTarget = settings.cmvTarget,
          IsSummary = false,
          MaxCMV = settings.maxCMV,
          MaxCMVPercent = settings.maxCMVPercent,
          MinCMV = settings.minCMV,
          MinCMVPercent = settings.minCMVPercent,
          OverrideTargetCMV = settings.overrideTargetCMV               
        };
      }
     
  }
}