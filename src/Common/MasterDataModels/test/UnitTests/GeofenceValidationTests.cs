using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.Utilities;

namespace VSS.MasterData.Models.UnitTests
{
  [TestClass]
  public class GeofenceValidationTests
  {
    private static string _checkBoundaryString;
    private readonly string _validBoundary;
    private readonly string _invalidBoundary_NotClosed;
    private readonly string _invalidBoundary_FewPoints;
    private readonly string _invalidBoundary_InvalidPoints;

    private static string _customerUid;

    public GeofenceValidationTests()
    {
      _validBoundary = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965,172.595831670724 -43.5427038560109))";
      _invalidBoundary_NotClosed = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965))";
      _invalidBoundary_FewPoints = "POLYGON((172.595831670724 -43.5427038560109))";
      _invalidBoundary_InvalidPoints = "POLYGON((-272.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965,172.595831670724 -43.5427038560109))";
    }

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      new List<Point>()
      {
        new Point(-43.5, 172.6),
        new Point(-43.5003, 172.6),
        new Point(-43.5003, 172.603),
        new Point(-43.5, 172.603)
      };

      _checkBoundaryString = "POLYGON((172.6 -43.5,172.6 -43.5003,172.603 -43.5003,172.603 -43.5,172.6 -43.5))";
      
      _customerUid = Guid.NewGuid().ToString();
    }

    [TestMethod]
    public void ValidateGeofence_HappyPath()
    {
      var result = GeofenceValidation.ValidateWKT(_validBoundary);
      Assert.AreEqual(GeofenceValidation.ValidationOk, result, "Should be a valid wicket");

      var points = GeofenceValidation.ParseGeometryDataPointLL(_validBoundary);
      var enumerable = points.ToList();
      Assert.AreEqual(4, enumerable.Count, "invalid point count");

      var wkt = GeofenceValidation.GetWicketFromPoints(GeofenceValidation.MakingValidPoints(enumerable));
      Assert.AreEqual(_validBoundary, wkt, "Invalid conversion to wkt");
    }

    [TestMethod]
    public void ValidateGeofence_EmptyBoundary()
    {
      var result = GeofenceValidation.ValidateWKT(string.Empty);
      Assert.AreEqual(GeofenceValidation.ValidationNoBoundary, result, "Should be a empty wicket");
    }

    [TestMethod]
    public void ValidateGeofence_LessThan3Points()
    {
      var result = GeofenceValidation.ValidateWKT(_invalidBoundary_FewPoints);
      Assert.AreEqual(GeofenceValidation.ValidationLessThan3Points, result, "Should be < 3 points");
    }

    [TestMethod]
    public void ValidateGeofence_InvalidLongitude()
    {
      var result = GeofenceValidation.ValidateWKT(_invalidBoundary_InvalidPoints);
      Assert.AreEqual(GeofenceValidation.ValidationInvalidPointValue, result, "Latitude or longitude value is wrong.");
    }

    [TestMethod]
    public void ValidateGeofence_InvalidFormat()
    {
      var result = GeofenceValidation.ValidateWKT("nothing here");
      Assert.AreEqual(GeofenceValidation.ValidationInvalidFormat, result, "Should be a invalid format");
    }

    [TestMethod]
    public void ValidateGeofence_MakeValidWkt_FromWKT()
    { 
      var wkt = GeofenceValidation.MakeGoodWkt(_invalidBoundary_NotClosed);
      Assert.AreEqual(_validBoundary, wkt, "Invalid conversion to wkt");
    }

    [TestMethod]
    public void ValidateGeofence_CloseTheOpenGeofence()
    {
      var result = GeofenceValidation.ValidateWKT(_invalidBoundary_NotClosed);
      Assert.AreEqual(GeofenceValidation.ValidationOk, result, "Should be a valid wicket");

      var points = GeofenceValidation.ParseGeometryDataPointLL(_invalidBoundary_NotClosed);
      var enumerable = points.ToList();
      Assert.AreEqual(3, enumerable.Count, "invalid point count");

      var wkt = GeofenceValidation.GetWicketFromPoints(GeofenceValidation.MakingValidPoints(enumerable));
      Assert.AreEqual(_validBoundary, wkt, "Invalid conversion to wkt");
    }

    [TestMethod]
    public void ValidateGeofence_CalculateArea()
    {
      var result = GeofenceValidation.CalculateAreaSqMeters(_validBoundary);
      Assert.AreEqual(14585, (int)result, "Invalid Area");
    }
  }
}
