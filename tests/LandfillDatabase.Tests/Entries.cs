using System;
using System.Linq;
using Common.Repository;
using LandfillService.Common.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LandfillDatabase.Tests
{
  [TestClass]
  public class Entries : TestBase
  {
    [TestMethod]
    public void SaveEntries_Succeeds()
    {
      var isCreatedOk = CreateAProjectWithLandfill(out _,
        out _, out _, out var projectUid, out var projectGeofenceUid, out _,
        out _);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      var projects = LandfillDb.GetProject(projectUid.ToString());
      Assert.IsNotNull(projects, "Error trying to get the created project.");
      Assert.AreEqual(1, projects.Count(), "Failed to get the created project.");
      var projectResponse = projects?.ToList()[0];
      Assert.AreEqual(projectUid.ToString(), projectResponse.projectUid, "Failed to get the correct projectUID.");

      var entry = new WeightEntry()
      {
        date = Convert.ToDateTime(DateTime.UtcNow.ToShortDateString()),
        weight = 55.67
      };
      LandfillDb.SaveEntry(projectResponse, projectGeofenceUid.ToString(), entry);

      var retrievedEntries = LandfillDb.GetEntries(projectResponse,
        projectGeofenceUid.ToString(),
        entry.date.AddDays(-1), entry.date).ToList();
      Assert.IsNotNull(retrievedEntries, "Error trying to get the created entries.");
      Assert.AreEqual(2, retrievedEntries.Count, "Failed to get the created entries.");
      Assert.AreEqual(0, retrievedEntries.ToList()[0].weight, "Failed to get the correct weight on day 1.");
      Assert.AreEqual(entry.weight, retrievedEntries.ToList()[1].weight, "Failed to get the correct weight on day 3.");
      Assert.AreEqual(0, retrievedEntries.ToList()[1].volume, "Failed to get the correct volume on day 3.");
    }

    [TestMethod]
    public void SaveVolume_Succeeds()
    {
      var isCreatedOk = CreateAProjectWithLandfill(out _,
        out _, out _, out var projectUid, out var projectGeofenceUid, out _,
        out _);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      DateTime date = Convert.ToDateTime(DateTime.UtcNow.ToShortDateString());
      double volume = 55.67;
      LandfillDb.SaveVolume(projectUid.ToString(), projectGeofenceUid.ToString(), date, volume);

      var projects = LandfillDb.GetProject(projectUid.ToString());
      Assert.IsNotNull(projects, "Error trying to get the created project.");
      Assert.AreEqual(1, projects.Count(), "Failed to get the created project.");
      var projectResponse = projects?.ToList()[0];
      Assert.AreEqual(projectUid.ToString(), projectResponse.projectUid, "Failed to get the correct projectUID.");

      var retrievedEntries = LandfillDb.GetEntries(projectResponse, projectGeofenceUid.ToString(), date, date).ToList();
      Assert.IsNotNull(retrievedEntries, "Error trying to get the created entries.");
      Assert.AreEqual(1, retrievedEntries.Count, "Failed to get the created entries.");
      Assert.AreEqual(0, retrievedEntries.ToList()[0].weight, "Failed to get the correct weight on day 1.");
      Assert.AreEqual(volume, retrievedEntries.ToList()[0].volume, "Failed to get the correct volume on day 1.");
    }

    [TestMethod]
    public void MarkVolumeNotRetrieved_Succeeds()
    {
      // note that at present the VolumeNotRetrieved is never retrieved. Very strange.
      //   I suspect this is a bug, but am not going to muck in the business logic
      //    for this task
      var isCreatedOk = CreateAProjectWithLandfill(out _,
        out _, out _, out var projectUid, out var projectGeofenceUid, out _,
        out _);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      DateTime date = Convert.ToDateTime(DateTime.UtcNow.ToShortDateString());
      double volume = 55.67;
      LandfillDb.SaveVolume(projectUid.ToString(), projectGeofenceUid.ToString(), date, volume);
      LandfillDb.MarkVolumeNotRetrieved(projectUid.ToString(), projectGeofenceUid.ToString(), date);
      ;

      var projects = LandfillDb.GetProject(projectUid.ToString());
      Assert.IsNotNull(projects, "Error trying to get the created project.");
      Assert.AreEqual(1, projects.Count(), "Failed to get the created project.");
      var projectResponse = projects?.ToList()[0];
      Assert.AreEqual(projectUid.ToString(), projectResponse.projectUid, "Failed to get the correct projectUID.");

      var retrievedEntries = LandfillDb.GetEntries(projectResponse, projectGeofenceUid.ToString(), date, date).ToList();
      Assert.IsNotNull(retrievedEntries, "Error trying to get the created entries.");
      Assert.AreEqual(1, retrievedEntries.Count, "Failed to get the created entries.");
      Assert.AreEqual(0, retrievedEntries.ToList()[0].weight, "Failed to get the correct weight on day 1.");
      Assert.AreEqual(volume, retrievedEntries.ToList()[0].volume, "Failed to get the correct volume on day 1.");
    }

    [TestMethod]
    public void MarkVolumeNotAvailable_Succeeds()
    {
      // note that at present the VolumeNotAvailable is never retrieved. Very strange???
      var isCreatedOk = CreateAProjectWithLandfill(out var legacyCustomerId,
        out var customerUid, out var userUid, out var projectUid, out var projectGeofenceUid,
        out var landfillGeofenceUid,
        out var subscriptionUid);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      DateTime date = Convert.ToDateTime(DateTime.UtcNow.ToShortDateString());
      double volume = 55.67;
      LandfillDb.SaveVolume(projectUid.ToString(), projectGeofenceUid.ToString(), date, volume);
      LandfillDb.MarkVolumeNotAvailable(projectUid.ToString(), projectGeofenceUid.ToString(), date);
      ;

      var projects = LandfillDb.GetProject(projectUid.ToString());
      Assert.IsNotNull(projects, "Error trying to get the created project.");
      Assert.AreEqual(1, projects.Count(), "Failed to get the created project.");
      var projectResponse = projects?.ToList()[0];
      Assert.AreEqual(projectUid.ToString(), projectResponse.projectUid, "Failed to get the correct projectUID.");

      var retrievedEntries = LandfillDb.GetEntries(projectResponse, projectGeofenceUid.ToString(), date, date).ToList();
      Assert.IsNotNull(retrievedEntries, "Error trying to get the created entries.");
      Assert.AreEqual(1, retrievedEntries.Count, "Failed to get the created entries.");
      Assert.AreEqual(0, retrievedEntries.ToList()[0].weight, "Failed to get the correct weight on day 1.");
      Assert.AreEqual(volume, retrievedEntries.ToList()[0].volume, "Failed to get the correct volume on day 1.");
    }

  }
}
