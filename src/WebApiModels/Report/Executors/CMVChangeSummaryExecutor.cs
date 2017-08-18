﻿using ASNode.CMVChange.RPC;
using ASNodeDecls;
using SVOICOptionsDecls;
using System;
using System.Collections.Generic;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Report.Executors
{
  /// <summary>
  /// Builds Summary speed report from Raptor
  /// </summary>
  public class CMVChangeSummaryExecutor : RequestExecutorContainer
  {
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

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          "Failed to get requested cmv change summary data"));
    }
  }
}