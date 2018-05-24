using System;
using System.Linq;
using Common.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LandfillDatabase.Tests
{
  [TestClass]
  public class CCA : TestBase
  {
    [TestMethod]
    public void SaveCCA_Succeeds()
    {
      var isCreatedOk = CreateAProjectWithLandfill(out int _,
        out Guid _, out _, out var projectUid, out var projectGeofenceUid, out _,
        out _);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      var projects = LandfillDb.GetProject(projectUid.ToString());
      Assert.IsNotNull(projects, "Error trying to get the created project.");
      Assert.AreEqual(1, projects.Count(), "Failed to get the created project.");
      var projectResponse = projects?.ToList()[0];
      Assert.AreEqual(projectUid.ToString(), projectResponse.projectUid, "Failed to get the correct projectUID.");

      DateTime date = Convert.ToDateTime(DateTime.UtcNow.ToShortDateString());
      long machineId = 355;
      int liftId = 8;
      double incomplete = 45.76;
      double complete = 32.9;
      double overcomplete = 2;

      LandfillDb.SaveCCA(projectUid.ToString(), projectGeofenceUid.ToString(), date, machineId, liftId, incomplete,
        complete, overcomplete);

      var retrievedCCAs = LandfillDb.GetCCA(projectResponse, projectGeofenceUid.ToString(),
        date, date, machineId, liftId).ToList();
      Assert.IsNotNull(retrievedCCAs, "Error trying to get the created CCAs.");
      Assert.AreEqual(1, retrievedCCAs.Count, "Failed to get the created CCAs.");
      Assert.AreEqual(machineId, retrievedCCAs.ToList()[0].machineId, "Failed to get the correct machineId on day 1.");
      Assert.AreEqual(liftId, retrievedCCAs.ToList()[0].liftId, "Failed to get the correct liftId on day 1.");
      Assert.AreEqual(incomplete, retrievedCCAs.ToList()[0].incomplete,
        "Failed to get the correct incomplete on day 1.");
      Assert.AreEqual(complete, retrievedCCAs.ToList()[0].complete, "Failed to get the correct complete on day 1.");
      Assert.AreEqual(overcomplete, retrievedCCAs.ToList()[0].overcomplete,
        "Failed to get the correct overcomplete on day 1.");
    }

    [TestMethod]
    public void MarkCCANotRetrieved_Succeeds()
    {
      // note that at present the CCANotRetrieved is never retrieved. Very strange.
      //   it does however set complete/incomplete etc
      //   I suspect this is a bug, but am not going to muck in the business logic
      //    for this task
      var isCreatedOk = CreateAProjectWithLandfill(out _,
        out _, out _, out var projectUid, out var projectGeofenceUid, out _,
        out _);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      var projects = LandfillDb.GetProject(projectUid.ToString());
      Assert.IsNotNull(projects, "Error trying to get the created project.");
      Assert.AreEqual(1, projects.Count(), "Failed to get the created project.");
      var projectResponse = projects?.ToList()[0];
      Assert.AreEqual(projectUid.ToString(), projectResponse.projectUid, "Failed to get the correct projectUID.");

      DateTime date = Convert.ToDateTime(DateTime.UtcNow.ToShortDateString());
      long machineId = 355;
      int liftId = 8;
      double incomplete = 45.76;
      double complete = 32.9;
      double overcomplete = 2;

      LandfillDb.SaveCCA(projectUid.ToString(), projectGeofenceUid.ToString(), date, machineId, liftId, incomplete,
        complete, overcomplete);
      LandfillDb.MarkCCANotRetrieved(projectUid.ToString(), projectGeofenceUid.ToString(), date, machineId, liftId);

      var retrievedCcAs = LandfillDb.GetCCA(projectResponse, projectGeofenceUid.ToString(),
        date, date, machineId, liftId).ToList();
      Assert.IsNotNull(retrievedCcAs, "Error trying to get the created CCAs.");
      Assert.AreEqual(1, retrievedCcAs.Count, "Failed to get the created CCAs.");
      Assert.AreEqual(machineId, retrievedCcAs.ToList()[0].machineId, "Failed to get the correct machineId on day 1.");
      Assert.AreEqual(liftId, retrievedCcAs.ToList()[0].liftId, "Failed to get the correct liftId on day 1.");
      Assert.AreEqual(0, retrievedCcAs.ToList()[0].incomplete, "Failed to get the correct incomplete on day 1.");
      Assert.AreEqual(0, retrievedCcAs.ToList()[0].complete, "Failed to get the correct complete on day 1.");
    }

    [TestMethod]
    public void MarkCCANotAvailable_Succeeds()
    {
      // note that at present the CCANotAvailable is never retrieved. Very strange.
      //   it does however set complete/incomplete etc
      //   I suspect this is a bug, but am not going to muck in the business logic
      //    for this task
      var isCreatedOk = CreateAProjectWithLandfill(out _,
        out _, out _, out var projectUid, out var projectGeofenceUid, out _,
        out _);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      var projects = LandfillDb.GetProject(projectUid.ToString());
      Assert.IsNotNull(projects, "Error trying to get the created project.");
      Assert.AreEqual(1, projects.Count(), "Failed to get the created project.");
      var projectResponse = projects?.ToList()[0];
      Assert.AreEqual(projectUid.ToString(), projectResponse.projectUid, "Failed to get the correct projectUID.");

      DateTime date = Convert.ToDateTime(DateTime.UtcNow.ToShortDateString());
      long machineId = 355;
      int liftId = 8;
      double incomplete = 45.76;
      double complete = 32.9;
      double overcomplete = 2;

      LandfillDb.SaveCCA(projectUid.ToString(), projectGeofenceUid.ToString(), date, machineId, liftId, incomplete,
        complete, overcomplete);
      LandfillDb.MarkCCANotAvailable(projectUid.ToString(), projectGeofenceUid.ToString(), date, machineId, liftId);

      var retrievedCCAs = LandfillDb.GetCCA(projectResponse, projectGeofenceUid.ToString(),
        date, date, machineId, liftId).ToList();
      Assert.IsNotNull(retrievedCCAs, "Error trying to get the created CCAs.");
      Assert.AreEqual(1, retrievedCCAs.Count, "Failed to get the created CCAs.");
      Assert.AreEqual(machineId, retrievedCCAs.ToList()[0].machineId, "Failed to get the correct machineId on day 1.");
      Assert.AreEqual(liftId, retrievedCCAs.ToList()[0].liftId, "Failed to get the correct liftId on day 1.");
      Assert.AreEqual(0, retrievedCCAs.ToList()[0].incomplete, "Failed to get the correct incomplete on day 1.");
      Assert.AreEqual(0, retrievedCCAs.ToList()[0].complete, "Failed to get the correct complete on day 1.");
    }
  }
}
