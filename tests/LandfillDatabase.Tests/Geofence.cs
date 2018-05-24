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
      var isCreatedOk = CreateAProjectWithLandfill(out _,
        out _, out _, out var projectUid, out var projectGeofenceUid, out _,
        out _);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      var projects = LandfillDb.GetProject(projectUid.ToString()).ToList();

      var geofenceUid = LandfillDb.GetGeofenceUidForProject(projects[0]);
      Assert.AreEqual(projectGeofenceUid.ToString(), geofenceUid, "Failed to get the correct projectGeofence.");
    }


    [TestMethod]
    public void GetGeofences_Succeeds()
    {
      var isCreatedOk = CreateAProjectWithLandfill(out _,
        out _, out _, out var projectUid, out var projectGeofenceUid, out var landfillGeofenceUid,
        out _);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      var geofences = LandfillDb.GetGeofences(projectUid.ToString()).ToList();
      Assert.AreEqual(2, geofences.Count, "Failed to get the created geofences.");
      Assert.AreEqual(1, (geofences.ToList()).Count(p => p.uid.ToString() == projectGeofenceUid.ToString()), "Failed to get the correct projectGeofence.");
      Assert.AreEqual(1, (geofences.ToList()).Count(p => p.uid.ToString() == landfillGeofenceUid.ToString()), "Failed to get the correct landfillGeofence.");
    }

    [TestMethod]
    public void GetProjectGeofencePoints_Succeeds()
    {
      var isCreatedOk = CreateAProjectWithLandfill(out _,
        out _, out _, out _, out var projectGeofenceUid, out _,
        out _);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      var points = LandfillDb.GetGeofencePoints(projectGeofenceUid.ToString()).ToList();
      Assert.AreEqual(6, points.Count, "Failed to get the project geofence points.");
    }

    [TestMethod]
    public void GetLandfillGeofencePoints_Succeeds()
    {
      var isCreatedOk = CreateAProjectWithLandfill(out _,
        out _, out _, out _, out _, out var landfillGeofenceUid,
        out _);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      var points = LandfillDb.GetGeofencePoints(landfillGeofenceUid.ToString()).ToList();
      Assert.AreEqual(8, points.Count, "Failed to get the landfill geofence points.");
    }

  }
}
