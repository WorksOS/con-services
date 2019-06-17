using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.Models.MapHandling;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Common.Utilities;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Types;
using FenceGeometry = VSS.Productivity3D.Models.Models.MapHandling.Geometry;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get design boundaries from TRex's site model/project.
  /// </summary>
  /// 
  public class DesignBoundariesExecutor : BaseExecutor
  {
    public DesignBoundariesExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DesignBoundariesExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as TRexDesignBoundariesRequest;

      if (request == null)
        ThrowRequestTypeCastException<TRexDesignBoundariesRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var csib = siteModel.CSIB();

      if (csib == string.Empty)
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          $"The project does not have Coordinate System definition data. Project UID: {siteModel.ID}"));

      var designBoundaryRequest = new DesignBoundaryRequest();
      var referenceDesign = new DesignOffset(request.DesignUid ?? Guid.Empty, 0.0);

      var designBoundaryResponse = designBoundaryRequest.Execute(new DesignBoundaryArgument
      {
        ProjectID = siteModel.ID,
        ReferenceDesign = referenceDesign,
        CellSize = siteModel.CellSize
      });

      if (designBoundaryResponse != null && 
          designBoundaryResponse.RequestResult == DesignProfilerRequestResult.OK && 
          designBoundaryResponse.Boundary != null)
        return ConvertResult(designBoundaryResponse.Boundary, request.Tolerance, siteModel.CellSize, csib, request.FileName);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested Design Boundary data"));
    }

    /// <summary>
    /// Converts DesignBoundaryResponse into DesignBoundaryResult data.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private DesignBoundaryResult ConvertResult(List<Fence> boundary, double tolerance, double cellSize, string csib, string fileName)
    {
      const string PROPERTY_TYPE = "type";
      const string PROPERTY_FEATURES = "features";
      const string PROPERTY_COORDINATES = "coordinates";
      const string PROPERTY_GEOMETRY = "geometry";
      const string PROPERTY_NAME = "name";
      const string PROPERTY_PROPERTIES = "properties";

      const string CONTENT_FEATURE_COLLECTION = "FeatureCollection";
      const string CONTENT_FEATURE = "Feature";
      const string CONTENT_POLYGON = "Polygon";
      const int VERTICES_LIMIT = 10000;

      var vertsCount = 0;

      // Create a main header...
      //var featuresArray = new JArray();

      //var jo = new JObject
      //{
      //  new JProperty(PROPERTY_TYPE, CONTENT_FEATURE_COLLECTION),
      //  new JProperty(PROPERTY_FEATURES, featuresArray)
      //};

      var geoJson = new GeoJson
      {
        Type = GeoJson.FeatureType.FEATURE_COLLECTION,
        Features = new List<Feature>()
      };

      // Could be multiple fences...
      foreach (var fence in boundary)
      {
        // Create a header for each polygon...
        //var featuresObj = new JObject {new JProperty(PROPERTY_TYPE, CONTENT_FEATURE) };
        //var geo = new JObject {new JProperty(PROPERTY_TYPE, CONTENT_POLYGON) };
        //var coords = new JArray();

        var geo = new FenceGeometry();
        geo.Type = FenceGeometry.Types.POLYGON;
        geo.Coordinates = new List<List<double[]>>();

        var feature = new Feature
        {
          Type = GeoJson.FeatureType.FEATURE,
          Geometry = geo,
          Properties = new Properties() { Name = fileName }
        };





        // Reduce vertices if too large...
        if (fence.Points.Count > VERTICES_LIMIT || tolerance > 0)
        {
          var toler = tolerance > 0 ? tolerance : cellSize;

          do
          {
            log.LogInformation($"{nameof(ConvertResult)}: Reducing fence verts. Tolerance: {toler}");
            fence.Compress(toler);
            toler = toler * 2;
          } while (fence.Points.Count < VERTICES_LIMIT);
        }

        var arrayCount = fence.Points.Count;
        vertsCount += arrayCount; // running total...

        var neeCoords = new XYZ[arrayCount];

        // Winding must be anticlockwise (righthand rule) which is worked out be calculating area...
        if (fence.IsWindingClockwise())
        {
          log.LogInformation($"{nameof(ConvertResult)}: Winding Clockwise.");
          // Reverse ordering...
          for (var i = fence.Points.Count - 1; i >= 0; i--)
            neeCoords[arrayCount - i - 1] = new XYZ(fence.Points[i].X, fence.Points[i].Y);
        }
        else
        {
          log.LogInformation($"{nameof(ConvertResult)}: Winding AntiClockwise.");

          for (var i = 0; i < fence.Points.Count; i++)
            neeCoords[i] = new XYZ(fence.Points[i].X, fence.Points[i].Y);
        }

        var coordConversionResult = ConvertCoordinates.NEEToLLH(csib, neeCoords);

        if (coordConversionResult.ErrorCode == RequestErrorStatus.OK)
        {
          var llhCoords = coordConversionResult.LLHCoordinates;

          //var polygon = new JArray();
          var fencePoints = new List<double[]>();

          for (var fencePointIdx = 0; fencePointIdx < llhCoords.Length; fencePointIdx++)
            AddPoint(llhCoords[fencePointIdx].X, llhCoords[fencePointIdx].Y, fencePoints);

          //coords.Add(polygon);
          geo.Coordinates.Add(fencePoints);

          //geo.Add(new JProperty(PROPERTY_COORDINATES, coords));

          //featuresObj.Add(new JProperty(PROPERTY_GEOMETRY, geo)); // Attach geometry obj to master...

          //var property = new JObject {new JProperty(PROPERTY_NAME, fileName)};

          //featuresObj.Add(new JProperty(PROPERTY_PROPERTIES, property));

          //featuresArray.Add(featuresObj);
          geoJson.Features.Add(feature);
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            $"Failed to convert design boundary data due to coordinate conversion failure. Error status: {coordConversionResult.ErrorCode}"));
        }
      }
      
      return new DesignBoundaryResult(geoJson);
    }

    private void AddPoint(double x, double y, List<double[]> fencePoints)
    {
      const int DECIMALS = -6;

      var point = new [] 
      { Math.Round(MathUtilities.RadiansToDegrees(x), DECIMALS),
        Math.Round(MathUtilities.RadiansToDegrees(y), DECIMALS)
      };

      fencePoints.Add(point);
    }
  }
}
