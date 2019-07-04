#if NET_4_7 
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Morph.Services.Core.Interfaces;
using Morph.Services.Core.Tools;
using VSS.Common.Abstractions.Http;
using VSS.Common.Exceptions;
using VSS.Hydrology.WebApi.Abstractions.Models;
using VSS.Hydrology.WebApi.Abstractions.Models.ResultHandling;
using VSS.Hydrology.WebApi.Common.Helpers;
using VSS.Hydrology.WebApi.Common.Utilities;
using VSS.MasterData.Models.ResultHandling.Abstractions;


namespace VSS.Hydrology.WebApi.Common.Executors
{
  /// <summary>
  /// Executor to generate hydro images from a design obtained from 3dp
  /// </summary>
  public class HydroExecutor : RequestExecutorContainer
  {
    public HydroExecutor()
    {
      ProcessErrorCodes();
    }

    protected sealed override void ProcessErrorCodes()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<HydroRequest>(item);

      //var currentGroundResult = GetCurrentGround3dp(request); 
      var currentGroundResult = GetCurrentGroundTest(request); // todoJeannie
     
      var localTempProjectPath = FilePathHelper.GetTempFolderForProject(request.ProjectUid);
      var dxfLocalPathAndFileName = Path.Combine(new[]
        {localTempProjectPath, (Path.GetFileNameWithoutExtension(request.FileName) + ".dxf")});
      using (var ms = new MemoryStream())
      {
        currentGroundResult.FileStream.CopyTo(ms);
        ms.Seek(0, 0);
        currentGroundResult.FileStream.Close();
        ConvertTTMToDXF(ms, dxfLocalPathAndFileName);
      }


      // 
      // generate and zip images
      var zipLocalPath = Path.Combine(new[]
        {localTempProjectPath, (Path.GetFileNameWithoutExtension(request.FileName))});
      if (!Directory.Exists(zipLocalPath))
        Directory.CreateDirectory(zipLocalPath);
      GenerateHydroImages(dxfLocalPathAndFileName, zipLocalPath, request.Options);

      var finalZippedFile =
        HydroRequestHelper.ZipImages(localTempProjectPath, zipLocalPath, request.FileName, Log, ServiceExceptionHandler);

      if (Directory.Exists(zipLocalPath) )
        Directory.Delete(zipLocalPath, true);

      if (File.Exists(dxfLocalPathAndFileName))
        File.Delete(dxfLocalPathAndFileName);
      return new HydroResult(finalZippedFile);
    }

    private FileStreamResult GetCurrentGround3dp(HydroRequest request)
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

    private FileStreamResult GetCurrentGroundTest(HydroRequest request)
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
      converter.CreateDXF(ms, dxfLocalPathAndFileName);

      Log.LogInformation($"{nameof(ConvertTTMToDXF)} dxfLocalPathAndFileName {dxfLocalPathAndFileName} " +
                         $"triangleCount dxf: {converter.DXFTriangleCount()} ttm: {converter.TTMTriangleCount()}");

      if (converter.DXFTriangleCount() != converter.TTMTriangleCount())
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(2006));

      return converter.DXFTriangleCount();
    }


    private void GenerateHydroImages(string dxfLocalPathAndFileName, string zipLocalPath, HydroOptions options)
    {
      ISurfaceInfo surfaceInfo;
      if (StringComparer.InvariantCultureIgnoreCase.Compare(Path.GetExtension(dxfLocalPathAndFileName), ".dxf") != 0)
        throw new ArgumentException(
          $"{nameof(GenerateHydroImages)} Only DXF original ground file type supported at present");

      try
      {
        surfaceInfo = LandLeveling.ImportSurface(dxfLocalPathAndFileName, null);
      }
      catch (Exception e)
      {
        Log.LogError(e, $"{nameof(GenerateHydroImages)} Surface import failed.");

        if (e.Source == "TD_SwigDbMgd")
          Log.LogError($"{nameof(GenerateHydroImages)} Failure is in reader 'TD_SwigDbMgd'.May be missing RecomputeDimBlock_4.00_11.tx in bin directory");
        throw e;
      }

      if (surfaceInfo == null)
        throw new ArgumentException(
          $"{nameof(GenerateHydroImages)} Unable to create Surface from: {dxfLocalPathAndFileName}");

      Log.LogInformation(
        $"{nameof(GenerateHydroImages)} Hydro SurfaceInfo: MinElevation {surfaceInfo.MinElevation} MaxElevation {surfaceInfo.MaxElevation} " +
        $"PointCount: {surfaceInfo.Points.Count} BoundaryPointCount: {surfaceInfo.Boundary.Count} " +
        $"TriangleCount: {surfaceInfo.Triangles.Count} " +
        $"FirstTriangle: {(surfaceInfo.Triangles.Count > 0 ? $"{surfaceInfo.Points[surfaceInfo.Triangles[0]]} - {surfaceInfo.Points[surfaceInfo.Triangles[1]]} - {surfaceInfo.Points[surfaceInfo.Triangles[2]]}" : "no triangles")}");

      try
      {
        GeneratePondingImageFile(surfaceInfo, zipLocalPath, options.Resolution);
      }
      catch (Exception e)
      {
        Log.LogError(e, $"{nameof(GenerateHydroImages)} Unable to generate a ponding image");
        throw e;
      }
    }


    private bool GeneratePondingImageFile(ISurfaceInfo surfaceInfo, string zipLocalPath,
      double resolution, int levelCount = 10)
    {
      Log.LogInformation($"{nameof(GeneratePondingImageFile)} Generating without sketchup");
      var pngLocalPathAndFileName = Path.Combine(new[] { zipLocalPath, "Ponding.png" });

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
