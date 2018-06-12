using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Common.Utilities;
using WGSPoint = VSS.Productivity3D.Common.Models.WGSPoint;

namespace VSS.Productivity3D.WebApiTests.RaptorServicesCommon.Models
{
  [TestClass]
  public class FilterTests
  {
    [TestMethod]
    public void CanCreateFilterTest()
    {
      var validator = new DataAnnotationsValidator();
      //everything filter
      List<WGSPoint> latLngs = new List<WGSPoint>
                                {
                                    WGSPoint.CreatePoint(35.13*ConversionConstants.DEGREES_TO_RADIANS, 179.2*ConversionConstants.DEGREES_TO_RADIANS),
                                    WGSPoint.CreatePoint(34.25*ConversionConstants.DEGREES_TO_RADIANS, 178.1*ConversionConstants.DEGREES_TO_RADIANS),
                                    WGSPoint.CreatePoint(36.4*ConversionConstants.DEGREES_TO_RADIANS, 177.34*ConversionConstants.DEGREES_TO_RADIANS)
                                };
      List<Point> gridPoints = new List<Point>
                                {
                                    Point.CreatePoint(12.4, 126.5),
                                    Point.CreatePoint(25.6, 99.2),
                                    Point.CreatePoint(15.2, 45.2),
                                    Point.CreatePoint(21.5, 89.3)
                                };
      DesignDescriptor desc = DesignDescriptor.CreateDesignDescriptor(1111, null, 0);
      List<MachineDetails> machines = new List<MachineDetails>
      {
        MachineDetails.Create(12345678, "Acme Compactor 1", false)
      };

      var filter = FilterResult.CreateFilter(null, null, null, new DateTime(2014, 1, 1), new DateTime(2014, 1, 31), 
          1111, new List<long> { 12345678, 87654321 }, true, true, ElevationType.First, latLngs, gridPoints,
          true, desc, 5.0, 100.0, 1.0, 2.0, "Acme Compactor", FilterLayerMethod.OffsetFromBench, desc, 0.3, 2,
          0.35, machines, new List<long> { 1, 2, 3, 4 }, true, GPSAccuracy.Medium, false, true, false, false, 
          desc, AutomaticsType.Manual, 100.0, 150.0, 3, 10);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(filter, out results));

      //null filter
      filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, true, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.IsTrue(validator.TryValidate(filter, out results), "null filter failed");

      //start station out of range
      filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, -10001, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "start station validate failed");

      //end station out of range
      filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 1000005,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "end station validate failed");

      //left offset out of range
      filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, 777, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "left offset validate failed");

      //right offset out of range
      filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, 987, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "right offset validate failed");

      //bench elevation out of range
      filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, 111111, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "bench elevation validate failed");

      //layer number out of range
      filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, -9876, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "layer number validate failed");

      //layer thickness out of range
      filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, 0.000001, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "layer thickness validate failed");

      //min temperature out of range
      filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, -1.0, null, null, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "min temperature validate failed");

      //max temperature out of range
      filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 1000.0, null, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "max temperature validate failed");

      //min pass count out of range
      filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, -1, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "min pass count validate failed");

      //max pass count out of range
      filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 9999);
      Assert.IsFalse(validator.TryValidate(filter, out results), "max pass count validate failed");
    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      DesignDescriptor desc = DesignDescriptor.CreateDesignDescriptor(1, null, 2.0);
      var filter = FilterResult.CreateFilter(null, null, null, new DateTime(2014, 1, 1), new DateTime(2014, 1, 31), null, null,
        null, null, null, null, null, null, desc, 100.0, 500.0, 1.0, 2.0, null, FilterLayerMethod.OffsetFromDesign, desc,
        null, 2, 0.5, null, null, null, null, null, null, null, null, desc, null, null, null, null, null);
      filter.Validate();
    }

    [TestMethod]
    public void ValidateFailInvalidDateRangeTest()
    {
      //start UTC > end UTC
      var filter = FilterResult.CreateFilter(null, null, null, new DateTime(2014, 1, 31), new DateTime(2014, 1, 1), null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());
    }

    [TestMethod]
    public void ValidateFailInvalidAlignmentFilterTest()
    {
      //missing alignment filter fields
      var filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, 100.0, 500.0, 2.0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
        null, null, null, null, null, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());
    }

    [TestMethod]
    public void ValidateFailInvalidLayerFilterTest()
    {
      //Invalid layer filter
      var filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, FilterLayerMethod.OffsetFromBench, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());
    }

    [TestMethod]
    public void ValidateFailInvalidLatLngPointsTest()
    {
      //too few points
      var filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, new List<WGSPoint>(),
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
        null, null, null, null, null, null, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());
    }

    [TestMethod]
    public void ValidateFailInvalidGridPointsTest()
    {
      //too few points
      var filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, new List<Point>(),
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
        null, null, null, null, null, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());

    }

    [TestMethod]
    public void ValidateFailInvalidBoundaryFilterTest()
    {
      //both LL and grid points specified
      List<WGSPoint> latLngs = new List<WGSPoint>
                            {
                                WGSPoint.CreatePoint(35.13*ConversionConstants.DEGREES_TO_RADIANS, 179.2*ConversionConstants.DEGREES_TO_RADIANS),
                                WGSPoint.CreatePoint(34.25*ConversionConstants.DEGREES_TO_RADIANS, 178.1*ConversionConstants.DEGREES_TO_RADIANS),
                                WGSPoint.CreatePoint(36.4*ConversionConstants.DEGREES_TO_RADIANS, 177.34*ConversionConstants.DEGREES_TO_RADIANS)
                            };
      List<Point> gridPoints = new List<Point>
                            {
                                Point.CreatePoint(12.4, 126.5),
                                Point.CreatePoint(25.6, 99.2),
                                Point.CreatePoint(15.2, 45.2),
                                Point.CreatePoint(21.5, 89.3)
                            };
      var filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, latLngs, gridPoints,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
        null, null, null, null, null, null, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());

    }

    [TestMethod]
    public void ValidateFailInvalidTemperatureRangeFilterTest()
    {
      //Min temperature > max temperature
      var filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, 150.0, 100.0, null, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());
    }

    [TestMethod]
    public void ValidateFailTemperatureOutOfRangeFilterTest()
    {
      //Max temperature too big
      var filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, 150.0, 500.0, null, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());
    }

    [TestMethod]
    public void ValidateFailMissingTemperatureRangeFilterTest()
    {
      //Missing max temperature
      var filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, 150.0, null, null, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());
    }

    [TestMethod]
    public void ValidateFailInvalidPassCountRangeFilterTest()
    {
      //Min pass count > max pass count
      var filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, 10, 1);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());
    }

    [TestMethod]
    public void ValidateFailPassCountOutOfRangeFilterTest()
    {
      //Max pass count too big
      var filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, 10, 1500);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());
    }

    [TestMethod]
    public void ValidateFailMissingPassCountRangeFilterTest()
    {
      //Missing max pass count
      var filter = FilterResult.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, 10, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());
    }

    [TestMethod]
    public void AsAtDateFilterCustom_Success()
    {
      var filter = Filter.CreateFilter(null, DateTime.UtcNow.AddDays(-1), null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, true);
      var filterResult = FilterResult.CreateFilter(filter, null, null, null, null, null, null);
      filterResult.Validate();
    }

    [TestMethod]
    public void AsAtDateFilterWithDateRangeType_Success()
    {
      //Need to use filter JSON as cannot set DateRangeType directly
      var filterJson = "{\"asAtDate\":true, \"dateRangeType\":0}";
      var filter = JsonConvert.DeserializeObject<Filter>(filterJson);
      var filterResult = FilterResult.CreateFilter(filter, null, null, null, null, null, null);
      filterResult.Validate();
    }

    [TestMethod]
    public void AsAtDateFilterFailure_MissingEndUtc()
    {
      var filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, true);
      var filterResult = FilterResult.CreateFilter(filter, null, null, null, null, null, null);
      Assert.ThrowsException<ServiceException>(() => filterResult.Validate());
    }

    [TestMethod]
    public void AsAtDateFilterFailure_MissingStartUtc()
    {
      var filter = Filter.CreateFilter(null, DateTime.UtcNow.AddDays(-1), null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, false);
      var filterResult = FilterResult.CreateFilter(filter, null, null, null, null, null, null);
      Assert.ThrowsException<ServiceException>(() => filterResult.Validate());
    }
  }
}
