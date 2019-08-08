using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.QuantizedMesh.GridFabric.Requests;
using VSS.TRex.QuantizedMesh.GridFabric.Arguments;
using VSS.TRex.Filters;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// QuantizedMeshTileExecutor controls execution of quantized mesh execution
  /// </summary>
  public class QuantizedMeshTileExecutor : BaseExecutor
  {

    public QuantizedMeshTileExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public QuantizedMeshTileExecutor()
    {
    }

    /// <summary>
    /// Process Quantized Mesh tile request from WebAPI controller 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as QMTileRequest;
      if (request == null)
        ThrowRequestTypeCastException<QMTileRequest>();

      var siteModel = GetSiteModel(request?.ProjectUid);
      var filter = ConvertFilter(request?.Filter, siteModel);
      var qmRequest = new QuantizedMeshRequest();

      var response = qmRequest.Execute(new QuantizedMeshRequestArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        DisplayMode = request.DisplayMode,
        X = request.X,
        Y = request.Y,
        Z = request.Z
      }); 
      return new QMTileResult(response.data); 
    }
  }
}
