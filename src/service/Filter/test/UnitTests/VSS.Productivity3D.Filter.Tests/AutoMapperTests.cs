using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class AutoMapperTests
  {
    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public void MapFilterDBModelToResult(FilterType filterType)
    {
      var filter = new MasterData.Repositories.DBModels.Filter
      {
        CustomerUid = Guid.NewGuid().ToString(),
        UserId = Guid.NewGuid().ToString(),
        ProjectUid = Guid.NewGuid().ToString(),
        FilterUid = Guid.NewGuid().ToString(),
        FilterType = filterType,
        Name = "the name",
        FilterJson = "the Json",
        IsDeleted = false,
        LastActionedUtc = new DateTime(2017, 01, 21)
      };

      var result = AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter);
      Assert.AreEqual(filter.FilterUid, result.FilterUid, "filterUid has not been mapped correctly");
      Assert.AreEqual(filter.Name, result.Name, "name has not been mapped correctly");
      Assert.AreEqual(filter.FilterJson, result.FilterJson, "FilterJson has not been mapped correctly");
      Assert.AreEqual(filter.FilterType, result.FilterType, "FilterType has not been mapped correctly");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public void MapFilterRequestToCreateKafkaEvent_UserContext(FilterType filterType)
    {
      var filterRequest = FilterRequestFull.Create
      (
        null,
        Guid.NewGuid().ToString(),
        false,
        Guid.NewGuid().ToString(),
        new ProjectData { ProjectUid = Guid.NewGuid().ToString() },
        new FilterRequest { FilterUid = Guid.NewGuid().ToString(), Name = "the name", FilterJson = "the Json", FilterType = filterType }

      );

      var result = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
      Assert.AreEqual(filterRequest.CustomerUid, result.CustomerUID.ToString(),
        "CustomerUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.UserId, result.UserID, "UserUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.ProjectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.FilterUid, result.FilterUID.ToString(), "filterUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.Name, result.Name, "name has not been mapped correctly");
      Assert.AreEqual(filterRequest.FilterJson, result.FilterJson, "FilterJson has not been mapped correctly");
      Assert.AreEqual(filterRequest.FilterType, result.FilterType, "FilterType has not been mapped correctly");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public void MapFilterRequestToCreateKafkaEvent_UserContext_NoFilterUID(FilterType filterType)
    {
      var filterRequest = FilterRequestFull.Create
      (
        null,
        Guid.NewGuid().ToString(),
        false,
        Guid.NewGuid().ToString(),
        new ProjectData { ProjectUid = Guid.NewGuid().ToString() },
        new FilterRequest { FilterUid = Guid.NewGuid().ToString(), Name = "the name", FilterJson = "the Json", FilterType = filterType }
      );
      filterRequest.FilterUid = Guid.NewGuid().ToString();

      var result = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
      Assert.AreEqual(filterRequest.CustomerUid, result.CustomerUID.ToString(),
        "CustomerUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.UserId, result.UserID, "UserUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.ProjectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.FilterUid, result.FilterUID.ToString(), "filterUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.Name, result.Name, "name has not been mapped correctly");
      Assert.AreEqual(filterRequest.FilterJson, result.FilterJson, "FilterJson has not been mapped correctly");
      Assert.AreEqual(filterRequest.FilterType, result.FilterType, "FilterType has not been mapped correctly");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public void MapFilterRequestToCreateKafkaEvent_ApplicationContext(FilterType filterType)
    {
      var filterRequest = FilterRequestFull.Create
      (
        null,
        Guid.NewGuid().ToString(),
        false,
        "ApplicationName",
        new ProjectData { ProjectUid = Guid.NewGuid().ToString() },
        new FilterRequest { FilterUid = Guid.NewGuid().ToString(), Name = "the name", FilterJson = "the Json", FilterType = filterType }
      );

      var result = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
      Assert.AreEqual(filterRequest.CustomerUid, result.CustomerUID.ToString(),
        "CustomerUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.UserId, result.UserID, "UserUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.ProjectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.FilterUid, result.FilterUID.ToString(), "filterUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.Name, result.Name, "name has not been mapped correctly");
      Assert.AreEqual(filterRequest.FilterJson, result.FilterJson, "FilterJson has not been mapped correctly");
      Assert.AreEqual(filterRequest.FilterType, result.FilterType, "FilterType has not been mapped correctly");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public void MapFilterRequestToCreateKafkaEvent_ApplicationContext_NoFilterUID(FilterType filterType)
    {
      var filterRequest = FilterRequestFull.Create
      (
        null,
        Guid.NewGuid().ToString(),
        false,
        "ApplicationName",
        new ProjectData { ProjectUid = Guid.NewGuid().ToString() },
        new FilterRequest { FilterUid = Guid.NewGuid().ToString(), Name = "the name", FilterJson = "the Json", FilterType = filterType }
      );
      filterRequest.FilterUid = Guid.NewGuid().ToString();

      var result = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
      Assert.AreEqual(filterRequest.CustomerUid, result.CustomerUID.ToString(),
        "CustomerUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.UserId, result.UserID, "UserUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.ProjectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.FilterUid, result.FilterUID.ToString(), "filterUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.Name, result.Name, "name has not been mapped correctly");
      Assert.AreEqual(filterRequest.FilterJson, result.FilterJson, "FilterJson has not been mapped correctly");
      Assert.AreEqual(filterRequest.FilterType, result.FilterType, "FilterType has not been mapped correctly");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public void MapFilterRequestToUpdateKafkaEvent(FilterType filterType)
    {
      var filterRequest = FilterRequestFull.Create
      (
        null,
        Guid.NewGuid().ToString(),
        false,
        Guid.NewGuid().ToString(),
        new ProjectData { ProjectUid = Guid.NewGuid().ToString() },
        new FilterRequest { FilterUid = Guid.NewGuid().ToString(), Name = "the name", FilterJson = "the Json", FilterType = filterType }
      );

      var result = AutoMapperUtility.Automapper.Map<UpdateFilterEvent>(filterRequest);
      Assert.AreEqual(filterRequest.CustomerUid, result.CustomerUID.ToString(),
        "CustomerUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.UserId, result.UserID, "UserUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.ProjectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.FilterUid, result.FilterUID.ToString(), "filterUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.Name, result.Name, "name has not been mapped correctly");
      Assert.AreEqual(filterRequest.FilterJson, result.FilterJson, "FilterJson has not been mapped correctly");
      Assert.AreEqual(filterRequest.FilterType, result.FilterType, "FilterType has not been mapped correctly");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public void MapFilterRequestToDeleteKafkaEvent(FilterType filterType)
    {
      var filterRequest = FilterRequestFull.Create
      (
        null,
        Guid.NewGuid().ToString(),
        false,
        Guid.NewGuid().ToString(),
        new ProjectData { ProjectUid = Guid.NewGuid().ToString() },
        new FilterRequest { FilterUid = Guid.NewGuid().ToString(), Name = "the name", FilterJson = "the Json", FilterType = filterType }
      );

      var result = AutoMapperUtility.Automapper.Map<DeleteFilterEvent>(filterRequest);
      Assert.AreEqual(filterRequest.CustomerUid, result.CustomerUID.ToString(),
        "CustomerUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.UserId, result.UserID, "UserUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.ProjectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.FilterUid, result.FilterUID.ToString(), "filterUid has not been mapped correctly");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public void MapFilterDBModelRequestToDeleteKafkaEvent(FilterType filterType)
    {
      var filter = new MasterData.Repositories.DBModels.Filter
      {
        CustomerUid = Guid.NewGuid().ToString(),
        UserId = Guid.NewGuid().ToString(),
        ProjectUid = Guid.NewGuid().ToString(),
        FilterUid = Guid.NewGuid().ToString(),
        FilterType = filterType,
        Name = "the name",
        FilterJson = "the Json",
        IsDeleted = false,
        LastActionedUtc = new DateTime(2017, 01, 21)
      };

      var result = AutoMapperUtility.Automapper.Map<DeleteFilterEvent>(filter);
      Assert.AreEqual(filter.CustomerUid, result.CustomerUID.ToString(),
        "CustomerUid has not been mapped correctly");
      Assert.AreEqual(filter.UserId, result.UserID, "UserUid has not been mapped correctly");
      Assert.AreEqual(filter.ProjectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filter.FilterUid, result.FilterUID.ToString(), "filterUid has not been mapped correctly");
    }

    [TestMethod]
    public void MapProjectGeofenceRequestToAssociateKafkaEvent()
    {
      var request = ProjectGeofenceRequest.Create(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
   
      var result = AutoMapperUtility.Automapper.Map<AssociateProjectGeofence>(request);
      Assert.AreEqual(request.ProjectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(request.BoundaryUid, result.GeofenceUID.ToString(), "BoundaryUid has not been mapped correctly");
    }

    [TestMethod]
    public void MapBoundaryDBModelToResult()
    {
      var geofence = new MasterData.Repositories.DBModels.Geofence
      {      
        CustomerUID = Guid.NewGuid().ToString(),
        UserUID = Guid.NewGuid().ToString(),
        GeofenceUID = Guid.NewGuid().ToString(),
        Name = "the name",
        Description = string.Empty,
        GeofenceType = GeofenceType.Filter,
        GeometryWKT = "POLYGON((80.257874 12.677856,79.856873 13.039345,80.375977 13.443052,80.257874 12.677856))",
        IsDeleted = false,
        LastActionedUTC = new DateTime(2017, 01, 21)
      };

      var result = AutoMapperUtility.Automapper.Map<GeofenceData>(geofence);
      Assert.AreEqual(geofence.GeofenceUID, result.GeofenceUID.ToString(), "GeofenceUID has not been mapped correctly");
      Assert.AreEqual(geofence.Name, result.GeofenceName, "Name has not been mapped correctly");
      Assert.AreEqual(geofence.GeometryWKT, result.GeometryWKT, "GeometryWKT has not been mapped correctly");
      Assert.AreEqual(geofence.GeofenceType.ToString(), result.GeofenceType, "GeofenceType has not been mapped correctly");
    }

    [TestMethod]
    public void MapBoundaryRequestToDeleteKafkaEvent()
    {
      var request = BoundaryUidRequestFull.Create
      (
        Guid.NewGuid().ToString(),
        false,
        new ProjectData() { ProjectUid = Guid.NewGuid().ToString() },
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString()
      );

      var result = AutoMapperUtility.Automapper.Map<DeleteGeofenceEvent>(request);
      Assert.AreEqual(request.UserUid, result.UserUID.ToString(), "UserUid has not been mapped correctly");
      Assert.AreEqual(request.BoundaryUid, result.GeofenceUID.ToString(),
        "BoundaryUid has not been mapped correctly");
    }

    [TestMethod]
    public void MapBoundaryRequestToCreateKafkaEvent()
    {
      var request = BoundaryRequestFull.Create
      (
        Guid.NewGuid().ToString(),
        false,
        new ProjectData() { ProjectUid = Guid.NewGuid().ToString() },
        Guid.NewGuid().ToString(),
        new BoundaryRequest { BoundaryUid = Guid.NewGuid().ToString(), Name = "the name", BoundaryPolygonWKT = "the WKT" }
      );

      var result = AutoMapperUtility.Automapper.Map<CreateGeofenceEvent>(request);
      Assert.AreEqual(request.CustomerUid, result.CustomerUID.ToString(), "CustomerUid has not been mapped correctly");
      Assert.AreEqual(request.UserUid, result.UserUID.ToString(), "UserUid has not been mapped correctly");
      Assert.AreEqual(request.Request.BoundaryUid, result.GeofenceUID.ToString(),
        "BoundaryUid has not been mapped correctly");
      Assert.AreEqual(request.Request.Name, result.GeofenceName,
        "Name has not been mapped correctly");
      Assert.AreEqual(request.Request.BoundaryPolygonWKT, result.GeometryWKT,
        "BoundaryPolygonWKT has not been mapped correctly");
      Assert.AreEqual(request.GeofenceType.ToString(), result.GeofenceType,
        "GeofenceType has not been mapped correctly");
    }

    [TestMethod]
    public void MapBoundaryRequestToUpdateKafkaEvent()
    {
      var request = BoundaryRequestFull.Create
      (
        Guid.NewGuid().ToString(),
        false,
        new ProjectData() { ProjectUid = Guid.NewGuid().ToString() },
        Guid.NewGuid().ToString(),
        new BoundaryRequest { BoundaryUid = Guid.NewGuid().ToString(), Name = "the name", BoundaryPolygonWKT = "the WKT" }
      );

      var result = AutoMapperUtility.Automapper.Map<UpdateGeofenceEvent>(request);
      Assert.AreEqual(request.UserUid, result.UserUID.ToString(), "UserUid has not been mapped correctly");
      Assert.AreEqual(request.Request.BoundaryUid, result.GeofenceUID.ToString(),
        "BoundaryUid has not been mapped correctly");
      Assert.AreEqual(request.Request.Name, result.GeofenceName,
        "Name has not been mapped correctly");
      Assert.AreEqual(request.Request.BoundaryPolygonWKT, result.GeometryWKT,
        "BoundaryPolygonWKT has not been mapped correctly");
      Assert.AreEqual(request.GeofenceType.ToString(), result.GeofenceType,
        "GeofenceType has not been mapped correctly");
    }
  }
}