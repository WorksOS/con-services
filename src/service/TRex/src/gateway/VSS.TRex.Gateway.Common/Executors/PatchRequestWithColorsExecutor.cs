using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common.Models;
using VSS.TRex.Exports.Patches.GridFabric.PatchRequest;
using VSS.TRex.Exports.Patches.GridFabric.PatchRequestWithColors;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.ResultHandling;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class PatchRequestWithColorsExecutor : BaseExecutor
  {
    public PatchRequestWithColorsExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) 
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public PatchRequestWithColorsExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      // TODO: a new request class PatchDataWithColorsRequest is required...
      var request = item as PatchDataRequest;//PatchDataWithColorsRequest;

      if (request == null)
        ThrowRequestTypeCastException<PatchDataRequest>();//PatchDataWithColorsRequest;

      var siteModel = GetSiteModel(request?.ProjectUid);

      var filter1 = ConvertFilter(request?.Filter, siteModel);
      var filter2 = ConvertFilter(request?.Filter2, siteModel);

      var req = new PatchRequestWithColors();

      var result = await req.ExecuteAndConvertToResult(new PatchRequestWithColorsArgument()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new[] { filter1, filter2 }),
        Mode = request.Mode,
        DataPatchNumber = request.PatchNumber,
        DataPatchSize = request.PatchSize,
        Overrides = AutoMapperUtility.Automapper.Map<OverrideParameters>(request.Overrides),
        LiftParams = ConvertLift(request.LiftSettings, request.Filter?.LayerType)
        //RenderValuesToColours = request.RenderValuesToColours,
        //ColourPalette = request.palette
      });

      result.CellSize = siteModel.CellSize;

      return new PatchDataResult(result.ConstructResultData());
    }
  }
}
