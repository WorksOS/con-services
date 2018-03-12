using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Validators;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class ValidationTests : ExecutorBaseTests
  {
    private readonly string custUid = Guid.NewGuid().ToString();
    private readonly string userUid = Guid.NewGuid().ToString();
    private readonly string projectUid = Guid.NewGuid().ToString();
    private readonly string filterUid = Guid.NewGuid().ToString();
    private readonly string boundaryUid = Guid.NewGuid().ToString();
    private const string Name = "blah";
    private const string FilterJson = "{\"designUID\": \"id\", \"vibeStateOn\": true}";
    private const string GeometryWKT =
      "POLYGON((80.257874 12.677856,79.856873 13.039345,80.375977 13.443052,80.257874 12.677856))";

    [TestMethod]
    public void FilterRequestValidation_InvalidCustomerUid()
    {
      var requestFull =
        FilterRequestFull.Create
        (
          null,
          "sfgsdfsf",
          false,
          userUid,
          projectUid,
          new FilterRequest {FilterUid = filterUid, Name = Name, FilterJson = FilterJson}
        );
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2027");
      StringAssert.Contains(ex.GetContent, "Invalid customerUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_MissingUserId()
    {
      var requestFull =
        FilterRequestFull.Create
        (
          null,
          custUid,
          false,
          string.Empty,
          projectUid,
          new FilterRequest {FilterUid = filterUid, Name = Name, FilterJson = FilterJson}
        );
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2028");
      StringAssert.Contains(ex.GetContent, "Invalid userUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_MissingProjectUid()
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, null,
          new FilterRequest {FilterUid = filterUid, Name = Name, FilterJson = FilterJson});
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2001");
      StringAssert.Contains(ex.GetContent, "Invalid projectUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterUid()
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, projectUid,
          new FilterRequest {FilterUid = "this is so wrong", Name = Name, FilterJson = FilterJson});
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2002");
      StringAssert.Contains(ex.GetContent, "Invalid filterUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterUid_Null()
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, projectUid,
          new FilterRequest {FilterUid = null, Name = Name, FilterJson = string.Empty});

      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidName()
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, projectUid,
          new FilterRequest {FilterUid = filterUid, Name = null, FilterJson = string.Empty});

      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterJson()
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, projectUid,
          new FilterRequest {FilterUid = filterUid, Name = Name, FilterJson = null});

      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("{ \"FilterUid\": \"00000000-0000-0000-0000-000000000000\" }")]
    public void FilterRequestValidation_Should_succeed_When_supplied_json_is_valid(string filterJson)
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, projectUid,
          new FilterRequest {FilterUid = filterUid, Name = "", FilterJson = filterJson});
      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    public void FilterRequestValidation_Should_fail_When_supplied_string_is_invalid_json()
    {
      var requestFull = FilterRequestFull.Create(null, custUid, false, userUid, projectUid,
        new FilterRequest {FilterUid = filterUid, Name = Name, FilterJson = "de blah"});
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2042");
      StringAssert.Contains(ex.GetContent,
        "Invalid filterJson. Exception: Unexpected character encountered while parsing value:");
    }

    [TestMethod]
    public void FilterRequestValidation_PartialFill()
    {
      var requestFull = FilterRequestFull.Create(null, custUid, false, userUid, projectUid);

      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    public async Task CustomerProjectValidation_HappyPath()
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var projects = new List<MasterData.Models.Models.ProjectData>
      {
        new MasterData.Models.Models.ProjectData {ProjectUid = projectUid, CustomerUid = custUid}
      };
      var customHeaders = new Dictionary<string, string>();
      projectListProxy.Setup(ps => ps.GetProjectsV4(It.IsAny<string>(), customHeaders)).ReturnsAsync(projects);

      await ValidationUtil.ValidateProjectForCustomer(projectListProxy.Object, log, serviceExceptionHandler,
        customHeaders, custUid, projectUid).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task CustomerProjectValidation_NoAssociation()
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var projects = new List<MasterData.Models.Models.ProjectData>();
      var customHeaders = new Dictionary<string, string>();
      projectListProxy.Setup(ps => ps.GetProjectsV4(It.IsAny<string>(), customHeaders)).ReturnsAsync(projects);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await ValidationUtil
        .ValidateProjectForCustomer(projectListProxy.Object, log, serviceExceptionHandler,
          customHeaders, custUid, projectUid).ConfigureAwait(false));

      StringAssert.Contains(ex.GetContent, "2008");
      StringAssert.Contains(ex.GetContent, "Validation of Customer/Project failed. Not allowed.");
    }

    [TestMethod]
    public async Task HydrateJsonWithBoundary_NoPolygonUid()
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();
      var geofenceRepo = new Mock<IGeofenceRepository>();

      var request = FilterRequestFull.Create(null, custUid, false, userUid, projectUid,
        new FilterRequest {FilterUid = filterUid, FilterJson = "{\"designUID\": \"id\", \"vibeStateOn\": true}", Name = "a filter"});

      var result = await ValidationUtil
        .HydrateJsonWithBoundary(geofenceRepo.Object, log, serviceExceptionHandler, request).ConfigureAwait(false);

      Assert.AreEqual(request.FilterJson, result, "Wrong hydated json");
    }

    [TestMethod]
    public async Task HydrateJsonWithBoundary_IncludeAlignment()
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();
      var geofenceRepo = new Mock<IGeofenceRepository>();
      var alignmentUid = Guid.NewGuid().ToString();
      var startStation = 100.456;
      var endStation = 200.5;
      var leftOffset = 4.5;
      var rightOffset = 2.5;

      var request = FilterRequestFull.Create(null, custUid, false, userUid, projectUid,
        new FilterRequest { FilterUid = filterUid, FilterJson = "{\"designUID\": \"id\", \"vibeStateOn\": true, \"alignmentUid\": \"" + alignmentUid + "\", \"startStation\":" + startStation + ", \"endStation\":" + endStation + ", \"leftOffset\":" + leftOffset + ", \"rightOffset\":" + rightOffset + "}", Name = "a filter" });

      var result = await ValidationUtil
        .HydrateJsonWithBoundary(geofenceRepo.Object, log, serviceExceptionHandler, request).ConfigureAwait(false);

      Assert.AreEqual(request.FilterJson, result, "Wrong hydated json");
    }

    [TestMethod]
    public async Task HydrateJsonWithBoundary_NoGeofence()
    {      
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();
      var geofenceRepo = new Mock<IGeofenceRepository>();
      geofenceRepo.Setup(g => g.GetGeofence(It.IsAny<string>())).ReturnsAsync((Geofence)null);

      var request = FilterRequestFull.Create(null, custUid, false, userUid, projectUid,
        new FilterRequest { FilterUid = filterUid, FilterJson = "{\"designUID\": \"id\", \"vibeStateOn\": true, \"polygonUID\": \"" + boundaryUid + "\"}", Name = "a filter" });

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await ValidationUtil
        .HydrateJsonWithBoundary(geofenceRepo.Object, log, serviceExceptionHandler, request).ConfigureAwait(false));

      StringAssert.Contains(ex.GetContent, "2040");
      StringAssert.Contains(ex.GetContent, "Validation of Project/Boundary failed. Not allowed.");
    }

    [TestMethod]
    public async Task HydrateJsonWithBoundary_InvalidBoundary()
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();
      var geofenceRepo = new Mock<IGeofenceRepository>();
      var geofence = new Geofence{GeometryWKT = "This is not a valid polygon WKT"};
      geofenceRepo.Setup(g => g.GetGeofence(It.IsAny<string>())).ReturnsAsync(geofence);

      var request = FilterRequestFull.Create(null, custUid, false, userUid, projectUid,
        new FilterRequest { FilterUid = filterUid, FilterJson = "{\"designUID\": \"id\", \"vibeStateOn\": true, \"polygonUID\": \"" + boundaryUid + "\"}", Name = "a filter" });

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await ValidationUtil
        .HydrateJsonWithBoundary(geofenceRepo.Object, log, serviceExceptionHandler, request).ConfigureAwait(false));

      StringAssert.Contains(ex.GetContent, "2045");
      StringAssert.Contains(ex.GetContent, "Invalid spatial filter boundary. One or more polygon components are missing.");
    }

    [TestMethod]
    public async Task FilterRequestValidation_InvalidAlignment()
    {
      var alignmentUid = Guid.NewGuid().ToString();
      var startStation = 100.456;
      var leftOffset = 4.5;
      var rightOffset = 2.5;

      var request = FilterRequestFull.Create(null, custUid, false, userUid, projectUid,
        new FilterRequest { FilterUid = filterUid, FilterJson = "{\"vibeStateOn\": true, \"alignmentUid\": \"" + alignmentUid + "\", \"startStation\":" + startStation + ", \"endStation\": null, \"leftOffset\":" + leftOffset + ", \"rightOffset\":" + rightOffset + "}", Name = "a filter" });

      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2065");
      StringAssert.Contains(ex.GetContent, "Invalid alignment filter. Start or end station are invalid.");
    }

    [TestMethod]
    public async Task FilterRequestValidation_ValidAlignment()
    {
      var alignmentUid = Guid.NewGuid().ToString();
      var startStation = 12.456;
      double endStation = 56.0;
      var leftOffset = 4.5;
      var rightOffset = 2.5;

      var request = FilterRequestFull.Create(null, custUid, false, userUid, projectUid,
        new FilterRequest { FilterUid = filterUid, FilterJson = "{\"vibeStateOn\": true, \"alignmentUid\": \"" + alignmentUid + "\", \"startStation\": " + startStation + ", \"endStation\": " + endStation + ", \"leftOffset\": " + leftOffset + ", \"rightOffset\": " + rightOffset + "}", Name = "a filter" });

      request.Validate(serviceExceptionHandler);
    }
    
    [TestMethod]
    public async Task HydrateJsonWithBoundary_HappyPath()
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();
      var geofenceRepo = new Mock<IGeofenceRepository>();
      var geofence = new Geofence { GeofenceUID = boundaryUid, Name = Name, GeometryWKT = GeometryWKT };
      geofenceRepo.Setup(g => g.GetGeofence(It.IsAny<string>())).ReturnsAsync(geofence);

      var request = FilterRequestFull.Create(null, custUid, false, userUid, projectUid,
        new FilterRequest { FilterUid = filterUid, FilterJson = "{\"designUid\": \"id\", \"vibeStateOn\": true, \"polygonUid\": \"" + geofence.GeofenceUID + "\"}", Name = "a filter" });

      var result = await ValidationUtil
        .HydrateJsonWithBoundary(geofenceRepo.Object, log, serviceExceptionHandler, request).ConfigureAwait(false);

      var expectedResult =
        "{\"designUid\":\"id\",\"vibeStateOn\":true,\"polygonUid\":\"" + geofence.GeofenceUID + "\",\"polygonName\":\"" + geofence.Name + "\",\"polygonLL\":[{\"Lat\":12.677856,\"Lon\":80.257874},{\"Lat\":13.039345,\"Lon\":79.856873},{\"Lat\":13.443052,\"Lon\":80.375977}]}";

      Assert.AreEqual(expectedResult, result, "Wrong hydrated json");
    }


    [TestMethod]
    public void BoundaryRequestValidation_InvalidCustomerUid()
    {
      var requestFull =
        BoundaryRequestFull.Create
        (
          "sfgsdfsf",
          false,
          projectUid,
          userUid,
          new BoundaryRequest { BoundaryUid = boundaryUid, Name = Name, BoundaryPolygonWKT = GeometryWKT }
        );
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2027");
      StringAssert.Contains(ex.GetContent, "Invalid customerUid.");
    }

    [TestMethod]
    public void BoundaryRequestValidation_MissingUserId()
    {
      var requestFull =
        BoundaryRequestFull.Create
        (
          custUid,
          false,
          projectUid,
          string.Empty,
          new BoundaryRequest { BoundaryUid = boundaryUid, Name = Name, BoundaryPolygonWKT = GeometryWKT }
        );
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2028");
      StringAssert.Contains(ex.GetContent, "Invalid userUid.");
    }

    [TestMethod]
    public void BoundaryRequestValidation_MissingProjectUid()
    {
      var requestFull =
        BoundaryRequestFull.Create(custUid, false, null, userUid,
          new BoundaryRequest { BoundaryUid = boundaryUid, Name = Name, BoundaryPolygonWKT = GeometryWKT });
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2001");
      StringAssert.Contains(ex.GetContent, "Invalid projectUid.");
    }

    [TestMethod]
    public void BoundaryRequestValidation_InvalidBoundaryUid()
    {
      var requestFull =
        BoundaryRequestFull.Create(custUid, false, projectUid, userUid,
          new BoundaryRequest { BoundaryUid = "this is so wrong", Name = Name, BoundaryPolygonWKT = GeometryWKT });
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2059");
      StringAssert.Contains(ex.GetContent, "Invalid boundaryUid.");
    }

    [TestMethod]
    public void BoundaryRequestValidation_InvalidName()
    {
      var requestFull =
        BoundaryRequestFull.Create(custUid, false, projectUid, userUid,
          new BoundaryRequest { BoundaryUid = boundaryUid, Name = null, BoundaryPolygonWKT = GeometryWKT });

      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2003");
      StringAssert.Contains(ex.GetContent, "Invalid name. Should not be null.");
    }

    [TestMethod]
    public void BoundaryRequestValidation_InvalidBoundaryWKT()
    {
      var requestFull =
        BoundaryRequestFull.Create(custUid, false, projectUid, userUid,
          new BoundaryRequest { BoundaryUid = boundaryUid, Name = Name, BoundaryPolygonWKT = null });

      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2063");
      StringAssert.Contains(ex.GetContent, "Invalid boundary polygon WKT. Should not be null.");
    }
  }
}