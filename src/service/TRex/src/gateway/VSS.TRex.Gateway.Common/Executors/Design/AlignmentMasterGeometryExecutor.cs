using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Productivity3D.Models.Designs;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors.Design
{
  /// <summary>
  /// Processes the request to get design alignment master alignment geometry from the TRex site model/project.
  /// </summary>
  /// 
  public class AlignmentMasterGeometryExecutor : BaseExecutor
  {
    private IConvertCoordinates _convertCoordinates = DIContext.Obtain<IConvertCoordinates>();

    public AlignmentMasterGeometryExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public AlignmentMasterGeometryExecutor() { }

    /// <summary>
    /// Takes the response computed for the alignment, extracts all coordinates into a single list,
    /// converts all coordinates with a call to the coordinate conversion service and inserts the
    /// modified coordinates into the result.
    /// </summary>
    /// <param name="csib"></param>
    /// <param name="geometryResponse"></param>
    private async void ConvertNEEToLLHCoords(string csib, AlignmentDesignGeometryResponse geometryResponse)
    {
      var coords = new List<XYZ>();
      if ((geometryResponse.Vertices?.Length ?? 0) > 0)
        coords.AddRange(geometryResponse.Vertices.SelectMany(x => x.Select(x => new XYZ(x[0], x[1], x[2])).ToArray()).ToList());
      if ((geometryResponse.Arcs?.Length ?? 0) > 0)
        coords.AddRange(geometryResponse.Arcs.SelectMany(x => new[] { new XYZ(x.Lon1, x.Lat1, x.Elev1), new XYZ(x.Lon2, x.Lat2, x.Elev2), new XYZ(x.LonC, x.LatC, x.ElevC) }).ToList());
      if ((geometryResponse.Labels?.Length ?? 0) > 0)
        coords.AddRange(geometryResponse.Labels.Select(x => new XYZ(x.Lon, x.Lat, 0)).ToList());

      var (errorCode, convertedCoords) = await _convertCoordinates.NEEToLLH(csib, coords.ToArray());

      if (errorCode != RequestErrorStatus.OK)
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          "Failed to convert grid coordinates to WGS84 coordinates."));
      }

      // Copy the converted coordinates to the geometry response ready for inclusion in the request result
      var index = 0;

      if ((geometryResponse.Vertices?.Length ?? 0) > 0)
      {
        for (var i = 0; i < geometryResponse.Vertices.Length; i++)
        {
          for (var j = 0; j < geometryResponse.Vertices[i].Length; j++)
          {
            geometryResponse.Vertices[i][j][0] = convertedCoords[index].Y; // Y is Latitude
            geometryResponse.Vertices[i][j][1] = convertedCoords[index].X; // X is Longitude 
            index++;
          }
        }
      }

      if ((geometryResponse.Arcs?.Length ?? 0) > 0)
      {
        for (var i = 0; i < geometryResponse.Arcs.Length; i++)
        {
          geometryResponse.Arcs[i].Lat1 = convertedCoords[index].Y;
          geometryResponse.Arcs[i].Lon1 = convertedCoords[index].X;
          geometryResponse.Arcs[i].Elev1 = convertedCoords[index].Z;
          index++;
          geometryResponse.Arcs[i].Lat2 = convertedCoords[index].Y;
          geometryResponse.Arcs[i].Lon2 = convertedCoords[index].X;
          geometryResponse.Arcs[i].Elev2 = convertedCoords[index].Z;
          index++;
          geometryResponse.Arcs[i].LatC = convertedCoords[index].Y;
          geometryResponse.Arcs[i].LonC = convertedCoords[index].X;
          geometryResponse.Arcs[i].ElevC = convertedCoords[index].Z;
          index++;
        }
      }

      if ((geometryResponse.Labels?.Length ?? 0) > 0)
      {
        for (var i = 0; i < geometryResponse.Labels.Length; i++)
        {
          geometryResponse.Labels[i].Lat = convertedCoords[index].Y;
          geometryResponse.Labels[i].Lon = convertedCoords[index].X;
          index++;
        }
      }

    }
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
        // Convert all coordinates from grid to lat/lon
        ConvertNEEToLLHCoords(siteModel.CSIB(), geometryResponse);

        var result = new AlignmentGeometryResult
        (ContractExecutionStatesEnum.ExecutedSuccessfully,
          geometryResponse.Vertices,
          geometryResponse.Arcs.Select(x => 
            new AlignmentGeometryResultArc
            (x.Lat1, x.Lon1, x.Elev1, 
             x.Lat2, x.Lon2, x.Elev2, 
             x.LatC, x.LonC, x.ElevC, x.CW)).ToArray(),
          geometryResponse.Labels.Select(x => 
            new AlignmentGeometryResultLabel(x.Station, x.Lat, x.Lon, x.Rotation)).ToArray());

        return result;
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
