using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CCSS.Geometry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.Cws;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// Executor for validating project changes from CWS
  /// </summary>
  public class ValidateProjectExecutor : RequestExecutorContainer
  {
    //private static ProjectErrorCodesProvider projectErrorCodesProvider = new ProjectErrorCodesProvider();

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var data = CastRequestObjectTo<ProjectValidation>(item, errorCode: 68);

      // TODO:
      // 1. What is the user changes from 3D to non-3D? Do we care about existing data in TRex?
      // 2. If the user changes from non-3D to 3D it will be an update and there must be a coordinate system file.

      // We only need to validate 3D enabled projects
      
      if (data.ProjectType == 0) // TODO: this should be non-3d enabled
        return new ContractExecutionResult();

      var userUid = new Guid(userId);
      if (data.UpdateType == ProjectUpdateType.Created)
      {
        //Validate required fields are present
        if (string.IsNullOrEmpty(data.ProjectName))
        {
          return new ContractExecutionResult(11, "Missing ProjectName.");
        }
        if (string.IsNullOrEmpty(data.ProjectBoundaryWKT))
        {
          return new ContractExecutionResult(8, "Missing ProjectBoundary.");
        }

        //Validate project name not duplicate
        // ProjectUID won't be filled yet
        var duplicates = await ValidateProjectName(data.CustomerUid, userUid, data.ProjectName,
          data.ProjectUid, log, serviceExceptionHandler, cwsProjectClient, customHeaders);
        if (duplicates > 0)
        {
          return new ContractExecutionResult(109, $"ProjectName must be unique. {duplicates} active project duplicates found.");
        }

        //Validate project boundary
        dynamic result = ValidateProjectBoundary(data.ProjectBoundaryWKT);
        if (result.code != ContractExecutionStatesEnum.ExecutedSuccessfully)
        {
          return new ContractExecutionResult(result.code, result.message);
        }

        //Validate coordinate system file
        /* TODO
        await ProjectRequestHelper.ValidateCoordSystemInProductivity3D(
          createProjectEvent.CoordinateSystemFileName, createProjectEvent.CoordinateSystemFileContent,
          serviceExceptionHandler, customHeaders, productivity3dV1ProxyCoord);
          */

        //Validate project boundary doesn't overlap existing projects
        log.LogDebug($"Testing if there are overlapping projects for project {data.ProjectName}");
        var overlaps = await ProjectRequestHelper.DoesProjectOverlap(data.CustomerUid,
          null, userUid, data.ProjectBoundaryWKT,
          log, serviceExceptionHandler, cwsProjectClient, customHeaders);
        if (overlaps)
        {
          return new ContractExecutionResult(43, "Project boundary overlaps another project.");
        }
      }

      else if (data.UpdateType == ProjectUpdateType.Updated)
      {
        //Validate projectUID
        if (!data.ProjectUid.HasValue)
        {
          return new ContractExecutionResult(5, "Missing ProjectUID.");
        }

        var typeChanged = data.ProjectType.HasValue;
        if (typeChanged)
        {
          // Get the existing project to validate name, boundary and coordinate system file
          var project = await cwsProjectClient.GetMyProject(data.ProjectUid.Value, userUid, customHeaders: customHeaders);
          if (project == null)
          {
            return new ContractExecutionResult(7, "Project does not exist.");
          }
          if (string.IsNullOrEmpty(data.ProjectName))
            data.ProjectName = project.ProjectName;
          if (string.IsNullOrEmpty(data.ProjectBoundaryWKT))
            data.ProjectBoundaryWKT = GeometryConversion.ProjectBoundaryToWKT(project.ProjectSettings.Boundary);
        }

        //Validate project name if changed
        if (!string.IsNullOrEmpty(data.ProjectName))
        {
          var duplicates = await ValidateProjectName(data.CustomerUid, userUid, data.ProjectName, 
            data.ProjectUid, log, serviceExceptionHandler, cwsProjectClient, customHeaders);
          if (duplicates > 0)
          {
            return new ContractExecutionResult(109, $"ProjectName must be unique. {duplicates} active project duplicates found.");
          }
        }

        //Validate project boundary of changed
        if (!string.IsNullOrEmpty(data.ProjectBoundaryWKT))
        {
          dynamic result = ValidateProjectBoundary(data.ProjectBoundaryWKT);
          if (result.code != ContractExecutionStatesEnum.ExecutedSuccessfully)
          {
            return new ContractExecutionResult(result.code, result.message);
          }
          var overlaps =await ProjectRequestHelper.DoesProjectOverlap(data.CustomerUid, data.ProjectUid, userUid,
            data.ProjectBoundaryWKT, log, serviceExceptionHandler, cwsProjectClient, customHeaders);
          if (overlaps)
          {
            return new ContractExecutionResult(43, "Project boundary overlaps another project.");
          }
        }

        /* TODO if coord sys file has changed or project type has changed to 3d enabled
        await ProjectRequestHelper.ValidateCoordSystemInProductivity3D(
          updateProjectEvent, serviceExceptionHandler, customHeaders, productivity3dV1ProxyCoord);       
       */
      }
      else if (data.UpdateType == ProjectUpdateType.Deleted)
      {
        if (!data.ProjectUid.HasValue)
        {
          return new ContractExecutionResult(5, "Missing ProjectUID.");
        }
        //no other validation for ProjectUpdateType.Deleted
      }

      return new ContractExecutionResult();
    }

    /// <summary>
    /// Validates a project name. Must be unique amongst active projects for the Customer.
    /// </summary>
    private async Task<int> ValidateProjectName(Guid customerUid, Guid userUid, string projectName, Guid? projectUid,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler,
      ICwsProjectClient cwsProjectClient, IHeaderDictionary customHeaders)
    {
      log.LogInformation($"{nameof(ValidateProjectName)} projectName: {projectName} projectUid: {projectUid}");

      var duplicateProjectNames =
        (await ProjectRequestHelper.GetProjectListForCustomer(customerUid, userUid,
          log, serviceExceptionHandler, cwsProjectClient, customHeaders))
        .Where(
          p => p.IsArchived == false &&
               string.Equals(p.Name, projectName, StringComparison.OrdinalIgnoreCase) &&
               (!projectUid.HasValue || !string.Equals(p.ProjectUID, projectUid.ToString(), StringComparison.OrdinalIgnoreCase)))
        .ToList();

      log.LogInformation($"{nameof(ValidateProjectName)} Any duplicateProjectNames? {JsonConvert.SerializeObject(duplicateProjectNames)} retrieved");
      return duplicateProjectNames.Count;
    }

    /// <summary>
    /// Does validation on the project boundary points.
    /// </summary>
    private object ValidateProjectBoundary(string projectBoundary)
    {
      var result = GeofenceValidation.ValidateWKT(projectBoundary);
      if (string.CompareOrdinal(result, GeofenceValidation.ValidationOk) != 0)
      {
        if (string.CompareOrdinal(result, GeofenceValidation.ValidationNoBoundary) == 0)
        {
          return new {code=23, message="Missing project boundary."};
        }

        if (string.CompareOrdinal(result, GeofenceValidation.ValidationLessThan3Points) == 0)
        {
          return new { code = 24, message = "Invalid project boundary as it should contain at least 3 points." }; 
        }

        if (string.CompareOrdinal(result, GeofenceValidation.ValidationInvalidFormat) == 0)
        {
          return new { code = 25, message = "Invalid project boundary." }; 
        }

        if (string.CompareOrdinal(result, GeofenceValidation.ValidationInvalidPointValue) == 0)
        {
          return new { code = 111, message = "Invalid project boundary points.Latitudes should be -90 through 90 and Longitude -180 through 180. Points around 0,0 are invalid" }; 
        }
      }

      if (PolygonUtils.SelfIntersectingPolygon(projectBoundary))
      {
        return new { code = 129, message = "Self-intersecting project boundary." };
      }
      return new { code = ContractExecutionStatesEnum.ExecutedSuccessfully, message = ContractExecutionResult.DefaultMessage };
    }

    protected override ContractExecutionResult ProcessEx<T>(T item) => throw new NotImplementedException();
  }
}
