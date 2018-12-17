using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VSS.ConfigurationStore;

namespace MockProjectWebApi.Controllers
{
  public class MockDataOceanController : Controller
  {
    private const string GENERATED_TILE_FOLDER_SUFFIX = "_Tiles$";
    //private string TILE_METADATA_ROUTE = $"{Path.DirectorySeparatorChar}/tiles{Path.DirectorySeparatorChar}/tiles.json";
    private string TILE_METADATA_ROUTE = "/{path}";

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
      Console.WriteLine($"{nameof(BrowseDirectories)}: {Request.QueryString}");

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
      Console.WriteLine($"{nameof(BrowseDirectories)} returning: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    [Route("/api/browse/files")]
    [HttpGet]
    public dynamic BrowseFiles([FromQuery]string name, [FromQuery]bool owner, [FromQuery]Guid? parent_id)
    {
      Console.WriteLine($"{nameof(BrowseFiles)}: {Request.QueryString}");

      var suffix = string.Empty;
      if (name.Contains(GENERATED_TILE_FOLDER_SUFFIX))
      {
        suffix = TILE_METADATA_ROUTE;
      }
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
            },
            download = new
            {
              url = $"{baseUrl}/dummy_download_signed_url{suffix}"
            }
          }
        }

      };
      Console.WriteLine($"{nameof(BrowseFiles)} returning: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    [Route("/api/files/{id}")]
    [HttpGet]
    public dynamic GetFile([FromRoute]Guid id)
    {
      Console.WriteLine($"{nameof(GetFile)}: {id}");

      var result = new
      {
        id = id,
        name = "some file",
        parent_id = (Guid?)null,
        status = "AVAILABLE",
        upload = new
        {
          url = $"{baseUrl}/dummy_upload_signed_url"
        },
        download = new
        {
          url = $"{baseUrl}/dummy_download_signed_url"
        }
      };
      Console.WriteLine($"{nameof(GetFile)} returning: {JsonConvert.SerializeObject(result)}");
      return result;
    }

    [Route("/api/files/{id}")]
    [HttpDelete]
    public HttpResponseMessage DeleteFile([FromRoute]Guid id)
    {
      Console.WriteLine($"{nameof(DeleteFile)}: {id}");

      return new HttpResponseMessage(HttpStatusCode.NoContent);
    }


    [Route("/api/directories")]
    [HttpPost]
    public dynamic CreateDirectory([FromBody]dynamic message)
    {
      Console.WriteLine($"{nameof(CreateDirectory)}: {JsonConvert.SerializeObject(message)}");


      var result = new
      {
        id = Guid.NewGuid(),
        name = message.directory.name,
        parent_id = message.directory.parent_id
      };
      Console.WriteLine($"{nameof(CreateDirectory)} returning: {JsonConvert.SerializeObject(result)}");
      return new CreatedResult(Request.Path, result);
    }

    [Route("/api/files")]
    [HttpPost]
    public dynamic CreateFile([FromBody]dynamic message)
    {
      Console.WriteLine($"{nameof(CreateFile)}: {JsonConvert.SerializeObject(message)}");

      var result = new
      {
        id = Guid.NewGuid(),
        name = message.file.name,
        parent_id = message.file.parent_id,
        status = "UPLOADABLE",
        upload = new
        {
          url = $"{baseUrl}/dummy_upload_signed_url"
        },
        download = new
        {
          url = $"{baseUrl}/dummy_download_signed_url"
        }
      };
      Console.WriteLine($"{nameof(CreateFile)} returning: {JsonConvert.SerializeObject(result)}");
      return new CreatedResult(Request.Path, result);
    }

    [Route("/dummy_upload_signed_url")]
    [HttpPut]
    public HttpResponseMessage UploadFile()
    {
      Console.WriteLine($"{nameof(UploadFile)}");

      return new HttpResponseMessage(HttpStatusCode.OK);
    }

    [Route("/dummy_download_signed_url")]
    [HttpGet]
    public Stream DownloadFile()
    {
      Console.WriteLine($"{nameof(DownloadFile)}");

      byte[] buffer = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3 };
      return new MemoryStream(buffer);
    }

    [Route("/dummy_download_signed_url/tiles/tiles.json")]
    [HttpGet]
    public Stream DownloadTilesMetadataFile()
    {
      Console.WriteLine($"{nameof(DownloadTilesMetadataFile)}");

      byte[] byteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(tileMetadata));
      return new MemoryStream(byteArray);
    }

    //We need a copy of this model class from DataOcean as we can't use dynamic due to the hypenated property names
    public class TileMetadata
    {
      [JsonProperty(PropertyName = "extents", Required = Required.Default)]
      public Extents Extents { get; set; }
      [JsonProperty(PropertyName = "start-zoom", Required = Required.Default)]
      public int MinZoom { get; set; }
      [JsonProperty(PropertyName = "end-zoom", Required = Required.Default)]
      public int MaxZoom { get; set; }
      [JsonProperty(PropertyName = "tile-count", Required = Required.Default)]
      public int TileCount { get; set; }

    }

    public class Extents
    {
      [JsonProperty(PropertyName = "north", Required = Required.Default)]
      public double North { get; set; }
      [JsonProperty(PropertyName = "south", Required = Required.Default)]
      public double South { get; set; }
      [JsonProperty(PropertyName = "east", Required = Required.Default)]
      public double East { get; set; }
      [JsonProperty(PropertyName = "west", Required = Required.Default)]
      public double West { get; set; }
      [JsonProperty(PropertyName = "coord_system", Required = Required.Default)]
      public CoordSystem CoordSystem { get; set; }
    }

    public class CoordSystem
    {
      [JsonProperty(PropertyName = "type", Required = Required.Default)]
      public string Type { get; set; }
      [JsonProperty(PropertyName = "value", Required = Required.Default)]
      public string Value { get; set; }
    }

    public static TileMetadata tileMetadata = new TileMetadata
    {
      Extents = new Extents
      {
        North = 0.6581020324759275,
        South = 0.6573494852112898,
        East = -1.9427990915164108,
        West = -1.9437871937920903,
        CoordSystem = new CoordSystem
        {
          Type = "EPSG",
          Value = "EPSG:4326"
        }
      },
      MaxZoom = 21,
      TileCount = 79
    };

  }
}
