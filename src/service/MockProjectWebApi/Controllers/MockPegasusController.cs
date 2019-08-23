using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MockProjectWebApi.Controllers
{
  public class MockPegasusController : BaseController
  {
    private const string PROCEDURE_NAME = "dxf_to_raster_tiles";
    private const string PROCEDURE_ID = "b8431158-1917-4d18-9f2e-e26b255900b7";

    public MockPegasusController(ILoggerFactory loggerFactory)
      : base(loggerFactory)
    { }

    [Route("/api/executions")]
    [HttpPost]
    public dynamic CreateExecution([FromBody]dynamic message)
    {
      Logger.LogInformation($"{nameof(CreateExecution)}: {JsonConvert.SerializeObject(message)}");

      var result = new
      {
        execution = new
        {
          id = Guid.NewGuid(),
          status = "CREATED",
          execution_status = "NOT_READY",
          procedure_identifier = PROCEDURE_NAME,
          procedure_id = message.execution.procedure_id,
          parameters = message.execution.parameters
        }
      };

      Logger.LogInformation($"{nameof(CreateExecution)} returning: {JsonConvert.SerializeObject(result)}");
      return new CreatedResult(Request.Path, result);
    }

    [Route("/api/executions/{id}")]
    [HttpDelete]
    public HttpResponseMessage DeleteExecution([FromRoute]Guid id)
    {
      Logger.LogInformation($"{nameof(DeleteExecution)}: {id}");

      return new HttpResponseMessage(HttpStatusCode.NoContent);
    }

    [Route("/api/executions/{id}/start")]
    [HttpPost]
    public dynamic StartExecution([FromRoute]Guid id)
    {
      Logger.LogInformation($"{nameof(StartExecution)}: {id}");

      var result = new
      {
        execution_attempt = new
        {
          id = Guid.NewGuid(),
          status = "EXECUTING"
        }
      };
      Logger.LogInformation($"{nameof(StartExecution)} returning: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    [Route("/api/executions/{id}/status")]
    [HttpGet]
    public dynamic GetExecutionStatus([FromRoute]Guid id)
    {
      Logger.LogInformation($"{nameof(GetExecutionStatus)}: {id}");

      var result = new
      {
        execution_attempt = new
        {
          id = Guid.NewGuid(),
          status = "FINISHED",
        }
      };
      Logger.LogInformation($"{nameof(GetExecutionStatus)} returning: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    [Route("/api/executions/{id}")]
    [HttpGet]
    public dynamic GetExecution([FromRoute]Guid id)
    {
      Logger.LogInformation($"{nameof(GetExecution)}: {id}");

      var result = new
      {
        execution = new
        {
          id = id,
          execution_status = "FINISHED",
          procedure_identifier = PROCEDURE_NAME,
          procedure_id = PROCEDURE_ID,
          latest_attempt = new
          {
            id = Guid.NewGuid(),
            status = "FINISHED"
          }
        }
      };
      Logger.LogInformation($"{nameof(GetExecution)} returning: {JsonConvert.SerializeObject(result)}");
      return result;
    }
  }
}
