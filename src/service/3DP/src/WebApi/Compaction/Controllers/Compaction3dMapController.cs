using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Compaction.ActionServices;
using VSS.Productivity3D.WebApi.Compaction.Controllers.Filters;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Interfaces;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.TCCFileAccess;
using VSS.TRex.Designs.TTM.Optimised;
using VSS.TRex.Designs.TTM.Optimised.Exceptions;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// A controller for getting 3d map tiles
  /// </summary>
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  [ProjectVerifier]
  public class Compaction3DMapController : BaseController<Compaction3DMapController>
  {
    /// <summary>
    /// Map Display Type for the 3d Maps control
    /// </summary>
    public enum MapDisplayType
    {
      /// <summary>
      /// Height Map image
      /// </summary>
      HeightMap = 0,
      /// <summary>
      /// Height Map representing the design
      /// </summary>
      DesignMap = 1,
      /// <summary>
      /// The texture to be displayed
      /// </summary>
      Texture = 2
    }

    /// <summary>
    /// Class to hold a face, used for the model generated
    /// TODO: Move this to it's own file, even own library if we need obj files after PoC
    /// </summary>
    private class Face
    {
      public int VertexIdx0 { get; set; }
      public int VertexIdx1 { get; set; }
      public int VertexIdx2 { get; set; }

      public int UvIdx0 { get; set; }
      public int UvIdx1 { get; set; }
      public int UvIdx2 { get; set; }
    }

    /// <summary>
    /// Class to hold a UV, used for the texture mapping on the model
    /// TODO: Move this to it's own file, even own library if we need obj files after PoC
    /// </summary>
    public class Uv
    {
      public double U { get; set; }
      public double V { get; set; }
    }

    private readonly IProductionDataTileService tileService;
    private readonly IBoundingBoxHelper boundingBoxHelper;

    /// <summary>
    /// Default Constructor
    /// </summary>
    public Compaction3DMapController(ILoggerFactory loggerFactory,
      IServiceExceptionHandler serviceExceptionHandler,
      IConfigurationStore configStore,
      IFileListProxy fileListProxy,
      IProjectSettingsProxy projectSettingsProxy,
      IFilterServiceProxy filterServiceProxy,
      ICompactionSettingsManager settingsManager,
      IProductionDataTileService tileService,
      IASNodeClient raptorClient,
      IBoundingBoxHelper boundingBoxHelper) : base(configStore, fileListProxy, settingsManager)
    {
      this.tileService = tileService;
      this.boundingBoxHelper = boundingBoxHelper;
    }

    /// <summary>
    /// Generates a image for use in the 3d map control
    /// These can be heightmaps / textures / designs
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Optional Filter UID</param>
    /// <param name="designUid">The Design File UID if showing the Design (ignored otherwise)</param>
    /// <param name="cutfillDesignUid">Cut fill design UID for the Texture, ignored for other modes</param>
    /// <param name="type">Map Display Type - Heightmap / Texture / Design</param>
    /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc (Ignored in Height Map type)</param>
    /// <param name="width">The width, in pixels, of the image tile to be rendered</param>
    /// <param name="height">The height, in pixels, of the image tile to be rendered</param>
    /// <param name="bbox">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
    /// <returns>An image representing the data requested</returns>
    [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
    [ValidateTileParameters]
    [Route("api/v2/map3d")]
    [HttpGet]
    public async Task<TileResult> GetMapTileData(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? designUid,
      [FromQuery] Guid? cutfillDesignUid,
      [FromQuery] MapDisplayType type,
      [FromQuery] DisplayMode mode,
      [FromQuery] ushort width,
      [FromQuery] ushort height,
      [FromQuery] string bbox)
    {
      var projectId = await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);

      CompactionProjectSettingsColors projectSettingsColors;
      DesignDescriptor design = null;
      DesignDescriptor cutFillDesign = null;
      if (type == MapDisplayType.DesignMap)
      {
        projectSettingsColors = GetGreyScaleHeightColors();
        design = await GetAndValidateDesignDescriptor(projectUid, designUid);
        mode = DisplayMode.Design3D;
      }
      else if (type == MapDisplayType.HeightMap)
      {
        projectSettingsColors = GetGreyScaleHeightColors();
        mode = DisplayMode.Height; // The height map must be of type height....
      }
      else if(type == MapDisplayType.Texture)
      {
        // Only used in texture mode
        cutFillDesign = cutfillDesignUid.HasValue
          ? await GetAndValidateDesignDescriptor(projectUid, cutfillDesignUid)
          : null;

        projectSettingsColors = await GetProjectSettingsColors(projectUid);
      }
      else
      {
        throw new NotImplementedException();
      }

      var filter = await GetCompactionFilter(projectUid, filterUid);

      var tileResult = WithServiceExceptionTryExecute(() =>
        tileService.GetProductionDataTile(projectSettings,
          projectSettingsColors,
          filter,
          projectId,
          projectUid,
          mode,
          width,
          height,
          boundingBoxHelper.GetBoundingBox(bbox),
          design ?? cutFillDesign, // If we have a design, it means we are asking for the design height map - otherwise we may have a cut fill design to determine the texture
          null,
          null,
          null,
          null,
          CustomHeaders));
      Response.Headers.Add("X-Warning", tileResult.TileOutsideProjectExtents.ToString());

      return tileResult;
    }

    /// <summary>
    /// Generates a raw image for use in the 3d map control
    /// These can be heightmaps / textures / designs
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Optional Filter UID</param>
    /// <param name="type">Map Display Type - Heightmap / Texture / Design</param>
    /// <param name="designUid">The Design File UID if showing the Design (ignored otherwise)</param>
    /// <param name="cutfillDesignUid">Cut fill design UID for the Texture, ignored for other modes</param>
    /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc (Ignored in Height Map type)</param>
    /// <param name="width">The width, in pixels, of the image tile to be rendered</param>
    /// <param name="height">The height, in pixels, of the image tile to be rendered</param>
    /// <param name="bbox">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
    /// <returns>An image representing the data requested</returns>
    [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
    [ValidateTileParameters]
    [Route("api/v2/map3d/png")]
    [HttpGet]
    [Obsolete("Use the TTM endpoint instead, it contains the model and texture in one result")]
    public async Task<FileResult> GetMapTileDataRaw(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? designUid,
      [FromQuery] Guid? cutfillDesignUid,
      [FromQuery] MapDisplayType type,
      [FromQuery] DisplayMode mode,
      [FromQuery] ushort width,
      [FromQuery] ushort height,
      [FromQuery] string bbox)
    {
      var result = await GetMapTileData(projectUid, filterUid, designUid, cutfillDesignUid, type, mode, width, height, bbox);
      return new FileStreamResult(new MemoryStream(result.TileData), "image/png");
    }

    /// <summary>
    /// Get the ttm files from raptor for production data and design if required
    /// Generated the texture for the production data
    /// Return a zip file containing them both
    /// </summary>
    /// <returns>A Zip file containing a file 'model.obj', containing the 3d model(s). and a Texture.png </returns>
    [ResponseCache(Duration = 900, VaryByQueryKeys = new[] {"*"})]
    [Route("api/v2/map3d/ttm")]
    [HttpGet]
    public async Task<FileResult> GetMapTileDataTtm(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? designUid,
      [FromQuery] DisplayMode mode,
      [FromServices] IPreferenceProxy prefProxy,
      [FromServices] ITRexCompactionDataProxy tRexCompactionDataProxy,
      [FromServices] IASNodeClient raptorClient,
      [FromServices] IProductionDataRequestFactory requestFactory,
      [FromServices] IFileRepository tccFileRepository)
    {
      const double radToDegrees = 180.0 / Math.PI;
      const double surfaceExportTollerance = 0.05;

      var tins = new List<TrimbleTINModel>();

      var project = await ((RaptorPrincipal) User).GetProject(projectUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var userPreferences = await prefProxy.GetUserPreferences(CustomHeaders);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var design = await GetAndValidateDesignDescriptor(projectUid, designUid);
      if (userPreferences == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Failed to retrieve preferences for current user"));
      }

      // Get the terrain mesh
      var exportRequest = requestFactory.Create<ExportRequestHelper>(r => r
          .ProjectUid(projectUid)
          .ProjectId(project.LegacyProjectId)
          .Headers(CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .SetUserPreferences(userPreferences)
        .SetRaptorClient(raptorClient)
        .SetProjectDescriptor(project)
        .CreateExportRequest(
          null, //startUtc,
          null, //endUtc,
          CoordType.LatLon,
          ExportTypes.SurfaceExport,
          "test.zip",
          true,
          false,
          OutputTypes.VedaAllPasses,
          string.Empty,
          surfaceExportTollerance);

      exportRequest.Validate();

      // First get the export of production data from Raptor
      // comes in a zip file
      var result = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionExportExecutor>(LoggerFactory, raptorClient, configStore: ConfigStore,
            trexCompactionDataProxy: tRexCompactionDataProxy, customHeaders: CustomHeaders)
          .Process(exportRequest) as CompactionExportResult);

      var zipStream = new FileStream(result.FullFileName, FileMode.Open);

      using (var archive = new ZipArchive(zipStream))
      {
        // The zip file will have exactly one file in it
        if (archive.Entries.Count == 1)
        {
          try
          {
            var tin = new TrimbleTINModel();
            using (var stream = archive.Entries[0].Open() as DeflateStream)
            using (var ms = new MemoryStream())
            {
              // Unzip the file, copy to memory as the TIN file needs the byte array, and stream
              stream.CopyTo(ms);
              ms.Seek(0, SeekOrigin.Begin);

              tin.LoadFromStream(ms, ms.GetBuffer());

              tins.Add(tin);
            }
          }
          catch (TTMFileReadException e)
          {
            // Not valid, continue
            Log.LogWarning(e, "Failed to parse ttm in zip file");
          }
        }

      }

      // If we didn't get a valid file, then we failed to read the ttm from raptor
      if (tins.Count == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Failed to retrieve raptor data"));
      }

      // If we have a design request, get the ttm and add it for parsing
      if (design != null)
      {
        var path = design.File.path + "/" + design.File.fileName;
        var file = await tccFileRepository.GetFile(design.File.filespaceId, path);
        using (var ms = new MemoryStream())
        {
          if (file != null)
          {
            file.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var tin = new TrimbleTINModel();
            tin.LoadFromStream(ms, ms.GetBuffer());
            tins.Add(tin);
          }
        }
      }

      // Calculating the bounding box for the model (including design if supplied)
      var minEasting = tins.Select(t => t.Header.MinimumEasting).Min();
      var maxEasting = tins.Select(t => t.Header.MaximumEasting).Max();
      var minNorthing = tins.Select(t => t.Header.MinimumNorthing).Min();
      var maxNorthing = tins.Select(t => t.Header.MaximumNorthing).Max();
      var centerEasting = (maxEasting + minEasting) / 2.0;
      var centerNorthing = (maxNorthing + minNorthing) / 2.0;
      
      var points = new TWGS84FenceContainer
      {
        FencePoints = new[]
        {
          TWGS84Point.Point(minEasting, minNorthing),
          TWGS84Point.Point(maxEasting, maxNorthing),
          TWGS84Point.Point(centerEasting, centerNorthing),
        }
      };
      
      // Convert the northing easting values to long lat values
      var res = raptorClient.GetGridCoordinates(project.LegacyProjectId, points, TCoordConversionType.ctNEEtoLLH, out var coordPointList);
      if (res != TCoordReturnCode.nercNoError)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Failed to retrieve long lat for boundary"));
      }

      // The values returned from Raptor are in rads, where we need degrees for the bbox
      var minLat = coordPointList.Points.Coords[0].Y * radToDegrees;
      var minLng = coordPointList.Points.Coords[0].X * radToDegrees;
      var maxLat = coordPointList.Points.Coords[1].Y * radToDegrees;
      var maxLng = coordPointList.Points.Coords[1].X * radToDegrees;
      var centerLat = coordPointList.Points.Coords[2].Y * radToDegrees;
      var centerLng = coordPointList.Points.Coords[2].X * radToDegrees;
      var bbox = $"{minLat},{minLng},{maxLat},{maxLng}"; 

      var outputStream = new MemoryStream();
      using (var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
      {
        var textureZipEntry = zipArchive.CreateEntry("texture.png");
        using (var stream = textureZipEntry.Open())
        {
          // Write the texture to the zip
          var textureFileStream = await GetTexture(projectUid, designUid, projectSettings, filter, mode, bbox);
          textureFileStream.FileStream.CopyTo(stream);
        }

        // Write the model to the zip
        var modelZipEntry = zipArchive.CreateEntry("model.obj");
        using (var stream = modelZipEntry.Open())
        {
          var modelFileStream = ConvertMultipleToObj(tins, centerEasting, centerNorthing);
          modelFileStream.FileStream.CopyTo(stream);
        }

        // Add some metadata to help with positioning of the model
        var metaDataEntry = zipArchive.CreateEntry("metadata.json");
        using (var stream = metaDataEntry.Open())
        {
          var metaData = new
          {
            Minimum = new
            {
              Lat = minLat,
              Lng = minLng
            },
            Maximum = new
            {
              Lat = maxLat,
              Lng = maxLng
            },
            Center = new
            {
              Lat = centerLat,
              Lng = centerLng
            },
            HasDesign = design != null
          };
          var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metaData));
          stream.Write(bytes,0, bytes.Length);
        }
      }

      // Don't forget to seek back, or else the content length will be 0
      outputStream.Seek(0, SeekOrigin.Begin);
      return new FileStreamResult(outputStream, "application/zip");
    }

    /// <summary>
    /// Get the texture for the model being created
    /// Generates the required boundign box, using the same information as used to generated the 3d model
    /// </summary>
    private async Task<FileStreamResult> GetTexture(Guid projectUid, Guid? cutfillDesignUid,
      CompactionProjectSettings projectSettings, FilterResult filter, DisplayMode mode, string bbox)
    {
      
      var project = await ((RaptorPrincipal) User).GetProject(projectUid);
      var cutFillDesign = cutfillDesignUid.HasValue
        ? await GetAndValidateDesignDescriptor(projectUid, cutfillDesignUid)
        : null;

      var projectSettingsColors = await GetProjectSettingsColors(projectUid);
      
      var tileResult = WithServiceExceptionTryExecute(() =>
        tileService.GetProductionDataTile(projectSettings,
          projectSettingsColors,
          filter,
          project.LegacyProjectId,
          projectUid,
          mode,
          4096,
          4096,
          boundingBoxHelper.GetBoundingBox(bbox),
          cutFillDesign, // If we have a design, it means we are asking for the design height map - otherwise we may have a cut fill design to determine the texture
          null,
          null,
          null,
          null,
          CustomHeaders));
      Response.Headers.Add("X-Warning", tileResult.TileOutsideProjectExtents.ToString());

      return new FileStreamResult(new MemoryStream(tileResult.TileData), "image/png");
    }

    /// <summary>
    /// Converts a collection of TTMs to a single obj model, including UV mapping
    /// </summary>
    private FileStreamResult ConvertMultipleToObj(IList<TrimbleTINModel> tins, double eastingOffset, double northingOffset)
    {
      // FileStreamResult will dispose of this once the response has been completed
      // See here: https://github.com/aspnet/Mvc/blob/25eb50120eceb62fd24ab5404210428fcdf0c400/src/Microsoft.AspNetCore.Mvc.Core/FileStreamResult.cs#L82
      var outputStream = new MemoryStream();
      using (var writer = new StreamWriter(outputStream, Encoding.UTF8, 32, true))
      {
        var
          vertexOffset =
            1; // With multiple objects in a file, the vertex indices used by faces does NOT reset between objects, therefor we have to keep a count
        var currentUvIndex = 1;
        var objIdx = 1;

        var zModifier = tins.SelectMany(t => t.Vertices.Items).Min(v => v.Z);

        var minX = tins.SelectMany(t => t.Vertices.Items).Min(v => v.X);
        var maxX = tins.SelectMany(t => t.Vertices.Items).Max(v => v.X);
        var width = maxX - minX;

        var minY = tins.SelectMany(t => t.Vertices.Items).Min(v => v.Y);
        var maxY = tins.SelectMany(t => t.Vertices.Items).Max(v => v.Y);
        var height = maxY - minY;

        foreach (var tin in tins)
        {
          var faces = new List<Face>();
          var uvs = new List<Uv>();
          writer.WriteLine($"o {tin.ModelName.Replace(" ", "")}.{objIdx++}");

          foreach (var vertex in tin.Vertices.Items)
          {
            writer.WriteLine($"v {(float) (vertex.X - eastingOffset)} " +
                             $"{(float) (vertex.Y - northingOffset)} " +
                             $"{(float) (vertex.Z - zModifier)}");
          }

          writer.WriteLine("");

          foreach (var face in tin.Triangles.Items)
          {
            var f = new Face
            {
              VertexIdx0 = face.Vertex0 + vertexOffset,
              VertexIdx1 = face.Vertex1 + vertexOffset,
              VertexIdx2 = face.Vertex2 + vertexOffset
            };

            foreach (var vertexIdx in new List<int> {face.Vertex0, face.Vertex1, face.Vertex2})
            {
              var vertex = tin.Vertices.Items[vertexIdx];
              var u = (vertex.X - minX) / width;
              var v = (vertex.Y - minY) / height;
              var uv = new Uv()
              {
                U = u,
                V = v
              };
              uvs.Add(uv);
              if (f.UvIdx0 == 0)
                f.UvIdx0 = currentUvIndex++;
              else if (f.UvIdx1 == 0)
                f.UvIdx1 = currentUvIndex++;
              else if (f.UvIdx2 == 0)
                f.UvIdx2 = currentUvIndex++;

            }

            faces.Add(f);
          }

          foreach (var uv in uvs)
          {
            writer.WriteLine($"vt {uv.U} {uv.V}");
          }

          writer.WriteLine("");
          foreach (var face in faces)
          {
            writer.WriteLine($"f {face.VertexIdx0}/{face.UvIdx0} " +
                             $"{face.VertexIdx1}/{face.UvIdx1} " +
                             $"{face.VertexIdx2}/{face.UvIdx2}");
          }

          writer.WriteLine("");

          // Update the vertex index for the next object, as the vertex index is global for the file (not per object)
          vertexOffset += tin.Vertices.Items.Length;
          writer.Flush();
        }

        outputStream.Seek(0, SeekOrigin.Begin);
        Log.LogInformation($"GetExportReportSurface completed: ExportData size={outputStream.Length}");
        return new FileStreamResult(outputStream, "text/plain");
      }
    }

    private CompactionProjectSettingsColors GetGreyScaleHeightColors()
    {
      var colors = new List<uint>();
      for (var i = 0; i <= 255; i++)
      {
        colors.Add((uint)i << 16 | (uint)i << 8 | (uint)i << 0);
      }

      return CompactionProjectSettingsColors.Create(false, colors);
    }
  }
}

