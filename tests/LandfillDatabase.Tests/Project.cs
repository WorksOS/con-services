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
      var isCreatedOk = CreateAProjectWithLandfill(out _,
        out _, out var userUid, out var projectUid, out _, out _,
        out _);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      var projects = LandfillDb.GetLandfillProjectsForUser(userUid.ToString()).ToList();
      Assert.IsNotNull(projects, "Error trying to get the created project.");
      Assert.AreEqual(1, projects.Count(), "Failed to get the created project.");
      Assert.AreEqual(projectUid.ToString(), projects?.ToList()[0].ProjectUID, "Failed to get the correct projectUID.");
      Assert.AreNotEqual("", projects?.ToList()[0].GeometryWKT, "Failed to get the correct GeometryWKT.");
    }

    [TestMethod]
    public void GetProjects_Succeeds()
    {
      var isCreatedOk = CreateAProjectWithLandfill(out _,
        out var customerUid, out var userUid, out var projectUid, out _, out _,
        out _);
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
      var isCreatedOk = CreateAProjectWithLandfill(out _,
        out _, out _, out var projectUid, out _, out _,
        out _);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      // returns ProjectResponse
      var projects = LandfillDb.GetListOfAvailableProjects().ToList();
      Assert.IsNotNull(projects, "Error trying to get the projects.");
      Assert.IsTrue(projects.Any(), "Failed to get the created project.");
      Assert.AreEqual(1, (projects?.ToList()).Count(p => p.projectUid == projectUid.ToString()),
        "Failed to get the correct projectUID.");
    }

    [TestMethod]
    public void GetProject_Succeeds()
    {
      var isCreatedOk = CreateAProjectWithLandfill(out _,
        out _, out _, out var projectUid, out _, out _,
        out _);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      // returns ProjectResponse
      var projects = LandfillDb.GetProject(projectUid.ToString()).ToList();
      Assert.IsNotNull(projects, "Error trying to get the projects.");
      Assert.IsTrue(projects.Any(), "Failed to get the created project.");
      Assert.AreEqual(1, (projects?.ToList()).Count(p => p.projectUid == projectUid.ToString()),
        "Failed to get the correct projectUID.");
    }
  }
}
