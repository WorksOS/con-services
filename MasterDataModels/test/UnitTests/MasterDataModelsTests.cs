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
        MachineDetails.CreateMachineDetails(1137642418461469, "VOLVO G946B", false)
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
      var machine1 = MachineDetails.CreateMachineDetails(1, "test", true);
      var machine2 = MachineDetails.CreateMachineDetails(1, "test", true);
      Assert.IsTrue(machine1 == machine2);
    }


    [TestMethod]
    public void CanCompareMachineDetailsNonequal()
    {
      var machine1 = MachineDetails.CreateMachineDetails(1, "test", true);
      var machine2 = MachineDetails.CreateMachineDetails(1, "test1", false);
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
      var filter1 = Filter.CreateFilter(DateTime.MinValue, DateTime.MaxValue, "design",
        new List<MachineDetails>() {MachineDetails.CreateMachineDetails(1, "test", true)}, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash1 = filter1.GetHashCode();
      var filter2 = Filter.CreateFilter(DateTime.MinValue, DateTime.MaxValue, "design",
        new List<MachineDetails>() {MachineDetails.CreateMachineDetails(1, "test", true)}, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash2 = filter2.GetHashCode();
      Assert.IsTrue(filter1 == filter2);
      Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void CanCompareFiltersNonEqual()
    {
      var filter1 = Filter.CreateFilter(DateTime.MinValue, DateTime.MaxValue, "design",
        new List<MachineDetails>() {MachineDetails.CreateMachineDetails(1, "test", true)}, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash1 = filter1.GetHashCode();
      var filter2 = Filter.CreateFilter(DateTime.MinValue, DateTime.MaxValue, "design",
        new List<MachineDetails>() {MachineDetails.CreateMachineDetails(1, "test2", true)}, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash2 = filter2.GetHashCode();
      Assert.IsTrue(filter1 != filter2);
      Assert.AreNotEqual(hash1, hash2);
    }

    [TestMethod]
    public void CanCompareFiltersEqualWithNulls()
    {
      var filter1 = Filter.CreateFilter(DateTime.MinValue, null, "design",
        new List<MachineDetails>() {MachineDetails.CreateMachineDetails(1, "test", true)}, 15,
        ElevationType.Lowest, true, new List<WGSPoint>(), false, -1, null, "123");
      var hash1 = filter1.GetHashCode();
      var filter2 = Filter.CreateFilter(DateTime.MinValue, null, "design",
        new List<MachineDetails>() {MachineDetails.CreateMachineDetails(1, "test", true)}, 15,
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
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), this._machines, 123,
        ElevationType.Lowest, true, this._polygonLL, true, 1);
      Assert.IsTrue(this._validator.TryValidate(filter, out results));
    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      // All properties' values are valid...
      Filter filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), this._machines, 123,
        ElevationType.Lowest, true, this._polygonLL, true, 1, this._boundaryUid, this._boundaryName);
      filter.Validate(this._serviceExceptionHandler);

      // Date range is not provided...
      filter = Filter.CreateFilter(null, null, new Guid().ToString(), this._machines, 123, ElevationType.Lowest, true, null,
        true, 1);
      filter.Validate(this._serviceExceptionHandler);

      // Design UID is not provided...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), null, this._machines, 123, ElevationType.Lowest, true, null,
        true, 1);
      filter.Validate(this._serviceExceptionHandler);

      // Machines' list is not provided...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), null, 123, ElevationType.Lowest,
        true, null, true, 1);
      filter.Validate(this._serviceExceptionHandler);

      // Machine's design name is not provided...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), this._machines, null,
        ElevationType.Lowest, true, null, true, 1);
      filter.Validate(this._serviceExceptionHandler);

      // Elevation type is not provided...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), this._machines, 123,
        ElevationType.Lowest, true, null, true, 1);
      filter.Validate(this._serviceExceptionHandler);

      // Vibration state is not provided...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), this._machines, 123,
        ElevationType.Lowest, null, null, true, 1);
      filter.Validate(this._serviceExceptionHandler);

      // Forward direction flag is not provided...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), this._machines, 123,
        ElevationType.Lowest, true, null, null, 1);
      filter.Validate(this._serviceExceptionHandler);

      // Layer number is not provided...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), this._machines, 123,
        ElevationType.Lowest, true, null, true, null);
      filter.Validate(this._serviceExceptionHandler);
    }

    [TestMethod]
    public void ValidateFailureTest()
    {
      // Start UTC date is not provided...
      var filter = Filter.CreateFilter(null, this._utcNow.AddDays(10), new Guid().ToString(), this._machines, 123,
        ElevationType.Lowest, true, this._polygonLL, true, 1);
      Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));

      // End UTC date is not provided...
      filter = Filter.CreateFilter(this._utcNow, null, new Guid().ToString(), this._machines, 123, ElevationType.Lowest, true,
        this._polygonLL, true, 1);
      Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));

      // Invalid design UID's Guid is provided...
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), this.INVALID_GUID, this._machines, 123, ElevationType.Lowest,
        true, this._polygonLL, true, 1);
      Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));

      // The provided polygon's boundary has less than 3 points...
      this._polygonLL.RemoveAt(this._polygonLL.Count - 1);
      filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), this._machines, 123,
        ElevationType.Lowest, true, this._polygonLL, true, 1);
      Assert.ThrowsException<ServiceException>(() => filter.Validate(this._serviceExceptionHandler));
    }

    [TestMethod]
    public void ValidateJsonStringTest()
    {
      var filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), this._machines, 123,
        ElevationType.Lowest, true, this._polygonLL, true, 1, this._boundaryUid, this._boundaryName);
      var jsonString = filter.ToJsonString();

      Assert.IsTrue(jsonString != String.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      filter.Validate(this._serviceExceptionHandler);
    }

    [TestMethod]
    public void HydrateJsonStringWithPolygonTest()
    {
      var filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), this._machines, 123,
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
      Assert.AreEqual(boundaryName, filter.polygonName, "polyName is wrong.");
      Assert.AreEqual(boundaryUid, filter.polygonUID, "polyUid is wrong.");
      Assert.AreEqual(4, filter.polygonLL.Count, "point count is wrong.");
      Assert.AreEqual(newBoundaryPoints[2].Lat, filter.polygonLL[2].Lat, "3rd filter point is invalid");
    }

    [TestMethod]
    public void HydrateJsonStringWithPolygonTest_Update()
    {
      var filter = Filter.CreateFilter(this._utcNow, this._utcNow.AddDays(10), new Guid().ToString(), this._machines, 123,
        ElevationType.Lowest, true, this._polygonLL, true, 1, this._boundaryUid, this._boundaryName);
      var jsonString = filter.ToJsonString();

      Assert.IsTrue(jsonString != String.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      filter.Validate(this._serviceExceptionHandler);
      Assert.AreEqual(this._boundaryName, filter.polygonName, "original polyName is wrong.");
      Assert.AreEqual(this._boundaryUid, filter.polygonUID, "original polyUid is wrong.");
      Assert.AreEqual(3, filter.polygonLL.Count, "original point count is wrong.");
      Assert.AreEqual(this._polygonLL[1].Lat, filter.polygonLL[1].Lat, "updated 2nd filter point is invalid");

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
      Assert.AreEqual(boundaryName, filter.polygonName, "updated polyName is wrong.");
      Assert.AreEqual(boundaryUid, filter.polygonUID, "updated polyUid is wrong.");
      Assert.AreEqual(4, filter.polygonLL.Count, "updated point count is wrong.");
      Assert.AreEqual(newBoundaryPoints[2].Lat, filter.polygonLL[2].Lat, "updated 3rd filter point is invalid");
    }

    private string INVALID_GUID = "39823294vf-vbfb";

  }
}
