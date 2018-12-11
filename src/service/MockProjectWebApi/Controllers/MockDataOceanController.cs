using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VSS.ConfigurationStore;

namespace MockProjectWebApi.Controllers
{
  public class MockDataOceanController : Controller
  {
    private readonly IConfigurationStore configStore;
    private readonly string baseUrl;

    public MockDataOceanController(IConfigurationStore configurationStore)
    {
      baseUrl = configurationStore.GetValueString("MOCK_WEBAPI_BASE_URL");
      if (string.IsNullOrEmpty(baseUrl))
      {
        throw new ArgumentException("Missing environment variable MOCK_WEBAPI_BASE_URL");
      }
    }


    [Route("/api/browse/directories")]
    [HttpGet]
    public dynamic BrowseDirectories([FromQuery]string name, [FromQuery]bool owner, [FromQuery]Guid? parent_id)
    {
      Console.WriteLine($"BrowseDirectories: {Request.QueryString}");

      var result = new
      {
        directories = new[]
        {
          new
          {
            id = Guid.NewGuid(),
            name = name,
            parent_id = parent_id
          }
        }
      

      };
      Console.WriteLine($"BrowseDirectories returning: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    [Route("/api/browse/files")]
    [HttpGet]
    public dynamic BrowseFiles([FromQuery]string name, [FromQuery]bool owner, [FromQuery]Guid? parent_id)
    {
      Console.WriteLine($"BrowseFiles: {Request.QueryString}");

      var result = new
      {
        files = new[]
        {
          new
          {
            id = Guid.NewGuid(),
            name = name,
            parent_id = parent_id,
            status = "AVAILABLE",
            upload = new
            {
              url = $"{baseUrl}/dummy_upload_signed_url"
            }
          }
        }

      };
      Console.WriteLine($"BrowseFiles returning: {JsonConvert.SerializeObject(result)}");
      return result;

    }

    [Route("/api/files/{id}")]
    [HttpGet]
    public dynamic GetFile([FromRoute]Guid id)
    {
      Console.WriteLine($"GetFile: {id}");

      var result = new
      {
        id = id,
        name = "some file",
        parent_id = (Guid?)null,
        status = "AVAILABLE",
        upload = new
        {
          url = $"{baseUrl}/dummy_upload_signed_url"
        }
      };
      Console.WriteLine($"GetFile returning: {JsonConvert.SerializeObject(result)}");
      return result;

    }

    [Route("/api/files/{id}")]
    [HttpDelete]
    public HttpResponseMessage DeleteFile([FromRoute]Guid id)
    {
      Console.WriteLine($"DeleteFile: {id}");

      return new HttpResponseMessage(HttpStatusCode.NoContent);
    }


    [Route("/api/directories")]
    [HttpPost]
    public dynamic CreateDirectory([FromBody]dynamic message)
    {
      Console.WriteLine($"CreateDirectory: {JsonConvert.SerializeObject(message)}");

      var result = new
      {
        id = Guid.NewGuid(),
        name = message.directory.name,
        parent_id = message.directory.parent_id
      };
      Console.WriteLine($"CreateDirectory returning: {JsonConvert.SerializeObject(result)}");
      return new CreatedResult(Request.Path, result);
    }

    [Route("/api/files")]
    [HttpPost]
    public dynamic CreateFile([FromBody]dynamic message)
    {
      Console.WriteLine($"CreateFile: {JsonConvert.SerializeObject(message)}");

      var result = new
      {
        id = Guid.NewGuid(),
        name = message.file.name,
        parent_id = message.file.parent_id,
        status = "UPLOADABLE",
        upload = new
        {
          url = $"{baseUrl}/dummy_upload_signed_url"
        }
      };
      Console.WriteLine($"CreateFile returning: {JsonConvert.SerializeObject(result)}");
      return new CreatedResult(Request.Path, result);
    }

    [Route("/dummy_upload_signed_url")]
    [HttpPut]
    public HttpResponseMessage UploadFile()
    {
      return new HttpResponseMessage(HttpStatusCode.OK);
    }

  }
}
