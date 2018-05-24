using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.MasterDataConsumer.Tests
{
  [TestClass]
  public class GeofenceEventsTests
  {

    [TestMethod]
    public void GeofenceEventsCopyModels()
    {
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);
      var geofence = new Geofence()
      {
        GeofenceUID = Guid.NewGuid().ToString(),
        Name = "Test Geofence",
        GeofenceType = GeofenceType.Borrow,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        FillColor = 16744448,
        IsTransparent = true,
        IsDeleted = false,
        Description = "The Description",
        AreaSqMeters = 123.456,
        UserUID = Guid.NewGuid().ToString(),
        LastActionedUTC = now
      };

      var kafkaGeofenceEvent = CopyModel(geofence);
      var copiedGeofence = CopyModel(kafkaGeofenceEvent);

      Assert.AreEqual(geofence, copiedGeofence, "Geofence model conversion not completed sucessfully");

    }


    #region private
   
    private CreateGeofenceEvent CopyModel(Geofence geofence)
    {
      return new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.Parse(geofence.GeofenceUID),
        GeofenceName = geofence.Name,
        Description = geofence.Description,
        GeofenceType = geofence.GeofenceType.ToString(),
        FillColor = geofence.FillColor.HasValue ? (int) geofence.FillColor.Value : 0,
        IsTransparent = geofence.IsTransparent.HasValue ? geofence.IsTransparent.Value : false,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        UserUID = Guid.Parse(geofence.UserUID), 
        ActionUTC = geofence.LastActionedUTC,
        AreaSqMeters = geofence.AreaSqMeters 
      };
    }

    private Geofence CopyModel(CreateGeofenceEvent kafkaGeofenceEvent)
    {
      return new Geofence()
      {
        GeofenceUID = kafkaGeofenceEvent.GeofenceUID.ToString(),
        Name = kafkaGeofenceEvent.GeofenceName,
        GeofenceType = (GeofenceType)Enum.Parse(typeof(GeofenceType), kafkaGeofenceEvent.GeofenceType, true),
        GeometryWKT = kafkaGeofenceEvent.GeometryWKT,
        FillColor = kafkaGeofenceEvent.FillColor,
        IsTransparent = kafkaGeofenceEvent.IsTransparent,
        IsDeleted = false,
        Description = kafkaGeofenceEvent.Description,
        UserUID = kafkaGeofenceEvent.UserUID.ToString(),
        LastActionedUTC = kafkaGeofenceEvent.ActionUTC,
        AreaSqMeters = kafkaGeofenceEvent.AreaSqMeters
      };
    }
    #endregion

  }
}

