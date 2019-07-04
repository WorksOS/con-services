using System.Collections.Generic;
using System.Linq;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.Utilities;
using Xunit;

namespace VSS.MasterData.Models.UnitTests
{
  public class GeofenceValidationTests
  {
    private const string _validBoundary = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965,172.595831670724 -43.5427038560109))";
    private const string _invalidBoundary_NotClosed = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965))";
    private const string _invalidBoundary_FewPoints = "POLYGON((172.595831670724 -43.5427038560109))";
    private const string _invalidBoundary_InvalidPoints = "POLYGON((-272.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965,172.595831670724 -43.5427038560109))";

    public GeofenceValidationTests()
    {
      new List<Point>
      {
        new Point(-43.5, 172.6),
        new Point(-43.5003, 172.6),
        new Point(-43.5003, 172.603),
        new Point(-43.5, 172.603)
      };
    }

    [Fact]
    public void ValidateGeofence_HappyPath()
    {
      var result = GeofenceValidation.ValidateWKT(_validBoundary);
      Assert.Equal(GeofenceValidation.ValidationOk, result);

      var points = GeofenceValidation.ParseGeometryDataPointLL(_validBoundary);
      var enumerable = points.ToList();
      Assert.Equal(4, enumerable.Count);

      var wkt = GeofenceValidation.GetWicketFromPoints(GeofenceValidation.MakingValidPoints(enumerable));
      Assert.Equal(_validBoundary, wkt);
    }

    [Fact]
    public void ValidateGeofence_EmptyBoundary()
    {
      var result = GeofenceValidation.ValidateWKT(string.Empty);
      Assert.Equal(GeofenceValidation.ValidationNoBoundary, result);
    }

    [Fact]
    public void ValidateGeofence_LessThan3Points()
    {
      var result = GeofenceValidation.ValidateWKT(_invalidBoundary_FewPoints);
      Assert.Equal(GeofenceValidation.ValidationLessThan3Points, result);
    }

    [Fact]
    public void ValidateGeofence_InvalidLongitude()
    {
      var result = GeofenceValidation.ValidateWKT(_invalidBoundary_InvalidPoints);
      Assert.Equal(GeofenceValidation.ValidationInvalidPointValue, result);
    }

    [Fact]
    public void ValidateGeofence_InvalidFormat()
    {
      var result = GeofenceValidation.ValidateWKT("nothing here");
      Assert.Equal(GeofenceValidation.ValidationInvalidFormat, result);
    }

    [Fact]
    public void ValidateGeofence_MakeValidWkt_FromWKT()
    { 
      var wkt = GeofenceValidation.MakeGoodWkt(_invalidBoundary_NotClosed);
      Assert.Equal(_validBoundary, wkt);
    }

    [Fact]
    public void ValidateGeofence_CloseTheOpenGeofence()
    {
      var result = GeofenceValidation.ValidateWKT(_invalidBoundary_NotClosed);
      Assert.Equal(GeofenceValidation.ValidationOk, result);

      var points = GeofenceValidation.ParseGeometryDataPointLL(_invalidBoundary_NotClosed);
      var enumerable = points.ToList();
      Assert.Equal(3, enumerable.Count);

      var wkt = GeofenceValidation.GetWicketFromPoints(GeofenceValidation.MakingValidPoints(enumerable));
      Assert.Equal(_validBoundary, wkt);
    }

    [Fact]
    public void ValidateGeofence_CalculateArea()
    {
      var result = GeofenceValidation.CalculateAreaSqMeters(_validBoundary);
      Assert.Equal(14585, (int)result);
    }
  }
}
