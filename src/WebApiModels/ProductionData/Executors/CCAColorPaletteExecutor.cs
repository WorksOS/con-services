using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

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
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      if (item != null)
      {
        try
        {
          //ProjectID projectId = (item as Tuple<ProjectID, DataID>).Item1;
          //DataID assetId = (item as Tuple<ProjectID, DataID>).Item2;

          var request = item as CCAColorPaletteRequest;

          if (request == null)
            ThrowRequestTypeCastException<CCAColorPaletteRequest>();

          TColourPalettes palettes;
          
          palettes.Transitions = new TColourPalette[0];

          //if (pdsClient.GetMachineCCAColourPalettes(projectId.projectId, assetId.dataId, out palettes))
          if (raptorClient.GetMachineCCAColourPalettes(request.ProjectId ?? -1, request.assetId, request.startUtc, request.endUtc, request.liftId, out palettes))
          {
            if (palettes.Transitions.Length == 0)
            {
              throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                  "Failed to process CCA data colour palettes request sent to Raptor."));
            }

            return CCAColorPaletteResult.CreateCCAColorPaletteResult(palettes.Transitions);
          }

          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              "Failed to process CCA data colour palettes request."));
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

    /// <summary>
    /// Populates ContractExecutionStates with Production Data Server error messages.
    /// </summary>
    /// 
    protected sealed override void ProcessErrorCodes()
    {
     //throw new NotImplementedException();
     RaptorResult.AddErrorMessages(ContractExecutionStates); 
    }
  }
}
