using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Now3D.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Now3D.Controllers
{
  public class ProjectsController : BaseController
  {
    private readonly ICustomerProxy customerProxy;
    private readonly IProjectProxy projectProxy;
    private readonly IFileImportProxy fileImportProxy;

    public ProjectsController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, ICustomerProxy customerProxy, IProjectProxy projectProxy, IFileImportProxy fileImportProxy)
      :base(loggerFactory, serviceExceptionHandler)
    {
      this.customerProxy = customerProxy;
      this.projectProxy = projectProxy;
      this.fileImportProxy = fileImportProxy;
    }

    /// <summary>
    /// Get a list of customers, projects and files for me
    /// </summary>
    /// <response code="200">A list of customers you can currently access.</response>
    /// <response code="403">Invalid access token provided</response>
    [HttpGet("api/v1/projects")]
    [ProducesResponseType(typeof(List<CustomerDisplayModel>), 200)]
    public async Task<IActionResult> GetMasterDataModels()
    {
      var customers = await customerProxy.GetCustomersForMe(UserId, CustomHeaders);

      var results = await ExecuteAgainstMultiple(customers.customer, GetCustomerModel);

      return Json(results);
    }

    /// <summary>
    /// For a customer master data model, populate customer display model with project information
    /// </summary>
    private async Task<CustomerDisplayModel> GetCustomerModel(CustomerData customerData)
    {
      var customerModel = new CustomerDisplayModel
      {
        CustomerName = customerData.name,
        CustomerUid = customerData.uid
      };

      var headers = CustomHeaders;
      // We want to remove any customer UID passed in, and replace it with the customer in question
      if (headers.ContainsKey(HeaderConstants.X_VISION_LINK_CUSTOMER_UID))
        headers.Remove(HeaderConstants.X_VISION_LINK_CUSTOMER_UID);
      
      headers.Add(HeaderConstants.X_VISION_LINK_CUSTOMER_UID, customerData.uid);

      var projects = await projectProxy.GetProjectsV4(customerData.uid, headers);

      customerModel.Projects = await ExecuteAgainstMultiple(projects, y => GetProjectModel(y, headers));

      return customerModel;
    }

    /// <summary>
    /// For a project master data model, populate our display project model with its details and files attached
    /// </summary>
    private async Task<ProjectDisplayModel> GetProjectModel(ProjectData project, IDictionary<string, string> headers)
    {
      var projectModel = new ProjectDisplayModel
      {
        ProjectName = project.Name,
        ProjectUid = project.ProjectUid,
        IsActive = !project.IsArchived
      };

      var files = await fileImportProxy.GetFiles(project.ProjectUid, UserId, headers);

      foreach (var fileData in files.Where(f => f.ImportedFileType == ImportedFileType.DesignSurface))
      {
        projectModel.Files.Add(new FileDisplayModel
        {
          FileName = fileData.Name,
          FileUid = fileData.ImportedFileUid,
          FileType = fileData.ImportedFileType,
          FileTypeName = fileData.ImportedFileTypeName
        });
      }

      return projectModel;
    }
    
    /// <summary>
    /// Helper method to execute many requests to fetch child objects for a given parent
    /// </summary>
    private static async Task<List<TChild>> ExecuteAgainstMultiple<TChild,TParent>(IEnumerable<TParent> objects, Func<TParent, Task<TChild>> method)
    {
      var tasks = objects.Select(method).ToArray();

      await Task.WhenAll(tasks);

      return tasks.Select(task => task.Result).ToList();
    }

  }
}
