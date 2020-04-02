using System;
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

    [Route("api/v1/projects")]
    [HttpPost]
    public CreateProjectResponseModel CreateProject([FromQuery] CreateProjectRequestModel createProjectRequestModel)
    {
      var createProjectResponseModel = new CreateProjectResponseModel
      {
        Id = Guid.NewGuid().ToString()
      };

      Logger.LogInformation($"CreateProject: createProjectRequestModel {JsonConvert.SerializeObject(createProjectRequestModel)} createProjectResponseModel {JsonConvert.SerializeObject(createProjectResponseModel)}");

      return createProjectResponseModel;
    }
  }
}
