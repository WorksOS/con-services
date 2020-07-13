using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CoreX.Interfaces;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI.Common;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Productivity3D.Models.Designs;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.Geometry;

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
    private void ConvertNEEToLLHCoords(string csib, AlignmentDesignGeometryResponse geometryResponse)
    {
      var coords = new List<XYZ>();
      if ((geometryResponse.Vertices?.Length ?? 0) > 0)
        coords.AddRange(geometryResponse.Vertices.SelectMany(x => x.Select(x => new XYZ(x[1], x[0], x[2])).ToArray()).ToList());
      if ((geometryResponse.Arcs?.Length ?? 0) > 0)
        coords.AddRange(geometryResponse.Arcs.SelectMany(x => new[] { new XYZ(x.Y1, x.X1, x.Z1), new XYZ(x.Y2, x.X2, x.Z2), new XYZ(x.YC, x.XC, x.ZC) }).ToList());
      if ((geometryResponse.Labels?.Length ?? 0) > 0)
        coords.AddRange(geometryResponse.Labels.Select(x => new XYZ(x.Y, x.X, 0.0)).ToList());

      var convertedCoords = _convertCoordinates
        .NEEToLLH(csib, coords.ToArray().ToCoreX_XYZ(), CoreX.Types.ReturnAs.Degrees)
        .ToTRex_XYZ();

      // Copy the converted coordinates to the geometry response ready for inclusion in the request result
      var index = 0;

      if ((geometryResponse.Vertices?.Length ?? 0) > 0)
      {
        for (var i = 0; i < geometryResponse.Vertices.Length; i++)
        {
          for (var j = 0; j < geometryResponse.Vertices[i].Length; j++)
          {
            geometryResponse.Vertices[i][j][0] = convertedCoords[index].X;
            geometryResponse.Vertices[i][j][1] = convertedCoords[index].Y;
            index++;
          }
        }
      }

      if ((geometryResponse.Arcs?.Length ?? 0) > 0)
      {
        for (var i = 0; i < geometryResponse.Arcs.Length; i++)
        {
          geometryResponse.Arcs[i].X1 = convertedCoords[index].X;
          geometryResponse.Arcs[i].Y1 = convertedCoords[index].Y;
          geometryResponse.Arcs[i].Z1 = convertedCoords[index].Z;
          index++;
          geometryResponse.Arcs[i].X2 = convertedCoords[index].X;
          geometryResponse.Arcs[i].Y2 = convertedCoords[index].Y;
          geometryResponse.Arcs[i].Z2 = convertedCoords[index].Z;
          index++;
          geometryResponse.Arcs[i].XC = convertedCoords[index].X;
          geometryResponse.Arcs[i].YC = convertedCoords[index].Y;
          geometryResponse.Arcs[i].ZC = convertedCoords[index].Z;
          index++;
        }
      }

      if ((geometryResponse.Labels?.Length ?? 0) > 0)
      {
        for (var i = 0; i < geometryResponse.Labels.Length; i++)
        {
          geometryResponse.Labels[i].X = convertedCoords[index].X;
          geometryResponse.Labels[i].Y = convertedCoords[index].Y;
          index++;
        }
      }

    }
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
        ConvertNEEToLLHCoords(siteModel.CSIB(), geometryResponse);

        // Populate the converted coordinates into the result. Note: At this point, X = Longitude and Y = Latitude
        var result = new AlignmentGeometryResult
        (ContractExecutionStatesEnum.ExecutedSuccessfully,
          request.DesignUid,
          geometryResponse.Vertices.Select(x =>
            x.Select(v => new[] { v[0], v[1], v[2] }).ToArray()).ToArray(),
          geometryResponse.Arcs.Select(x =>
            new AlignmentGeometryResultArc
            (x.Y1, x.X1, x.Z1,
              x.Y2, x.X2, x.Z2,
              x.YC, x.XC, x.ZC, x.CW)).ToArray(),
          geometryResponse.Labels.Select(x =>
            new AlignmentGeometryResultLabel(x.Station, x.Y, x.X, x.Rotation)).ToArray());

        return await AlignmentGeometryHelper.ConvertGeometry(result, request.FileName);
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
