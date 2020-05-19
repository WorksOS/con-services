using System;
using System.Collections.Generic;
using System.IO;
using CCSS.CWS.Client;
using Mvc=Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;

namespace MockProjectWebApi.Controllers
{
  public class MockCwsProjectController : BaseController
  {
    private readonly string baseUrl;
    // Map by projectTrn and projectConfigurationType
    private static Dictionary<string, Dictionary<string, ProjectConfigurationFileResponseModel>> projectConfigFilesMap = new Dictionary<string, Dictionary<string, ProjectConfigurationFileResponseModel>>();
    // Map filespaceIds to file names in CreateFile for generating sensible responses from Save/Update
    private static Dictionary<string, string> filespaceIdNameMap = new Dictionary<string, string>();

    public MockCwsProjectController(ILoggerFactory loggerFactory, IConfigurationStore configurationStore) : base(loggerFactory)
    {
      baseUrl = configurationStore.GetValueString("MOCK_WEBAPI_BASE_URL");
      if (string.IsNullOrEmpty(baseUrl))
      {
        throw new ArgumentException("Missing environment variable MOCK_WEBAPI_BASE_URL");
      }
    }

    #region CWS Project Client
    [Mvc.Route("api/v1/projects")]
    [Mvc.HttpPost]
    public CreateProjectResponseModel CreateProject([Mvc.FromQuery] CreateProjectRequestModel createProjectRequestModel)
    {
      var createProjectResponseModel = new CreateProjectResponseModel
      {
        TRN = TRNHelper.MakeTRN(Guid.NewGuid(), TRNHelper.TRN_PROJECT)
      };

      Logger.LogInformation($"{nameof(CreateProject)}: createProjectRequestModel {JsonConvert.SerializeObject(createProjectRequestModel)} createProjectResponseModel {JsonConvert.SerializeObject(createProjectResponseModel)}");

      return createProjectResponseModel;
    }

    [Mvc.Route("api/v1/projects/{projectTrn}")]
    [Mvc.HttpPut]
    public void UpdateProjectDetails(string projectTrn, [Mvc.FromQuery] UpdateProjectDetailsRequestModel updateProjectDetailsRequestModel)
    {
      Logger.LogInformation($"{nameof(UpdateProjectDetails)}: projectTrn {projectTrn} updateProjectDetailsRequestModel {JsonConvert.SerializeObject(updateProjectDetailsRequestModel)}");

      return;
    }

    [Mvc.Route("api/v1/projects/{projectTrn}/boundary")]
    [Mvc.HttpPut]
    public void UpdateProjectDetails(string projectTrn, [Mvc.FromQuery] ProjectBoundary projectBoundary)
    {
     
      Logger.LogInformation($"{nameof(UpdateProjectDetails)}: projectTrn {projectTrn} projectBoundary {JsonConvert.SerializeObject(projectBoundary)}");

      return;
    }
    #endregion

    #region CWS Design Client
    [Mvc.Route("api/v1/projects/{projectTrn}/file")]
    [Mvc.HttpPost]
    public CreateFileResponseModel CreateFile(string projectTrn, [Mvc.FromBody] CreateFileRequestModel createFileRequest)
    {
      var createFileResponse = new CreateFileResponseModel
      {
        FileSpaceId = Guid.NewGuid().ToString(),
        UploadUrl = $"{baseUrl}/another_fake_upload_signed_url"
      };
      Logger.LogInformation($"{nameof(CreateFile)}: projectTrn {projectTrn} createFileRequest {JsonConvert.SerializeObject(createFileRequest)} createFileResponse {JsonConvert.SerializeObject(createFileResponse)}");
      filespaceIdNameMap.Add(createFileResponse.FileSpaceId, createFileRequest.FileName);
      return createFileResponse;
    }

    [Mvc.HttpPut("/another_fake_upload_signed_url")]
    public Mvc.IActionResult UploadFile()
    {
      Logger.LogInformation($"{nameof(UploadFile)}");

      return Ok();
    }

    [Mvc.HttpGet("/another_fake_download_signed_url")]
    public Stream DownloadFile()
    {
      Logger.LogInformation($"{nameof(DownloadFile)}");

      byte[] buffer = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3 };
      return new MemoryStream(buffer);
    }
    #endregion

    #region CWS Profile Settings Client
    [Mvc.Route("api/v1/projects/{projectTrn}/configuration")]
    [Mvc.HttpGet]
    public Mvc.ActionResult<ProjectConfigurationFileListResponseModel> GetProjectConfigurations(string projectTrn)
    {
      List<ProjectConfigurationFileResponseModel> list = null;
      if (projectConfigFilesMap.ContainsKey(projectTrn))
      {
        list = new List<ProjectConfigurationFileResponseModel>();
        foreach (var key in projectConfigFilesMap[projectTrn].Keys)
        {
          list.Add(projectConfigFilesMap[projectTrn][key]);
        }
      }
      var projectConfigurationFileListResponse = new ProjectConfigurationFileListResponseModel
      {
        ProjectConfigurationFiles = list
      };
      Logger.LogInformation($"{nameof(GetProjectConfiguration)}: projectTrn {projectTrn} projectConfigurationFileListResponse {JsonConvert.SerializeObject(projectConfigurationFileListResponse)}");

      if (list == null)
        return NotFound();
      return Ok(projectConfigurationFileListResponse);
    }

    [Mvc.Route("api/v1/projects/{projectTrn}/configuration/{projectConfigurationType}")]
    [Mvc.HttpGet]
    public Mvc.ActionResult<ProjectConfigurationFileResponseModel> GetProjectConfiguration(string projectTrn, string projectConfigurationType)
    {
      ProjectConfigurationFileResponseModel projectConfigurationFileResponse = null;
      if (projectConfigFilesMap.ContainsKey(projectTrn)&& projectConfigFilesMap[projectTrn].ContainsKey(projectConfigurationType))
      {
        projectConfigurationFileResponse = projectConfigFilesMap[projectTrn][projectConfigurationType];
      }
      Logger.LogInformation($"{nameof(GetProjectConfiguration)}: projectTrn {projectTrn} projectConfigurationType {projectConfigurationType} projectConfigurationFileResponse {JsonConvert.SerializeObject(projectConfigurationFileResponse)}");

      if (projectConfigurationFileResponse == null)
        return NotFound();
      return Ok(projectConfigurationFileResponse);
    }

    [Mvc.Route("api/v1/projects/{projectTrn}/configuration/{projectConfigurationType}")]
    [Mvc.HttpPost]
    public ProjectConfigurationFileResponseModel SaveProjectConfiguration(string projectTrn, string projectConfigurationType, [Mvc.FromBody] ProjectConfigurationFileRequestModel projectConfigurationFileRequest)
    {
      var projectConfigurationFileResponse = new ProjectConfigurationFileResponseModel
      {
        FileName = !string.IsNullOrEmpty(projectConfigurationFileRequest.MachineControlFilespaceId) ? filespaceIdNameMap[projectConfigurationFileRequest.MachineControlFilespaceId] : null,
        FileDownloadLink = !string.IsNullOrEmpty(projectConfigurationFileRequest.MachineControlFilespaceId) ? $"{baseUrl}/another_fake_download_signed_url" : null,
        SiteCollectorFileName = !string.IsNullOrEmpty(projectConfigurationFileRequest.SiteCollectorFilespaceId) ? filespaceIdNameMap[projectConfigurationFileRequest.SiteCollectorFilespaceId] : null,
        SiteCollectorFileDownloadLink = !string.IsNullOrEmpty(projectConfigurationFileRequest.SiteCollectorFilespaceId) ? $"{baseUrl}/another_fake_download_signed_url" : null,
      };
      if (!projectConfigFilesMap.ContainsKey(projectTrn))
      {
        projectConfigFilesMap.Add(projectTrn, new Dictionary<string, ProjectConfigurationFileResponseModel>());
      }
      if (!projectConfigFilesMap[projectTrn].ContainsKey(projectConfigurationType))
      {
        projectConfigFilesMap[projectTrn].Add(projectConfigurationType, projectConfigurationFileResponse);
      }

      Logger.LogInformation($"{nameof(SaveProjectConfiguration)}: projectTrn {projectTrn} projectConfigurationType {projectConfigurationType} projectConfigurationFileRequest {JsonConvert.SerializeObject(projectConfigurationFileRequest)} projectConfigurationFileResponse {JsonConvert.SerializeObject(projectConfigurationFileResponse)}");
      return projectConfigurationFileResponse;
    }

    [Mvc.Route("api/v1/projects/{projectTrn}/configuration/{projectConfigurationType}")]
    [Mvc.HttpPut]
    public Mvc.ActionResult<ProjectConfigurationFileResponseModel> UpdateProjectConfiguration(string projectTrn, string projectConfigurationType, [Mvc.FromBody] ProjectConfigurationFileRequestModel projectConfigurationFileRequest)
    {
      ProjectConfigurationFileResponseModel projectConfigurationFileResponse = null;
      if (projectConfigFilesMap.ContainsKey(projectTrn) && projectConfigFilesMap[projectTrn].ContainsKey(projectConfigurationType))
      {
        projectConfigurationFileResponse = projectConfigFilesMap[projectTrn][projectConfigurationType];
        if (!string.IsNullOrEmpty(projectConfigurationFileRequest.MachineControlFilespaceId))
        {
          projectConfigurationFileResponse.FileName = filespaceIdNameMap[projectConfigurationFileRequest.MachineControlFilespaceId];
          projectConfigurationFileResponse.FileDownloadLink = $"{baseUrl}/another_fake_download_signed_url";
        }
        if (!string.IsNullOrEmpty(projectConfigurationFileRequest.SiteCollectorFilespaceId))
        {
          projectConfigurationFileResponse.SiteCollectorFileName = filespaceIdNameMap[projectConfigurationFileRequest.SiteCollectorFilespaceId];
          projectConfigurationFileResponse.SiteCollectorFileDownloadLink = $"{baseUrl}/another_fake_download_signed_url";
        }
      }

      Logger.LogInformation($"{nameof(UpdateProjectConfiguration)}: projectTrn {projectTrn} projectConfigurationType {projectConfigurationType} projectConfigurationFileRequest {JsonConvert.SerializeObject(projectConfigurationFileRequest)} projectConfigurationFileResponse {JsonConvert.SerializeObject(projectConfigurationFileResponse)}");
      if (projectConfigurationFileResponse == null)
        return NotFound();
      return Ok(projectConfigurationFileResponse);
    }

    [Mvc.Route("api/v1/projects/{projectTrn}/configuration/{projectConfigurationType}")]
    [Mvc.HttpDelete]
    public Mvc.ActionResult DeleteProjectConfiguration(string projectTrn, string projectConfigurationType)
    {
      Logger.LogInformation($"{nameof(DeleteProjectConfiguration)}: projectTrn {projectTrn} projectConfigurationType {projectConfigurationType}");
      if (projectConfigFilesMap.ContainsKey(projectTrn) && projectConfigFilesMap[projectTrn].ContainsKey(projectConfigurationType))
      {
        projectConfigFilesMap[projectTrn].Remove(projectConfigurationType);
        return Ok();
      }

      return NotFound();

    }
    #endregion

  }
}
