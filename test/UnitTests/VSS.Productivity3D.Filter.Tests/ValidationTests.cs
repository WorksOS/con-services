using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Common.Filters.Authentication;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Validators;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

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
    private const FilterType filterType = FilterType.Persistent;
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
          new ProjectData { ProjectUid = projectUid },
          new FilterRequest {FilterUid = filterUid, Name = Name, FilterJson = FilterJson, FilterType = filterType}
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
          new ProjectData { ProjectUid = projectUid },
          new FilterRequest {FilterUid = filterUid, Name = Name, FilterJson = FilterJson, FilterType = filterType }
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
          new FilterRequest {FilterUid = filterUid, Name = Name, FilterJson = FilterJson, FilterType = filterType });
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2001");
      StringAssert.Contains(ex.GetContent, "Invalid projectUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterUid()
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
          new FilterRequest {FilterUid = "this is so wrong", Name = Name, FilterJson = FilterJson, FilterType = filterType });
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2002");
      StringAssert.Contains(ex.GetContent, "Invalid filterUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterUid_Null()
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
          new FilterRequest {FilterUid = null, Name = Name, FilterJson = string.Empty, FilterType = filterType });

      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    public void FilterRequestValidation_MissingName()
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
          new FilterRequest { FilterUid = filterUid, Name = string.Empty, FilterJson = string.Empty, FilterType = filterType });

      Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidName()
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
          new FilterRequest {FilterUid = filterUid, Name = null, FilterJson = string.Empty, FilterType = FilterType.Transient });

      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterJson()
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
          new FilterRequest {FilterUid = filterUid, Name = Name, FilterJson = null, FilterType = filterType });

      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("{ \"FilterUid\": \"00000000-0000-0000-0000-000000000000\" }")]
    public void FilterRequestValidation_Should_succeed_When_supplied_json_is_valid(string filterJson)
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
          new FilterRequest {FilterUid = filterUid, Name = string.Empty, FilterJson = filterJson, FilterType = FilterType.Transient});
      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    public void FilterRequestValidation_Should_fail_When_supplied_string_is_invalid_json()
    {
      var requestFull = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
        new FilterRequest {FilterUid = filterUid, Name = Name, FilterJson = "de blah", FilterType = filterType });
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2042");
      StringAssert.Contains(ex.GetContent, "Invalid filterJson. Exception: ");
    }

    [TestMethod]
    public void FilterRequestValidation_PartialFill()
    {
      var requestFull = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid });

      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    public async Task CustomerProjectValidation_HappyPath()
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var projectData = new ProjectData { ProjectUid = projectUid, CustomerUid = custUid };

      var customHeaders = new Dictionary<string, string>();
      projectListProxy.Setup(ps => ps.GetProjectForCustomer(It.IsAny<string>(), projectUid, customHeaders)).ReturnsAsync(projectData);

      FilterPrincipal principal = new FilterPrincipal(new System.Security.Claims.ClaimsIdentity(),
        custUid, string.Empty,string.Empty, false, projectListProxy.Object, customHeaders);

      var actual = await principal.GetProject(projectUid);
      Assert.AreEqual(projectData, actual);
    }

    [TestMethod]
    public async Task CustomerProjectValidation_NoAssociation()
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var customHeaders = new Dictionary<string, string>();
      projectListProxy.Setup(ps => ps.GetProjectForCustomer(It.IsAny<string>(), It.IsAny<string>(), customHeaders)).ReturnsAsync((ProjectData)null);

      FilterPrincipal principal = new FilterPrincipal(new System.Security.Claims.ClaimsIdentity(),
        custUid, string.Empty, string.Empty, false, projectListProxy.Object, customHeaders);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(() => principal.GetProject(projectUid));

      StringAssert.Contains(ex.GetContent, "-5");
      StringAssert.Contains(ex.GetContent, "Missing Project or project does not belong to customer");
    }


    [TestMethod]
    [Ignore("This test logic needs to be moved to the proxies now the retry is done there")]
    public async Task CustomerProjectValidation_HappyPath_CacheSimulation()
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var projectData = new ProjectData { ProjectUid = projectUid, CustomerUid = custUid };
      var projects = new List<ProjectData> { projectData };

      var customHeaders = new Dictionary<string, string>();
      projectListProxy.SetupSequence(ps => ps.GetProjectsV4(It.IsAny<string>(), customHeaders))
        .ReturnsAsync(new List<ProjectData>())
        .ReturnsAsync(projects);

      FilterPrincipal principal = new FilterPrincipal(new System.Security.Claims.ClaimsIdentity(),
        custUid, string.Empty, string.Empty, false, projectListProxy.Object, customHeaders);

      var actual = await principal.GetProject(projectUid);
      Assert.AreEqual(projectData, actual);
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    [DataRow(FilterType.Transient)]
    public async Task HydrateJsonWithBoundary_NoPolygonUid(FilterType filterType)
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();
      var geofenceRepo = new Mock<IGeofenceRepository>();

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
        new FilterRequest {FilterUid = filterUid, FilterType = filterType, FilterJson = "{\"designUID\": \"id\", \"vibeStateOn\": true}", Name = "a filter" });

      var result = await ValidationUtil
        .HydrateJsonWithBoundary(geofenceRepo.Object, log, serviceExceptionHandler, request).ConfigureAwait(false);

      Assert.AreEqual(request.FilterJson, result, "Wrong hydated json");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    [DataRow(FilterType.Transient)]
    public async Task HydrateJsonWithBoundary_IncludeAlignment(FilterType filterType)
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();
      var geofenceRepo = new Mock<IGeofenceRepository>();
      var alignmentUid = Guid.NewGuid().ToString();
      var startStation = 100.456;
      var endStation = 200.5;
      var leftOffset = 4.5;
      var rightOffset = 2.5;

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
        new FilterRequest { FilterUid = filterUid, Name = "a filter", FilterType = filterType, FilterJson = "{\"designUID\": \"id\", \"vibeStateOn\": true, \"alignmentUid\": \"" + alignmentUid + "\", \"startStation\":" + startStation + ", \"endStation\":" + endStation + ", \"leftOffset\":" + leftOffset + ", \"rightOffset\":" + rightOffset + "}" });

      var result = await ValidationUtil
        .HydrateJsonWithBoundary(geofenceRepo.Object, log, serviceExceptionHandler, request).ConfigureAwait(false);

      Assert.AreEqual(request.FilterJson, result, "Wrong hydated json");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    [DataRow(FilterType.Transient)]
    public async Task HydrateJsonWithBoundary_NoGeofence(FilterType filterType)
    {      
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();
      var geofenceRepo = new Mock<IGeofenceRepository>();
      geofenceRepo.Setup(g => g.GetGeofence(It.IsAny<string>())).ReturnsAsync((Geofence)null);

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
        new FilterRequest { FilterUid = filterUid, Name = "a filter", FilterType = filterType, FilterJson = "{\"designUID\": \"id\", \"vibeStateOn\": true, \"polygonUID\": \"" + boundaryUid + "\"}" });

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await ValidationUtil
        .HydrateJsonWithBoundary(geofenceRepo.Object, log, serviceExceptionHandler, request).ConfigureAwait(false));

      StringAssert.Contains(ex.GetContent, "2040");
      StringAssert.Contains(ex.GetContent, "Validation of Project/Boundary failed. Not allowed.");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    [DataRow(FilterType.Transient)]
    public async Task HydrateJsonWithBoundary_InvalidBoundary(FilterType filterType)
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();
      var geofenceRepo = new Mock<IGeofenceRepository>();
      var geofence = new Geofence{GeometryWKT = "This is not a valid polygon WKT"};
      geofenceRepo.Setup(g => g.GetGeofence(It.IsAny<string>())).ReturnsAsync(geofence);

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
        new FilterRequest { FilterUid = filterUid, Name = "a filter", FilterType = filterType, FilterJson = "{\"designUID\": \"id\", \"vibeStateOn\": true, \"polygonUID\": \"" + boundaryUid + "\"}" });

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await ValidationUtil
        .HydrateJsonWithBoundary(geofenceRepo.Object, log, serviceExceptionHandler, request).ConfigureAwait(false));

      StringAssert.Contains(ex.GetContent, "2045");
      StringAssert.Contains(ex.GetContent, "Invalid spatial filter boundary. One or more polygon components are missing.");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    [DataRow(FilterType.Transient)]
    public void FilterRequestValidation_InvalidAlignment(FilterType filterType)
    {
      var alignmentUid = Guid.NewGuid().ToString();
      var startStation = 100.456;
      var leftOffset = 4.5;
      var rightOffset = 2.5;

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
        new FilterRequest { FilterUid = filterUid, Name = "a filter", FilterType = filterType, FilterJson = "{\"vibeStateOn\": true, \"alignmentUid\": \"" + alignmentUid + "\", \"startStation\":" + startStation + ", \"endStation\": null, \"leftOffset\":" + leftOffset + ", \"rightOffset\":" + rightOffset + "}" });

      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2065");
      StringAssert.Contains(ex.GetContent, "Invalid alignment filter. Start or end station are invalid.");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    [DataRow(FilterType.Transient)]
    public void FilterRequestValidation_ValidAlignment(FilterType filterType)
    {
      var alignmentUid = Guid.NewGuid().ToString();
      var startStation = 12.456;
      double endStation = 56.0;
      var leftOffset = 4.5;
      var rightOffset = 2.5;

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
        new FilterRequest { FilterUid = filterUid, Name = "a filter", FilterType = filterType, FilterJson = "{\"vibeStateOn\": true, \"alignmentUid\": \"" + alignmentUid + "\", \"startStation\": " + startStation + ", \"endStation\": " + endStation + ", \"leftOffset\": " + leftOffset + ", \"rightOffset\": " + rightOffset + "}" });

      request.Validate(serviceExceptionHandler);
    }
    
    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    [DataRow(FilterType.Transient)]
    public async Task HydrateJsonWithBoundary_HappyPath(FilterType filterType)
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();
      var geofenceRepo = new Mock<IGeofenceRepository>();
      var geofence = new Geofence { GeofenceUID = boundaryUid, Name = Name, GeometryWKT = GeometryWKT };
      geofenceRepo.Setup(g => g.GetGeofence(It.IsAny<string>())).ReturnsAsync(geofence);

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
        new FilterRequest { FilterUid = filterUid, Name = "a filter", FilterType = filterType, FilterJson = "{\"designUid\": \"id\", \"vibeStateOn\": true, \"polygonUid\": \"" + geofence.GeofenceUID + "\"}" });

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
          new ProjectData() { ProjectUid = projectUid },
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
          new ProjectData() { ProjectUid = projectUid },
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
        BoundaryRequestFull.Create(custUid, false, new ProjectData() { ProjectUid = projectUid }, userUid,
          new BoundaryRequest { BoundaryUid = "this is so wrong", Name = Name, BoundaryPolygonWKT = GeometryWKT });
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2059");
      StringAssert.Contains(ex.GetContent, "Invalid boundaryUid.");
    }

    [TestMethod]
    public void BoundaryRequestValidation_InvalidName()
    {
      var requestFull =
        BoundaryRequestFull.Create(custUid, false, new ProjectData() { ProjectUid = projectUid }, userUid,
          new BoundaryRequest { BoundaryUid = boundaryUid, Name = null, BoundaryPolygonWKT = GeometryWKT });

      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2003");
      StringAssert.Contains(ex.GetContent, "Invalid name. Should not be null.");
    }

    [TestMethod]
    public void BoundaryRequestValidation_InvalidBoundaryWKT()
    {
      var requestFull =
        BoundaryRequestFull.Create(custUid, false, new ProjectData() { ProjectUid = projectUid }, userUid,
          new BoundaryRequest { BoundaryUid = boundaryUid, Name = Name, BoundaryPolygonWKT = null });

      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2063");
      StringAssert.Contains(ex.GetContent, "Invalid boundary polygon WKT. Should not be null.");
    }
  }
}