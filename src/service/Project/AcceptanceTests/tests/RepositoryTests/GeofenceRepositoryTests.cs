using System;
using System.Linq;
using RepositoryTests.Internal;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Repository;
using VSS.Productivity3D.Project.Repository;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace RepositoryTests
{
  public class GeofenceRepositoryTests : TestControllerBase
  {
    private const string GeometryWKT =
      "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))";
    GeofenceRepository geofenceRepo = null;
    ProjectRepository projectRepo = null;

    public GeofenceRepositoryTests()
    {
      SetupLogging();
      geofenceRepo = new GeofenceRepository(configStore, loggerFactory);
      projectRepo = new ProjectRepository(configStore, loggerFactory);      
    }

    #region Geofence

    /// <summary>
    /// Create Geofence - Happy path 
    ///   geofence doesn't exist.
    /// </summary>
    [Fact]
    public void CreateGeofence_HappyPath()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid().ToString();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = GeometryWKT,
        CustomerUID = new Guid(customerUid),
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var s = geofenceRepo.StoreEvent(createGeofenceEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = geofenceRepo.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      var projectGeofences = g.Result.ToList();
      Assert.Single(projectGeofences);
      Assert.Equal(createGeofenceEvent.GeofenceUID.ToString(), projectGeofences[0].GeofenceUID);
    }

    /// <summary>
    /// Create Geofence - Happy path 
    ///   null description
    /// </summary>
    [Fact]
    public void CreateGeofence_NullDescription()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid().ToString();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = null,
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = GeometryWKT,
        CustomerUID = new Guid(customerUid),
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var s = geofenceRepo.StoreEvent(createGeofenceEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = geofenceRepo.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      var projectGeofences = g.Result.ToList();
      Assert.Single(projectGeofences);
      Assert.Equal(createGeofenceEvent.GeofenceUID.ToString(), projectGeofences[0].GeofenceUID);
    }

    /// <summary>
    /// Create Geofenced - Already exists
    ///   geofence exists already.
    /// </summary>
    [Fact]
    public void CreateGeofence_AlreadyExists()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid().ToString();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = GeometryWKT,
        CustomerUID = new Guid(customerUid),
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      geofenceRepo.StoreEvent(createGeofenceEvent).Wait();
      var s = geofenceRepo.StoreEvent(createGeofenceEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = geofenceRepo.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      var projectGeofences = g.Result.ToList();
      Assert.Single(projectGeofences);
      Assert.Equal(createGeofenceEvent.GeofenceUID.ToString(), projectGeofences[0].GeofenceUID);
    }


    /// <summary>
    /// Update Geofence - happyPath
    /// exists, just update whichever fields are provided.
    /// </summary>
    [Fact]
    public void UpdateGeofence_HappyPath_FieldsChanged()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid().ToString();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = GeometryWKT,
        CustomerUID = new Guid(customerUid),
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var updateGeofenceEvent = new UpdateGeofenceEvent()
      {
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        GeofenceName = "Test Geofence changed",
        Description = "Testing 123 changed",
        GeofenceType = GeofenceType.Stockpile.ToString(),
        FillColor = 56666,
        IsTransparent = false,
        GeometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 5,
        ActionUTC = actionUtc.AddMinutes(2)
      };

      geofenceRepo.StoreEvent(createGeofenceEvent).Wait();
      var s = geofenceRepo.StoreEvent(updateGeofenceEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = geofenceRepo.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      var projectGeofences = g.Result.ToList();
      Assert.Single(projectGeofences);
      Assert.Equal(updateGeofenceEvent.GeofenceName, projectGeofences[0].Name);
      Assert.Equal(updateGeofenceEvent.Description, projectGeofences[0].Description);
      Assert.Equal(updateGeofenceEvent.GeofenceType, projectGeofences[0].GeofenceType.ToString());
      Assert.Equal(updateGeofenceEvent.FillColor, projectGeofences[0].FillColor);
      Assert.Equal(updateGeofenceEvent.IsTransparent, projectGeofences[0].IsTransparent);
      Assert.Equal(updateGeofenceEvent.GeometryWKT, projectGeofences[0].GeometryWKT);
      Assert.Equal(updateGeofenceEvent.UserUID.ToString(), projectGeofences[0].UserUID);
      Assert.Equal(updateGeofenceEvent.AreaSqMeters, projectGeofences[0].AreaSqMeters);
      Assert.Equal(updateGeofenceEvent.ActionUTC, projectGeofences[0].LastActionedUTC);
    }

    /// <summary>
    /// Update Geofence - happyPath
    /// exists, nothing should be changed.
    /// </summary>
    [Fact]
    public void UpdateGeofence_HappyPath_NoFieldsChanged()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid().ToString();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = GeometryWKT,
        CustomerUID = new Guid(customerUid),
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var updateGeofenceEvent = new UpdateGeofenceEvent()
      {
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        ActionUTC = actionUtc.AddMinutes(2)
      };

      geofenceRepo.StoreEvent(createGeofenceEvent).Wait();
      var s = geofenceRepo.StoreEvent(updateGeofenceEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = geofenceRepo.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      var projectGeofences = g.Result.ToList();
      Assert.Single(projectGeofences);
      Assert.Equal(createGeofenceEvent.GeofenceName, projectGeofences[0].Name);
      Assert.Equal(createGeofenceEvent.Description, projectGeofences[0].Description);
      Assert.Equal(createGeofenceEvent.GeofenceType, projectGeofences[0].GeofenceType.ToString());
      Assert.Equal(createGeofenceEvent.FillColor, projectGeofences[0].FillColor);
      Assert.Equal(createGeofenceEvent.IsTransparent, projectGeofences[0].IsTransparent);
      Assert.Equal(createGeofenceEvent.GeometryWKT, projectGeofences[0].GeometryWKT);
      Assert.Equal(createGeofenceEvent.UserUID.ToString(), projectGeofences[0].UserUID);

      // Note that AreaSqMeters is stored as 0 dp in database
      Assert.Equal(Math.Truncate(createGeofenceEvent.AreaSqMeters), projectGeofences[0].AreaSqMeters);
      Assert.Equal(updateGeofenceEvent.ActionUTC, projectGeofences[0].LastActionedUTC);
    }

    /// <summary>
    /// Create Geofence - invalidGeometry
    ///   should fail
    /// </summary>
    [Fact]
    public void UpdateGeofence_GeofenceDoesntExist()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid().ToString();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = GeometryWKT,
        CustomerUID = new Guid(customerUid),
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var updateGeofenceEvent = new UpdateGeofenceEvent()
      {
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        GeofenceName = createGeofenceEvent.GeofenceName,
        Description = createGeofenceEvent.Description,
        GeofenceType = createGeofenceEvent.GeofenceType,
        FillColor = 56666,
        IsTransparent = false,
        GeometryWKT = createGeofenceEvent.GeometryWKT,
        AreaSqMeters = createGeofenceEvent.AreaSqMeters,
        ActionUTC = actionUtc
      };

      var s = geofenceRepo.StoreEvent(updateGeofenceEvent);
      s.Wait();
      Assert.Equal(1, s.Result);
    }

    /// <summary>
    /// Delete Geofence - Happy path 
    ///   geofence exists.
    /// </summary>
    [Fact]
    public void DeleteGeofence_HappyPath()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid().ToString();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = GeometryWKT,
        CustomerUID = new Guid(customerUid),
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var deleteGeofenceEvent = new DeleteGeofenceEvent()
      {
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        UserUID = createGeofenceEvent.UserUID,
        ActionUTC = actionUtc
      };

      geofenceRepo.StoreEvent(createGeofenceEvent).Wait();
      var s = geofenceRepo.StoreEvent(deleteGeofenceEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = geofenceRepo.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      Assert.Empty(g.Result);

      var u = geofenceRepo.GetGeofence_UnitTest(createGeofenceEvent.GeofenceUID.ToString());
      u.Wait();
      Assert.NotNull(u.Result);
      Assert.Equal(createGeofenceEvent.GeofenceUID.ToString(), u.Result.GeofenceUID);
    }

    /// <summary>
    /// Delete Geofence - geofence doesn'tExist
    /// </summary>
    [Fact]
    public void DeleteGeofence_DoesntExist()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid().ToString();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = GeometryWKT,
        CustomerUID = new Guid(customerUid),
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var deleteGeofenceEvent = new DeleteGeofenceEvent()
      {
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        UserUID = createGeofenceEvent.UserUID,
        ActionUTC = actionUtc
      };

      var s = geofenceRepo.StoreEvent(deleteGeofenceEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = geofenceRepo.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      Assert.Empty(g.Result);

      var u = geofenceRepo.GetGeofence_UnitTest(createGeofenceEvent.GeofenceUID.ToString());
      u.Wait();
      Assert.NotNull(u.Result);
      Assert.Equal(createGeofenceEvent.GeofenceUID.ToString(), u.Result.GeofenceUID);
    }

    #endregion

    #region AssociateGeofenceWithProject

    /// <summary>
    /// Associate Project Geofence - Happy Path
    ///   project and Geofence added.
    ///   Project legacyCustomerID updated and ActionUTC is later
    /// </summary>
    [Fact]
    public void GetGeofencesForCustomer_HappyPath()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = GeometryWKT,
        CustomerUID = new Guid(customerUid),
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var associateProjectGeofenceEvent = new AssociateProjectGeofence()
      {
        ProjectUID = new Guid(projectUid),
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        ActionUTC = actionUtc.AddDays(1)
      };

      geofenceRepo.StoreEvent(createGeofenceEvent).Wait();
      var s = projectRepo.StoreEvent(associateProjectGeofenceEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = geofenceRepo.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      var projectGeofences = g.Result.Where(x => x.GeofenceUID == createGeofenceEvent.GeofenceUID.ToString()).ToList();
      Assert.Single(projectGeofences);
      Assert.Equal(createGeofenceEvent.GeofenceUID.ToString(), projectGeofences[0].GeofenceUID);
    }

    /// <summary>
    /// Associate Project Geofence - already exists
    /// </summary>
    [Fact]
    public void AssociateProjectWithGeofence_AlreadyExists()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = GeometryWKT,
        CustomerUID = new Guid(customerUid),
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var associateProjectGeofenceEvent = new AssociateProjectGeofence()
      {
        ProjectUID = new Guid(projectUid),
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        ActionUTC = actionUtc.AddDays(1)
      };

      geofenceRepo.StoreEvent(createGeofenceEvent).Wait();
      projectRepo.StoreEvent(associateProjectGeofenceEvent).Wait();
      var s = projectRepo.StoreEvent(associateProjectGeofenceEvent);
      s.Wait();
      Assert.Equal(0, s.Result);

      var g = geofenceRepo.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      var projectGeofences = g.Result.Where(x => x.GeofenceUID == createGeofenceEvent.GeofenceUID.ToString()).ToList();
      Assert.Single(projectGeofences);
      Assert.Equal(createGeofenceEvent.GeofenceUID.ToString(), projectGeofences[0].GeofenceUID);
    }

    #endregion
  }
}
