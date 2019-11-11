using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;

namespace MockProjectWebApi.Controllers
{
  public class MockDataOceanController : BaseController
  {
    private const string GENERATED_TILE_FOLDER_SUFFIX = "_Tiles$";
    private const string TILE_METADATA_ROUTE = "/{path}";

    private readonly string baseUrl;

    public MockDataOceanController(ILoggerFactory loggerFactory, IConfigurationStore configurationStore)
      : base(loggerFactory)
    {
      baseUrl = configurationStore.GetValueString("MOCK_WEBAPI_BASE_URL");
      if (string.IsNullOrEmpty(baseUrl))
      {
        throw new ArgumentException("Missing environment variable MOCK_WEBAPI_BASE_URL");
      }
    }

    [HttpGet("/api/browse/directories")]
    [HttpGet("/api/browse/keyset_directories")]
    public IActionResult BrowseDirectories([FromQuery]string name, [FromQuery]bool owner, [FromQuery]Guid? parent_id)
    {
      Logger.LogInformation($"{nameof(BrowseDirectories)}: {Request.QueryString}");

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

      Logger.LogInformation($"{nameof(BrowseDirectories)} returning: {JsonConvert.SerializeObject(result)}");
      return Ok(result);
    }

    [HttpGet("/api/browse/files")]
    [HttpGet("/api/browse/keyset_files")]
    public IActionResult BrowseFiles([FromQuery]string name, [FromQuery]bool owner, [FromQuery]Guid? parent_id)
    {
      Logger.LogInformation($"{nameof(BrowseFiles)}: {Request.QueryString}");

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
              url = $"{baseUrl}/fake_upload_signed_url"
            },
            download = new
            {
              url = $"{baseUrl}/fake_download_signed_url{suffix}"
            },
            multifile = !string.IsNullOrEmpty(suffix)
          }
        }
      };

      Logger.LogInformation($"{nameof(BrowseFiles)} returning: {JsonConvert.SerializeObject(result)}");
      return Ok(result);
    }

    [HttpGet("/api/files/{id}")]
    public IActionResult GetFile([FromRoute]Guid id)
    {
      Logger.LogInformation($"{nameof(GetFile)}: {id}");

      var result = new
      {
        file = new
        {
          id = id,
          name = "some file",
          parent_id = (Guid?)null,
          status = "AVAILABLE",
          upload = new
          {
            url = $"{baseUrl}/fake_upload_signed_url"
          },
          download = new
          {
            url = $"{baseUrl}/fake_download_signed_url"
          }
        }
      };

      Logger.LogInformation($"{nameof(GetFile)} returning: {JsonConvert.SerializeObject(result)}");
      return Ok(result);
    }

    [HttpDelete("/api/files/{id}")]
    public IActionResult DeleteFile([FromRoute]Guid id)
    {
      Logger.LogInformation($"{nameof(DeleteFile)}: {id}");

      return NoContent();
    }


    [HttpPost("/api/directories")]
    public IActionResult CreateDirectory([FromBody]dynamic message)
    {
      Logger.LogInformation($"{nameof(CreateDirectory)}: {JsonConvert.SerializeObject(message)}");

      var result = new
      {
        directory = new
        {
          id = Guid.NewGuid(),
          name = message.directory.name,
          parent_id = message.directory.parent_id
        }
      };

      Logger.LogInformation($"{nameof(CreateDirectory)} returning: {JsonConvert.SerializeObject(result)}");
      return Ok(result);
    }

    [HttpPost("/api/files")]
    public IActionResult CreateFile([FromBody]dynamic message)
    {
      Logger.LogInformation($"{nameof(CreateFile)}: {JsonConvert.SerializeObject(message)}");

      var result = new
      {
        file = new
        {
          id = Guid.NewGuid(),
          name = message.file.name,
          parent_id = message.file.parent_id,
          status = "UPLOADABLE",
          upload = new
          {
            url = $"{baseUrl}/fake_upload_signed_url"
          },
          download = new
          {
            url = $"{baseUrl}/fake_download_signed_url"
          }
        }
      };

      Logger.LogInformation($"{nameof(CreateFile)} returning: {JsonConvert.SerializeObject(result)}");
      return Ok(result);
    }

    [HttpPut("/fake_upload_signed_url")]
    public IActionResult UploadFile()
    {
      Logger.LogInformation($"{nameof(UploadFile)}");

      return Ok();
    }

    [HttpGet("/fake_download_signed_url")]
    public Stream DownloadFile()
    {
      Logger.LogInformation($"{nameof(DownloadFile)}");

      byte[] buffer = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3 };
      return new MemoryStream(buffer);
    }

    [HttpGet("/fake_download_signed_url/tiles/tiles.json")]
    public Stream DownloadTilesMetadataFile()
    {
      Logger.LogInformation($"{nameof(DownloadTilesMetadataFile)}");

      return new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(tileMetadata)));
    }

    [HttpGet("/fake_download_signed_url/tiles/xyz/{zoomLevel}/{yTile}/{xTile}.png")]
    public IActionResult DownloadTilesLineworkFile([FromRoute] int zoomLevel, [FromRoute] int yTile, [FromRoute] int xTile)
    {
      Logger.LogInformation($"{nameof(DownloadTilesLineworkFile)}");

      var fileName = $"Resources/Z{zoomLevel}-Y{yTile}-X{xTile}.png";

      if (!System.IO.File.Exists(fileName))
      {
        //This is what DataOcean returns for a tile that doesn't exist
        return Forbid("Access denied");
      }

      return new FileStreamResult(new FileStream(fileName, FileMode.Open, FileAccess.Read), ContentTypeConstants.ImagePng);
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

    private static readonly TileMetadata tileMetadata = new TileMetadata
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
