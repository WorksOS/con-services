using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MockProjectWebApi.Controllers
{
  public class MockPegasusController : Controller
  {
    private const string PROCEDURE_NAME = "dxf_to_raster_tiles";
    private const string PROCEDURE_ID = "b8431158-1917-4d18-9f2e-e26b255900b7";

    [Route("/api/executions")]
    [HttpPost]
    public dynamic CreateExecution([FromBody]dynamic message)
    {
      Console.WriteLine($"CreateExecution: {JsonConvert.SerializeObject(message)}");

      var result = new
      {
        id = Guid.NewGuid(),
        status = "CREATED",
        execution_status = "NOT_READY",
        procedure_identifier = PROCEDURE_NAME,
        procedure_id = message.execution.procedure_id,
        parameters = message.execution.parameters
      };
      Console.WriteLine($"CreateExecution returning: {JsonConvert.SerializeObject(result)}");
      return new CreatedResult(Request.Path, result);
    }

    [Route("/api/executions/{id}")]
    [HttpDelete]
    public HttpResponseMessage DeleteExecution([FromRoute]Guid id)
    {
      Console.WriteLine($"DeleteExecution: {id}");

      return new HttpResponseMessage(HttpStatusCode.NoContent);
    }

    [Route("/api/executions/{id}/status")]
    [HttpGet]
    public dynamic GetExecutionStatus([FromRoute]Guid id)
    {
      Console.WriteLine($"GetExecutionStatus: {id}");

      var result = new
      {
        execution_attempt = new { 
          id = Guid.NewGuid(),
          status = "FINISHED",         
        }
      };
      Console.WriteLine($"GetExecutionStatus returning: {JsonConvert.SerializeObject(result)}");
      return result;

    }

    [Route("/api/executions/{id}")]
    [HttpGet]
    public dynamic GetExecution([FromRoute]Guid id)
    {
      Console.WriteLine($"GetExecution: {id}");

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
      Console.WriteLine($"GetExecution returning: {JsonConvert.SerializeObject(result)}");
      return result;

    }

  }
}
