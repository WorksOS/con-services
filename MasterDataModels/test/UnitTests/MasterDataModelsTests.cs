using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.UnitTests.ResultsHandling;

namespace VSS.MasterData.Models.UnitTests
{
  [TestClass]
  public class MasterDataModelsTests : BaseTest
  {
    private DateTime _utcNow;
    private List<MachineDetails> _machines;
    private List<WGSPoint> _polygonLL;
    private string _boundaryUid = Guid.NewGuid().ToString();
    private string _boundaryName = "myBoundaryName";
    private IServiceExceptionHandler _serviceExceptionHandler;
    private DataAnnotationsValidator _validator;

    /// <summary>
    /// Initializes the test.
    /// </summary>
    [TestInitialize]
    public override void InitTest()
    {
      base.InitTest();

      this._utcNow = DateTime.UtcNow;
      this._machines = new List<MachineDetails>()
      {
        MachineDetails.Create(1137642418461469, "VOLVO G946B", false)
      };

      this._polygonLL = new List<WGSPoint>()
      {
        WGSPoint.CreatePoint(0.612770247622, -1.860592122242),
        WGSPoint.CreatePoint(0.61341601944523627132, -1.860592122242),
        WGSPoint.CreatePoint(0.612770247622, -1.86120298748019675)
      };

      this._serviceExceptionHandler = this.ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      this._validator = new DataAnnotationsValidator();
    }


    [TestMethod]
    public void CanCompareMachineDetailsEqual()
    {
      var machine1 = MachineDetails.Create(1, "test", true);
      var machine2 = MachineDetails.Create(1, "test", true);
      Assert.IsTrue(machine1 == machine2);
    }


    [TestMethod]
    public void CanCompareMachineDetailsNonequal()
    {
      var machine1 = MachineDetails.Create(1, "test", true);
      var machine2 = MachineDetails.Create(1, "test1", false);
      Assert.IsTrue(machine1 != machine2);
    }


    [TestMethod]
    public void CanCompareWGSPointEqual()
    {
      var point1 = WGSPoint.CreatePoint(10, 10);
      var point2 = WGSPoint.CreatePoint(10, 10);
      Assert.IsTrue(point1 == point2);
    }

    [TestMethod]
    public void CanCompareWGSPointNonequal()
    {
      var point1 = WGSPoint.CreatePoint(10, 10);
      var point2 = WGSPoint.CreatePoint(11, 10);
      Assert.IsTrue(point1 != point2);
    }

    [TestMethod]
    public void CanCompareFilters()
    {
      var designUid = Guid.NewGuid().ToString();

      var filter1 = Filter.CreateFilter(DateTime.MinValue, DateTime.MaxValue, designUid, "designName",
        new List<MachineDetails>() { MachineDetails.Create(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash1 = filter1.GetHashCode();
      var filter2 = Filter.CreateFilter(DateTime.MinValue, DateTime.MaxValue, designUid, "designName",
        new List<MachineDetails>() { MachineDetails.Create(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash2 = filter2.GetHashCode();
      Assert.IsTrue(filter1 == filter2);
      Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void CanCompareFiltersMachineDirection()
    {
      var filter1 = Filter.CreateFilter(DateTime.MinValue, DateTime.MaxValue, Guid.NewGuid().ToString(), "designName",
        new List<MachineDetails>() { MachineDetails.Create(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash1 = filter1.GetHashCode();
      var filter2 = Filter.CreateFilter(DateTime.MinValue, DateTime.MaxValue, Guid.NewGuid().ToString(), "designName",
        new List<MachineDetails>() { MachineDetails.Create(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), null, -1, null, "123");
      var hash2 = filter2.GetHashCode();
      Assert.IsTrue(filter1 != filter2);
      Assert.AreNotEqual(hash1, hash2);
    }

    [TestMethod]
    public void CanCompareFiltersMachineDirectionTrue()
    {
      var filter1 = Filter.CreateFilter(DateTime.MinValue, DateTime.MaxValue, Guid.NewGuid().ToString(), "designName",
        new List<MachineDetails>() { MachineDetails.Create(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), true, -1, null, "123");
      var hash1 = filter1.GetHashCode();
      var filter2 = Filter.CreateFilter(DateTime.MinValue, DateTime.MaxValue, Guid.NewGuid().ToString(), "designName",
        new List<MachineDetails>() { MachineDetails.Create(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), null, -1, null, "123");
      var hash2 = filter2.GetHashCode();
      Assert.IsTrue(filter1 != filter2);
      Assert.AreNotEqual(hash1, hash2);
    }

    [TestMethod]
    public void CanCompareFiltersMachineDirectionNulls()
    {
      var designUid = Guid.NewGuid().ToString();
      var filter1 = Filter.CreateFilter(DateTime.MinValue, DateTime.MaxValue, designUid, "designName",
        new List<MachineDetails>() { MachineDetails.Create(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), null, -1, null, "123");
      var hash1 = filter1.GetHashCode();
      var filter2 = Filter.CreateFilter(DateTime.MinValue, DateTime.MaxValue, designUid, "designName",
        new List<MachineDetails>() { MachineDetails.Create(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), null, -1, null, "123");
      var hash2 = filter2.GetHashCode();
      Assert.IsTrue(filter1 == filter2);
      Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void CanCompareFiltersNonEqual()
    {
      var filter1 = Filter.CreateFilter(DateTime.MinValue, DateTime.MaxValue, Guid.NewGuid().ToString(), "designName",
        new List<MachineDetails>() { MachineDetails.Create(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash1 = filter1.GetHashCode();
      var filter2 = Filter.CreateFilter(DateTime.MinValue, DateTime.MaxValue, Guid.NewGuid().ToString(), "designName",
        new List<MachineDetails>() { MachineDetails.Create(1, "test2", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash2 = filter2.GetHashCode();
      Assert.IsTrue(filter1 != filter2);
      Assert.AreNotEqual(hash1, hash2);
    }

    [TestMethod]
    public void CanCompareFiltersEqualWithNulls()
    {
      var designUid = Guid.NewGuid().ToString();

      var filter1 = Filter.CreateFilter(DateTime.MinValue, null, designUid, "designName",
        new List<MachineDetails>() { MachineDetails.Create(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash1 = filter1.GetHashCode();
      var filter2 = Filter.CreateFilter(DateTime.MinValue, null, designUid, "designName",
        new List<MachineDetails>() { MachineDetails.Create(1, "test", true) }, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash2 = filter2.GetHashCode();
      Assert.IsTrue(filter1 == filter2);
      Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void CanCreateFilterTest()
    {
      // Empty filter...
      Filter filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null);
      ICollection<ValidationResult> results;
      Assert.IsTrue(this._validator.TryValidate(filter, out results));

      // Complete filter...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, this._polygonLL, true, 1);
      Assert.IsTrue(this._validator.TryValidate(filter, out results));
    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      // All properties' values are valid...
      Filter filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, this._polygonLL, true, 1, this._boundaryUid, this._boundaryName);
      filter.Validate(this._serviceExceptionHandler);

      // Date range is not provided...
      filter = Filter.CreateFilter(null, null, new Guid().ToString(), "designName", this._machines, 123, ElevationType.Lowest, true, null,
        true, 1);
      filter.Validate(this._serviceExceptionHandler);

      // Design UID is not provided...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), null, "designName", this._machines, 123, ElevationType.Lowest, true, null,
        true, 1);
      filter.Validate(this._serviceExceptionHandler);

      // Machines' list is not provided...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", null, 123, ElevationType.Lowest,
        true, null, true, 1);
      filter.Validate(this._serviceExceptionHandler);

      // Machine's design name is not provided...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, null,
        ElevationType.Lowest, true, null, true, 1);
      filter.Validate(this._serviceExceptionHandler);

      // Elevation type is not provided...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, null, true, 1);
      filter.Validate(this._serviceExceptionHandler);

      // Vibration state is not provided...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, null, null, true, 1);
      filter.Validate(this._serviceExceptionHandler);

      // Forward direction flag is not provided...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, null, null, 1);
      filter.Validate(this._serviceExceptionHandler);

      // Layer number is not provided...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, null, true, null);
      filter.Validate(this._serviceExceptionHandler);
    }

    [TestMethod]
    public void ValidateFailureTest()
    {
      // Start UTC date is not provided...
      var filter = Filter.CreateFilter(null, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, this._polygonLL, true, 1);
      Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));

      // End UTC date is not provided...
      filter = Filter.CreateFilter(this._utcNow, null, new Guid().ToString(), "designName", this._machines, 123, ElevationType.Lowest, true,
        this._polygonLL, true, 1);
      Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));

      // Invalid design UID's Guid is provided...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), this.INVALID_GUID, "designName", this._machines, 123, ElevationType.Lowest,
        true, this._polygonLL, true, 1);
      Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));

      // The provided polygon's boundary has less than 3 points...
      this._polygonLL.RemoveAt(this._polygonLL.Count - 1);
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, this._polygonLL, true, 1);
      Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));
    }

    [TestMethod]
    public void ValidateJsonStringTest()
    {
      var filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, this._polygonLL, true, 1, this._boundaryUid, this._boundaryName);
      var jsonString = JsonConvert.SerializeObject(filter);

      Assert.IsTrue(jsonString != String.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      filter.Validate(this._serviceExceptionHandler);
    }

    [TestMethod]
    public void HydrateJsonStringWithPolygonTest()
    {
      var filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, null, true, 1);

      // now add the polygon
      var boundaryUid = Guid.NewGuid().ToString();
      var boundaryName = "myBoundaryName";
      var newBoundaryPoints = new List<VSS.MasterData.Models.Models.WGSPoint>
      {
        WGSPoint.CreatePoint(1, 170),
        WGSPoint.CreatePoint(6, 160),
        WGSPoint.CreatePoint(8, 150),
        WGSPoint.CreatePoint(1, 170)
      };

      filter.AddBoundary(boundaryUid, boundaryName, newBoundaryPoints);
      var jsonString = JsonConvert.SerializeObject(filter);
      Assert.IsTrue(jsonString != String.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      filter.Validate(this._serviceExceptionHandler);
      Assert.AreEqual(boundaryName, filter.PolygonName, "polyName is wrong.");
      Assert.AreEqual(boundaryUid, filter.PolygonUid, "polyUid is wrong.");
      Assert.AreEqual(4, filter.PolygonLL.Count, "point count is wrong.");
      Assert.AreEqual(newBoundaryPoints[2].Lat, filter.PolygonLL[2].Lat, "3rd filter point is invalid");
    }

    [TestMethod]
    public void HydrateJsonStringWithPolygonTest_Update()
    {
      var filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, this._polygonLL, true, 1, this._boundaryUid, this._boundaryName);
      var jsonString = JsonConvert.SerializeObject(filter);

      Assert.IsTrue(jsonString != String.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      filter.Validate(this._serviceExceptionHandler);
      Assert.AreEqual(this._boundaryName, filter.PolygonName, "original polyName is wrong.");
      Assert.AreEqual(this._boundaryUid, filter.PolygonUid, "original polyUid is wrong.");
      Assert.AreEqual(3, filter.PolygonLL.Count, "original point count is wrong.");
      Assert.AreEqual(this._polygonLL[1].Lat, filter.PolygonLL[1].Lat, "updated 2nd filter point is invalid");

      // now update the polygon
      var boundaryUid = Guid.NewGuid().ToString();
      var boundaryName = "new myBoundaryName";
      var newBoundaryPoints = new List<VSS.MasterData.Models.Models.WGSPoint>
      {
        WGSPoint.CreatePoint(1, 170),
        WGSPoint.CreatePoint(6, 160),
        WGSPoint.CreatePoint(8, 150),
        WGSPoint.CreatePoint(1, 170)
      };

      filter.AddBoundary(boundaryUid, boundaryName, newBoundaryPoints);
      jsonString = JsonConvert.SerializeObject(filter);
      Assert.IsTrue(jsonString != String.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      filter.Validate(this._serviceExceptionHandler);
      Assert.AreEqual(boundaryName, filter.PolygonName, "updated polyName is wrong.");
      Assert.AreEqual(boundaryUid, filter.PolygonUid, "updated polyUid is wrong.");
      Assert.AreEqual(4, filter.PolygonLL.Count, "updated point count is wrong.");
      Assert.AreEqual(newBoundaryPoints[2].Lat, filter.PolygonLL[2].Lat, "updated 3rd filter point is invalid");
    }

    [TestMethod]
    public void IncludeAlignmentSuccess()
    {
      var alignmentUid = Guid.NewGuid().ToString();
      double? startStation = 10.0;
      double? endStation = 50.6;
      double? leftOffset = 4.5;
      double? rightOffset = 8.94;
      var filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, null, true, 1, null, null,
        alignmentUid, null, startStation, endStation, leftOffset, rightOffset);

      var jsonString = JsonConvert.SerializeObject(filter);
      Assert.IsTrue(jsonString != String.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      filter.Validate(this._serviceExceptionHandler);
      Assert.AreEqual(alignmentUid, filter.AlignmentUid, "alignmentUid is wrong.");
      Assert.AreEqual(startStation, filter.StartStation, "startStation is wrong.");
      Assert.AreEqual(endStation, filter.EndStation, "endStation is wrong.");
      Assert.AreEqual(leftOffset, filter.LeftOffset, "leftOffset is wrong.");
      Assert.AreEqual(rightOffset, filter.RightOffset, "rightOffset is wrong.");
    }

    [TestMethod]
    public void IncludeAlignmentFailure_InvalidAlignmentUid()
    {
      string alignmentUid = "34545";
      double? startStation = 10;
      double? endStation = 50.6;
      double? leftOffset = 4.5;
      double? rightOffset = 8.94;
      var filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, null, true, 1, null, null,
        alignmentUid, null, startStation, endStation, leftOffset, rightOffset);

      var jsonString = JsonConvert.SerializeObject(filter);
      Assert.IsTrue(jsonString != String.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      var ex = Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));
      Assert.IsTrue(ex.GetContent.Contains(":2064"), "wrong code for invalid alignment Uid.");
    }
    [TestMethod]
    public void IncludeAlignmentFailure_NoAlignmentUid()
    {
      string alignmentUid = null;
      double? startStation = 10;
      double? endStation = 50.6;
      double? leftOffset = 4.5;
      double? rightOffset = 8.94;
      var filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, null, true, 1, null, null,
        alignmentUid, null, startStation, endStation, leftOffset, rightOffset);

      var jsonString = JsonConvert.SerializeObject(filter);
      Assert.IsTrue(jsonString != String.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      var ex = Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));
      Assert.IsTrue(ex.GetContent.Contains(":2067"), "wrong code for missing alignment definition.");
    }

    [TestMethod]
    public void IncludeAlignmentFailure_MissingEndStation()
    {
      string alignmentUid = Guid.NewGuid().ToString();
      double? startStation = 10;
      double? endStation = null;
      double? leftOffset = 4.5;
      double? rightOffset = 8.94;
      var filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, null, true, 1, null, null,
        alignmentUid, null, startStation, endStation, leftOffset, rightOffset);

      var jsonString = JsonConvert.SerializeObject(filter);
      Assert.IsTrue(jsonString != String.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      var ex = Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));
      Assert.IsTrue(ex.GetContent.Contains(":2065"), "wrong code for missing station.");
    }

    [TestMethod]
    public void IncludeAlignmentFailure_InvalidEndStation()
    {
      string alignmentUid = Guid.NewGuid().ToString();
      double? startStation = 50;
      double? endStation = 49.5;
      double? leftOffset = 0;
      double? rightOffset = 29.5;
      var filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, null, true, 1, null, null,
        alignmentUid, null, startStation, endStation, leftOffset, rightOffset);

      var jsonString = JsonConvert.SerializeObject(filter);
      Assert.IsTrue(jsonString != String.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      var ex = Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));
      Assert.IsTrue(ex.GetContent.Contains(":2065"), "wrong code for invalid Station.");
    }

    [TestMethod]
    public void IncludeAlignmentFailure_InvalidRightOffset()
    {
      string alignmentUid = Guid.NewGuid().ToString();
      double? startStation = 50;
      double? endStation = 65;
      double? leftOffset = 0;
      double? rightOffset = null;
      var filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, null, true, 1, null, null,
        alignmentUid, null, startStation, endStation, leftOffset, rightOffset);

      var jsonString = JsonConvert.SerializeObject(filter);
      Assert.IsTrue(jsonString != String.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      var ex = Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));
      Assert.IsTrue(ex.GetContent.Contains(":2066"), "wrong code for invalid Offset.");
    }

    [TestMethod]
    public void IncludeAlignmentSuccess_WithNegativeOffset()
    {
      var alignmentUid = Guid.NewGuid().ToString();
      double? startStation = 10.0;
      double? endStation = 50.6;
      double? leftOffset = -20;
      double? rightOffset = 25;
      var filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), "designName", this._machines, 123,
        ElevationType.Lowest, true, null, true, 1, null, null,
        alignmentUid, null, startStation, endStation, leftOffset, rightOffset);

      var jsonString = JsonConvert.SerializeObject(filter);
      Assert.IsTrue(jsonString != String.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      filter.Validate(this._serviceExceptionHandler);
      Assert.AreEqual(alignmentUid, filter.AlignmentUid, "alignmentUid is wrong.");
      Assert.AreEqual(startStation, filter.StartStation, "startStation is wrong.");
      Assert.AreEqual(endStation, filter.EndStation, "endStation is wrong.");
      Assert.AreEqual(leftOffset, filter.LeftOffset, "leftOffset is wrong.");
      Assert.AreEqual(rightOffset, filter.RightOffset, "rightOffset is wrong.");
    }

    [TestMethod]
    public void AsAtDateFilterCustom_Success()
    {
      var filter = Filter.CreateFilter(null, DateTime.UtcNow.AddDays(-1), null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, true);
      filter.Validate(_serviceExceptionHandler);
    }

    [TestMethod]
    public void AsAtDateFilterWithDateRangeType_Success()
    {
      //Need to use filter JSON as cannot set DateRangeType directly
      var filterJson = "{\"asAtDate\":true, \"dateRangeType\":0}";
      var filter = JsonConvert.DeserializeObject<Filter>(filterJson);
      filter.Validate(_serviceExceptionHandler);
    }

    [TestMethod]
    public void AsAtDateFilterFailure_MissingEndUtc()
    {
      var filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, true);
      Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));
    }

    [TestMethod]
    public void AsAtDateFilterFailure_MissingStartUtc()
    {
      var filter = Filter.CreateFilter(null, DateTime.UtcNow.AddDays(-1), null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, false);
      Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));
    }

    [TestMethod]
    public void AutomaticsFilterSuccess()
    {
      var filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, AutomaticsType.Manual);
      filter.Validate(_serviceExceptionHandler);
    }

    [TestMethod]
    public void TemperatureRangeFilterSuccess()
    {
      var filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, 100.0, 120.5);
      filter.Validate(_serviceExceptionHandler);
    }

    [TestMethod]
    public void TemperatureRangeFilter_MissingMax()
    {
      var filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, 100.0);
      var ex = Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));
      Assert.IsTrue(ex.GetContent.Contains(":2072"), "wrong code for missing max temperature");
    }

    [TestMethod]
    public void TemperatureRangeFilter_OutOfRange()
    {
      var filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, -10.0, 320.0);
      var ex = Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));
      Assert.IsTrue(ex.GetContent.Contains(":2076"), "wrong code for temperature out of range");
    }

    [TestMethod]
    public void TemperatureRangeFilter_InvalidRange()
    {
      var filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, 255.0, 127.5);
      var ex = Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));
      Assert.IsTrue(ex.GetContent.Contains(":2074"), "wrong code for invalid temperature range");
    }

    [TestMethod]
    public void PassCountRangeFilterSuccess()
    {
      var filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, 5, 10);
      filter.Validate(_serviceExceptionHandler);

    }

    [TestMethod]
    public void PassCountRangeFilter_MissingMax()
    {
      var filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, 5);
      var ex = Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));
      Assert.IsTrue(ex.GetContent.Contains(":2073"), "wrong code for missing max pass count");
    }

    [TestMethod]
    public void PassCountRangeFilter_OutOfRange()
    {
      var filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, 900, 1100);
      var ex = Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));
      Assert.IsTrue(ex.GetContent.Contains(":2077"), "wrong code for pass count out of range");

    }

    [TestMethod]
    public void PassCountRangeFilter_InvalidRange()
    {
      var filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, 25, 15);
      var ex = Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));
      Assert.IsTrue(ex.GetContent.Contains(":2075"), "wrong code for invalid pass count range");
    }

    private string INVALID_GUID = "39823294vf-vbfb";

  }
}
