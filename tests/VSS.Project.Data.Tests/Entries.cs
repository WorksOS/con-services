using System;
using System.Linq;
using Common.Repository;
using LandfillService.Common.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace LandfillDatabase.Tests
{
  [TestClass]
  public class Entries : TestBase
  {
    [TestMethod]
    public void SaveEntries_Succeeds()
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

      var projects = LandfillDb.GetProject(projectUid.ToString());
      Assert.IsNotNull(projects, "Error trying to get the created project.");
      Assert.AreEqual(1, projects.Count(), "Failed to get the created project.");
      var projectResponse = projects?.ToList()[0];
      Assert.AreEqual(projectUid.ToString(), projectResponse.projectUid, "Failed to get the correct projectUID.");

      var entry = new WeightEntry()
      {
        date =  DateTime.UtcNow,
        weight = 55.67
      };
      LandfillDb.SaveEntry(projectResponse, projectGeofenceUid.ToString(), entry);

      var retrievedEntries = LandfillDb.GetEntries(projectResponse, 
                        projectGeofenceUid.ToString(),
                        entry.date.AddDays(-2), DateTime.UtcNow).ToList();
      Assert.IsNotNull(retrievedEntries, "Error trying to get the created entries.");
      Assert.AreEqual(3, retrievedEntries.Count, "Failed to get the created entries.");
      Assert.AreEqual(0, retrievedEntries.ToList()[0].weight, "Failed to get the correct weight on day 1.");
      Assert.AreEqual(entry.weight, retrievedEntries.ToList()[2].weight, "Failed to get the correct weight on day 3.");
    }

    [TestMethod]
    public void IfNeedToTouchEntries_Succeeds()
    {
      throw new NotImplementedException();
    }

    [TestMethod]
    public void SaveVolume_Succeeds()
    {
      throw new NotImplementedException();
    }

    [TestMethod]
    public void MarkVolumeNotRetrieved_Succeeds()
    {
      throw new NotImplementedException();
    }

    [TestMethod]
    public void MarkVolumeNotAvailable_Succeeds()
    {
      throw new NotImplementedException();
    }
    
  }
}
