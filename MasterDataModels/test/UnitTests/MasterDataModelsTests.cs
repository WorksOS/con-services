using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.UnitTests;
using VSS.MasterData.Models.UnitTests.ResultsHandling;
using VSS.Productivity3D.Common.Models;

namespace VSS.MasterData.Models.Tests
{
  [TestClass]
  public class MasterDataModelsTests :BaseTest
  {
    private DateTime _utcNow;
    private List<MachineDetails> _machines;
    private List<WGSPoint> _polygonLL;
    private IServiceExceptionHandler _serviceExceptionHandler;
    private DataAnnotationsValidator _validator;

    /// <summary>
    /// Initializes the test.
    /// </summary>
    [TestInitialize]
    public override void InitTest()
    {
      base.InitTest();

      _utcNow = DateTime.UtcNow;
      _machines = new List<MachineDetails>() { MachineDetails.HelpSample };

      _polygonLL = new List<WGSPoint>() {
        WGSPoint.HelpSample,
        WGSPoint.CreatePoint(WGSPoint.HelpSample.Lat + 0.035, WGSPoint.HelpSample.Lon),
        WGSPoint.CreatePoint(WGSPoint.HelpSample.Lat, WGSPoint.HelpSample.Lon + 0.035)};

      _serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      _validator = new DataAnnotationsValidator();
    }

    [TestMethod]
    public void CanCreateFilterTest()
    {
      // Empty filter...
      Filter filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null);
      ICollection<ValidationResult> results;
      Assert.IsTrue(_validator.TryValidate(filter, out results));

      // Complete filter...
      filter = Filter.CreateFilter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), _machines, 123, ElevationType.Lowest, true, _polygonLL, true, 1, FilterLayerMethod.MapReset);
      Assert.IsTrue(_validator.TryValidate(filter, out results));
    }

    [TestMethod]
    public void ValidateSuccessTest()
    {
      // All properties' values are valid...
      Filter filter = Filter.CreateFilter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), _machines, 123, ElevationType.Lowest, true, _polygonLL, true, 1, FilterLayerMethod.MapReset);
      filter.Validate(_serviceExceptionHandler);

      // Date range is not provided...
      filter = Filter.CreateFilter(null, null, new Guid().ToString(), _machines, 123, ElevationType.Lowest, true, _polygonLL, true, 1, FilterLayerMethod.MapReset);
      filter.Validate(_serviceExceptionHandler);

      // Design UID is not provided...
      filter = Filter.CreateFilter(_utcNow, _utcNow.AddDays(10), null, _machines, 123, ElevationType.Lowest, true, _polygonLL, true, 1, FilterLayerMethod.MapReset);
      filter.Validate(_serviceExceptionHandler);

      // Machines' list is not provided...
      filter = Filter.CreateFilter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), null, 123, ElevationType.Lowest, true, _polygonLL, true, 1, FilterLayerMethod.MapReset);
      filter.Validate(_serviceExceptionHandler);

      // Machine's design name is not provided...
      filter = Filter.CreateFilter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), _machines, null, ElevationType.Lowest, true, _polygonLL, true, 1, FilterLayerMethod.MapReset);
      filter.Validate(_serviceExceptionHandler);

      // Elevation type is not provided...
      filter = Filter.CreateFilter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), _machines, 123, ElevationType.Lowest, true, _polygonLL, true, 1, FilterLayerMethod.MapReset);
      filter.Validate(_serviceExceptionHandler);

      // Vibration state is not provided...
      filter = Filter.CreateFilter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), _machines, 123, ElevationType.Lowest, null, _polygonLL, true, 1, FilterLayerMethod.MapReset);
      filter.Validate(_serviceExceptionHandler);

      // Polygon is not provided...
      filter = Filter.CreateFilter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), _machines, 123, ElevationType.Lowest, true, null, true, 1, FilterLayerMethod.MapReset);
      filter.Validate(_serviceExceptionHandler);

      // Forward direction flag is not provided...
      filter = Filter.CreateFilter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), _machines, 123, ElevationType.Lowest, true, _polygonLL, null, 1, FilterLayerMethod.MapReset);
      filter.Validate(_serviceExceptionHandler);

      // Layer number is not provided...
      filter = Filter.CreateFilter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), _machines, 123, ElevationType.Lowest, true, _polygonLL, true, null, FilterLayerMethod.MapReset);
      filter.Validate(_serviceExceptionHandler);
    }

    [TestMethod]
    public void ValidateFailureTest()
    {
      // Start UTC date is not provided...
      var filter = Filter.CreateFilter(null, _utcNow.AddDays(10), new Guid().ToString(), _machines, 123, ElevationType.Lowest, true, _polygonLL, true, 1, FilterLayerMethod.MapReset);
      Assert.ThrowsException<ServiceException>(() => filter.Validate(_serviceExceptionHandler));

      // End UTC date is not provided...
      filter = Filter.CreateFilter(_utcNow, null, new Guid().ToString(), _machines, 123, ElevationType.Lowest, true, _polygonLL, true, 1, FilterLayerMethod.MapReset);
      Assert.ThrowsException<ServiceException>(() => filter.Validate(_serviceExceptionHandler));

      // Invalid design UID's Guid is provided...
      filter = Filter.CreateFilter(_utcNow, _utcNow.AddDays(10), INVALID_GUID, _machines, 123, ElevationType.Lowest, true, _polygonLL, true, 1, FilterLayerMethod.MapReset);
      Assert.ThrowsException<ServiceException>(() => filter.Validate(_serviceExceptionHandler));

      // No layer type is provided for a layer filter...
      filter = Filter.CreateFilter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), _machines, 123, ElevationType.Lowest, true, _polygonLL, true, 1, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate(_serviceExceptionHandler));

      // No layer number is provided for a layer filter...
      filter = Filter.CreateFilter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), _machines, 123, ElevationType.Lowest, true, _polygonLL, true, null, FilterLayerMethod.TagfileLayerNumber);
      Assert.ThrowsException<ServiceException>(() => filter.Validate(_serviceExceptionHandler));

      // The provided polygon's boundary has less than 3 points...
      _polygonLL.RemoveAt(_polygonLL.Count - 1);
      filter = Filter.CreateFilter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), _machines, 123, ElevationType.Lowest, true, _polygonLL, true, 1, FilterLayerMethod.MapReset);
      Assert.ThrowsException<ServiceException>(() => filter.Validate(_serviceExceptionHandler));
    }

    [TestMethod]
    public void ValidateJsonStringTest()
    {
      var filter = Filter.CreateFilter(_utcNow, _utcNow.AddDays(10), new Guid().ToString(), _machines, 123, ElevationType.Lowest, true, _polygonLL, true, 1, FilterLayerMethod.MapReset);
      var jsonString = filter.ToJsonString();

      Assert.IsTrue(jsonString != String.Empty);

      filter = JsonConvert.DeserializeObject<Filter>(jsonString);
      filter.Validate(_serviceExceptionHandler);
    }

    private string INVALID_GUID = "39823294vf-vbfb";
  }
}
