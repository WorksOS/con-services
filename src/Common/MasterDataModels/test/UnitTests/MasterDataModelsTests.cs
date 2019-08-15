using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.UnitTests.ResultsHandling;
using VSS.MasterData.Repositories.DBModels;
using Xunit;
using Filter = VSS.Productivity3D.Filter.Abstractions.Models.Filter;

namespace VSS.MasterData.Models.UnitTests
{
  public class MasterDataModelsTests : BaseTest
  {
    private DateTime _utcNow;
    private List<MachineDetails> _machines;
    private List<WGSPoint> _polygonLL;
    private string _boundaryUid = Guid.NewGuid().ToString();
    private string _boundaryName = "myBoundaryName";
    private GeofenceType _boundaryType = GeofenceType.Filter;
    private IServiceExceptionHandler _serviceExceptionHandler;
    private DataAnnotationsValidator _validator;

    /// <summary>
    /// Initializes the test.
    /// </summary>
    public MasterDataModelsTests()
    {
      base.InitTest();

      _utcNow = DateTime.UtcNow;
      _machines = new List<MachineDetails>
                  {
        new MachineDetails(1137642418461469, "VOLVO G946B", false)
      };

      _polygonLL = new List<WGSPoint>
                   {
        new WGSPoint(0.612770247622, -1.860592122242),
        new WGSPoint(0.61341601944523627132, -1.860592122242),
        new WGSPoint(0.612770247622, -1.86120298748019675)
      };

      _serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      _validator = new DataAnnotationsValidator();
    }


    [Fact]
    public void CanCompareMachineDetailsEqual()
    {
      var machine1 = new MachineDetails(1, "test", true);
      var machine2 = new MachineDetails(1, "test", true);
      Assert.True(machine1 == machine2);
    }


    [Fact]
    public void CanCompareMachineDetailsNonequal()
    {
      var machine1 = new MachineDetails(1, "test", true);
      var machine2 = new MachineDetails(1, "test1", false);
      Assert.True(machine1 != machine2);
    }


    [Fact]
    public void CanCompareWGSPointEqual()
    {
      var point1 = new WGSPoint(10, 10);
      var point2 = new WGSPoint(10, 10);
      Assert.True(point1 == point2);
    }

    [Fact]
    public void CanCompareWGSPointNonequal()
    {
      var point1 = new WGSPoint(10, 10);
      var point2 = new WGSPoint(11, 10);
      Assert.True(point1 != point2);
    }

    [Fact]
    public void CanCompareFilters()
    {
      var designUid = Guid.NewGuid().ToString();

      var filter1 = new Filter(DateTime.MinValue, DateTime.MaxValue, designUid, "designName",
        new List<MachineDetails> { new MachineDetails(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash1 = filter1.GetHashCode();
      var filter2 = new Filter(DateTime.MinValue, DateTime.MaxValue, designUid, "designName",
        new List<MachineDetails> { new MachineDetails(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash2 = filter2.GetHashCode();
      Assert.True(filter1 == filter2);
      Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void CanCompareFiltersMachineDirection()
    {
      var filter1 = new Filter(DateTime.MinValue, DateTime.MaxValue, Guid.NewGuid().ToString(), "designName",
        new List<MachineDetails> { new MachineDetails(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash1 = filter1.GetHashCode();
      var filter2 = new Filter(DateTime.MinValue, DateTime.MaxValue, Guid.NewGuid().ToString(), "designName",
        new List<MachineDetails> { new MachineDetails(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), null, -1, null, "123");
      var hash2 = filter2.GetHashCode();
      Assert.True(filter1 != filter2);
      Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void CanCompareFiltersMachineDirectionTrue()
    {
      var filter1 = new Filter(DateTime.MinValue, DateTime.MaxValue, Guid.NewGuid().ToString(), "designName",
        new List<MachineDetails> { new MachineDetails(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), true, -1, null, "123");
      var hash1 = filter1.GetHashCode();
      var filter2 = new Filter(DateTime.MinValue, DateTime.MaxValue, Guid.NewGuid().ToString(), "designName",
        new List<MachineDetails> { new MachineDetails(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), null, -1, null, "123");
      var hash2 = filter2.GetHashCode();
      Assert.True(filter1 != filter2);
      Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void CanCompareFiltersMachineDirectionNulls()
    {
      var designUid = Guid.NewGuid().ToString();
      var filter1 = new Filter(DateTime.MinValue, DateTime.MaxValue, designUid, "designName",
        new List<MachineDetails> { new MachineDetails(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), null, -1, null, "123");
      var hash1 = filter1.GetHashCode();
      var filter2 = new Filter(DateTime.MinValue, DateTime.MaxValue, designUid, "designName",
        new List<MachineDetails> { new MachineDetails(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), null, -1, null, "123");
      var hash2 = filter2.GetHashCode();
      Assert.True(filter1 == filter2);
      Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void CanCompareFiltersNonEqual()
    {
      var filter1 = new Filter(DateTime.MinValue, DateTime.MaxValue, Guid.NewGuid().ToString(), "designName",
        new List<MachineDetails> { new MachineDetails(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash1 = filter1.GetHashCode();
      var filter2 = new Filter(DateTime.MinValue, DateTime.MaxValue, Guid.NewGuid().ToString(), "designName",
        new List<MachineDetails> { new MachineDetails(1, "test2", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash2 = filter2.GetHashCode();
      Assert.True(filter1 != filter2);
      Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void CanCompareFiltersEqualWithNulls()
    {
      var designUid = Guid.NewGuid().ToString();

      var filter1 = new Filter(DateTime.MinValue, null, designUid, "designName",
        new List<MachineDetails> { new MachineDetails(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash1 = filter1.GetHashCode();
      var filter2 = new Filter(DateTime.MinValue, null, designUid, "designName",
        new List<MachineDetails> { new MachineDetails(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash2 = filter2.GetHashCode();
      Assert.True(filter1 == filter2);
      Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void CanCreateFilterTest()
    {
      // Empty filter...
      var filter = new Filter();
      Assert.True(_validator.TryValidate(filter, out _));

      // Complete filter...
      filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, _polygonLL, true, 1);
      Assert.True(_validator.TryValidate(filter, out _));
    }

    [Fact]
    public void ValidateSuccessTest()
    {
      // All properties' values are valid...
      var filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, _polygonLL, true, 1, _boundaryUid, _boundaryName);
      filter.Validate(_serviceExceptionHandler);

      // Date range is not provided...
      filter = new Filter(null, null, new Guid().ToString(), "designName", _machines, 123, ElevationType.Lowest, true, null,
        true, 1);
      filter.Validate(_serviceExceptionHandler);

      // Design UID is not provided...
      filter = new Filter(_utcNow, _utcNow.AddDays(10), null, "designName", _machines, 123, ElevationType.Lowest, true, null,
        true, 1);
      filter.Validate(_serviceExceptionHandler);

      // Machines' list is not provided...
      filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", null, 123, ElevationType.Lowest,
        true, null, true, 1);
      filter.Validate(_serviceExceptionHandler);

      // Machine's design name is not provided...
      filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, null,
        ElevationType.Lowest, true, null, true, 1);
      filter.Validate(_serviceExceptionHandler);

      // Elevation type is not provided...
      filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, null, true, 1);
      filter.Validate(_serviceExceptionHandler);

      // Vibration state is not provided...
      filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, null, null, true, 1);
      filter.Validate(_serviceExceptionHandler);

      // Forward direction flag is not provided...
      filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, null, null, 1);
      filter.Validate(_serviceExceptionHandler);

      // Layer number is not provided...
      filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, null, true, null);
      filter.Validate(_serviceExceptionHandler);
    }

    [Fact]
    public void ValidateFailureTest()
    {
      // Start UTC date is not provided...
      var filter = new Filter(null, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, _polygonLL, true, 1);
      Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));

      // End UTC date is not provided...
      filter = new Filter(_utcNow, null, new Guid().ToString(), "designName", _machines, 123, ElevationType.Lowest, true,
        _polygonLL, true, 1);
      Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));

      // Invalid design UID's Guid is provided...
      filter = new Filter(_utcNow, _utcNow.AddDays(10), INVALID_GUID, "designName", _machines, 123, ElevationType.Lowest,
        true, _polygonLL, true, 1);
      Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));

      // The provided polygon's boundary has less than 3 points...
      _polygonLL.RemoveAt(_polygonLL.Count - 1);
      filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, _polygonLL, true, 1);
      Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));
    }

    [Fact]
    public void ValidateJsonStringTest()
    {
      var filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, _polygonLL, true, 1, _boundaryUid, _boundaryName);
      var jsonString = JsonConvert.SerializeObject(filter);

      Assert.True(jsonString != string.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      filter.Validate(_serviceExceptionHandler);
    }

    [Fact]
    public void HydrateJsonStringWithPolygonTest()
    {
      var filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, null, true, 1);

      // now add the polygon
      var boundaryUid = Guid.NewGuid().ToString();
      var boundaryName = "myBoundaryName";
      var newBoundaryPoints = new List<VSS.MasterData.Models.Models.WGSPoint>
      {
        new WGSPoint(1, 170),
        new WGSPoint(6, 160),
        new WGSPoint(8, 150),
        new WGSPoint(1, 170)
      };
      var boundaryType = GeofenceType.Filter;

      filter.AddBoundary(boundaryUid, boundaryName, newBoundaryPoints, boundaryType);
      var jsonString = JsonConvert.SerializeObject(filter);
      Assert.True(jsonString != string.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      filter.Validate(_serviceExceptionHandler);
      Assert.Equal(boundaryName, filter.PolygonName);
      Assert.Equal(boundaryUid, filter.PolygonUid);
      Assert.Equal(4, filter.PolygonLL.Count);
      Assert.Equal(newBoundaryPoints[2].Lat, filter.PolygonLL[2].Lat);
      Assert.Equal(boundaryType, filter.PolygonType);
    }

    [Fact]
    public void HydrateJsonStringWithPolygonTest_Update()
    {
      var filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, _polygonLL, true, 1, _boundaryUid, _boundaryName, polygonType: _boundaryType);
      var jsonString = JsonConvert.SerializeObject(filter);

      Assert.True(jsonString != string.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      filter.Validate(_serviceExceptionHandler);
      Assert.Equal(_boundaryName, filter.PolygonName);
      Assert.Equal(_boundaryUid, filter.PolygonUid);
      Assert.Equal(3, filter.PolygonLL.Count);
      Assert.Equal(_polygonLL[1].Lat, filter.PolygonLL[1].Lat);
      Assert.Equal(_boundaryType, filter.PolygonType);


      // now update the polygon
      var boundaryUid = Guid.NewGuid().ToString();
      var boundaryName = "new myBoundaryName";
      var newBoundaryPoints = new List<VSS.MasterData.Models.Models.WGSPoint>
      {
        new WGSPoint(1, 170),
        new WGSPoint(6, 160),
        new WGSPoint(8, 150),
        new WGSPoint(1, 170)
      };
      var boundaryType = GeofenceType.Generic;

      filter.AddBoundary(boundaryUid, boundaryName, newBoundaryPoints, boundaryType);
      jsonString = JsonConvert.SerializeObject(filter);
      Assert.True(jsonString != string.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      filter.Validate(_serviceExceptionHandler);
      Assert.Equal(boundaryName, filter.PolygonName);
      Assert.Equal(boundaryUid, filter.PolygonUid);
      Assert.Equal(4, filter.PolygonLL.Count);
      Assert.Equal(newBoundaryPoints[2].Lat, filter.PolygonLL[2].Lat);
      Assert.Equal(boundaryType, filter.PolygonType);
    }

    [Fact]
    public void IncludeAlignmentSuccess()
    {
      var alignmentUid = Guid.NewGuid().ToString();
      double? startStation = 10.0;
      double? endStation = 50.6;
      double? leftOffset = 4.5;
      double? rightOffset = 8.94;
      var filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, null, true, 1, null, null,
        alignmentUid, null, startStation, endStation, leftOffset, rightOffset);

      var jsonString = JsonConvert.SerializeObject(filter);
      Assert.True(jsonString != string.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      filter.Validate(_serviceExceptionHandler);
      Assert.Equal(alignmentUid, filter.AlignmentUid);
      Assert.Equal(startStation, filter.StartStation);
      Assert.Equal(endStation, filter.EndStation);
      Assert.Equal(leftOffset, filter.LeftOffset);
      Assert.Equal(rightOffset, filter.RightOffset);
    }

    [Fact]
    public void IncludeAlignmentFailure_InvalidAlignmentUid()
    {
      string alignmentUid = "34545";
      double? startStation = 10;
      double? endStation = 50.6;
      double? leftOffset = 4.5;
      double? rightOffset = 8.94;
      var filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, null, true, 1, null, null,
        alignmentUid, null, startStation, endStation, leftOffset, rightOffset);

      var jsonString = JsonConvert.SerializeObject(filter);
      Assert.True(jsonString != string.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      var ex = Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));
      Assert.True(ex.GetContent.Contains(":2064"), "wrong code for invalid alignment Uid.");
    }
    [Fact]
    public void IncludeAlignmentFailure_NoAlignmentUid()
    {
      string alignmentUid = null;
      double? startStation = 10;
      double? endStation = 50.6;
      double? leftOffset = 4.5;
      double? rightOffset = 8.94;
      var filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, null, true, 1, null, null,
        alignmentUid, null, startStation, endStation, leftOffset, rightOffset);

      var jsonString = JsonConvert.SerializeObject(filter);
      Assert.True(jsonString != string.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      var ex = Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));
      Assert.True(ex.GetContent.Contains(":2067"), "wrong code for missing alignment definition.");
    }

    [Fact]
    public void IncludeAlignmentFailure_MissingEndStation()
    {
      string alignmentUid = Guid.NewGuid().ToString();
      double? startStation = 10;
      double? endStation = null;
      double? leftOffset = 4.5;
      double? rightOffset = 8.94;
      var filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, null, true, 1, null, null,
        alignmentUid, null, startStation, endStation, leftOffset, rightOffset);

      var jsonString = JsonConvert.SerializeObject(filter);
      Assert.True(jsonString != string.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      var ex = Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));
      Assert.True(ex.GetContent.Contains(":2065"), "wrong code for missing station.");
    }

    [Fact]
    public void IncludeAlignmentFailure_InvalidEndStation()
    {
      string alignmentUid = Guid.NewGuid().ToString();
      double? startStation = 50;
      double? endStation = 49.5;
      double? leftOffset = 0;
      double? rightOffset = 29.5;
      var filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, null, true, 1, null, null,
        alignmentUid, null, startStation, endStation, leftOffset, rightOffset);

      var jsonString = JsonConvert.SerializeObject(filter);
      Assert.True(jsonString != string.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      var ex = Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));
      Assert.True(ex.GetContent.Contains(":2065"), "wrong code for invalid Station.");
    }

    [Fact]
    public void IncludeAlignmentFailure_InvalidRightOffset()
    {
      string alignmentUid = Guid.NewGuid().ToString();
      double? startStation = 50;
      double? endStation = 65;
      double? leftOffset = 0;
      double? rightOffset = null;
      var filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, null, true, 1, null, null,
        alignmentUid, null, startStation, endStation, leftOffset, rightOffset);

      var jsonString = JsonConvert.SerializeObject(filter);
      Assert.True(jsonString != string.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      var ex = Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));
      Assert.True(ex.GetContent.Contains(":2066"), "wrong code for invalid Offset.");
    }

    [Fact]
    public void IncludeAlignmentSuccess_WithNegativeOffset()
    {
      var alignmentUid = Guid.NewGuid().ToString();
      double? startStation = 10.0;
      double? endStation = 50.6;
      double? leftOffset = -20;
      double? rightOffset = 25;
      var filter = new Filter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), "designName", _machines, 123,
        ElevationType.Lowest, true, null, true, 1, null, null,
        alignmentUid, null, startStation, endStation, leftOffset, rightOffset);

      var jsonString = JsonConvert.SerializeObject(filter);
      Assert.True(jsonString != string.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      filter.Validate(_serviceExceptionHandler);
      Assert.Equal(alignmentUid, filter.AlignmentUid);
      Assert.Equal(startStation, filter.StartStation);
      Assert.Equal(endStation, filter.EndStation);
      Assert.Equal(leftOffset, filter.LeftOffset);
      Assert.Equal(rightOffset, filter.RightOffset);
    }

    [Fact]
    public void AsAtDateFilterCustom_Success()
    {
      var filter = new Filter(null, DateTime.UtcNow.AddDays(-1), null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, true);
      filter.Validate(_serviceExceptionHandler);
    }

    [Fact]
    public void AsAtDateFilterWithDateRangeType_Success()
    {
      //Need to use filter JSON as cannot set DateRangeType directly
      var filterJson = "{\"asAtDate\":true, \"dateRangeType\":0}";
      var filter = JsonConvert.DeserializeObject<Filter>(filterJson);
      filter.Validate(_serviceExceptionHandler);
    }

    [Fact]
    public void AsAtDateFilterFailure_MissingEndUtc()
    {
      var filter = new Filter(asAtDate:true);
      Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));
    }

    [Fact]
    public void AsAtDateFilterFailure_MissingStartUtc()
    {
      var filter = new Filter(endUtc:DateTime.UtcNow.AddDays(-1), asAtDate:false);
      Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));
    }

    [Fact]
    public void AutomaticsFilterSuccess()
    {
      var filter = new Filter(automaticsType:AutomaticsType.Manual);
      filter.Validate(_serviceExceptionHandler);
    }

    [Fact]
    public void TemperatureRangeFilterSuccess()
    {
      var filter = new Filter(temperatureRangeMin:100.0, temperatureRangeMax:120.5);
      filter.Validate(_serviceExceptionHandler);
    }

    [Fact]
    public void TemperatureRangeFilter_MissingMax()
    {
      var filter = new Filter(temperatureRangeMin: 100.0);
      var ex = Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));
      Assert.True(ex.GetContent.Contains(":2072"), "wrong code for missing max temperature");
    }

    [Fact]
    public void TemperatureRangeFilter_OutOfRange()
    {
      var filter = new Filter(temperatureRangeMin:-10.0, temperatureRangeMax:320.0);
      var ex = Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));
      Assert.True(ex.GetContent.Contains(":2076"), "wrong code for temperature out of range");
    }

    [Fact]
    public void TemperatureRangeFilter_InvalidRange()
    {
      var filter = new Filter(temperatureRangeMin:255.0, temperatureRangeMax:127.5);
      var ex = Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));
      Assert.True(ex.GetContent.Contains(":2074"), "wrong code for invalid temperature range");
    }

    [Fact]
    public void PassCountRangeFilterSuccess()
    {
      var filter = new Filter(passCountRangeMin:5, passCountRangeMax:10);
      filter.Validate(_serviceExceptionHandler);

    }

    [Fact]
    public void PassCountRangeFilter_MissingMax()
    {
      var filter = new Filter(passCountRangeMin:5);
      var ex = Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));
      Assert.True(ex.GetContent.Contains(":2073"), "wrong code for missing max pass count");
    }

    [Fact]
    public void PassCountRangeFilter_OutOfRange()
    {
      var filter = new Filter(passCountRangeMin:900, passCountRangeMax:1100);
      var ex = Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));
      Assert.True(ex.GetContent.Contains(":2077"), "wrong code for pass count out of range");

    }

    [Fact]
    public void PassCountRangeFilter_InvalidRange()
    {
      var filter = new Filter(passCountRangeMin:25, passCountRangeMax:15);
      var ex = Assert.Throws<ServiceException>(() => filter.Validate(_serviceExceptionHandler));
      Assert.True(ex.GetContent.Contains(":2075"), "wrong code for invalid pass count range");
    }

    private string INVALID_GUID = "39823294vf-vbfb";

  }
}
