#if NET_4_7 
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Morph.Services.Core.Interfaces;
using Morph.Services.Core.Tools;
using VSS.Common.Abstractions.Http;
using VSS.Hydrology.WebApi.Abstractions.Models;
using VSS.Hydrology.WebApi.Abstractions.Models.ResultHandling;
using VSS.Hydrology.WebApi.Common.Utilities;
using VSS.MasterData.Models.ResultHandling.Abstractions;


namespace VSS.Hydrology.WebApi.Common.Executors
{
  /// <summary>
  /// Executor to generate a ponding map from a design obtained from 3dp
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

      //
      // get latestSurface via 3dp

      //var currentGroundResult = GetCurrentGround3dp(request); 
      var currentGroundResult = GetCurrentGroundTest(request); // todoJeannie


      //
      // convert ttm to dxf mesh

      var localTempProjectPath = FilePathHelper.GetTempFolderForProject(request.ProjectUid);
      var dxfLocalPathAndFileName = Path.Combine(new[]
        {localTempProjectPath, (Path.GetFileNameWithoutExtension(request.FileName) + ".dxf")});
      using (var ms = new MemoryStream())
      {
        currentGroundResult.FileStream.CopyTo(ms);
        ms.Seek(0, 0);
        currentGroundResult.FileStream.Close();
        var triangleCount = ConvertTTMToDXF(ms, dxfLocalPathAndFileName);
      }


      // 
      // generate ponding image from dxf mesh

      var pngLocalPathAndFileName = Path.ChangeExtension(dxfLocalPathAndFileName, "png");
      GeneratePondingImageFile(dxfLocalPathAndFileName, pngLocalPathAndFileName, request.Resolution);

      return new PondingResult(pngLocalPathAndFileName);
    }

    private FileStreamResult GetCurrentGround3dp(PondingRequest request)
    {
      // <param name="tolerance">Controls triangulation density in the output .TTM file.</param>
      // "RAPTOR_3DPM_API_URL": "https://api-stg.trimble.com/t/trimble.com/vss-dev-3dproductivity/2.0",
      // "RAPTOR_3DPM_API_URL": "http://localhost:5001/api/v2", note there is not mockRaptorController  [Route("api/v2/export/surface")] 

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
      return null;
    }

    private FileStreamResult GetCurrentGroundTest(PondingRequest request)
    {
      // var ttmLocalPathAndFileName = "..\\..\\test\\UnitTests\\TestData\\DesignSurfaceGoodContent.ttm"; // hydro throws exception with 2 triangles.
      // var ttmLocalPathAndFileName = "..\\..\\test\\UnitTests\\TestData\\AlphaDimensions2012_milling_surface5.ttm";
      // var ttmLocalPathAndFileName = "..\\..\\test\\UnitTests\\TestData\\R2_Zone C3 Wrights East Tri_TX04_20Oct2010.ttm";
      // var ttmLocalPathAndFileName = "..\\..\\test\\UnitTests\\TestData\\TestDesignSurface1.ttm"; // no triangles error opening with TTMviewer also
      // var ttmLocalPathAndFileName = "..\\..\\test\\UnitTests\\TestData\\DesignSurfaceGoodContent.ttm"; // hydro throws exception with 2 triangles.
      var ttmLocalPathAndFileName = "..\\..\\test\\UnitTests\\TestData\\Large Sites Road - Trimble Road.ttm";
      Log.LogDebug($"{Environment.CurrentDirectory}");
      if (!File.Exists(ttmLocalPathAndFileName))
        throw new InvalidOperationException("todoJeannie unable to find temp ttm");

      var fileStream = new FileStream(ttmLocalPathAndFileName, FileMode.Open);
      Log.LogInformation($"{nameof(GetCurrentGroundTest)} completed: ExportData size={fileStream.Length}");
      return new FileStreamResult(fileStream, ContentTypeConstants.ApplicationZip);
    }

    private int ConvertTTMToDXF(MemoryStream ms, string dxfLocalPathAndFileName)
    {
      if (File.Exists(dxfLocalPathAndFileName))
        File.Delete(dxfLocalPathAndFileName);

      var converter = new TTMtoDXFConverter(base.Log);
      converter.WriteDXFFromTTMStream(ms, dxfLocalPathAndFileName);

      Log.LogInformation($"{nameof(ConvertTTMToDXF)} dxfLocalPathAndFileName {dxfLocalPathAndFileName} " +
                         $"triangleCount dxf: {converter.DXFTriangleCount()} ttm: {converter.TTMTriangleCount()}");

      if (converter.DXFTriangleCount() != converter.TTMTriangleCount())
        throw new ArgumentException(
          $"{nameof(ConvertTTMToDXF)} TTM conversion failed. triangleCount dxf: {converter.DXFTriangleCount()} ttm: {converter.TTMTriangleCount()}");

      // can you have ponding where < 3 triangles? hydro libraries don't seem to process 2 triangles anyways
      if (converter.DXFTriangleCount() < 3) // todo serviceException
        throw new ArgumentException(
          $"{nameof(ConvertTTMToDXF)} Unable to determine ponding on <3 triangles. triangleCount dxf: {converter.DXFTriangleCount()}");

      return converter.DXFTriangleCount();
    }


    private void GeneratePondingImageFile(string dxfLocalPathAndFileName, string pngLocalPathAndFileName, double resolution)
    {
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
          $"{nameof(ProcessAsyncEx)} Hydro SurfaceInfo: MinElevation {surfaceInfo.MinElevation} MaxElevation {surfaceInfo.MaxElevation} " +
          $"PointCount: {surfaceInfo.Points.Count} BoundaryPointCount: {surfaceInfo.Boundary.Count} " +
          $"TriangleCount: {surfaceInfo.Triangles.Count} " +
          $"FirstTriangle: {(surfaceInfo.Triangles.Count > 0 ? $"{surfaceInfo.Points[surfaceInfo.Triangles[0]]} - {surfaceInfo.Points[surfaceInfo.Triangles[1]]} - {surfaceInfo.Points[surfaceInfo.Triangles[2]]}" : "no triangles")}");

        GeneratePondingImageFile(surfaceInfo, dxfLocalPathAndFileName, pngLocalPathAndFileName, resolution);
      }
      catch (Exception e)
      {
        Log.LogError(e, $"{nameof(ProcessAsyncEx)} Surface import failed");
        throw e;
      }
    }


    private bool GeneratePondingImageFile(ISurfaceInfo surfaceInfo, string dxfLocalPathAndFileName,
      string pngLocalPathAndFileName,
      double resolution, int levelCount = 10)
    {
      Log.LogInformation($"{nameof(GeneratePondingImageFile)} Generating without sketchup");

      // can throw "E_INVALIDARG: An invalid parameter was passed to the returning function (-2147024809)"
      // if resolution 'too' small? (tried 0.01 on Large Sites Road - Trimble Road.dxf)
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
      if (File.Exists(targetFilenameAndPath))
        File.Delete(targetFilenameAndPath);

      var pngBitmapEncoder = new PngBitmapEncoder();
      pngBitmapEncoder.Frames.Add(BitmapFrame.Create(pondMap));
      using (var stream = (Stream) File.Create(targetFilenameAndPath))
        pngBitmapEncoder.Save(stream);
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

  
private int ConvertTTMToDXF(string ttmLocalPathAndFileName, string dxfLocalPathAndFileName)
{
  var triangleCount = 0;

  using (var ms = new MemoryStream(File.ReadAllBytes(ttmLocalPathAndFileName)))
  {
    triangleCount = ConvertTTMToDXF(ms, dxfLocalPathAndFileName);
  }

  return triangleCount;
}

*/
