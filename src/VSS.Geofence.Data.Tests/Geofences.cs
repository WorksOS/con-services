using System;
using System.Collections.Generic;
using System.Linq;
using ClipperLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using VSS.Geofence.Data.Models;
using VSS.MasterData.Common.Helpers;
using VSS.Project.Data;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Geofence.Data.Tests
{
  [TestClass]
  public class Geofences
  {
    private readonly MySqlGeofenceRepository _geofenceService;
    private readonly MySqlProjectRepository _projectService;

    private readonly string _projectGeometry =
        "POLYGON((172.572898643208 -43.5416737200023,172.591910140712 -43.542202557018,172.591652648643 -43.5491392248023,172.572941558559 -43.5492947463212,172.572898643208 -43.5416737200023,172.572898643208 -43.5416737200023))";
    private readonly string _insideGeofenceGeometry =
          "POLYGON((172.577791 -43.547148,172.583284 -43.543976,172.585301 -43.545904,172.578521 -43.548082,172.577791 -43.547148))";
    private readonly string _outsideGeofenceGeometry =
          "POLYGON((172.59329966542 -43.542486101965,172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965,172.59329966542 -43.542486101965))";

    public Geofences()
    {
      _geofenceService = new MySqlGeofenceRepository();
      _projectService = new MySqlProjectRepository();
      _projectService.SetInTransactionState(true);
    }

    #region Privates
    private CreateGeofenceEvent GetNewCreateGeofenceEvent(Guid geofenceUid, GeofenceType geofenceType, Guid customerUid, string name=null)
    {
      return new CreateGeofenceEvent()
      {
        GeofenceUID = geofenceUid,
        GeofenceName = name ?? "Test Geofence",
        Description = "Testing 123",
        GeofenceType = geofenceType.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = customerUid,
        UserUID = Guid.NewGuid(),
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow.AddMilliseconds(1000)
      };
    }

    private UpdateGeofenceEvent GetNewUpdateGeofenceEvent(Guid geofenceUID, string geofenceName, string geofenceType, string geometry, int fillColor, bool isTransparent, DateTime lastActionedUTC)
    {
      return new UpdateGeofenceEvent()
      {
        GeofenceUID = geofenceUID,
        GeofenceName = geofenceName,
        GeofenceType = geofenceType,
        GeometryWKT = geometry,
        FillColor = fillColor,
        IsTransparent = isTransparent,
        ActionUTC = lastActionedUTC,
        ReceivedUTC = DateTime.UtcNow.AddMilliseconds(100)
      };
    }

    private DeleteGeofenceEvent GetNewDeleteGeofenceEvent(Guid geofenceUID, Guid userUID, DateTime lastActionedUTC)
    {
      return new DeleteGeofenceEvent()
      {
        GeofenceUID = geofenceUID,
        UserUID = userUID,
        ActionUTC = lastActionedUTC,
        ReceivedUTC = DateTime.UtcNow.AddMilliseconds(100)
      };
    }

    private CreateProjectEvent GetNewCreateProjectEvent(Guid projectUid)
    {
      return new CreateProjectEvent()
      {
        ProjectUID = projectUid,
        ProjectID = 123,
        ProjectName = "Test Project",
        ProjectTimezone = "New Zealand Standard Time",
        ProjectType = ProjectType.LandFill,
        ProjectStartDate = DateTime.UtcNow.AddDays(-1).Date,
        ProjectEndDate = DateTime.UtcNow.AddDays(1).Date,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow.AddMilliseconds(1000)
      };
    }

    private AssociateProjectCustomer GetNewAssociateProjectCustomerEvent(Guid projectUid, Guid customerUid)
    {
      return new AssociateProjectCustomer()
      {
        ProjectUID = projectUid,
        CustomerUID = customerUid,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
    }

    #endregion

    [TestMethod]
    public void CreateNewGeofence_Succeeds()
    {
      _geofenceService.InRollbackTransaction<object>(o =>
      {
        //Should be able to create a Landfill geofence
        var createGeofenceEvent = GetNewCreateGeofenceEvent(Guid.NewGuid(), GeofenceType.Landfill, Guid.NewGuid());
        var upsertCount = _geofenceService.StoreGeofence(createGeofenceEvent);
        Assert.AreEqual(1, upsertCount, "Failed to create a geofence!");

        var geofence = _geofenceService.GetGeofence(createGeofenceEvent.GeofenceUID.ToString());
        Assert.IsNotNull(geofence, "Failed to get the created geofence!");

        //Should not be able to create a generic geofence
        createGeofenceEvent.GeofenceType = GeofenceType.Generic.ToString();
        upsertCount = _geofenceService.StoreGeofence(createGeofenceEvent);
        Assert.AreEqual(0, upsertCount, "Created a geofence when it shouldn't!");

        return null;
      });
    }

    [TestMethod]
    public void UpsertGeofence_Fails()
    {
      var upsertCount = _geofenceService.StoreGeofence(null);
      Assert.AreEqual(0, upsertCount, "Should fail to upsert a geofence!");
    }

    [TestMethod]
    public void UpdateGeofence_Succeeds()
    {
      _geofenceService.InRollbackTransaction<object>(o =>
      {
        var createGeofenceEvent = GetNewCreateGeofenceEvent(Guid.NewGuid(), GeofenceType.Project, Guid.NewGuid());
        var upsertCount = _geofenceService.StoreGeofence(createGeofenceEvent);
        Assert.AreEqual(1, upsertCount, "Failed to create a geofence!");

        int fillColor = 123;
        bool isTransparent = false;
        var updateGeofenceEvent = GetNewUpdateGeofenceEvent(createGeofenceEvent.GeofenceUID,
                                                          createGeofenceEvent.GeofenceName,
                                                          createGeofenceEvent.GeofenceType,
                                                          createGeofenceEvent.GeometryWKT,
                                                          fillColor,
                                                          isTransparent,
                                                          DateTime.UtcNow);
        upsertCount = _geofenceService.StoreGeofence(updateGeofenceEvent);
        Assert.AreEqual(1, upsertCount, "Failed to update the geofence!");

        var geofence = _geofenceService.GetGeofence(createGeofenceEvent.GeofenceUID.ToString());
        Assert.IsNotNull(geofence, "Failed to get the updated geofence!");
        Assert.AreEqual(fillColor, geofence.FillColor, "Wrong fillColor");
        Assert.AreEqual(isTransparent, geofence.IsTransparent, "Wrong isTransparent");
        return null;
      });
    }

    [TestMethod]
    public void DeleteGeofence_Succeeds()
    {
      _geofenceService.InRollbackTransaction<object>(o =>
      {
        var createGeofenceEvent = GetNewCreateGeofenceEvent(Guid.NewGuid(), GeofenceType.Landfill, Guid.NewGuid());
        var upsertCount = _geofenceService.StoreGeofence(createGeofenceEvent);
        Assert.AreEqual(1, upsertCount, "Failed to create a geofence!");

        var deleteGeofenceEvent = GetNewDeleteGeofenceEvent(createGeofenceEvent.GeofenceUID, createGeofenceEvent.UserUID, DateTime.UtcNow);

        upsertCount = _geofenceService.StoreGeofence(deleteGeofenceEvent);
        Assert.AreEqual(1, upsertCount, "Failed to delete the geofence!");

        var geofence = _geofenceService.GetGeofence(createGeofenceEvent.GeofenceUID.ToString());
        Assert.AreEqual(true, geofence.IsDeleted, "Geofence is not deleted!");

        return null;
      });
    }

    [TestMethod]
    public void GetProjectUidForNameTest()
    {
      _geofenceService.InRollbackTransaction<object>(o =>
      {
        _projectService.SetConnection((MySqlConnection)o);

        var projectUid = Guid.NewGuid();
        var createProjectEvent = GetNewCreateProjectEvent(projectUid);
        var upsertCount = _projectService.StoreProject(createProjectEvent);
        Assert.AreEqual(1, upsertCount, "Failed to create project");

        var customerUid = Guid.NewGuid();
        var associateProjectEvent = GetNewAssociateProjectCustomerEvent(projectUid, customerUid);
        upsertCount = _projectService.StoreProject(associateProjectEvent);
        Assert.AreEqual(1, upsertCount, "Failed to associate project with customer");

        var retrievedUid = _projectService.GetProjectUidForName(customerUid.ToString(), createProjectEvent.ProjectName);
        Assert.AreEqual(projectUid.ToString(), retrievedUid, "Wrong projectUid for name");

        retrievedUid = _projectService.GetProjectUidForName(Guid.NewGuid().ToString(), createProjectEvent.ProjectName);
        Assert.IsNull(retrievedUid, "Should be null projectUid for different customer");

        retrievedUid = _projectService.GetProjectUidForName(customerUid.ToString(), "Dummy name");
        Assert.IsNull(retrievedUid, "Should be null projectUid for different name");

        return null;
      });
    }

    [TestMethod]
    public void GeofencesOverlapTest()
    {
      Assert.IsTrue(Geometry.GeofencesOverlap(_projectGeometry, _insideGeofenceGeometry), "Geofence should be inside project!");
      Assert.IsFalse(Geometry.GeofencesOverlap(_projectGeometry, _outsideGeofenceGeometry), "Geofence should be outside project!");
    }

    [TestMethod]
    public void GetProjectGeofencesTest()
    {
      _geofenceService.InRollbackTransaction<object>(o =>
      {
        var customerUid1 = Guid.NewGuid();
        var projectGeofence1 = GetNewCreateGeofenceEvent(Guid.NewGuid(), GeofenceType.Project, customerUid1);
        var landfillGeofence1 = GetNewCreateGeofenceEvent(Guid.NewGuid(), GeofenceType.Landfill, customerUid1);
        var customerUid2 = Guid.NewGuid();
        var projectGeofence2 = GetNewCreateGeofenceEvent(Guid.NewGuid(), GeofenceType.Project, customerUid2);

        var upsertCount =
            _geofenceService.StoreGeofence(projectGeofence1) +
            _geofenceService.StoreGeofence(landfillGeofence1) +
            _geofenceService.StoreGeofence(projectGeofence2);
        Assert.AreEqual(3, upsertCount, "Failed to create all geofences");

        var projectGeofences = _geofenceService.GetProjectGeofences(customerUid1.ToString()).ToList();
        Assert.AreEqual(1, projectGeofences.Count, "Wrong number of project geofences");
        Assert.AreEqual(projectGeofence1.GeofenceUID.ToString(), projectGeofences.First().GeofenceUID, "Wrong project geofence returned");

        return null;
      });     
    }

    [TestMethod]
    public void AssignGeofenceToProjectTest()
    {
      _geofenceService.InRollbackTransaction<object>(o =>
      {
        _projectService.SetConnection((MySqlConnection)o);

        var projectUid = Guid.NewGuid();
        var createProjectEvent = GetNewCreateProjectEvent(projectUid);
        var upsertCount = _projectService.StoreProject(createProjectEvent);
        Assert.AreEqual(1, upsertCount, "Failed to create project");

        var customerUid = Guid.NewGuid();
        var associateProjectEvent = GetNewAssociateProjectCustomerEvent(projectUid, customerUid);
        upsertCount = _projectService.StoreProject(associateProjectEvent);
        Assert.AreEqual(1, upsertCount, "Failed to associate project with customer");

        var projectGeofence = GetNewCreateGeofenceEvent(Guid.NewGuid(), GeofenceType.Project, customerUid);
        var landfillGeofence = GetNewCreateGeofenceEvent(Guid.NewGuid(), GeofenceType.Landfill, customerUid);

        upsertCount =
            _geofenceService.StoreGeofence(projectGeofence) +
            _geofenceService.StoreGeofence(landfillGeofence);
        Assert.AreEqual(2, upsertCount, "Failed to create all geofences");

        upsertCount = _geofenceService.AssignGeofenceToProject(projectGeofence.GeofenceUID.ToString(), projectUid.ToString());
        Assert.AreEqual(1, upsertCount, "Failed to assign project geofence to project");

        upsertCount = _geofenceService.AssignGeofenceToProject(landfillGeofence.GeofenceUID.ToString(), projectUid.ToString());
        Assert.AreEqual(1, upsertCount, "Failed to assign landfill geofence to project");

        return null;
      });        
    }

    [TestMethod]
    public void GetUnassignedLandfillGeofencesTest()
    {
      _geofenceService.InRollbackTransaction<object>(o =>
      {
        var customerUid1 = Guid.NewGuid();
        var projectGeofence1 = GetNewCreateGeofenceEvent(Guid.NewGuid(), GeofenceType.Project, customerUid1);
        var landfillGeofence1 = GetNewCreateGeofenceEvent(Guid.NewGuid(), GeofenceType.Landfill, customerUid1);
        var customerUid2 = Guid.NewGuid();
        var landfillGeofence2 = GetNewCreateGeofenceEvent(Guid.NewGuid(), GeofenceType.Landfill, customerUid2);

        var upsertCount =
            _geofenceService.StoreGeofence(projectGeofence1) +
            _geofenceService.StoreGeofence(landfillGeofence1) +
            _geofenceService.StoreGeofence(landfillGeofence2);
        Assert.AreEqual(3, upsertCount, "Failed to create all geofences");

        var unassignedLandfillGeofences = _geofenceService.GetUnassignedLandfillGeofences(customerUid1.ToString());
        Assert.AreEqual(1, unassignedLandfillGeofences.Count(), "Wrong number of unassigned landfill geofences");
        Assert.AreEqual(landfillGeofence1.GeofenceUID.ToString(), unassignedLandfillGeofences.First().GeofenceUID, "Wrong landfill geofence returned");

        return null;
      });   
    }
 
  }
}
