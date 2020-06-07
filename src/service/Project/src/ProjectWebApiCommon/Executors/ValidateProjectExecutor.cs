using System;
using System.Linq;
using System.Threading.Tasks;
using CCSS.Geometry;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.Cws;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// Executor for validating project changes from CWS
  /// </summary>
  public class ValidateProjectExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var data = CastRequestObjectTo<ProjectValidation>(item, errorCode: 68);

      //TODO: Use CustmerUID from data rather than headers or check they match (ask Steve)

      var userUid = new Guid(userId);
      if (data.UpdateType == ProjectUpdateType.Created)
      {
        //Validate required fields are present
        if (!data.ProjectType.HasValue)
        {
          return new ContractExecutionResult(130, "Missing project type.");
        }
        if (string.IsNullOrEmpty(data.ProjectName))
        {
          return new ContractExecutionResult(11, "Missing Project Name.");
        }
        if (string.IsNullOrEmpty(data.ProjectBoundaryWKT))
        {
          return new ContractExecutionResult(8, "Missing Project Boundary.");
        }
        if (string.IsNullOrEmpty(data.CoordinateSystemFileSpaceId))
        {
          return new ContractExecutionResult(132, "Missing coordinate system filespaceId.");
        }

        //Validate project name not duplicate
        // ProjectUID won't be filled yet
        var duplicates = await ValidateProjectName(data.CustomerUid, userUid, data.ProjectName, data.ProjectUid);
        if (duplicates > 0)
        {
          return new ContractExecutionResult(109, $"Project Name must be unique. {duplicates} active project duplicates found.");
        }

        //Validate project boundary
        dynamic boundaryResult = ValidateProjectBoundary(data.ProjectBoundaryWKT);
        if (boundaryResult.code != ContractExecutionStatesEnum.ExecutedSuccessfully)
        {
          return new ContractExecutionResult(boundaryResult.code, boundaryResult.message);
        }

        //Validate project boundary doesn't overlap existing projects
        log.LogDebug($"Testing if there are overlapping projects for project {data.ProjectName}");
        var overlaps = await ProjectRequestHelper.DoesProjectOverlap(data.CustomerUid,
          null, userUid, data.ProjectBoundaryWKT,
          log, serviceExceptionHandler, cwsProjectClient, customHeaders);
        if (overlaps)
        {
          return new ContractExecutionResult(43, "Project boundary overlaps another project.");
        }

        //Validate coordinate system file
        dynamic coordSysResult = await ValidateCoordinateSystemFile(data.ProjectUid, data.CoordinateSystemFileSpaceId);
        if (coordSysResult.code != ContractExecutionStatesEnum.ExecutedSuccessfully)
        {
          return new ContractExecutionResult(coordSysResult.code, coordSysResult.message);
        }
      }

      else if (data.UpdateType == ProjectUpdateType.Updated)
      {
        //Validate projectUID
        if (!data.ProjectUid.HasValue)
        {
          return new ContractExecutionResult(5, "Missing ProjectUID.");
        }

        string downloadUrl = null;
        string filename = null;
        if (data.ProjectType.HasValue)
        {
          //Changing from non 3d-enabled to 3d-enabled.
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
          if (string.IsNullOrEmpty(data.CoordinateSystemFileSpaceId))
          {
            var projectConfiguration = project.ProjectSettings.Config?.FirstOrDefault(c => c.FileType == ProjectConfigurationFileType.CALIBRATION.ToString());
            downloadUrl = projectConfiguration?.FileDownloadLink;
            //filename format is: "trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f||2020-03-25 23:03:45.314||BootCamp 2012.dc"
            filename = projectConfiguration?.FileName.Split(ProjectConfigurationModel.FilenamePathSeparator)[2].Trim();
          }
        }

        //Validate project name if changed
        if (!string.IsNullOrEmpty(data.ProjectName))
        {
          var duplicates = await ValidateProjectName(data.CustomerUid, userUid, data.ProjectName, data.ProjectUid);
          if (duplicates > 0)
          {
            return new ContractExecutionResult(109, $"Project Name must be unique. {duplicates} active project duplicates found.");
          }
        }

        //Validate project boundary if changed
        if (!string.IsNullOrEmpty(data.ProjectBoundaryWKT))
        {
          dynamic boundaryResult = ValidateProjectBoundary(data.ProjectBoundaryWKT);
          if (boundaryResult.code != ContractExecutionStatesEnum.ExecutedSuccessfully)
          {
            return new ContractExecutionResult(boundaryResult.code, boundaryResult.message);
          }
          var overlaps =await ProjectRequestHelper.DoesProjectOverlap(data.CustomerUid, data.ProjectUid, userUid,
            data.ProjectBoundaryWKT, log, serviceExceptionHandler, cwsProjectClient, customHeaders);
          if (overlaps)
          {
            return new ContractExecutionResult(43, "Project boundary overlaps another project.");
          }
        }

        //Validate coordinate system file if changed
        if (!string.IsNullOrEmpty(data.CoordinateSystemFileSpaceId))
        {
          dynamic coordSysResult = await ValidateCoordinateSystemFile(data.ProjectUid, data.CoordinateSystemFileSpaceId);
          if (coordSysResult.code != ContractExecutionStatesEnum.ExecutedSuccessfully)
          {
            return new ContractExecutionResult(coordSysResult.code, coordSysResult.message);
          }
        }
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
    private async Task<int> ValidateProjectName(Guid customerUid, Guid userUid, string projectName, Guid? projectUid)
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
          return new { code=23, message="Missing project boundary." };
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
          return new { code = 111, message = "Invalid project boundary points. Latitudes should be -90 through 90 and Longitude -180 through 180. Points around 0,0 are invalid." }; 
        }
      }

      if (PolygonUtils.SelfIntersectingPolygon(projectBoundary))
      {
        return new { code = 129, message = "Self-intersecting project boundary." };
      }
      return new { code = ContractExecutionStatesEnum.ExecutedSuccessfully, message = ContractExecutionResult.DefaultMessage };
    }

    /// <summary>
    /// Validates the coordinate system file in TRex. In CWS this is the project configuration file of type CALIBRATION.
    /// </summary>
    private async Task<object> ValidateCoordinateSystemFile(Guid? projectUid, string filespaceId)
    {
      /*
      GetFileWithContentsModel file = null;
      try
      {
        var response = await cwsDesignClient.GetAndDownloadFile(projectUid.Value, filespaceId, customHeaders);
      }
      catch (Exception e)
      {
        return new { code = 131, message = $"A problem occurred downloading the calibration file. Exception: {e.Message}" };
      }

      //Validate file in 3dpm (TRex)
      CoordinateSystemSettingsResult coordinateSystemSettingsResult = null;
      try
      {
        //TODO: Extract the actual file name here from CWS one. Waiting on Jeannie's PR.
        var fileName = CwsFileNameHelper.ExtractFileName(file.FileName);
        coordinateSystemSettingsResult = await productivity3dV1ProxyCoord
          .CoordinateSystemValidate(file.FileContents, fileName, customHeaders);
      }
      catch (Exception e)
      {
        return new { code = 57, message = $"A problem occurred at the validate CoordinateSystem endpoint in 3dpm. Exception: {e.Message}" };
      }

      if (coordinateSystemSettingsResult == null)
        return new { code = 46, message = "Invalid CoordinateSystem." };

      if (coordinateSystemSettingsResult.Code != 0) //TASNodeErrorStatus.asneOK
      {
        return new { code = 47, message = $"Unable to validate CoordinateSystem in 3dpm: {coordinateSystemSettingsResult.Code} {coordinateSystemSettingsResult.Message}." };
      }
      */
      return new { code = ContractExecutionStatesEnum.ExecutedSuccessfully, message = ContractExecutionResult.DefaultMessage };
    }

    protected override ContractExecutionResult ProcessEx<T>(T item) => throw new NotImplementedException();
  }
}
