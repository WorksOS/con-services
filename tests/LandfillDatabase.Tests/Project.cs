using System;
using System.Linq;
using Common.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LandfillDatabase.Tests
{
  [TestClass]
  public class Project : TestBase
  {

    [TestMethod]
    public void GetLandfillProjectsForUser_Succeeds()
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

      var projects = LandfillDb.GetLandfillProjectsForUser(userUid.ToString()).ToList();
      Assert.IsNotNull(projects, "Error trying to get the created project.");
      Assert.AreEqual(1, projects.Count(), "Failed to get the created project.");
      Assert.AreEqual(projectUid.ToString(), projects?.ToList()[0].ProjectUID, "Failed to get the correct projectUID.");
    }

    [TestMethod]
    public void GetProjects_Succeeds()
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

      // returns ProjectResponse
      var projects = LandfillDb.GetProjects(userUid.ToString(), customerUid.ToString()).ToList();
      Assert.IsNotNull(projects, "Error trying to get the projects.");
      Assert.AreEqual(1, projects.Count(), "Failed to get the created project.");
      Assert.AreEqual(projectUid.ToString(), projects?.ToList()[0].projectUid, "Failed to get the correct projectUID.");
    }

    [TestMethod]
    public void GetListOfAvailableProjects_Succeeds()
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

      // returns ProjectResponse
      var projects = LandfillDb.GetListOfAvailableProjects().ToList();
      Assert.IsNotNull(projects, "Error trying to get the projects.");
      Assert.IsTrue(projects.Any(), "Failed to get the created project.");
      Assert.AreEqual(1, (projects?.ToList()).Count(p => p.projectUid == projectUid.ToString()), "Failed to get the correct projectUID.");
    }

    [TestMethod]
    public void GetProject_Succeeds()
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

      // returns ProjectResponse
      var projects = LandfillDb.GetProject(projectUid.ToString()).ToList();
      Assert.IsNotNull(projects, "Error trying to get the projects.");
      Assert.IsTrue(projects.Any(), "Failed to get the created project.");
      Assert.AreEqual(1, (projects?.ToList()).Count(p => p.projectUid == projectUid.ToString()), "Failed to get the correct projectUID.");
    }
  }
}
