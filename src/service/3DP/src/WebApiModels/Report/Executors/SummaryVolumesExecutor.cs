using System;
using ASNode.Volumes.RPC;
using ASNodeDecls;
using SVOICOptionsDecls;
using SVOICVolumeCalculationsDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Executors.Utilities;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  public class SummaryVolumesExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryVolumesExecutor()
    {
      ProcessErrorCodes();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = item as SummaryVolumesRequest;

        if (request == null)
          ThrowRequestTypeCastException<SummaryVolumesRequest>();

        TASNodeSimpleVolumesResult result;

        var baseFilter = RaptorConverters.ConvertFilter(request.BaseFilter);
        var topFilter = RaptorConverters.ConvertFilter(request.TopFilter);
        var baseDesignDescriptor = RaptorConverters.DesignDescriptor(request.BaseDesignDescriptor);
        var topDesignDescriptor = RaptorConverters.DesignDescriptor(request.TopDesignDescriptor);

        var volType = RaptorConverters.ConvertVolumesType(request.VolumeCalcType);

        if (volType == TComputeICVolumesType.ic_cvtBetween2Filters)
        {
          RaptorConverters.AdjustFilterToFilter(ref baseFilter, topFilter);
        }

        RaptorConverters.reconcileTopFilterAndVolumeComputationMode(ref baseFilter, ref topFilter, request.VolumeCalcType);

        TASNodeErrorStatus raptorResult;

        if (request.CutTolerance != null && request.FillTolerance != null)
        {
          raptorResult = raptorClient.GetSummaryVolumes(request.ProjectId ?? -1,
            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
              TASNodeCancellationDescriptorType.cdtVolumeSummary),
            volType,
            baseFilter,
            baseDesignDescriptor,
            topFilter,
            topDesignDescriptor,
            RaptorConverters.ConvertFilter(request.AdditionalSpatialFilter), (double)request.CutTolerance,
            (double)request.FillTolerance,
            RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone),
            out result);
        }
        else
        {
          raptorResult = raptorClient.GetSummaryVolumes(request.ProjectId ?? -1,
            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
              TASNodeCancellationDescriptorType.cdtVolumeSummary),
            volType,
            baseFilter,
            RaptorConverters.DesignDescriptor(request.BaseDesignDescriptor),
            topFilter,
            RaptorConverters.DesignDescriptor(request.TopDesignDescriptor),
            RaptorConverters.ConvertFilter(request.AdditionalSpatialFilter),
            RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone),
            out result);
        }

        if (raptorResult == TASNodeErrorStatus.asneOK)
        {
          return ResultConverter.SimpleVolumesResultToSummaryVolumesResult(result);
        }

        throw CreateServiceException<SummaryVolumesExecutor>((int)raptorResult);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }
  }
}
