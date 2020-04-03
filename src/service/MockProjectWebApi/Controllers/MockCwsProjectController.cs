using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Clients.CWS.Utilities;

namespace MockProjectWebApi.Controllers
{
  public class MockCwsProjectController : BaseController
  {
    public MockCwsProjectController(ILoggerFactory loggerFactory) : base(loggerFactory)
    { }

    [Route("api/v1/projects")]
    [HttpPost]
    public CreateProjectResponseModel CreateProject([FromQuery] CreateProjectRequestModel createProjectRequestModel)
    {
      var createProjectResponseModel = new CreateProjectResponseModel
      {
        Id = TRNHelper.MakeTRN(Guid.NewGuid(), TRNHelper.TRN_PROJECT)
      };

      Logger.LogInformation($"CreateProject: createProjectRequestModel {JsonConvert.SerializeObject(createProjectRequestModel)} createProjectResponseModel {JsonConvert.SerializeObject(createProjectResponseModel)}");

      return createProjectResponseModel;
    }

    [Route("api/v1/projects/{projectTrn}")]
    [HttpPut]
    public void UpdateProjectDetails(string projectTrn, [FromQuery] UpdateProjectDetailsRequestModel updateProjectDetailsRequestModel)
    {
      Logger.LogInformation($"UpdateProjectDetails: projectTrn {projectTrn} updateProjectDetailsRequestModel {JsonConvert.SerializeObject(updateProjectDetailsRequestModel)}");

      return;
    }

    [Route("api/v1/projects/{projectTrn}/boundary")]
    [HttpPut]
    public void UpdateProjectDetails(string projectTrn, [FromQuery] ProjectBoundary projectBoundary)
    {
      Logger.LogInformation($"UpdateProjectDetails: projectTrn {projectTrn} projectBoundary {JsonConvert.SerializeObject(projectBoundary)}");

      return;
    }
  }
}
