using System;
using System.Threading.Tasks;
using DesignProfilerDecls;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// 
  /// </summary>
  public class AlignmentLineworkExecutor : RequestExecutorContainer
  {
    public AlignmentLineworkExecutor()
    {
      ProcessErrorCodes();
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<AlignmentLineworkRequest>(item);

        if (UseTRexGateway("ENABLE_TREX_GATEWAY_DESIGN_BOUNDARY"))
        {
          return ProcessWithTRex(request);
        }

        return ProcessWithRaptor(request);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    private AlignmentLineworkResult ProcessWithTRex(AlignmentLineworkRequest request)
    {
      throw new NotImplementedException("TRex Gateway not yet implemented for AlignmentLineworkExecutor");
    }

    private AlignmentLineworkResult ProcessWithRaptor(AlignmentLineworkRequest request)
    {
      const double ImperialFeetToMetres = 0.3048;
      const double USFeetToMetres = 0.304800609601;

      //NOTE: For alignment files only (not surfaces), there are labels generated as part of the DXF file.
      //They need to be in the user units.
      double interval;
      TVLPDDistanceUnits raptorUnits;
      switch (request.UserUnits)
      {
        case DxfUnitsType.ImperialFeet:
          raptorUnits = TVLPDDistanceUnits.vduImperialFeet;
          interval = 300 * ImperialFeetToMetres;
          break;

        case DxfUnitsType.Meters:
          raptorUnits = TVLPDDistanceUnits.vduMeters;
          interval = 100;
          break;
        case DxfUnitsType.UsSurveyFeet:
        default:
          raptorUnits = TVLPDDistanceUnits.vduUSSurveyFeet;
          interval = 300 * USFeetToMetres;
          break;
      }

      log.LogDebug($"Getting DXF design boundary from Raptor for {request.FileDescriptor.FileName} for project {request.ProjectUid}");

      raptorClient.GetDesignBoundary(
        DesignProfiler.ComputeDesignBoundary.RPC.__Global.Construct_CalculateDesignBoundary_Args
        (request.ProjectId.Value,
          request.FileDescriptor.DesignDescriptor(configStore, log, 0, 0),
          DesignProfiler.ComputeDesignBoundary.RPC.TDesignBoundaryReturnType.dbrtDXF,
          interval, raptorUnits, 0), out var memoryStream, out var designProfilerResult);


      if (memoryStream == null || designProfilerResult != TDesignProfilerRequestResult.dppiOK)
      {
        log.LogWarning($"Failed to generate DXF boundary for file {request.FileDescriptor.FileName} for project {request.ProjectUid}. Raptor error {designProfilerResult}");
        throw CreateServiceException<AlignmentLineworkExecutor>((int)designProfilerResult);
      }
 
      return new AlignmentLineworkResult(memoryStream);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }

    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddDesignProfileErrorMessages(ContractExecutionStates);
#endif
    }
  }
}
