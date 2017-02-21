
using System.Net;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;


namespace VSS.Raptor.Service.WebApiModels.ProductionData.Executors
{
  /// <summary>
  /// Executes GET method on the CCA data colour palettes resource.
  /// </summary>
  /// 
  public class CCAColorPaletteExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    /// 
    public CCAColorPaletteExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CCAColorPaletteExecutor()
    {
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
      ContractExecutionResult result = null;

      if ((object)item != null)
      {
        try
        {
          //ProjectID projectId = (item as Tuple<ProjectID, DataID>).Item1;
          //DataID assetId = (item as Tuple<ProjectID, DataID>).Item2;

          CCAColorPaletteRequest request = item as CCAColorPaletteRequest;

          TColourPalettes palettes;
          
          palettes.Transitions = new TColourPalette[0];

          //if (pdsClient.GetMachineCCAColourPalettes(projectId.projectId, assetId.dataId, out palettes))
          if (raptorClient.GetMachineCCAColourPalettes(request.projectId ?? -1, request.assetId, request.startUtc, request.endUtc, request.liftId, out palettes))
          {
            if (palettes.Transitions.Length == 0)
            {
              throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                  "Failed to process CCA data colour palettes request sent to Raptor."));
            }

            result = CCAColorPaletteResult.CreateCCAColorPaletteResult(palettes.Transitions);
          }
          else
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                "Failed to process CCA data colour palettes request."));
          }
        }
        finally
        {
          ContractExecutionStates.ClearDynamic();
        }
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            "No CCA data colour palettes request sent."));
      }

      return result;
    }

    /// <summary>
    /// Populates ContractExecutionStates with Production Data Server error messages.
    /// </summary>
    /// 
    protected override void ProcessErrorCodes()
    {
     //throw new NotImplementedException();
     RaptorResult.AddErrorMessages(ContractExecutionStates); 
    }
  }
}