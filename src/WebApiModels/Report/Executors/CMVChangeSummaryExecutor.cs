using System;
using System.Collections.Generic;
using System.Net;
using ASNode.CMVChange.RPC;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using SVOICOptionsDecls;
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
  /// Builds Summary speed report from Raptor
  /// </summary>
  public class CMVChangeSummaryExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public CMVChangeSummaryExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CMVChangeSummaryExecutor()
    {
    }

    private CMVChangeSummaryResult ConvertResult(TASNodeCMVChangeResult result)
    {

      return CMVChangeSummaryResult.CreateSummarySpeedResult
          (
              result.Values,
              result.CoverageArea
          );
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        CMVChangeSummaryRequest request = item as CMVChangeSummaryRequest;
        TASNodeCMVChangeResult result = new TASNodeCMVChangeResult();

        TASNodeCMVChangeSettings settings = new TASNodeCMVChangeSettings(request.CMVChangeSummaryValues);

        bool success = raptorClient.GetCMVChangeSummary(request.projectId ?? -1,
            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0,
                TASNodeCancellationDescriptorType.cdtCMVChange),
            settings,
            RaptorConverters.ConvertFilter(request.filterId, request.filter, request.projectId, null, null,
                new List<long>()),
            RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
            out result);
        if (success)
        {
          return ConvertResult(result);
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                  "Failed to get requested cmv change summary data"));
        }
      }
      finally
      {
      }

    }


    protected override void ProcessErrorCodes()
    {
    }
  }
}