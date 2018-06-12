using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryTests.Internal;
using System;
using System.Linq;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace RepositoryTests
{
  [TestClass]
  public class GeofenceRepositoryTests : TestControllerBase
  {
    GeofenceRepository geofenceContext = null;
    ProjectRepository projectContext = null;

    [TestInitialize]
    public void Init()
    {
      SetupLogging();

      geofenceContext = new GeofenceRepository(ServiceProvider.GetService<IConfigurationStore>(), ServiceProvider.GetService<ILoggerFactory>());
      projectContext = new ProjectRepository(ServiceProvider.GetService<IConfigurationStore>(), ServiceProvider.GetService<ILoggerFactory>());
    }

    #region Geofence

    /// <summary>
    /// Create Geofence - Happy path 
    ///   geofence doesn't exist.
    /// </summary>
    [TestMethod]
    public void CreateGeofence_HappyPath()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = customerUid,
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var s = geofenceContext.StoreEvent(createGeofenceEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Unable to store geofence");

      var g = geofenceContext.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      var projectGeofences = g.Result.ToList();
      Assert.AreEqual(1, projectGeofences.Count(), "Wrong number of geofences");
      Assert.AreEqual(createGeofenceEvent.GeofenceUID.ToString(), projectGeofences[0].GeofenceUID, "Wrong project geofence returned");
    }

    /// <summary>
    /// Create Geofence - Happy path 
    ///   null description
    /// </summary>
    [TestMethod]
    public void CreateGeofence_NullDescription()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = null,
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = customerUid,
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var s = geofenceContext.StoreEvent(createGeofenceEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Unable to store geofence");

      var g = geofenceContext.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      var projectGeofences = g.Result.ToList();
      Assert.AreEqual(1, projectGeofences.Count(), "Wrong number of geofences");
      Assert.AreEqual(createGeofenceEvent.GeofenceUID.ToString(), projectGeofences[0].GeofenceUID, "Wrong project geofence returned");
    }

    /// <summary>
    /// Create Geofenced - Already exists
    ///   geofence exists already.
    /// </summary>
    [TestMethod]
    public void CreateGeofence_AlreadyExists()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = customerUid,
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      geofenceContext.StoreEvent(createGeofenceEvent).Wait();
      var s = geofenceContext.StoreEvent(createGeofenceEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Shouldn't store duplicate, but this is how it is implementd");

      var g = geofenceContext.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      var projectGeofences = g.Result.ToList();
      Assert.AreEqual(1, projectGeofences.Count(), "Wrong number of geofences");
      Assert.AreEqual(createGeofenceEvent.GeofenceUID.ToString(), projectGeofences[0].GeofenceUID, "Wrong project geofence returned");
    }


    /// <summary>
    /// Update Geofence - happyPath
    /// exists, just update whichever fields are provided.
    /// </summary>
    [TestMethod]
    public void UpdateGeofence_HappyPath_FieldsChanged()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = customerUid,
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
        GeometryWKT = "POLYGON((166 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 5,
        ActionUTC = actionUtc.AddMinutes(2)
      };

      geofenceContext.StoreEvent(createGeofenceEvent).Wait();
      var s = geofenceContext.StoreEvent(updateGeofenceEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Unable to update geofence");

      var g = geofenceContext.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      var projectGeofences = g.Result.ToList();
      Assert.AreEqual(1, projectGeofences.Count(), "Wrong number of geofences");
      Assert.AreEqual(updateGeofenceEvent.GeofenceName, projectGeofences[0].Name, "Wrong Name returned");
      Assert.AreEqual(updateGeofenceEvent.Description, projectGeofences[0].Description, "Wrong Description returned");
      Assert.AreEqual(updateGeofenceEvent.GeofenceType, projectGeofences[0].GeofenceType.ToString(), "Wrong GeofenceType returned");
      Assert.AreEqual(updateGeofenceEvent.FillColor, projectGeofences[0].FillColor, "Wrong fillcolor returned");
      Assert.AreEqual(updateGeofenceEvent.IsTransparent, projectGeofences[0].IsTransparent, "Wrong IsTransparent returned");
      Assert.AreEqual(updateGeofenceEvent.GeometryWKT, projectGeofences[0].GeometryWKT, "Wrong GeometryWKT returned");
      Assert.AreEqual(updateGeofenceEvent.UserUID.ToString(), projectGeofences[0].UserUID, "Wrong UserUID returned");
      Assert.AreEqual(updateGeofenceEvent.AreaSqMeters, projectGeofences[0].AreaSqMeters, "Wrong AreaSqMeters returned");
      Assert.AreEqual(updateGeofenceEvent.ActionUTC, projectGeofences[0].LastActionedUTC, "Wrong ActionUTC returned");
    }

    /// <summary>
    /// Update Geofence - happyPath
    /// exists, nothing should be changed.
    /// </summary>
    [TestMethod]
    public void UpdateGeofence_HappyPath_NoFieldsChanged()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = customerUid,
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var updateGeofenceEvent = new UpdateGeofenceEvent()
      {
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        ActionUTC = actionUtc.AddMinutes(2)
      };

      geofenceContext.StoreEvent(createGeofenceEvent).Wait();
      var s = geofenceContext.StoreEvent(updateGeofenceEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Unable to update geofence");

      var g = geofenceContext.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      var projectGeofences = g.Result.ToList();
      Assert.AreEqual(1, projectGeofences.Count(), "Wrong number of geofences");
      Assert.AreEqual(createGeofenceEvent.GeofenceName, projectGeofences[0].Name, "Wrong Name returned");
      Assert.AreEqual(createGeofenceEvent.Description, projectGeofences[0].Description, "Wrong Description returned");
      Assert.AreEqual(createGeofenceEvent.GeofenceType, projectGeofences[0].GeofenceType.ToString(), "Wrong GeofenceType returned");
      Assert.AreEqual(createGeofenceEvent.FillColor, projectGeofences[0].FillColor, "Wrong fillcolor returned");
      Assert.AreEqual(createGeofenceEvent.IsTransparent, projectGeofences[0].IsTransparent, "Wrong IsTransparent returned");
      Assert.AreEqual(createGeofenceEvent.GeometryWKT, projectGeofences[0].GeometryWKT, "Wrong GeometryWKT returned");
      Assert.AreEqual(createGeofenceEvent.UserUID.ToString(), projectGeofences[0].UserUID, "Wrong UserUID returned");

      // Note that AreaSqMeters is stored as 0 dp in database
      Assert.AreEqual(Math.Truncate(createGeofenceEvent.AreaSqMeters), projectGeofences[0].AreaSqMeters, "Wrong AreaSqMeters returned");
      Assert.AreEqual(updateGeofenceEvent.ActionUTC, projectGeofences[0].LastActionedUTC, "Wrong ActionUTC returned");
    }

    /// <summary>
    /// Create Geofence - invalidGeometry
    ///   should fail
    /// </summary>
    [TestMethod]
    public void UpdateGeofence_GeofenceDoesntExist()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = customerUid,
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

      var s = geofenceContext.StoreEvent(updateGeofenceEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Should create a minimal geofence");
    }

    /// <summary>
    /// Delete Geofence - Happy path 
    ///   geofence exists.
    /// </summary>
    [TestMethod]
    public void DeleteGeofence_HappyPath()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = customerUid,
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

      geofenceContext.StoreEvent(createGeofenceEvent).Wait();
      var s = geofenceContext.StoreEvent(deleteGeofenceEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Unable to delete geofence");

      var g = geofenceContext.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      Assert.AreEqual(0, g.Result.Count(), "Wrong number of geofences");

      var u = geofenceContext.GetGeofence_UnitTest(createGeofenceEvent.GeofenceUID.ToString());
      u.Wait();
      Assert.IsNotNull(u.Result, "Wrong number of geofences");
      Assert.AreEqual(createGeofenceEvent.GeofenceUID.ToString(), u.Result.GeofenceUID, "Wrong number of geofences");
    }

    /// <summary>
    /// Delete Geofence - geofence doesn'tExist
    /// </summary>
    [TestMethod]
    public void DeleteGeofence_DoesntExist()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = customerUid,
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

      var s = geofenceContext.StoreEvent(deleteGeofenceEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Unable to delete geofence");

      var g = geofenceContext.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      Assert.AreEqual(0, g.Result.Count(), "Wrong number of geofences");

      var u = geofenceContext.GetGeofence_UnitTest(createGeofenceEvent.GeofenceUID.ToString());
      u.Wait();
      Assert.IsNotNull(u.Result, "Wrong number of geofences");
      Assert.AreEqual(createGeofenceEvent.GeofenceUID.ToString(), u.Result.GeofenceUID, "Wrong number of geofences");
    }

    #endregion

    #region AssociateGeofenceWithProject

    /// <summary>
    /// Associate Project Geofence - Happy Path
    ///   project and Geofence added.
    ///   Project legacyCustomerID updated and ActionUTC is later
    /// </summary>
    [TestMethod]
    public void GetGeofencesForCustomer_HappyPath()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();
      var projectUid = Guid.NewGuid();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = customerUid,
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var associateProjectGeofenceEvent = new AssociateProjectGeofence()
      {
        ProjectUID = projectUid,
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        ActionUTC = actionUtc.AddDays(1)
      };

      geofenceContext.StoreEvent(createGeofenceEvent).Wait();
      var s = projectContext.StoreEvent(associateProjectGeofenceEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Unable to associate geofence");

      var g = geofenceContext.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      var projectGeofences = g.Result.Where(x => x.GeofenceUID == createGeofenceEvent.GeofenceUID.ToString()).ToList();
      Assert.AreEqual(1, projectGeofences.Count(), "Wrong number of geofences");
      Assert.AreEqual(createGeofenceEvent.GeofenceUID.ToString(), projectGeofences[0].GeofenceUID, "Wrong project geofence returned");
    }

    /// <summary>
    /// Associate Project Geofence - already exists
    /// </summary>
    [TestMethod]
    public void AssociateProjectWithGeofence_AlreadyExists()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();
      var projectUid = Guid.NewGuid();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = customerUid,
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var associateProjectGeofenceEvent = new AssociateProjectGeofence()
      {
        ProjectUID = projectUid,
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        ActionUTC = actionUtc.AddDays(1)
      };

      geofenceContext.StoreEvent(createGeofenceEvent).Wait();
      projectContext.StoreEvent(associateProjectGeofenceEvent).Wait();
      var s = projectContext.StoreEvent(associateProjectGeofenceEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Unable to associate geofence");

      var g = geofenceContext.GetCustomerGeofences(customerUid.ToString());
      g.Wait();
      var projectGeofences = g.Result.Where(x => x.GeofenceUID == createGeofenceEvent.GeofenceUID.ToString()).ToList();
      Assert.AreEqual(1, projectGeofences.Count(), "Wrong number of geofences");
      Assert.AreEqual(createGeofenceEvent.GeofenceUID.ToString(), projectGeofences[0].GeofenceUID, "Wrong project geofence returned");
    }

    #endregion
  }
}