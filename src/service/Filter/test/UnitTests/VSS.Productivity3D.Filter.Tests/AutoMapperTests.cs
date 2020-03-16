using System;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.Productivity3D.Filter.Tests
{
  public class AutoMapperTests
  {
    [Fact]
    public void AssertConfigurationIsValid()
    {
      var exception = Record.Exception(() => AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid());
      Assert.Null(exception);
    }

    [Theory]
    [InlineData(FilterType.Persistent)]
    [InlineData(FilterType.Report)]
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
      Assert.Equal(filter.FilterUid, result.FilterUid);
      Assert.Equal(filter.Name, result.Name);
      Assert.Equal(filter.FilterJson, result.FilterJson);
      Assert.Equal(filter.FilterType, result.FilterType);
    }

    [Theory]
    [InlineData(FilterType.Persistent)]
    [InlineData(FilterType.Report)]
    public void MapFilterRequestToCreateKafkaEvent_UserContext(FilterType filterType)
    {
      var filterRequest = FilterRequestFull.Create
      (
        null,
        Guid.NewGuid().ToString(),
        false,
        Guid.NewGuid().ToString(),
        new ProjectData { ProjectUID = Guid.NewGuid().ToString() },
        new FilterRequest { FilterUid = Guid.NewGuid().ToString(), Name = "the name", FilterJson = "the Json", FilterType = filterType }

      );

      var result = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
      Assert.Equal(filterRequest.CustomerUid, result.CustomerUID.ToString());
      Assert.Equal(filterRequest.UserId, result.UserID);
      Assert.Equal(filterRequest.ProjectUid, result.ProjectUID.ToString());
      Assert.Equal(filterRequest.FilterUid, result.FilterUID.ToString());
      Assert.Equal(filterRequest.Name, result.Name);
      Assert.Equal(filterRequest.FilterJson, result.FilterJson);
      Assert.Equal(filterRequest.FilterType, result.FilterType);
    }

    [Theory]
    [InlineData(FilterType.Persistent)]
    [InlineData(FilterType.Report)]
    public void MapFilterRequestToCreateKafkaEvent_UserContext_NoFilterUID(FilterType filterType)
    {
      var filterRequest = FilterRequestFull.Create
      (
        null,
        Guid.NewGuid().ToString(),
        false,
        Guid.NewGuid().ToString(),
        new ProjectData { ProjectUID = Guid.NewGuid().ToString() },
        new FilterRequest { FilterUid = Guid.NewGuid().ToString(), Name = "the name", FilterJson = "the Json", FilterType = filterType }
      );
      filterRequest.FilterUid = Guid.NewGuid().ToString();

      var result = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
      Assert.Equal(filterRequest.CustomerUid, result.CustomerUID.ToString());
      Assert.Equal(filterRequest.UserId, result.UserID);
      Assert.Equal(filterRequest.ProjectUid, result.ProjectUID.ToString());
      Assert.Equal(filterRequest.FilterUid, result.FilterUID.ToString());
      Assert.Equal(filterRequest.Name, result.Name);
      Assert.Equal(filterRequest.FilterJson, result.FilterJson);
      Assert.Equal(filterRequest.FilterType, result.FilterType);
    }

    [Theory]
    [InlineData(FilterType.Persistent)]
    [InlineData(FilterType.Report)]
    public void MapFilterRequestToCreateKafkaEvent_ApplicationContext(FilterType filterType)
    {
      var filterRequest = FilterRequestFull.Create
      (
        null,
        Guid.NewGuid().ToString(),
        false,
        "ApplicationName",
        new ProjectData { ProjectUID = Guid.NewGuid().ToString() },
        new FilterRequest { FilterUid = Guid.NewGuid().ToString(), Name = "the name", FilterJson = "the Json", FilterType = filterType }
      );

      var result = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
      Assert.Equal(filterRequest.CustomerUid, result.CustomerUID.ToString());
      Assert.Equal(filterRequest.UserId, result.UserID);
      Assert.Equal(filterRequest.ProjectUid, result.ProjectUID.ToString());
      Assert.Equal(filterRequest.FilterUid, result.FilterUID.ToString());
      Assert.Equal(filterRequest.Name, result.Name);
      Assert.Equal(filterRequest.FilterJson, result.FilterJson);
      Assert.Equal(filterRequest.FilterType, result.FilterType);
    }

    [Theory]
    [InlineData(FilterType.Persistent)]
    [InlineData(FilterType.Report)]
    public void MapFilterRequestToCreateKafkaEvent_ApplicationContext_NoFilterUID(FilterType filterType)
    {
      var filterRequest = FilterRequestFull.Create
      (
        null,
        Guid.NewGuid().ToString(),
        false,
        "ApplicationName",
        new ProjectData { ProjectUID = Guid.NewGuid().ToString() },
        new FilterRequest { FilterUid = Guid.NewGuid().ToString(), Name = "the name", FilterJson = "the Json", FilterType = filterType }
      );
      filterRequest.FilterUid = Guid.NewGuid().ToString();

      var result = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
      Assert.Equal(filterRequest.CustomerUid, result.CustomerUID.ToString());
      Assert.Equal(filterRequest.UserId, result.UserID);
      Assert.Equal(filterRequest.ProjectUid, result.ProjectUID.ToString());
      Assert.Equal(filterRequest.FilterUid, result.FilterUID.ToString());
      Assert.Equal(filterRequest.Name, result.Name);
      Assert.Equal(filterRequest.FilterJson, result.FilterJson);
      Assert.Equal(filterRequest.FilterType, result.FilterType);
    }

    [Theory]
    [InlineData(FilterType.Persistent)]
    [InlineData(FilterType.Report)]
    public void MapFilterRequestToUpdateKafkaEvent(FilterType filterType)
    {
      var filterRequest = FilterRequestFull.Create
      (
        null,
        Guid.NewGuid().ToString(),
        false,
        Guid.NewGuid().ToString(),
        new ProjectData { ProjectUID = Guid.NewGuid().ToString() },
        new FilterRequest { FilterUid = Guid.NewGuid().ToString(), Name = "the name", FilterJson = "the Json", FilterType = filterType }
      );

      var result = AutoMapperUtility.Automapper.Map<UpdateFilterEvent>(filterRequest);
      Assert.Equal(filterRequest.CustomerUid, result.CustomerUID.ToString());
      Assert.Equal(filterRequest.UserId, result.UserID);
      Assert.Equal(filterRequest.ProjectUid, result.ProjectUID.ToString());
      Assert.Equal(filterRequest.FilterUid, result.FilterUID.ToString());
      Assert.Equal(filterRequest.Name, result.Name);
      Assert.Equal(filterRequest.FilterJson, result.FilterJson);
      Assert.Equal(filterRequest.FilterType, result.FilterType);
    }

    [Theory]
    [InlineData(FilterType.Persistent)]
    [InlineData(FilterType.Report)]
    public void MapFilterRequestToDeleteKafkaEvent(FilterType filterType)
    {
      var filterRequest = FilterRequestFull.Create
      (
        null,
        Guid.NewGuid().ToString(),
        false,
        Guid.NewGuid().ToString(),
        new ProjectData { ProjectUID = Guid.NewGuid().ToString() },
        new FilterRequest { FilterUid = Guid.NewGuid().ToString(), Name = "the name", FilterJson = "the Json", FilterType = filterType }
      );

      var result = AutoMapperUtility.Automapper.Map<DeleteFilterEvent>(filterRequest);
      Assert.Equal(filterRequest.CustomerUid, result.CustomerUID.ToString());
      Assert.Equal(filterRequest.UserId, result.UserID);
      Assert.Equal(filterRequest.ProjectUid, result.ProjectUID.ToString());
      Assert.Equal(filterRequest.FilterUid, result.FilterUID.ToString());
    }

    [Theory]
    [InlineData(FilterType.Persistent)]
    [InlineData(FilterType.Report)]
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
      Assert.Equal(filter.CustomerUid, result.CustomerUID.ToString());
      Assert.Equal(filter.UserId, result.UserID);
      Assert.Equal(filter.ProjectUid, result.ProjectUID.ToString());
      Assert.Equal(filter.FilterUid, result.FilterUID.ToString());
    }

    [Fact]
    public void MapProjectGeofenceRequestToAssociateKafkaEvent()
    {
      var request = ProjectGeofenceRequest.Create(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
   
      var result = AutoMapperUtility.Automapper.Map<AssociateProjectGeofence>(request);
      Assert.Equal(request.ProjectUid, result.ProjectUID.ToString());
      Assert.Equal(request.BoundaryUid, result.GeofenceUID.ToString());
    }

    [Fact]
    public void MapBoundaryDBModelToResult()
    {
      var geofence = new Geofence
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
      Assert.Equal(geofence.GeofenceUID, result.GeofenceUID.ToString());
      Assert.Equal(geofence.Name, result.GeofenceName);
      Assert.Equal(geofence.GeometryWKT, result.GeometryWKT);
      Assert.Equal(geofence.GeofenceType.ToString(), result.GeofenceType);
    }

    [Fact]
    public void MapBoundaryRequestToDeleteKafkaEvent()
    {
      var request = BoundaryUidRequestFull.Create
      (
        Guid.NewGuid().ToString(),
        false,
        new ProjectData { ProjectUID = Guid.NewGuid().ToString() },
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString()
      );

      var result = AutoMapperUtility.Automapper.Map<DeleteGeofenceEvent>(request);
      Assert.Equal(request.UserUid, result.UserUID.ToString());
      Assert.Equal(request.BoundaryUid, result.GeofenceUID.ToString());
    }

    [Fact]
    public void MapBoundaryRequestToCreateKafkaEvent()
    {
      var request = BoundaryRequestFull.Create
      (
        Guid.NewGuid().ToString(),
        false,
        new ProjectData { ProjectUID = Guid.NewGuid().ToString() },
        Guid.NewGuid().ToString(),
        new BoundaryRequest { BoundaryUid = Guid.NewGuid().ToString(), Name = "the name", BoundaryPolygonWKT = "the WKT" }
      );

      var result = AutoMapperUtility.Automapper.Map<CreateGeofenceEvent>(request);
      Assert.Equal(request.CustomerUid, result.CustomerUID.ToString());
      Assert.Equal(request.UserUid, result.UserUID.ToString());
      Assert.Equal(request.Request.BoundaryUid, result.GeofenceUID.ToString());
      Assert.Equal(request.Request.Name, result.GeofenceName);
      Assert.Equal(request.Request.BoundaryPolygonWKT, result.GeometryWKT);
      Assert.Equal(request.GeofenceType.ToString(), result.GeofenceType);
    }

    [Fact]
    public void MapBoundaryRequestToUpdateKafkaEvent()
    {
      var request = BoundaryRequestFull.Create
      (
        Guid.NewGuid().ToString(),
        false,
        new ProjectData { ProjectUID = Guid.NewGuid().ToString() },
        Guid.NewGuid().ToString(),
        new BoundaryRequest { BoundaryUid = Guid.NewGuid().ToString(), Name = "the name", BoundaryPolygonWKT = "the WKT" }
      );

      var result = AutoMapperUtility.Automapper.Map<UpdateGeofenceEvent>(request);
      Assert.Equal(request.UserUid, result.UserUID.ToString());
      Assert.Equal(request.Request.BoundaryUid, result.GeofenceUID.ToString());
      Assert.Equal(request.Request.Name, result.GeofenceName);
      Assert.Equal(request.Request.BoundaryPolygonWKT, result.GeometryWKT);
      Assert.Equal(request.GeofenceType.ToString(), result.GeofenceType);
    }

    [Fact]
    public void MapGeofenceDataToGeofence()
    {
      var geofenceData = new GeofenceData
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "whatever",
        GeofenceType = GeofenceType.CutZone.ToString(),
        GeometryWKT = "blah",
        CustomerUID = Guid.NewGuid(),
        UserUID = Guid.NewGuid(),
        IsTransparent = true,
        Description = null,
        AreaSqMeters = 1.234,
        FillColor = 0,
      };
      var result = AutoMapperUtility.Automapper.Map<Geofence>(geofenceData);
      Assert.Equal(geofenceData.GeofenceUID.ToString(), result.GeofenceUID);
      Assert.Equal(geofenceData.GeofenceName, result.Name);
      Assert.Equal(geofenceData.GeofenceType, result.GeofenceType.ToString());
      Assert.Equal(geofenceData.GeometryWKT, result.GeometryWKT);
      Assert.Equal(geofenceData.CustomerUID.ToString(), result.CustomerUID);
      Assert.Equal(geofenceData.UserUID.ToString(), result.UserUID);
      Assert.Equal(geofenceData.IsTransparent, result.IsTransparent);
      Assert.Equal(geofenceData.Description, result.Description);
      Assert.Equal(geofenceData.AreaSqMeters, result.AreaSqMeters);
      Assert.Equal(geofenceData.FillColor, result.FillColor);
    }
  }
}
