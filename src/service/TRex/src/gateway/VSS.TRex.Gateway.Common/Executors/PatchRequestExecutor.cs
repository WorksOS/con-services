using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Exports.Patches;
using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.Exports.Servers.Client;
using VSS.TRex.Filters;
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

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as PatchDataRequest;

      if (request == null)
        ThrowRequestTypeCastException<PatchRequest>();

      var siteModel = GetSiteModel(request?.ProjectUid);

      var filter1 = ConvertFilter(request?.Filter1, siteModel);
      var filter2 = ConvertFilter(request?.Filter2, siteModel);
      
      PatchRequestServer server = new PatchRequestServer();

      PatchResult result = server.Execute(new PatchRequestArgument()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new[] { filter1, filter2 }),
        Mode = request.Mode,
        DataPatchNumber = request.PatchNumber,
        DataPatchSize = request.PatchSize
      });

      result.CellSize = siteModel.Grid.CellSize;

      return new PatchDataResult(result.ConstructResultData());
    }

    /// <summary>
    /// Processes the tile request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
