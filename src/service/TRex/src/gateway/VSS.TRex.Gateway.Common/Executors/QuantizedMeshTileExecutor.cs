using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.QuantizedMesh.GridFabric.Requests;
using VSS.TRex.QuantizedMesh.GridFabric.Arguments;
using VSS.TRex.Filters;
using System;

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
    /// Processes the QM tile request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }

    /// <summary>
    /// Process Quantized Mesh tile request from WebAPI controller 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item">QMTileRequest</param>
    /// <returns>Zipped Quantized Mesh Tile</returns>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as QMTileRequest;
      if (request == null)
        ThrowRequestTypeCastException<QMTileRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);
      var filter = ConvertFilter(request.Filter, siteModel);
      var qmRequest = new QuantizedMeshRequest();

      var response = await qmRequest.ExecuteAsync(new QuantizedMeshRequestArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        DisplayMode = request.DisplayMode,
        HasLighting = request.HasLighting,
        X = request.X,
        Y = request.Y,
        Z = request.Z
      }); 

      return new QMTileResult(response.data); 
    }
  }
}
