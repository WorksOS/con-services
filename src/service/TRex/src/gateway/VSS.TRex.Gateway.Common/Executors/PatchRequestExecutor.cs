using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common.Models;
using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.ResultHandling;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class PatchRequestExecutor : BaseExecutor
  {
    public PatchRequestExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public PatchRequestExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as PatchDataRequest;

      if (request == null)
        ThrowRequestTypeCastException<PatchRequest>();

      var siteModel = GetSiteModel(request?.ProjectUid);

      var filter1 = ConvertFilter(request?.Filter, siteModel);
      var filter2 = ConvertFilter(request?.Filter2, siteModel);
      
      var req = new PatchRequest();

      var result = await req.ExecuteAndConvertToResult(new PatchRequestArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new[] { filter1, filter2 }),
        Mode = request.Mode,
        DataPatchNumber = request.PatchNumber,
        DataPatchSize = request.PatchSize,
        Overrides = AutoMapperUtility.Automapper.Map<OverrideParameters>(request.Overrides),
        LiftParams = AutoMapperUtility.Automapper.Map<LiftParameters>(request.LiftSettings)
      });

      result.CellSize = siteModel.CellSize;

      return new PatchDataResult(result.ConstructResultData());
    }

    /// <summary>
    /// Processes the tile request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
