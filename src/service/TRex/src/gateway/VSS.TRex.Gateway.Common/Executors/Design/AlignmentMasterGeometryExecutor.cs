using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Productivity3D.Models.Designs;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Gateway.Common.Executors.Design
{
  /// <summary>
  /// Processes the request to get design alignment master alignment geometry from the TRex site model/project.
  /// </summary>
  /// 
  public class AlignmentMasterGeometryExecutor : BaseExecutor
  {
    public AlignmentMasterGeometryExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public AlignmentMasterGeometryExecutor() { }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as VSS.TRex.Gateway.Common.Requests.AlignmentDesignGeometryRequest;

      if (request == null)
      {
        ThrowRequestTypeCastException<VSS.TRex.Gateway.Common.Requests.AlignmentDesignGeometryRequest>();
      }

      var siteModel = GetSiteModel(request.ProjectUid);
      var geometryRequest = new VSS.TRex.Designs.GridFabric.Requests.AlignmentDesignGeometryRequest();
      var geometryResponse = await geometryRequest.ExecuteAsync(new AlignmentDesignGeometryArgument
      {
        ProjectID = siteModel.ID,
        AlignmentDesignID = request.DesignUid
      });

      if (geometryResponse != null && geometryResponse.RequestResult != DesignProfilerRequestResult.OK)
      {
        return new AlignmentGeometryResult
        (ContractExecutionStatesEnum.ExecutedSuccessfully,
          geometryResponse.Vertices,
          geometryResponse.Arcs.Select(x => 
            new AlignmentGeometryResultArc(x.Lat1, x.Lon1, x.Elev1, x.Lat2, x.Lon2, x.Elev2, x.LatC, x.LonC, x.ElevC, x.CW)).ToArray(),
          geometryResponse.Labels.Select(x => 
            new AlignmentGeometryResultLabel(x.Station, x.Lat, x.Lon, x.Rotation)).ToArray());
      }

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested Alignment Design geometry."));
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
