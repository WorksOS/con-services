using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
#if RAPTOR
using VLPDDecls;
#endif
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  /// <summary>
  /// Executes GET method on the CCA data colour palettes resource.
  /// </summary>
  public class CCAColorPaletteExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CCAColorPaletteExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    ///  CCA data colour palettes executor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item">A Domain object.</param>
    /// <returns>An instance of the ContractExecutionResult class with CCA data colour palettes.</returns>}
    /// 
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      if (item != null)
      {
        try
        {
          var request = CastRequestObjectTo<CCAColorPaletteRequest>(item);
#if RAPTOR
          if (configStore.GetValueBool("ENABLE_TREX_GATEWAY_CCA_PALETTE") ?? false)
          {
#endif
            var filter = FilterResult.CreateFilterForCCATileRequest
            (
              request.startUtc,
              request.endUtc,
              new List<long> { request.assetId },
              null,
              request.liftId.HasValue ? FilterLayerMethod.TagfileLayerNumber : FilterLayerMethod.None,
              request.liftId,
              new List<MachineDetails> { new MachineDetails(request.assetId, null, false, request.assetUid) }
            );
            var trexRequest = new CCAColorPaletteTrexRequest(request.ProjectUid.Value, filter);
            return await trexCompactionDataProxy.SendDataPostRequest<CCAColorPaletteResult, CCAColorPaletteTrexRequest>(trexRequest, "/ccacolors", customHeaders);
#if RAPTOR
          }

          return ProcessWithRaptor(request);
#endif
        }
        finally
        {
          ContractExecutionStates.ClearDynamic();
        }
      }

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          "No CCA data colour palettes request sent."));
    }

#if RAPTOR
    private CCAColorPaletteResult ProcessWithRaptor(CCAColorPaletteRequest request)
    {
      TColourPalettes palettes;

      palettes.Transitions = new TColourPalette[0];

      if (raptorClient.GetMachineCCAColourPalettes(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, request.assetId, request.startUtc, request.endUtc, request.liftId, out palettes))
      {
        if (palettes.Transitions.Length == 0)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              "Failed to process CCA data colour palettes request sent to Raptor."));
        }

        return new CCAColorPaletteResult(RaptorConverters.convertColorPalettes(palettes.Transitions));
      }

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          "Failed to process CCA data colour palettes request."));
    }
#endif

    /// <summary>
    /// Populates ContractExecutionStates with Production Data Server error messages.
    /// </summary>
    /// 
    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddErrorMessages(ContractExecutionStates);
#endif
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
