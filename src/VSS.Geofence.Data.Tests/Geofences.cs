using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Geofence.Data.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Geofence.Data.Tests
{
  [TestClass]
  public class Geofences
  {
    private readonly MySqlGeofenceRepository _geofenceService;

    public Geofences()
    {
      _geofenceService = new MySqlGeofenceRepository();
    }

    private CreateGeofenceEvent GetNewCreateGeofenceEvent(GeofenceType geofenceType)
    {
      return new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Tesing 123",
        GeofenceType = geofenceType.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = Guid.NewGuid(),
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

    [TestMethod]
    public void CreateNewGeofence_Succeeds()
    {
      _geofenceService.InRollbackTransaction<object>(o =>
      {
        //Should be able to create a Landfill geofence
        var createGeofenceEvent = GetNewCreateGeofenceEvent(GeofenceType.Landfill);
        var upsertCount = _geofenceService.StoreGeofence(createGeofenceEvent);
        Assert.IsTrue(upsertCount == 1, "Failed to create a geofence!");

        var geofence = _geofenceService.GetGeofence(createGeofenceEvent.GeofenceUID.ToString());
        Assert.IsNotNull(geofence, "Failed to get the created geofence!");

        //Should not be able to create a generic geofence
        createGeofenceEvent.GeofenceType = GeofenceType.Generic.ToString();
        upsertCount = _geofenceService.StoreGeofence(createGeofenceEvent);
        Assert.IsTrue(upsertCount == 0, "Created a geofence when it shouldn't!");

        return null;
      });
    }

    [TestMethod]
    public void UpsertGeofence_Fails()
    {
      var upsertCount = _geofenceService.StoreGeofence(null);
      Assert.IsTrue(upsertCount == 0, "Should fail to upsert a geofence!");
    }

    [TestMethod]
    public void UpdateGeofence_Succeeds()
    {
      _geofenceService.InRollbackTransaction<object>(o =>
      {
        var createGeofenceEvent = GetNewCreateGeofenceEvent(GeofenceType.Project);
        var upsertCount = _geofenceService.StoreGeofence(createGeofenceEvent);
        Assert.IsTrue(upsertCount == 1, "Failed to create a geofence!");

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
        Assert.IsTrue(upsertCount == 1, "Failed to update the geofence!");

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
        var createGeofenceEvent = GetNewCreateGeofenceEvent(GeofenceType.Landfill);
        var upsertCount = _geofenceService.StoreGeofence(createGeofenceEvent);
        Assert.IsTrue(upsertCount == 1, "Failed to create a geofence!");

        var deleteGeofenceEvent = GetNewDeleteGeofenceEvent(createGeofenceEvent.GeofenceUID, createGeofenceEvent.UserUID, DateTime.UtcNow);

        upsertCount = _geofenceService.StoreGeofence(deleteGeofenceEvent);
        Assert.IsTrue(upsertCount == 1, "Failed to delete the geofence!");

        var geofence = _geofenceService.GetGeofence(createGeofenceEvent.GeofenceUID.ToString());
        Assert.AreEqual(true, geofence.IsDeleted, "Geofence is not deleted!");

        return null;
      });
    }
  }
}
