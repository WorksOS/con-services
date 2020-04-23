using System;
using CCSS.CWS.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockCwsProjectController : BaseController
  {
    public MockCwsProjectController(ILoggerFactory loggerFactory) : base(loggerFactory)
    { }

    #region CWS Project Client
    [Route("api/v1/projects")]
    [HttpPost]
    public CreateProjectResponseModel CreateProject([FromQuery] CreateProjectRequestModel createProjectRequestModel)
    {
      var createProjectResponseModel = new CreateProjectResponseModel
      {
        Id = TRNHelper.MakeTRN(Guid.NewGuid(), TRNHelper.TRN_PROJECT)
      };

      Logger.LogInformation($"{nameof(CreateProject)}: createProjectRequestModel {JsonConvert.SerializeObject(createProjectRequestModel)} createProjectResponseModel {JsonConvert.SerializeObject(createProjectResponseModel)}");

      return createProjectResponseModel;
    }

    [Route("api/v1/projects/{projectTrn}")]
    [HttpPut]
    public void UpdateProjectDetails(string projectTrn, [FromQuery] UpdateProjectDetailsRequestModel updateProjectDetailsRequestModel)
    {
      Logger.LogInformation($"{nameof(UpdateProjectDetails)}: projectTrn {projectTrn} updateProjectDetailsRequestModel {JsonConvert.SerializeObject(updateProjectDetailsRequestModel)}");

      return;
    }

    [Route("api/v1/projects/{projectTrn}/boundary")]
    [HttpPut]
    public void UpdateProjectDetails(string projectTrn, [FromQuery] ProjectBoundary projectBoundary)
    {
     
      Logger.LogInformation($"{nameof(UpdateProjectDetails)}: projectTrn {projectTrn} projectBoundary {JsonConvert.SerializeObject(projectBoundary)}");

      return;
    }
    #endregion

    #region CWS Design Client
    [Route("api/v1/projects/{projectTrn}/file")]
    [HttpPost]
    public CreateFileResponseModel CreateFile(string projectTrn, [FromBody] CreateFileRequestModel createFileRequest)
    {
      var createFileResponse = new CreateFileResponseModel
      {
        FileSpaceId = Guid.NewGuid().ToString(),
        UploadUrl = "some upload url"
      };
      Logger.LogInformation($"{nameof(CreateFile)}: projectTrn {projectTrn} createFileRequest {JsonConvert.SerializeObject(createFileRequest)} createFileResponse {JsonConvert.SerializeObject(createFileResponse)}");

      return createFileResponse;
    }
    #endregion
 
    #region CWS Profile Settings Client
    [Route("api/v1/projects/{projectTrn}/configuration/{projectConfigurationFileType}")]
    [HttpPost]
    public ProjectConfigurationFileResponseModel SaveConfigurationFile(string projectTrn, string projectConfigurationType, [FromBody] ProjectConfigurationFileRequestModel projectConfigurationFileRequest)
    {
      var projectConfigurationFileResponse = new ProjectConfigurationFileResponseModel
      {
        FileName = "my file",
        FileDownloadLink = "my download link"
      };
      Logger.LogInformation($"{nameof(SaveConfigurationFile)}: projectTrn {projectTrn} projectConfigurationType {projectConfigurationType} projectConfigurationFileRequest {JsonConvert.SerializeObject(projectConfigurationFileRequest)} projectConfigurationFileResponse {JsonConvert.SerializeObject(projectConfigurationFileResponse)}");

      return projectConfigurationFileResponse;
    }

    [Route("api/v1/projects/{projectTrn}/configuration/{projectConfigurationFileType}")]
    [HttpPut]
    public ProjectConfigurationFileResponseModel UpdateConfigurationFile(string projectTrn, string projectConfigurationType, [FromBody] ProjectConfigurationFileRequestModel projectConfigurationFileRequest)
    {
      var projectConfigurationFileResponse = new ProjectConfigurationFileResponseModel
      {
        FileName = "my file",
        FileDownloadLink = "my download link"
      };
      Logger.LogInformation($"{nameof(UpdateConfigurationFile)}: projectTrn {projectTrn} projectConfigurationType {projectConfigurationType} projectConfigurationFileRequest {JsonConvert.SerializeObject(projectConfigurationFileRequest)} projectConfigurationFileResponse {JsonConvert.SerializeObject(projectConfigurationFileResponse)}");

      return projectConfigurationFileResponse;
    }
    #endregion

  }
}
