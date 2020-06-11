using System;
using System.Collections.Generic;
using System.IO;
using Mvc = Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Project.Abstractions.Models.Cws;

namespace MockProjectWebApi.Controllers
{
  public class MockCwsProjectController : BaseController
  {
    private readonly string _baseUrl;
   
    // Map by projectTrn and projectConfigurationType
    private static readonly Dictionary<string, Dictionary<string, ProjectConfigurationModel>> _projectConfigFilesMap = new Dictionary<string, Dictionary<string, ProjectConfigurationModel>>();
    
    // Map filespaceIds to file names in CreateFile for generating sensible responses from Save/Update
    private static readonly Dictionary<string, string> _fileSpaceIdNameMap = new Dictionary<string, string>();

    // keyed by accountTrn then projectTrn
    private static readonly Dictionary<string, Dictionary<string, ProjectDetailResponseModel>> _projects = new Dictionary<string, Dictionary<string, ProjectDetailResponseModel>>();

    public MockCwsProjectController(ILoggerFactory loggerFactory, IConfigurationStore configurationStore) : base(loggerFactory)
    {
      _baseUrl = configurationStore.GetValueString("MOCK_WEBAPI_BASE_URL");
      if (string.IsNullOrEmpty(_baseUrl))
      {
        throw new ArgumentException("Missing environment variable MOCK_WEBAPI_BASE_URL");
      }
    }

    #region CWS Project Client
    [Mvc.Route("api/v1/projects")]
    [Mvc.HttpPost]
    public CreateProjectResponseModel CreateProject([Mvc.FromBody] CreateProjectRequestModel createProjectRequestModel)
    {
      var newProjectUid = Guid.NewGuid();
      var newProjectTrn = TRNHelper.MakeTRN(newProjectUid);
      var projectDetailResponseModel = new ProjectDetailResponseModel()
      {
        AccountTRN = TRNHelper.MakeTRN(createProjectRequestModel.AccountId, TRNHelper.TRN_ACCOUNT),
        ProjectTRN = newProjectTrn,
        ProjectName = createProjectRequestModel.ProjectName,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        UserProjectRole = UserProjectRoleEnum.Admin,
        LastUpdate = DateTime.UtcNow,
        ProjectSettings = new ProjectSettingsModel()
        {
          ProjectTRN = newProjectTrn,
          Boundary = createProjectRequestModel.Boundary,
          Config = new List<ProjectConfigurationModel>(),
        }
      };

      if (!_projects.ContainsKey(projectDetailResponseModel.AccountTRN))
        _projects.Add(projectDetailResponseModel.AccountTRN, new Dictionary<string, ProjectDetailResponseModel>());
      _projects[projectDetailResponseModel.AccountTRN].Add(newProjectTrn, projectDetailResponseModel);

      var createProjectResponseModel = new CreateProjectResponseModel() { TRN = newProjectTrn };
      Logger.LogInformation($"{nameof(CreateProject)}: createProjectRequestModel {JsonConvert.SerializeObject(createProjectRequestModel)} createProjectResponseModel {JsonConvert.SerializeObject(createProjectResponseModel)}");
      return createProjectResponseModel;
    }

    [Mvc.Route("api/v1/projects/{projectTrn}")]
    [Mvc.HttpPut]
    public void UpdateProjectDetails(string projectTrn, [Mvc.FromBody] UpdateProjectDetailsRequestModel updateProjectDetailsRequestModel)
    {
      foreach (var projectDict in _projects)
      {
        if (projectDict.Value.ContainsKey(projectTrn))
        {
          projectDict.Value[projectTrn].ProjectName = updateProjectDetailsRequestModel.projectName;
          Logger.LogInformation($"{nameof(UpdateProjectDetails)}: project found and updated {JsonConvert.SerializeObject(projectDict.Value[projectTrn])}");
        }
      }

      Logger.LogInformation($"{nameof(UpdateProjectDetails)}: projectTrn {projectTrn} updateProjectDetailsRequestModel {JsonConvert.SerializeObject(updateProjectDetailsRequestModel)}");
    }

    [Mvc.Route("api/v1/projects/{projectTrn}/boundary")]
    [Mvc.HttpPut]
    public void UpdateProjectBoundary(string projectTrn, [Mvc.FromBody] ProjectBoundary projectBoundary)
    {
      Logger.LogInformation($"{nameof(UpdateProjectBoundary)}: projectTrn {projectTrn} projectBoundary {JsonConvert.SerializeObject(projectBoundary)}");

      foreach (var projectDict in _projects)
      {
        if (projectDict.Value.ContainsKey(projectTrn))
        {
          projectDict.Value[projectTrn].ProjectSettings.Boundary = projectBoundary;
          Logger.LogInformation($"{nameof(UpdateProjectBoundary)}: project found and updated {JsonConvert.SerializeObject(projectDict.Value[projectTrn])}");
        }
      }

      Logger.LogInformation($"{nameof(UpdateProjectBoundary)}: project not found");
    }

    [Mvc.Route("api/v1/projects/{projectTrn}")]
    [Mvc.HttpGet]
    public ProjectDetailResponseModel GetProject(string projectTrn)
    {
      ProjectDetailResponseModel project;
      foreach (var projectDict in _projects)
      {
        if (projectDict.Value.ContainsKey(projectTrn))
        {
          project = projectDict.Value[projectTrn] as ProjectDetailResponseModel;
          //Get calibration file for project
          var configResult = GetProjectConfiguration(projectTrn, ProjectConfigurationFileType.CALIBRATION.ToString());
          if ((configResult.Result as Mvc.OkObjectResult)?.Value is ProjectConfigurationModel config)
          {
            project.ProjectSettings.Config.Add(new ProjectConfigurationModel
            {
              SiteCollectorFileName = config.SiteCollectorFileName,
              SiteCollectorFileDownloadLink = config.SiteCollectorFileDownloadLink,
              FileName = config.FileName,
              FileDownloadLink = config.FileDownloadLink,
              FileType = config.FileType
            });
          }

          Logger.LogInformation($"{nameof(GetProject)}: projectTrn {projectTrn} project {JsonConvert.SerializeObject(project)}");
          return project;
        }
      }

      Logger.LogInformation($"{nameof(GetProject)}: projectTrn {projectTrn} not found");
      return null;
    }

    [Mvc.Route("api/v1/accounts/{accountTrn}/projects")]
    [Mvc.HttpGet]
    public ProjectSummaryListResponseModel GetProjectsForCustomer(string accountTrn, [Mvc.FromQuery] int from, [Mvc.FromQuery] int limit)
    {
      var projectSummaryListResponseModel = new ProjectSummaryListResponseModel() { Projects = new List<ProjectSummaryResponseModel>() };
      if (_projects.ContainsKey(accountTrn))
      {
        foreach (var projectDict in _projects[accountTrn])
        {
          var projectSummaryResponseModel = new ProjectSummaryResponseModel
          {
            ProjectTRN = projectDict.Value.ProjectTRN,
            ProjectName = projectDict.Value.ProjectName,
            UserProjectRole = projectDict.Value.UserProjectRole,
            Boundary = projectDict.Value.ProjectSettings.Boundary,
            TimeZone = projectDict.Value.ProjectSettings.TimeZone,
            ProjectType = CwsProjectType.AcceptsTagFiles,
          };
          projectSummaryListResponseModel.Projects.Add(projectSummaryResponseModel);
        }
      }

      Logger.LogInformation($"{nameof(GetProjectsForCustomer)}: accountTrn {accountTrn} projectSummaryListResponseModel {JsonConvert.SerializeObject(projectSummaryListResponseModel)}");
      return projectSummaryListResponseModel;
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
        UploadUrl = $"{_baseUrl}/another_fake_upload_signed_url"
      };
      Logger.LogInformation($"{nameof(CreateFile)}: projectTrn {projectTrn} createFileRequest {JsonConvert.SerializeObject(createFileRequest)} createFileResponse {JsonConvert.SerializeObject(createFileResponse)}");
      _fileSpaceIdNameMap.Add(createFileResponse.FileSpaceId, $"{projectTrn}||{DateTime.UtcNow}||{createFileRequest.FileName}");
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
    [Obsolete("UI to use cws directly now")]
    public Mvc.ActionResult<ProjectConfigurationFileListResponseModel> GetProjectConfigurations(string projectTrn)
    {
      List<ProjectConfigurationModel> list = null;
      if (_projectConfigFilesMap.ContainsKey(projectTrn))
      {
        list = new List<ProjectConfigurationModel>();
        foreach (var key in _projectConfigFilesMap[projectTrn].Keys)
        {
          list.Add(_projectConfigFilesMap[projectTrn][key]);
        }
      }

      var projectConfigurationFileListResponse = new ProjectConfigurationFileListResponseModel();
      projectConfigurationFileListResponse.AddRange(list);
      Logger.LogInformation($"{nameof(GetProjectConfiguration)}: projectTrn {projectTrn} projectConfigurationFileListResponse {JsonConvert.SerializeObject(projectConfigurationFileListResponse)}");

      if (list == null)
        return NotFound();
      return Ok(projectConfigurationFileListResponse);
    }

    [Mvc.Route("api/v1/projects/{projectTrn}/configuration/{projectConfigurationType}")]
    [Mvc.HttpGet]
    [Obsolete("UI to use cws directly now")]
    public Mvc.ActionResult<ProjectConfigurationModel> GetProjectConfiguration(string projectTrn, string projectConfigurationType)
    {
      ProjectConfigurationModel projectConfigurationModel = null;
      if (_projectConfigFilesMap.ContainsKey(projectTrn) && _projectConfigFilesMap[projectTrn].ContainsKey(projectConfigurationType))
      {
        projectConfigurationModel = _projectConfigFilesMap[projectTrn][projectConfigurationType];
      }
      Logger.LogInformation($"{nameof(GetProjectConfiguration)}: projectTrn {projectTrn} projectConfigurationType {projectConfigurationType} projectConfigurationModel {JsonConvert.SerializeObject(projectConfigurationModel)}");

      if (projectConfigurationModel == null)
        return NotFound();
      return Ok(projectConfigurationModel);
    }

    [Mvc.Route("api/v1/projects/{projectTrn}/configuration/{projectConfigurationType}")]
    [Mvc.HttpPost]
    [Obsolete("UI to use cws directly now")]
    public ProjectConfigurationModel SaveProjectConfiguration(string projectTrn, string projectConfigurationType, [Mvc.FromBody] ProjectConfigurationFileRequestModel projectConfigurationFileRequest)
    {
      var projectConfigurationModel = new ProjectConfigurationModel
      {
        FileType = projectConfigurationType,
        FileName = !string.IsNullOrEmpty(projectConfigurationFileRequest.MachineControlFilespaceId) ? _fileSpaceIdNameMap[projectConfigurationFileRequest.MachineControlFilespaceId] : null,
        FileDownloadLink = !string.IsNullOrEmpty(projectConfigurationFileRequest.MachineControlFilespaceId) ? $"{_baseUrl}/another_fake_download_signed_url" : null,
        SiteCollectorFileName = !string.IsNullOrEmpty(projectConfigurationFileRequest.SiteCollectorFilespaceId) ? _fileSpaceIdNameMap[projectConfigurationFileRequest.SiteCollectorFilespaceId] : null,
        SiteCollectorFileDownloadLink = !string.IsNullOrEmpty(projectConfigurationFileRequest.SiteCollectorFilespaceId) ? $"{_baseUrl}/another_fake_download_signed_url" : null,
      };
      if (!_projectConfigFilesMap.ContainsKey(projectTrn))
      {
        _projectConfigFilesMap.Add(projectTrn, new Dictionary<string, ProjectConfigurationModel>());
      }
      if (!_projectConfigFilesMap[projectTrn].ContainsKey(projectConfigurationType))
      {
        _projectConfigFilesMap[projectTrn].Add(projectConfigurationType, projectConfigurationModel);
      }

      Logger.LogInformation($"{nameof(SaveProjectConfiguration)}: projectTrn {projectTrn} projectConfigurationType {projectConfigurationType} projectConfigurationFileRequest {JsonConvert.SerializeObject(projectConfigurationFileRequest)} projectConfigurationModel {JsonConvert.SerializeObject(projectConfigurationModel)}");
      return projectConfigurationModel;
    }

    [Mvc.Route("api/v1/projects/{projectTrn}/configuration/{projectConfigurationType}")]
    [Mvc.HttpPut]
    [Obsolete("UI to use cws directly now")]
    public Mvc.ActionResult<ProjectConfigurationModel> UpdateProjectConfiguration(string projectTrn, string projectConfigurationType, [Mvc.FromBody] ProjectConfigurationFileRequestModel projectConfigurationFileRequest)
    {
      ProjectConfigurationModel projectConfigurationModel = null;
      if (_projectConfigFilesMap.ContainsKey(projectTrn) && _projectConfigFilesMap[projectTrn].ContainsKey(projectConfigurationType))
      {
        projectConfigurationModel = _projectConfigFilesMap[projectTrn][projectConfigurationType];
        if (!string.IsNullOrEmpty(projectConfigurationFileRequest.MachineControlFilespaceId))
        {
          projectConfigurationModel.FileName = _fileSpaceIdNameMap[projectConfigurationFileRequest.MachineControlFilespaceId];
          projectConfigurationModel.FileDownloadLink = $"{_baseUrl}/another_fake_download_signed_url";
        }
        if (!string.IsNullOrEmpty(projectConfigurationFileRequest.SiteCollectorFilespaceId))
        {
          projectConfigurationModel.SiteCollectorFileName = _fileSpaceIdNameMap[projectConfigurationFileRequest.SiteCollectorFilespaceId];
          projectConfigurationModel.SiteCollectorFileDownloadLink = $"{_baseUrl}/another_fake_download_signed_url";
        }
      }

      Logger.LogInformation($"{nameof(UpdateProjectConfiguration)}: projectTrn {projectTrn} projectConfigurationType {projectConfigurationType} projectConfigurationFileRequest {JsonConvert.SerializeObject(projectConfigurationFileRequest)} projectConfigurationModel {JsonConvert.SerializeObject(projectConfigurationModel)}");
      if (projectConfigurationModel == null)
        return NotFound();
      return Ok(projectConfigurationModel);
    }

    [Mvc.Route("api/v1/projects/{projectTrn}/configuration/{projectConfigurationType}")]
    [Mvc.HttpDelete]
    [Obsolete("UI to use cws directly now")]
    public Mvc.ActionResult DeleteProjectConfiguration(string projectTrn, string projectConfigurationType)
    {
      Logger.LogInformation($"{nameof(DeleteProjectConfiguration)}: projectTrn {projectTrn} projectConfigurationType {projectConfigurationType}");
      if (_projectConfigFilesMap.ContainsKey(projectTrn) && _projectConfigFilesMap[projectTrn].ContainsKey(projectConfigurationType))
      {
        _projectConfigFilesMap[projectTrn].Remove(projectConfigurationType);
        return Ok();
      }

      return NotFound();

    }
    #endregion

  }
}
