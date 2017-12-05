using System;
using System.Linq;
using Common.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LandfillDatabase.Tests
{
  [TestClass]
  public class Geofence : TestBase
  {

    [TestMethod]
    public void GetGeofenceUidForProject_Succeeds()
    {
      int legacyCustomerId;
      Guid customerUid;
      Guid userUid;
      Guid projectUid;
      Guid projectGeofenceUid;
      Guid landfillGeofenceUid;
      Guid subscriptionUid;
      var isCreatedOk = CreateAProjectWithLandfill(out legacyCustomerId,
        out customerUid, out userUid, out projectUid, out projectGeofenceUid, out landfillGeofenceUid,
        out subscriptionUid);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      var projects = LandfillDb.GetProject(projectUid.ToString()).ToList();

      var geofenceUid = LandfillDb.GetGeofenceUidForProject(projects[0]);
      Assert.AreEqual(projectGeofenceUid.ToString(), geofenceUid, "Failed to get the correct projectGeofence.");
    }


    [TestMethod]
    public void GetGeofences_Succeeds()
    {
      int legacyCustomerId;
      Guid customerUid;
      Guid userUid;
      Guid projectUid;
      Guid projectGeofenceUid;
      Guid landfillGeofenceUid;
      Guid subscriptionUid;
      var isCreatedOk = CreateAProjectWithLandfill(out legacyCustomerId,
        out customerUid, out userUid, out projectUid, out projectGeofenceUid, out landfillGeofenceUid,
        out subscriptionUid);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      var geofences = LandfillDb.GetGeofences(projectUid.ToString()).ToList();
      Assert.AreEqual(2, geofences.Count, "Failed to get the created geofences.");
      Assert.AreEqual(1, (geofences.ToList()).Count(p => p.uid.ToString() == projectGeofenceUid.ToString()), "Failed to get the correct projectGeofence.");
      Assert.AreEqual(1, (geofences.ToList()).Count(p => p.uid.ToString() == landfillGeofenceUid.ToString()), "Failed to get the correct landfillGeofence.");
    }

    [TestMethod]
    public void GetGeofencePoints_Succeeds()
    {
      int legacyCustomerId;
      Guid customerUid;
      Guid userUid;
      Guid projectUid;
      Guid projectGeofenceUid;
      Guid landfillGeofenceUid;
      Guid subscriptionUid;
      var isCreatedOk = CreateAProjectWithLandfill(out legacyCustomerId,
        out customerUid, out userUid, out projectUid, out projectGeofenceUid, out landfillGeofenceUid,
        out subscriptionUid);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      var points = LandfillDb.GetGeofencePoints(projectGeofenceUid.ToString()).ToList();
      Assert.AreEqual(8, points.Count, "Failed to get the project geofence points.");
    }

  }
}
