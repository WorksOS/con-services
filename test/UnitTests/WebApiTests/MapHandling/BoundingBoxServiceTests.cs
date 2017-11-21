using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DesignProfiler.ComputeDesignBoundary.RPC;
using DesignProfilerDecls;
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

      var logger = new Mock<ILoggerFactory>();
      var raptorClient = new Mock<IASNodeClient>();

      var service = new BoundingBoxService(logger.Object, raptorClient.Object);
      var bbox = service.GetBoundingBox(project, filter, null, null, null);
      Assert.AreEqual(polygonPoints.Min(p => p.Lat), bbox.minLat);
      Assert.AreEqual(polygonPoints.Min(p => p.Lon), bbox.minLng);
      Assert.AreEqual(polygonPoints.Max(p => p.Lat), bbox.maxLat);
      Assert.AreEqual(polygonPoints.Max(p => p.Lon), bbox.maxLng);
    }

    [TestMethod]
    public void GetBoundingBoxDesignBoundaryFilter()
    {
      DesignDescriptor design = DesignDescriptor.CreateDesignDescriptor(-1, null, 0);
      List<WGSPoint> polygonPoints = new List<WGSPoint>
      {
        WGSPoint.CreatePoint(36.003, -115.145),
        WGSPoint.CreatePoint(36.185, -115.765),
        WGSPoint.CreatePoint(36.111, -115.445)
      };
      Filter filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, 
        polygonPoints, null, null, null, null, null, null, null, null, null, design, null, null, null, 
        null, null, null, null, null, null, null, null);

      var logger = new Mock<ILoggerFactory>();
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

        var service = new BoundingBoxService(logger.Object, raptorClient.Object);
        var bbox = service.GetBoundingBox(project, filter, null, null, null);
        //bbox is a mixture of polgon and design boundary (see GeoJson)
        Assert.AreEqual(-115.765, bbox.minLng);
        Assert.AreEqual(36.003, bbox.minLat);
        Assert.AreEqual(-115.018, bbox.maxLng);
        Assert.AreEqual(36.214, bbox.maxLat);
      }
    }

    [TestMethod]
    public void GetBoundingBoxPolygonAndDesignBoundaryFilter()
    {
      DesignDescriptor design = DesignDescriptor.CreateDesignDescriptor(-1, null, 0);
      Filter filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, design, null, null, null, null, null, null,
        null, null, null, null, null);

      var logger = new Mock<ILoggerFactory>();
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

        var service = new BoundingBoxService(logger.Object, raptorClient.Object);
        var bbox = service.GetBoundingBox(project, filter, null, null, null);
        Assert.AreEqual(-115.123, bbox.minLng);
        Assert.AreEqual(36.175, bbox.minLat);
        Assert.AreEqual(-115.018, bbox.maxLng);
        Assert.AreEqual(36.214, bbox.maxLat);
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

      var logger = new Mock<ILoggerFactory>();
      var raptorClient = new Mock<IASNodeClient>();

      var service = new BoundingBoxService(logger.Object, raptorClient.Object);
      var bbox = service.GetBoundingBox(project, null, null, baseFilter, topFilter);

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
      var prodDataMinLat = projMinLat + 1;
      var prodDataMinLng = projMinLng + 1;
      var prodDataMaxLat = projMaxLat - 1;
      var prodDataMaxLng = projMaxLng - 1;

      var logger = new Mock<ILoggerFactory>();
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
            new TCoordPoint {X = prodDataMinLng.LonDegreesToRadians(), Y = prodDataMinLat.LatDegreesToRadians()},
            new TCoordPoint {X = prodDataMaxLng.LonDegreesToRadians(), Y = prodDataMaxLat.LatDegreesToRadians()}
          }
        }
      };
      raptorClient
        .Setup(x => x.GetGridCoordinates(project.projectId, It.IsAny<TWGS84FenceContainer>(),
          TCoordConversionType.ctNEEtoLLH, out pointList))
        .Returns(TCoordReturnCode.nercNoError);

      var service = new BoundingBoxService(logger.Object, raptorClient.Object);
      var bbox = service.GetBoundingBox(project, null, DisplayMode.CCVSummary, null, null);
      Assert.AreEqual(prodDataMinLat, bbox.minLat, 0.001); //0.001 To handle radians/degrees conversion
      Assert.AreEqual(prodDataMaxLat, bbox.maxLat, 0.001);
      Assert.AreEqual(prodDataMinLng, bbox.minLng, 0.001);
      Assert.AreEqual(prodDataMaxLng, bbox.maxLng, 0.001);
    }

    [TestMethod]
    public void GetBoundingBoxInvalidProductionDataExtents()
    {
      //Production data outside project boundary is invalid
      var prodDataMinLat = projMinLat - 10;
      var prodDataMinLng = projMinLng - 10;
      var prodDataMaxLat = projMaxLat + 10;
      var prodDataMaxLng = projMaxLng + 10;

      var logger = new Mock<ILoggerFactory>();
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
            new TCoordPoint {X = prodDataMinLng.LonDegreesToRadians(), Y = prodDataMinLat.LatDegreesToRadians()},
            new TCoordPoint {X = prodDataMaxLng.LonDegreesToRadians(), Y = prodDataMaxLat.LatDegreesToRadians()}
          }
        }
      };
      raptorClient
        .Setup(x => x.GetGridCoordinates(project.projectId, It.IsAny<TWGS84FenceContainer>(),
          TCoordConversionType.ctNEEtoLLH, out pointList))
        .Returns(TCoordReturnCode.nercNoError);

      var service = new BoundingBoxService(logger.Object, raptorClient.Object);
      var bbox = service.GetBoundingBox(project, null, DisplayMode.CCVSummary, null, null);
      Assert.AreEqual(projMinLat, bbox.minLat);
      Assert.AreEqual(projMaxLat, bbox.maxLat);
      Assert.AreEqual(projMinLng, bbox.minLng);
      Assert.AreEqual(projMaxLng, bbox.maxLng);
    }

    [TestMethod]
    public void GetBoundingBoxProjectExtentsNoMode()
    {
      var logger = new Mock<ILoggerFactory>();
      var raptorClient = new Mock<IASNodeClient>();

      TICDataModelStatistics statistics = new TICDataModelStatistics();
      raptorClient
        .Setup(x => x.GetDataModelStatistics(project.projectId, It.IsAny<TSurveyedSurfaceID[]>(), out statistics))
        .Returns(false);

      var service = new BoundingBoxService(logger.Object, raptorClient.Object);
      var bbox = service.GetBoundingBox(project, null, null, null, null);
      Assert.AreEqual(projMinLat, bbox.minLat);
      Assert.AreEqual(projMaxLat, bbox.maxLat);
      Assert.AreEqual(projMinLng, bbox.minLng);
      Assert.AreEqual(projMaxLng, bbox.maxLng);
    }

    [TestMethod]
    public void GetBoundingBoxProjectExtentsWithMode()
    {
      var logger = new Mock<ILoggerFactory>();
      var raptorClient = new Mock<IASNodeClient>();

      TICDataModelStatistics statistics = new TICDataModelStatistics();
      raptorClient
        .Setup(x => x.GetDataModelStatistics(project.projectId, It.IsAny<TSurveyedSurfaceID[]>(), out statistics))
        .Returns(false);

      var service = new BoundingBoxService(logger.Object, raptorClient.Object);
      var bbox = service.GetBoundingBox(project, null, DisplayMode.CCVSummary, null, null);
      Assert.AreEqual(projMinLat, bbox.minLat);
      Assert.AreEqual(projMaxLat, bbox.maxLat);
      Assert.AreEqual(projMinLng, bbox.minLng);
      Assert.AreEqual(projMaxLng, bbox.maxLng);
    }


    private static List<Point> projectPoints = new List<Point>
    {
      new Point {y = 80.25, x = 12.67},
      new Point {y = 90.85, x = 13.26},
      new Point {y = 85.79, x = 20.44},
      new Point {y = 82.15, x = 19.98}
    };

    private static ProjectDescriptor project = new ProjectDescriptor
    {
      projectId = 1234,
      projectGeofenceWKT = TestUtils.GetWicketFromPoints(projectPoints)
    };

    private static double projMinLat = projectPoints.Min(p => p.Latitude);
    private static double projMinLng = projectPoints.Min(p => p.Longitude);
    private static double projMaxLat = projectPoints.Max(p => p.Latitude);
    private static double projMaxLng = projectPoints.Max(p => p.Longitude);

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
