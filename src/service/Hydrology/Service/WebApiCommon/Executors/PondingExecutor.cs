#if NET_4_7 
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Morph.Services.Core.Interfaces;
using Morph.Services.Core.Tools;
using VSS.Common.Exceptions;
using VSS.Hydrology.WebApi.Abstractions.Models;
using VSS.Hydrology.WebApi.Abstractions.Models.ResultHandling;
using VSS.Hydrology.WebApi.Common.Utilities;
using VSS.Hydrology.WebApi.DXF;
using VSS.MasterData.Models.ResultHandling.Abstractions;


namespace VSS.Hydrology.WebApi.Common.Executors
{
  /// <summary>
  /// Executor for ...
  /// </summary>
  public class PondingExecutor : RequestExecutorContainer
  {
    public PondingExecutor()
    {
      ProcessErrorCodes();
    }

    protected sealed override void ProcessErrorCodes()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<PondingRequest>(item);

      // todo get weather for (how long?) for (this location?)
      // todo handle filter (design or geofence or boundary etc)

      //
      // get latestSurface from TRex - currently using entire project

      // <param name="tolerance">Controls triangulation density in the output .TTM file.</param>
      // "RAPTOR_3DPM_API_URL": "https://api-stg.trimble.com/t/trimble.com/vss-dev-3dproductivity/2.0",
      // "RAPTOR_3DPM_API_URL": "http://localhost:5001/api/v2", note there is not mockRaptorController  [Route("api/v2/export/surface")] 
      var targetPondingFileNameNoExtn = Path.GetFileNameWithoutExtension(request.FileName);
      //var route =
      //  $"/export/surface?projectUid={request.ProjectUid}&fileName={ttmFileName}&filterUid={request.FilterUid}";
      //var fileResult =
      //  await RaptorProxy.ExecuteGenericV2Request<FileResult>(route, HttpMethod.Get, null, CustomHeaders) as
      //    FileStreamResult;
      //if (fileResult == null)
      //{
      //  throw new ServiceException(HttpStatusCode.InternalServerError,
      //    new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
      //      $"No latest Ground returned from 3dp"));
      //}

      //
      // convert ttm to dxf mesh

      var localTempProjectPath = FilePathHelper.GetTempFolderForProject(request.ProjectUid);

      //var dxfLocalPathAndFileName = Path.Combine(new[] { localTempProjectPath, (targetPondingFileNameNoExtn + ".dxf") });
      //ConvertTTMToDXF(fileResult.FileStream as MemoryStream, dxfLocalPathAndFileName);

      // todoJeannie temporarily use this sample mesh
      // var ttmLocalPathAndFileName = "..\\..\\test\\UnitTests\\TestData\\DesignSurfaceGoodContent.ttm"; // hydro throws exception with 2 triangles.
      var ttmLocalPathAndFileName = "..\\..\\test\\UnitTests\\TestData\\AlphaDimensions2012_milling_surface5.ttm";
      Log.LogDebug($"{Environment.CurrentDirectory}");
      var tt = Environment.CurrentDirectory;
      if (!File.Exists(ttmLocalPathAndFileName))
        throw new InvalidOperationException("todoJeannie unable to find temp ttm");

      var dxfLocalPathAndFileName = Path.Combine(new[] { localTempProjectPath, (targetPondingFileNameNoExtn + ".dxf") });
      ConvertTTMToDXF(ttmLocalPathAndFileName, dxfLocalPathAndFileName);


      // 
      // generate ponding image from dxf mesh

      // todoJeannie temporarily use this sample mesh
      // dxfLocalPathAndFileName = "..\\..\\test\\UnitTests\\TestData\\Sample\\Triangle.dxf";
      var pngLocalPathAndFileName = Path.ChangeExtension(dxfLocalPathAndFileName, "png");
      try
      {
        ISurfaceInfo surfaceInfo;

        if (StringComparer.InvariantCultureIgnoreCase.Compare(Path.GetExtension(dxfLocalPathAndFileName), ".dxf") == 0)
          surfaceInfo = LandLeveling.ImportSurface(dxfLocalPathAndFileName, null);
        else
          throw new ArgumentException(
            $"{nameof(ProcessAsyncEx)} Only DXF original ground file type supported at present");

        if (surfaceInfo == null)
          throw new ArgumentException(
            $"{nameof(ProcessAsyncEx)} Unable to create Surface from: {dxfLocalPathAndFileName}");

        Log.LogInformation(
          $"{nameof(ProcessAsyncEx)} SurfaceInfo: MinElevation {surfaceInfo.MinElevation} MaxElevation {surfaceInfo.MaxElevation} " +
          $"PointCount: {surfaceInfo.Points.Count} BoundaryPointCount: {surfaceInfo.Boundary.Count} " +
          $"TriangleCount: {surfaceInfo.Triangles.Count} " +
          $"FirstTriangle: {(surfaceInfo.Triangles.Count > 0 ? $"{surfaceInfo.Points[surfaceInfo.Triangles[0]]} - {surfaceInfo.Points[surfaceInfo.Triangles[1]]} - {surfaceInfo.Points[surfaceInfo.Triangles[2]]}" : "no triangles")}");

        GeneratePondingImageFile(surfaceInfo, dxfLocalPathAndFileName, pngLocalPathAndFileName, request.Resolution);
      }
      catch (Exception e)
      {
        Log.LogError(e, $"{nameof(ProcessAsyncEx)} Surface import failed");
        throw e;
      }

      return new PondingResult(pngLocalPathAndFileName);
    }

    private bool GeneratePondingImageFile(ISurfaceInfo surfaceInfo, string dxfLocalPathAndFileName,
      string pngLocalPathAndFileName,
      double resolution, int levelCount = 10)
    {
      Log.LogInformation($"{nameof(GeneratePondingImageFile)} Generating without sketchup");

      var pondMap = surfaceInfo.GeneratePondMap(resolution, levelCount, null, null);
      if (pondMap == null)
        throw new ArgumentException(
          $"{nameof(GeneratePondingImageFile)} Unable to create pond map: resolution: {resolution} levelCount: {levelCount}");

      SaveBitmap(pondMap, pngLocalPathAndFileName);

      if (!File.Exists(pngLocalPathAndFileName))
        throw new FileNotFoundException(
          $"{nameof(GeneratePondingImageFile)} Ponding map not found {pngLocalPathAndFileName}");

      Log.LogInformation($"{nameof(GeneratePondingImageFile)} targetPondingFile: {pngLocalPathAndFileName}");
      return true;
    }

    private void SaveBitmap(BitmapSource pondMap, string targetFilenameAndPath)
    {
      var pngBitmapEncoder = new PngBitmapEncoder();
      pngBitmapEncoder.Frames.Add(BitmapFrame.Create(pondMap));
      using (var stream = (Stream) File.Create(targetFilenameAndPath))
        pngBitmapEncoder.Save(stream);
    }

    // convert ttm to dxf mesh
    private int ConvertTTMToDXF(string ttmLocalPathAndFileName, string dxfLocalPathAndFileName)
    {
      var triangleCount = 0;

      using (var ms = new MemoryStream(File.ReadAllBytes(ttmLocalPathAndFileName)))
      {
        triangleCount = ConvertTTMToDXF(ms, dxfLocalPathAndFileName);
      }
      return triangleCount;
    }

    private int ConvertTTMToDXF(MemoryStream ms, string dxfLocalPathAndFileName)
    {
      var converter = new TTMtoDXFConverter(base.Log);
      converter.WriteDXFFromTTMStream(ms, dxfLocalPathAndFileName);
      
      return converter.DXFTriangleCount();
    }


    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }

  }
}
#endif


/*
private bool GenerateWithSketchup(ISurfaceInfo surfaceInfo, TestCase useCase, string sourcePathAndFilename, int levelCount = 10, double resolution = 5)
{
  Log.LogInformation($"{nameof(GenerateWithSketchup)} Generating with sketchup");
  var targetSketchupFilenameAndPath =
    Path.ChangeExtension(
      Path.Combine(Path.GetDirectoryName(Path.GetFullPath(sourcePathAndFilename)),
        Path.GetFileNameWithoutExtension(sourcePathAndFilename)), "skp");

  using (var skuModel = new SkuModel(useCase.IsMetric))
  {
    skuModel.Name = Path.GetFileNameWithoutExtension(targetSketchupFilenameAndPath);
    var pondMapVizTool = new PondMap { Levels = levelCount, Resolution = resolution };
    // this writes the png file
    pondMapVizTool.GenerateAndSaveTexture(surfaceInfo, targetSketchupFilenameAndPath, "original");
  }

  var targetPondingFilenameAndPath =
    Path.ChangeExtension(
      Path.Combine(Path.GetDirectoryName(Path.GetFullPath(targetSketchupFilenameAndPath)),
                   $"{Path.GetFileNameWithoutExtension(targetSketchupFilenameAndPath)}-original-pondmap"
                   ),
"png");

  if (!File.Exists(targetPondingFilenameAndPath))
    throw new FileNotFoundException($"{nameof(GenerateWithSketchup)} Ponding map not found {targetPondingFilenameAndPath}");

  Log.LogInformation($"{nameof(GenerateWithSketchup)} targetPondingFile: {targetPondingFilenameAndPath}");
  return true;
}
*/
