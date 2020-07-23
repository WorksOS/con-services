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
using VSS.TRex.Gateway.Common.Helpers;

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
      var request = item as Requests.AlignmentDesignGeometryRequest;

      if (request == null)
      {
        ThrowRequestTypeCastException<Requests.AlignmentDesignGeometryRequest>();
      }

      var siteModel = GetSiteModel(request.ProjectUid);
      var geometryRequest = new Designs.GridFabric.Requests.AlignmentDesignGeometryRequest();
      var geometryResponse = await geometryRequest.ExecuteAsync(new AlignmentDesignGeometryArgument
      {
        ProjectID = siteModel.ID,
        AlignmentDesignID = request.DesignUid
      });

      if (geometryResponse != null && geometryResponse.RequestResult == DesignProfilerRequestResult.OK)
      {
        // Convert all coordinates from grid to lat/lon
        AlignmentMasterGeometryHelper.ConvertNEEToLLHCoords(siteModel.CSIB(), geometryResponse);

        // Populate the converted coordinates into the result. Note: At this point, X = Longitude and Y = Latitude
        return new AlignmentGeometryResult(
        ContractExecutionStatesEnum.ExecutedSuccessfully,
          new AlignmentGeometry(
            request.DesignUid,
            request.FileName,
            geometryResponse.Vertices.Select(x =>
            x.Select(v => new[] { v[0], v[1], v[2] }).ToArray()).ToArray(),
            geometryResponse.Arcs?.Select(x =>
              new AlignmentGeometryResultArc
              (x.Y1, x.X1, x.Z1,
                x.Y2, x.X2, x.Z2,
                x.YC, x.XC, x.ZC, x.CW)).ToArray(),
            geometryResponse.Labels?.Select(x =>
              new AlignmentGeometryResultLabel(x.Station, x.Y, x.X, x.Rotation)).ToArray()));
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
