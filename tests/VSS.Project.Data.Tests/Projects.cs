using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace VSS.Project.Data.Tests
{
  [TestClass]
  public class Projects
  {
     private readonly MySqlProjectRepository _projectService;

     public Projects()
    {
      _projectService = new MySqlProjectRepository();
    }

    private CreateProjectEvent GetNewCreateProjectEvent()
    {
      return new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
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

    private UpdateProjectEvent GetNewUpdateProjectEvent(Guid projectUID, string projectName, string projectTimeZone, DateTime projectEndDate, DateTime lastActionedUTC)
    {
      return new UpdateProjectEvent()
      {
        ProjectUID = projectUID,
        ProjectName = projectName,
        ProjectTimezone = projectTimeZone,
        ProjectType = ProjectType.LandFill,
        ProjectEndDate = projectEndDate,
        ActionUTC = lastActionedUTC,
        ReceivedUTC = DateTime.UtcNow.AddMilliseconds(100)
      };
    }

    private DeleteProjectEvent GetNewDeleteProjectEvent(Guid projectUID, DateTime lastActionedUTC)
    {
      return new DeleteProjectEvent()
      {
        ProjectUID = projectUID,
        ActionUTC = lastActionedUTC,
        ReceivedUTC = DateTime.UtcNow.AddMilliseconds(100)
      };
    }

    private AssociateProjectCustomer GetNewAssociateProjectCustomerEvent(Guid projectUID, Guid customerUID, DateTime receivedUTC)
    {
      return new AssociateProjectCustomer()
      {
        ProjectUID = projectUID,
        CustomerUID = customerUID,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = receivedUTC
      };
    }

    private DissociateProjectCustomer GetNewDissociateProjectCustomerEvent(Guid projectUID, Guid customerUID, DateTime receivedUTC)
    {
      return new DissociateProjectCustomer()
      {
        ProjectUID = projectUID,
        CustomerUID = customerUID,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = receivedUTC
      };
    }

    [TestMethod]
    public void CreateNewProject_Succeeds()
    {
      _projectService.InRollbackTransaction<object>(o =>
      {
        var createProjectEvent = GetNewCreateProjectEvent();
        var upsertCount = _projectService.StoreProject(createProjectEvent);
        Assert.IsTrue(upsertCount == 1, "Failed to create a project!");

        var project = _projectService.GetProject(createProjectEvent.ProjectUID.ToString());
        Assert.IsNotNull(project, "Failed to get the created project!");

        return null;
      });
    }

    [TestMethod]
    public void UpsertProject_Fails()
    {
      var upsertCount = _projectService.StoreProject(null);
      Assert.IsTrue(upsertCount == 0, "Should fail to upsert a project!");
    }

    [TestMethod]
    public void UpdateProject_Succeeds()
    {
      _projectService.InRollbackTransaction<object>(o =>
      {
        var createProjectEvent = GetNewCreateProjectEvent();
        var upsertCount = _projectService.StoreProject(createProjectEvent);
        Assert.IsTrue(upsertCount == 1, "Failed to create a project!");

        var updateProjectEvent = GetNewUpdateProjectEvent(createProjectEvent.ProjectUID, 
                                                          createProjectEvent.ProjectName, 
                                                          createProjectEvent.ProjectTimezone,
                                                          createProjectEvent.ProjectEndDate.AddDays(3),
                                                          DateTime.UtcNow);
        upsertCount = _projectService.StoreProject(updateProjectEvent);
        Assert.IsTrue(upsertCount == 1, "Failed to update the project!");

        var project = _projectService.GetProject(createProjectEvent.ProjectUID.ToString());
        Assert.IsNotNull(project, "Failed to get the updated project!");

        Assert.IsTrue(project.projectUid == updateProjectEvent.ProjectUID.ToString(), "ProjectUID should not be changed!");
        Assert.IsTrue(project.name == updateProjectEvent.ProjectName, "Project Name should not be changed!");
        Assert.IsTrue(project.projectTimeZone == updateProjectEvent.ProjectTimezone, "Project Time Zone should not be changed!");
        Assert.IsTrue((project.projectEndDate - createProjectEvent.ProjectEndDate).Days == 3, "ProjectEndDate of the updated project was incorectly updated!");
        Assert.IsTrue(project.lastActionedUtc > createProjectEvent.ActionUTC, "LastActionedUtc of the updated project was incorectly updated!");

        return null;
      });
    }

    [TestMethod]
    public void DeleteProject_Succeeds()
    {
      _projectService.InRollbackTransaction<object>(o =>
      {
        var createProjectEvent = GetNewCreateProjectEvent();
        var upsertCount = _projectService.StoreProject(createProjectEvent);
        Assert.IsTrue(upsertCount == 1, "Failed to create a project!");

        var deleteProjectEvent = GetNewDeleteProjectEvent(createProjectEvent.ProjectUID, DateTime.UtcNow);

        upsertCount = _projectService.StoreProject(deleteProjectEvent);
        Assert.IsTrue(upsertCount == 1, "Failed to delete the project!");

        var project = _projectService.GetProject(createProjectEvent.ProjectUID.ToString());
        Assert.IsNull(project, "Succeeded to get the deleted project!");

        return null;
      });
    }

    [TestMethod]
    public void AssociateProjectCustomer_Succeeds()
    {
      _projectService.InRollbackTransaction<object>(o =>
      {
        var createProjectEvent = GetNewCreateProjectEvent();
        var upsertCount = _projectService.StoreProject(createProjectEvent);
        Assert.IsTrue(upsertCount == 1, "Failed to create a project!");

        var associateProjectCustomerEvent = GetNewAssociateProjectCustomerEvent(createProjectEvent.ProjectUID, Guid.NewGuid(), DateTime.UtcNow);

        upsertCount = _projectService.StoreProject(associateProjectCustomerEvent);
        Assert.IsTrue(upsertCount == 1, "Failed to associate the project with a customer!");

        var project = _projectService.GetProject(createProjectEvent.ProjectUID.ToString());
        Assert.IsNotNull(project, "Failed to get the customer associated project!");

        Assert.IsTrue(project.customerUid == associateProjectCustomerEvent.CustomerUID.ToString(), "The project was associated with wrong customer!");

        return null;
      });
    }
  }
}
