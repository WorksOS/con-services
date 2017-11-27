using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DesignProfiler.ComputeDesignBoundary.RPC;
using DesignProfilerDecls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SVOICStatistics;
using VLPDDecls;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using Point = VSS.Productivity3D.WebApi.Models.MapHandling.Point;

namespace VSS.Productivity3D.WebApiTests.MapHandling
{
  [TestClass]
  public class BoundingBoxServiceTests
  {
    public IServiceProvider serviceProvider;

    [TestInitialize]
    public void InitTest()
    {
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();

      var serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);

      serviceProvider = serviceCollection.BuildServiceProvider();
    }


    [TestMethod]
    [DataRow(4096, 4096, 4096, 4096, 0.63136, -2.00751, 0.63144, -2.00741)]//tile larger than bbox
    [DataRow(2048, 2048, 2656, 2656, 0.63137, -2.00749, 0.63142, -2.00742)]//square tile smaller than bbox 
    [DataRow(2048, 1024, 5302, 2656, 0.63137, -2.00752, 0.63142, -2.00739)]//rectangular tile smaller than bbox
    public void ShouldExpandBoundingBoxToFit(int tileWidth, int tileHeight, int expectedWidth, int expectedHeight,
      double expectedMinLat, double expectedMinLng, double expectedMaxLat, double expectedMaxLng)
    {
      var minLat = 0.63137;//36.175°
      var minLng = -2.00748;//-115.020°
      var maxLat = 0.63142;//36.178°
      var maxLng = -2.00744;//-115.018°
      MapBoundingBox bbox = new MapBoundingBox
      {
        minLat = minLat,
        minLng = minLng,
        maxLat = maxLat,
        maxLng = maxLng
      };

      var raptorClient = new Mock<IASNodeClient>();

      int mapWidth, mapHeight;
      var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>(), raptorClient.Object);
      //numTiles = 1048576 for Z10
      service.AdjustBoundingBoxToFit(bbox, 1048576, tileWidth, tileHeight, out mapWidth, out mapHeight);
      Assert.AreEqual(expectedWidth, mapWidth);
      Assert.AreEqual(expectedHeight, mapHeight);
      Assert.AreEqual(expectedMinLat, bbox.minLat, 0.00001);
      Assert.AreEqual(expectedMinLng, bbox.minLng, 0.00001);
      Assert.AreEqual(expectedMaxLat, bbox.maxLat, 0.00001);
      Assert.AreEqual(expectedMaxLng, bbox.maxLng, 0.00001);
    }

    [TestMethod]
    public void GetBoundingBoxPolygonFilter()
    {
      List<WGSPoint> polygonPoints = new List<WGSPoint>
      {
        WGSPoint.CreatePoint(10, 20),
        WGSPoint.CreatePoint(15, 20),
        WGSPoint.CreatePoint(13, 15),
        WGSPoint.CreatePoint(25, 30),
        WGSPoint.CreatePoint(27, 27)
      };

      Filter filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, 
        polygonPoints, null, null, null, null, null, null, null, null, null, null, null, null, null, 
        null, null, null, null, null, null, null, null);

      var raptorClient = new Mock<IASNodeClient>();

      var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>(), raptorClient.Object);
      var bbox = service.GetBoundingBox(project, filter, false, null, null);
      Assert.AreEqual(polygonPoints.Min(p => p.Lat), bbox.minLat);
      Assert.AreEqual(polygonPoints.Min(p => p.Lon), bbox.minLng);
      Assert.AreEqual(polygonPoints.Max(p => p.Lat), bbox.maxLat);
      Assert.AreEqual(polygonPoints.Max(p => p.Lon), bbox.maxLng);
    }

    [TestMethod]
    public void GetBoundingBoxPolygonAndDesignBoundaryFilter()
    {  
      //design boundary points: -115.018,36.208 -115.025,36.214 -115.123,36.17 -115.018,36.208
      DesignDescriptor design = DesignDescriptor.CreateDesignDescriptor(-1, null, 0);
      List<WGSPoint> polygonPoints = new List<WGSPoint>
      {
        WGSPoint.CreatePoint(35.98.LatDegreesToRadians(), -115.11.LonDegreesToRadians()),
        WGSPoint.CreatePoint(36.15.LatDegreesToRadians(), -115.74.LonDegreesToRadians()),
        WGSPoint.CreatePoint(36.10.LatDegreesToRadians(), -115.39.LonDegreesToRadians())
      };
      Filter filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, 
        polygonPoints, null, null, null, null, null, null, null, null, null, design, null, null, null, 
        null, null, null, null, null, null, null, null);

      var raptorClient = new Mock<IASNodeClient>();

      TDesignProfilerRequestResult designProfilerResult = TDesignProfilerRequestResult.dppiOK;
      var ms = new MemoryStream();
      using (var writer = new StreamWriter(ms))
      {
        writer.Write(designGeoJson);
        writer.Flush();

        raptorClient
          .Setup(x => x.GetDesignBoundary(It.IsAny<TDesignProfilerServiceRPCVerb_CalculateDesignBoundary_Args>(),
            out ms, out designProfilerResult))
          .Returns(true);

        var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>(), raptorClient.Object);
        var bbox = service.GetBoundingBox(project, filter, false, null, null);
        //bbox is a mixture of polgon and design boundary (see GeoJson)
        Assert.AreEqual(-115.74.LonDegreesToRadians(), bbox.minLng);
        Assert.AreEqual(35.98.LatDegreesToRadians(), bbox.minLat);
        Assert.AreEqual(-115.018.LonDegreesToRadians(), bbox.maxLng);
        Assert.AreEqual(36.214.LatDegreesToRadians(), bbox.maxLat);
      }
    }

    [TestMethod]
    public void GetBoundingBoxDesignBoundaryFilter()
    {
      DesignDescriptor design = DesignDescriptor.CreateDesignDescriptor(-1, null, 0);
      Filter filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, design, null, null, null, null, null, null,
        null, null, null, null, null);

      var raptorClient = new Mock<IASNodeClient>();

      TDesignProfilerRequestResult designProfilerResult = TDesignProfilerRequestResult.dppiOK;
      var ms = new MemoryStream();
      using (var writer = new StreamWriter(ms))
      {
        writer.Write(designGeoJson);
        writer.Flush();

        raptorClient
          .Setup(x => x.GetDesignBoundary(It.IsAny<TDesignProfilerServiceRPCVerb_CalculateDesignBoundary_Args>(),
            out ms, out designProfilerResult))
          .Returns(true);

        var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>(), raptorClient.Object);
        var bbox = service.GetBoundingBox(project, filter, false, null, null);
        //Values are from GeoJson below
        Assert.AreEqual(-115.123.LonDegreesToRadians(), bbox.minLng);
        Assert.AreEqual(36.175.LatDegreesToRadians(), bbox.minLat);
        Assert.AreEqual(-115.018.LonDegreesToRadians(), bbox.maxLng);
        Assert.AreEqual(36.214.LatDegreesToRadians(), bbox.maxLat);
      }
    }


    [TestMethod]
    public void GetBoundingBoxSummaryVolumesFilter()
    {
      List<WGSPoint> polygonPoints1 = new List<WGSPoint>
      {
        WGSPoint.CreatePoint(10, 20),
        WGSPoint.CreatePoint(15, 20),
        WGSPoint.CreatePoint(13, 15),
        WGSPoint.CreatePoint(25, 30),
        WGSPoint.CreatePoint(27, 27)
      };

      Filter baseFilter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null,
        polygonPoints1, null, null, null, null, null, null, null, null, null, null, null, null, null, 
        null, null, null, null, null, null, null, null);

      List<WGSPoint> polygonPoints2 = new List<WGSPoint>
      {
        WGSPoint.CreatePoint(30, 20),
        WGSPoint.CreatePoint(25, 25),
        WGSPoint.CreatePoint(50, 35),
        WGSPoint.CreatePoint(25, 15),
        WGSPoint.CreatePoint(32, 16)
      };

      Filter topFilter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, 
        polygonPoints2, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
        null, null, null, null, null, null, null);

      var raptorClient = new Mock<IASNodeClient>();

      var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>(), raptorClient.Object);
      var bbox = service.GetBoundingBox(project, null, false, baseFilter, topFilter);

      var expectedMinLat = Math.Min(polygonPoints1.Min(p => p.Lat), polygonPoints2.Min(p => p.Lat));
      var expectedMinLng = Math.Min(polygonPoints1.Min(p => p.Lon), polygonPoints2.Min(p => p.Lon));
      var expectedMaxLat = Math.Max(polygonPoints1.Max(p => p.Lat), polygonPoints2.Max(p => p.Lat));
      var expectedMaxLng = Math.Max(polygonPoints1.Max(p => p.Lon), polygonPoints2.Max(p => p.Lon));
      Assert.AreEqual(expectedMinLat, bbox.minLat);
      Assert.AreEqual(expectedMinLng, bbox.minLng);
      Assert.AreEqual(expectedMaxLat, bbox.maxLat);
      Assert.AreEqual(expectedMaxLng, bbox.maxLng);
    }

    [TestMethod]
    public void GetBoundingBoxValidProductionDataExtents()
    {
      //Production data inside project boundary is valid
      var prodDataMinLat = projMinLatRadians + 0.01;
      var prodDataMinLng = projMinLngRadians + 0.01;
      var prodDataMaxLat = projMaxLatRadians - 0.01;
      var prodDataMaxLng = projMaxLngRadians - 0.01;

      var raptorClient = new Mock<IASNodeClient>();

      TICDataModelStatistics statistics = new TICDataModelStatistics();
      raptorClient
        .Setup(x => x.GetDataModelStatistics(project.projectId, It.IsAny<TSurveyedSurfaceID[]>(), out statistics))
        .Returns(true);

      TCoordPointList pointList = new TCoordPointList
      {
        ReturnCode = TCoordReturnCode.nercNoError,
        Points = new TCoordContainer
        {
          Coords = new TCoordPoint[]
          {
            new TCoordPoint {X = prodDataMinLng, Y = prodDataMinLat},
            new TCoordPoint {X = prodDataMaxLng, Y = prodDataMaxLat}
          }
        }
      };
      raptorClient
        .Setup(x => x.GetGridCoordinates(project.projectId, It.IsAny<TWGS84FenceContainer>(),
          TCoordConversionType.ctNEEtoLLH, out pointList))
        .Returns(TCoordReturnCode.nercNoError);

      var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>(), raptorClient.Object);
      var bbox = service.GetBoundingBox(project, null, true, null, null);
      Assert.AreEqual(prodDataMinLat, bbox.minLat);
      Assert.AreEqual(prodDataMaxLat, bbox.maxLat);
      Assert.AreEqual(prodDataMinLng, bbox.minLng);
      Assert.AreEqual(prodDataMaxLng, bbox.maxLng);
    }

    [TestMethod]
    public void GetBoundingBoxInvalidProductionDataExtents()
    {
      //Production data outside project boundary is invalid
      var prodDataMinLat = projMinLatRadians - 0.2;
      var prodDataMinLng = projMinLngRadians - 0.2;
      var prodDataMaxLat = projMaxLatRadians + 0.2;
      var prodDataMaxLng = projMaxLngRadians + 0.2;

      var raptorClient = new Mock<IASNodeClient>();

      TICDataModelStatistics statistics = new TICDataModelStatistics();
      raptorClient
        .Setup(x => x.GetDataModelStatistics(project.projectId, It.IsAny<TSurveyedSurfaceID[]>(), out statistics))
        .Returns(true);

      TCoordPointList pointList = new TCoordPointList
      {
        ReturnCode = TCoordReturnCode.nercNoError,
        Points = new TCoordContainer
        {
          Coords = new TCoordPoint[]
          {
            new TCoordPoint {X = prodDataMinLng, Y = prodDataMinLat},
            new TCoordPoint {X = prodDataMaxLng, Y = prodDataMaxLat}
          }
        }
      };
      raptorClient
        .Setup(x => x.GetGridCoordinates(project.projectId, It.IsAny<TWGS84FenceContainer>(),
          TCoordConversionType.ctNEEtoLLH, out pointList))
        .Returns(TCoordReturnCode.nercNoError);

      var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>(), raptorClient.Object);
      var bbox = service.GetBoundingBox(project, null, true, null, null);
      Assert.AreEqual(projMinLatRadians, bbox.minLat);
      Assert.AreEqual(projMaxLatRadians, bbox.maxLat);
      Assert.AreEqual(projMinLngRadians, bbox.minLng);
      Assert.AreEqual(projMaxLngRadians, bbox.maxLng);
    }

    [TestMethod]
    public void GetBoundingBoxProjectExtentsNoMode()
    {
      var raptorClient = new Mock<IASNodeClient>();

      TICDataModelStatistics statistics = new TICDataModelStatistics();
      raptorClient
        .Setup(x => x.GetDataModelStatistics(project.projectId, It.IsAny<TSurveyedSurfaceID[]>(), out statistics))
        .Returns(false);

      var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>(), raptorClient.Object);
      var bbox = service.GetBoundingBox(project, null, false, null, null);
      Assert.AreEqual(projMinLatRadians, bbox.minLat);
      Assert.AreEqual(projMaxLatRadians, bbox.maxLat);
      Assert.AreEqual(projMinLngRadians, bbox.minLng);
      Assert.AreEqual(projMaxLngRadians, bbox.maxLng);
    }

    [TestMethod]
    public void GetBoundingBoxProjectExtentsWithMode()
    {
      var raptorClient = new Mock<IASNodeClient>();

      TICDataModelStatistics statistics = new TICDataModelStatistics();
      raptorClient
        .Setup(x => x.GetDataModelStatistics(project.projectId, It.IsAny<TSurveyedSurfaceID[]>(), out statistics))
        .Returns(false);

      var service = new BoundingBoxService(serviceProvider.GetRequiredService<ILoggerFactory>(), raptorClient.Object);
      var bbox = service.GetBoundingBox(project, null, true, null, null);
      Assert.AreEqual(projMinLatRadians, bbox.minLat);
      Assert.AreEqual(projMaxLatRadians, bbox.maxLat);
      Assert.AreEqual(projMinLngRadians, bbox.minLng);
      Assert.AreEqual(projMaxLngRadians, bbox.maxLng);
    }


    private static List<Point> projectPoints = new List<Point>
    {
      new Point {y = 36.208, x = -115.018},
      new Point {y = 36.145, x = -115.665},
      new Point {y = 36.877, x = -115.109},
      new Point {y = 36.103, x = -115.687}
    };

    private static ProjectDescriptor project = new ProjectDescriptor
    {
      projectUid = Guid.NewGuid().ToString(),
      projectId = 1234,
      projectGeofenceWKT = TestUtils.GetWicketFromPoints(projectPoints)
    };

    private static double projMinLatRadians = projectPoints.Min(p => p.Latitude).LatDegreesToRadians();
    private static double projMinLngRadians = projectPoints.Min(p => p.Longitude).LonDegreesToRadians();
    private static double projMaxLatRadians = projectPoints.Max(p => p.Latitude).LatDegreesToRadians();
    private static double projMaxLngRadians = projectPoints.Max(p => p.Longitude).LonDegreesToRadians();

    private string designGeoJson = @"
      {
        ""type"": ""FeatureCollection"",
        ""features"": [
          {
            ""type"": ""Feature"",
            ""geometry"": {
              ""type"": ""Polygon"",
              ""coordinates"": [
                [
                  [
                    -115.018,
                    36.208
                  ],
                  [
                    -115.025,
                    36.214
                  ],
                  [
                    -115.123,
                    36.175
                  ],
				          [
                    -115.018,
                    36.208
                  ]
                ]
              ]
            },
            ""properties"": {
              ""name"": ""Acme Design.TTM""
            }
          }
        ]
		  }
    ";

  }
}
