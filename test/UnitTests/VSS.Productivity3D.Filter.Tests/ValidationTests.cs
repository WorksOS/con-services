using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.Utilities;
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

    private readonly string _invalidBoundary_NotClosed =
      "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965))";
    private readonly string _validBoundary =
      "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965,172.595831670724 -43.5427038560109))";


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
          new ProjectData {ProjectUid = projectUid},
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
          new ProjectData {ProjectUid = projectUid},
          new FilterRequest {FilterUid = filterUid, Name = Name, FilterJson = FilterJson, FilterType = filterType}
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
          new FilterRequest {FilterUid = filterUid, Name = Name, FilterJson = FilterJson, FilterType = filterType});
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2001");
      StringAssert.Contains(ex.GetContent, "Invalid projectUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterUid()
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData {ProjectUid = projectUid},
          new FilterRequest
          {
            FilterUid = "this is so wrong",
            Name = Name,
            FilterJson = FilterJson,
            FilterType = filterType
          });
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2002");
      StringAssert.Contains(ex.GetContent, "Invalid filterUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterUid_Null()
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData {ProjectUid = projectUid},
          new FilterRequest {FilterUid = null, Name = Name, FilterJson = string.Empty, FilterType = filterType});

      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    public void FilterRequestValidation_MissingName()
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData {ProjectUid = projectUid},
          new FilterRequest
          {
            FilterUid = filterUid,
            Name = string.Empty,
            FilterJson = string.Empty,
            FilterType = filterType
          });

      Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidName()
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData {ProjectUid = projectUid},
          new FilterRequest
          {
            FilterUid = filterUid,
            Name = null,
            FilterJson = string.Empty,
            FilterType = FilterType.Transient
          });

      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterJson()
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData {ProjectUid = projectUid},
          new FilterRequest {FilterUid = filterUid, Name = Name, FilterJson = null, FilterType = filterType});

      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("{ \"FilterUid\": \"00000000-0000-0000-0000-000000000000\" }")]
    public void FilterRequestValidation_Should_succeed_When_supplied_json_is_valid(string filterJson)
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData {ProjectUid = projectUid},
          new FilterRequest
          {
            FilterUid = filterUid,
            Name = string.Empty,
            FilterJson = filterJson,
            FilterType = FilterType.Transient
          });
      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    public void FilterRequestValidation_Should_fail_When_supplied_string_is_invalid_json()
    {
      var requestFull = FilterRequestFull.Create(null, custUid, false, userUid,
        new ProjectData {ProjectUid = projectUid},
        new FilterRequest {FilterUid = filterUid, Name = Name, FilterJson = "de blah", FilterType = filterType});
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2042");
      StringAssert.Contains(ex.GetContent, "Invalid filterJson. Exception: ");
    }

    [TestMethod]
    public void FilterRequestValidation_PartialFill()
    {
      var requestFull =
        FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData {ProjectUid = projectUid});

      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    public async Task CustomerProjectValidation_HappyPath()
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var projectData = new ProjectData {ProjectUid = projectUid, CustomerUid = custUid};

      var customHeaders = new Dictionary<string, string>();
      projectListProxy.Setup(ps => ps.GetProjectForCustomer(It.IsAny<string>(), projectUid, customHeaders))
        .ReturnsAsync(projectData);

      FilterPrincipal principal = new FilterPrincipal(new System.Security.Claims.ClaimsIdentity(),
        custUid, string.Empty, string.Empty, false, projectListProxy.Object, customHeaders);

      var actual = await principal.GetProject(projectUid);
      Assert.AreEqual(projectData, actual);
    }

    [TestMethod]
    public async Task CustomerProjectValidation_NoAssociation()
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var customHeaders = new Dictionary<string, string>();
      projectListProxy.Setup(ps => ps.GetProjectForCustomer(It.IsAny<string>(), It.IsAny<string>(), customHeaders))
        .ReturnsAsync((ProjectData) null);

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
      var projectData = new ProjectData {ProjectUid = projectUid, CustomerUid = custUid};
      var projects = new List<ProjectData> {projectData};

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

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData {ProjectUid = projectUid},
        new FilterRequest
        {
          FilterUid = filterUid,
          FilterType = filterType,
          FilterJson = "{\"designUID\": \"id\", \"vibeStateOn\": true}",
          Name = "a filter"
        });

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

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData {ProjectUid = projectUid},
        new FilterRequest
        {
          FilterUid = filterUid,
          Name = "a filter",
          FilterType = filterType,
          FilterJson = "{\"designUID\": \"id\", \"vibeStateOn\": true, \"alignmentUid\": \"" + alignmentUid +
                       "\", \"startStation\":" + startStation + ", \"endStation\":" + endStation + ", \"leftOffset\":" +
                       leftOffset + ", \"rightOffset\":" + rightOffset + "}"
        });

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
      geofenceRepo.Setup(g => g.GetGeofence(It.IsAny<string>())).ReturnsAsync((Geofence) null);

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData {ProjectUid = projectUid},
        new FilterRequest
        {
          FilterUid = filterUid,
          Name = "a filter",
          FilterType = filterType,
          FilterJson = "{\"designUID\": \"id\", \"vibeStateOn\": true, \"polygonUID\": \"" + boundaryUid + "\"}"
        });

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
      var geofence = new Geofence {GeometryWKT = "This is not a valid polygon WKT"};
      geofenceRepo.Setup(g => g.GetGeofence(It.IsAny<string>())).ReturnsAsync(geofence);

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData {ProjectUid = projectUid},
        new FilterRequest
        {
          FilterUid = filterUid,
          Name = "a filter",
          FilterType = filterType,
          FilterJson = "{\"designUID\": \"id\", \"vibeStateOn\": true, \"polygonUID\": \"" + boundaryUid + "\"}"
        });

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await ValidationUtil
        .HydrateJsonWithBoundary(geofenceRepo.Object, log, serviceExceptionHandler, request).ConfigureAwait(false));

      StringAssert.Contains(ex.GetContent, "2045");
      StringAssert.Contains(ex.GetContent,
        "Invalid spatial filter boundary. One or more polygon components are missing.");
    }

    [TestMethod]
    [DataRow(FilterType.Transient)]
    public async Task HydrateJsonWithBoundary_InvalidBoundary2(FilterType filterType)
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();
      var geofenceRepo = new Mock<IGeofenceRepository>();
      var geofence = new Geofence
      {
        GeometryWKT =
          "POLYGON((-115.0200886019198 36.20745605916501,-115.02005976817289 36.20734622441246,-115.01992699882665 36.2073559634608,-115.0198176988093 36.207342978062755,-115.01973320922532 36.20734027277125,-115.01974729082266 36.20738950906242,-115.01975466689743 36.2074300884,-115.01996052643932 36.20746201079744))"
      };
      // var geofence = new Geofence { GeometryWKT = "POLYGON((-115.02022874734084 36.20751287018342,-115.02025556943099 36.207300775504265,-115.02001953503766 36.20729428280093,-115.01966816565673 36.20726506562927,-115.01945493004004 36.20714170411769,-115.0192846097676 36.20734189594616,-115.01962927362601 36.20748581732266,-115.02022874734084 36.20751287018342))" };
      geofenceRepo.Setup(g => g.GetGeofence(It.IsAny<string>())).ReturnsAsync(geofence);

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData {ProjectUid = projectUid},
        new FilterRequest
        {
          FilterUid = filterUid,
          Name = "a filter",
          FilterType = filterType,
          FilterJson = "{\"designUID\": \"id\", \"vibeStateOn\": true, \"polygonUID\": \"" + boundaryUid + "\"}"
        });

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await ValidationUtil
        .HydrateJsonWithBoundary(geofenceRepo.Object, log, serviceExceptionHandler, request).ConfigureAwait(false));

      StringAssert.Contains(ex.GetContent, "2045");
      StringAssert.Contains(ex.GetContent,
        "Invalid spatial filter boundary. One or more polygon components are missing.");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    [DataRow(FilterType.Transient)]
    public void FilterRequestValidation_InvalidTemperatureRange(FilterType filterType)
    {
      var minTemperature = 100.5;

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData {ProjectUid = projectUid},
        new FilterRequest
        {
          FilterUid = filterUid,
          Name = "a filter",
          FilterType = filterType,
          FilterJson = "{\"vibeStateOn\": true, \"temperatureRangeMin\": \"" + minTemperature + "\", \"temperatureRangeMax\": null }"
        });

      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2072");
      StringAssert.Contains(ex.GetContent, "Invalid temperature range filter. Both minimum and maximum must be provided.");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    [DataRow(FilterType.Transient)]
    public void FilterRequestValidation_ValidTemperatureRange(FilterType filterType)
    {
      var minTemperature = 100.5;
      var maxTemperature = 120.9;

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
        new FilterRequest
        {
          FilterUid = filterUid,
          Name = "a filter",
          FilterType = filterType,
          FilterJson = "{\"vibeStateOn\": true, \"temperatureRangeMin\": \"" + minTemperature + "\", \"temperatureRangeMax\": \"" + maxTemperature + "\" }"
        });

      request.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    [DataRow(FilterType.Transient)]
    public void FilterRequestValidation_InvalidPassCountRange(FilterType filterType)
    {
      var maxPassCount = 10;

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
        new FilterRequest
        {
          FilterUid = filterUid,
          Name = "a filter",
          FilterType = filterType,
          FilterJson = "{\"vibeStateOn\": true, \"passCountRangeMin\": null, \"passCountRangeMax\": \"" + maxPassCount + "\" }"
        });

      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2073");
      StringAssert.Contains(ex.GetContent, "Invalid pass count range filter. Both minimum and maximum must be provided.");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    [DataRow(FilterType.Transient)]
    public void FilterRequestValidation_ValidPassCountRange(FilterType filterType)
    {
      var minPassCount = 5;
      var maxPassCount = 8;

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid },
        new FilterRequest
        {
          FilterUid = filterUid,
          Name = "a filter",
          FilterType = filterType,
          FilterJson = "{\"vibeStateOn\": true, \"passCountRangeMin\": \"" + minPassCount + "\", \"passCountRangeMax\": \"" + maxPassCount + "\" }"
        });

      request.Validate(serviceExceptionHandler);
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
        new FilterRequest
        {
          FilterUid = filterUid,
          Name = "a filter",
          FilterType = filterType,
          FilterJson = "{\"vibeStateOn\": true, \"alignmentUid\": \"" + alignmentUid + "\", \"startStation\":" +
                       startStation + ", \"endStation\": null, \"leftOffset\":" + leftOffset + ", \"rightOffset\":" +
                       rightOffset + "}"
        });

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
        new FilterRequest
        {
          FilterUid = filterUid,
          Name = "a filter",
          FilterType = filterType,
          FilterJson = "{\"vibeStateOn\": true, \"alignmentUid\": \"" + alignmentUid + "\", \"startStation\": " +
                       startStation + ", \"endStation\": " + endStation + ", \"leftOffset\": " + leftOffset +
                       ", \"rightOffset\": " + rightOffset + "}"
        });

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
      var geofence = new Geofence {GeofenceUID = boundaryUid, Name = Name, GeometryWKT = GeometryWKT};
      geofenceRepo.Setup(g => g.GetGeofence(It.IsAny<string>())).ReturnsAsync(geofence);

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData {ProjectUid = projectUid},
        new FilterRequest
        {
          FilterUid = filterUid,
          Name = "a filter",
          FilterType = filterType,
          FilterJson = "{\"designUid\": \"id\", \"vibeStateOn\": true, \"polygonUid\": \"" + geofence.GeofenceUID +
                       "\"}"
        });

      var result = await ValidationUtil
        .HydrateJsonWithBoundary(geofenceRepo.Object, log, serviceExceptionHandler, request).ConfigureAwait(false);

      var expectedResult =
        "{\"designUid\":\"id\",\"vibeStateOn\":true,\"polygonUid\":\"" + geofence.GeofenceUID +
        "\",\"polygonName\":\"" + geofence.Name +
        "\",\"polygonLL\":[{\"Lat\":12.677856,\"Lon\":80.257874},{\"Lat\":13.039345,\"Lon\":79.856873},{\"Lat\":13.443052,\"Lon\":80.375977}]}";

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
          new ProjectData() {ProjectUid = projectUid},
          userUid,
          new BoundaryRequest {BoundaryUid = boundaryUid, Name = Name, BoundaryPolygonWKT = GeometryWKT}
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
          new ProjectData() {ProjectUid = projectUid},
          string.Empty,
          new BoundaryRequest {BoundaryUid = boundaryUid, Name = Name, BoundaryPolygonWKT = GeometryWKT}
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
          new BoundaryRequest {BoundaryUid = boundaryUid, Name = Name, BoundaryPolygonWKT = GeometryWKT});
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2001");
      StringAssert.Contains(ex.GetContent, "Invalid projectUid.");
    }

    [TestMethod]
    public void BoundaryRequestValidation_InvalidBoundaryUid()
    {
      var requestFull =
        BoundaryRequestFull.Create(custUid, false, new ProjectData() {ProjectUid = projectUid}, userUid,
          new BoundaryRequest {BoundaryUid = "this is so wrong", Name = Name, BoundaryPolygonWKT = GeometryWKT});
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2059");
      StringAssert.Contains(ex.GetContent, "Invalid boundaryUid.");
    }

    [TestMethod]
    public void BoundaryRequestValidation_InvalidName()
    {
      var requestFull =
        BoundaryRequestFull.Create(custUid, false, new ProjectData() {ProjectUid = projectUid}, userUid,
          new BoundaryRequest {BoundaryUid = boundaryUid, Name = null, BoundaryPolygonWKT = GeometryWKT});

      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2003");
      StringAssert.Contains(ex.GetContent, "Invalid name. Should not be null.");
    }

    [TestMethod]
    public void BoundaryRequestValidation_BoundaryWKTMissing()
    {
      var requestFull =
        BoundaryRequestFull.Create(custUid, false, new ProjectData() {ProjectUid = projectUid}, userUid,
          new BoundaryRequest {BoundaryUid = boundaryUid, Name = Name, BoundaryPolygonWKT = null});

      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2069");
      StringAssert.Contains(ex.GetContent, "Invalid boundary polygon WKT. Should not be null.");
    }

    [TestMethod]
    public void BoundaryRequestValidation_BoundaryWKTLessThan3Points()
    {
      var requestFull =
        BoundaryRequestFull.Create(custUid, false, new ProjectData() {ProjectUid = projectUid}, userUid,
          new BoundaryRequest
          {
            BoundaryUid = boundaryUid,
            Name = Name,
            BoundaryPolygonWKT = "POLYGON((172.595831670724 -43.5427038560109))"
          });

      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2070");
      StringAssert.Contains(ex.GetContent, "Invalid boundary polygon WKT. Should be > 3 points.");
    }

    [TestMethod]
    public void BoundaryRequestValidation_BoundaryWKTInvalidFormat()
    {
      var requestFull =
        BoundaryRequestFull.Create(custUid, false, new ProjectData() {ProjectUid = projectUid}, userUid,
          new BoundaryRequest {BoundaryUid = boundaryUid, Name = Name, BoundaryPolygonWKT = "Nothing here"});

      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2071");
      StringAssert.Contains(ex.GetContent, "Invalid boundary polygon WKT. Invalid format.");
    }

    [TestMethod]
    public void BoundaryRequestValidation_Fix()
    {
      var wkt = GeofenceValidation.MakeGoodWkt(_invalidBoundary_NotClosed);
      Assert.AreEqual(_validBoundary, wkt, "Invalid conversion to wkt");
    }
  }
}