#if NET_4_7 
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using Morph.Services.Core.Interfaces;
using Morph.Services.Core.Tools;
using VSS.Hydrology.WebApi.Abstractions.Models;
using VSS.Hydrology.WebApi.Abstractions.Models.ResultHandling;
using VSS.Hydrology.WebApi.Abstractions.ResultsHandling;
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
    private static readonly HydroErrorCodesProvider HydroErrorCodesProvider = new HydroErrorCodesProvider();
    private const string DEFAULT_TIMEOUTMS = "120000";
    private int? getExportTimeoutMs = null;

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

      // even though 3dp-raptor/trex call is internal, there is still a 60 second default timeout
      if (int.TryParse(ConfigStore.GetValueString("HYDRO_GETEXPORTSURFACE_TIMEOUTMS", DEFAULT_TIMEOUTMS), out var timeoutMs))
        getExportTimeoutMs = timeoutMs;
      var currentGroundTTMStream = await HydroRequestHelperCurrentGround.GetCurrentGround3Dp(request, Log, ServiceExceptionHandler, CustomHeaders, Productivity3dV2ProxyCompaction, getExportTimeoutMs);
      //var currentGroundTTMStream = HydroRequestHelperCurrentGround.GetCurrentGroundTest(Log); 

      var localTempProjectPath = FilePathHelper.GetTempFolderForProject(request.ProjectUid);

      // convert ttm to dxf
      var dxfLocalPathAndFileName = Path.Combine(new[] { localTempProjectPath, (Path.GetFileNameWithoutExtension(request.FileName) + ".dxf") });
      var triangleCount = ConvertTTMToDXF(currentGroundTTMStream, dxfLocalPathAndFileName);
      currentGroundTTMStream.Close();
      if (triangleCount < 3)
        return new ContractExecutionResult(5, HydroErrorCodesProvider.FirstNameWithOffset(5));

      // generate and zip images
      var zipLocalPath = Path.Combine(new[] { localTempProjectPath, (Path.GetFileNameWithoutExtension(request.FileName)) });
      if (Directory.Exists(zipLocalPath))
        Directory.Delete(zipLocalPath);
      Directory.CreateDirectory(zipLocalPath);
      GenerateHydroImages(dxfLocalPathAndFileName, zipLocalPath, request.Options);

      var finalZippedFile = HydroRequestHelper.ZipImages(localTempProjectPath, zipLocalPath, request.FileName, Log, ServiceExceptionHandler);

      // clean up temp files
      if (Directory.Exists(zipLocalPath))
        Directory.Delete(zipLocalPath, true);
      if (File.Exists(dxfLocalPathAndFileName))
        File.Delete(dxfLocalPathAndFileName);

      return new HydroResult(finalZippedFile);
    }

    private int ConvertTTMToDXF(Stream currentGroundTTMStream, string dxfLocalPathAndFileName)
    {
      Log.LogInformation($"{nameof(ConvertTTMToDXF)} dxfLocalPathAndFileName {dxfLocalPathAndFileName}");

      if (File.Exists(dxfLocalPathAndFileName))
        File.Delete(dxfLocalPathAndFileName);

      var converter = new TTMtoDXFConverter(Log);
      currentGroundTTMStream.Seek(0, 0);
      converter.CreateDXF(currentGroundTTMStream as MemoryStream, dxfLocalPathAndFileName);

      Log.LogInformation(
        $"{nameof(ConvertTTMToDXF)} triangleCount dxf: {converter.DXFTriangleCount()} ttm: {converter.TTMTriangleCount()}");
      if (converter.DXFTriangleCount() != converter.TTMTriangleCount())
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 6);

      return converter.DXFTriangleCount();
    }

    private void GenerateHydroImages(string dxfLocalPathAndFileName, string zipLocalPath, HydroOptions options)
    {
      ISurfaceInfo surfaceInfo = null;
      if (StringComparer.InvariantCultureIgnoreCase.Compare(Path.GetExtension(dxfLocalPathAndFileName), ".dxf") != 0)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 17, Path.GetExtension(dxfLocalPathAndFileName));

      try
      {
        surfaceInfo = LandLeveling.ImportSurface(dxfLocalPathAndFileName, null);
      }
      catch (Exception e)
      {
        var errorMessage = $"{nameof(GenerateHydroImages)} Surface import failed.";
        if (e.Source == "TD_SwigDbMgd" || e.Source == "TD_SwigCoreMgd")
          errorMessage += $" Failure is in reader {e.Source}. 1/more morph components may be missing from bin dir e.g. RecomputeDimBlock_4.00_11.tx file";

        Log.LogError(e, errorMessage);
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 9, errorMessage1: e.Message);
      }

      if (surfaceInfo == null)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 10);

      Log.LogInformation(
        $"{nameof(GenerateHydroImages)} Hydro SurfaceInfo: MinElevation {surfaceInfo.MinElevation} MaxElevation {surfaceInfo.MaxElevation} " +
        $"PointCount: {surfaceInfo.Points.Count} BoundaryPointCount: {surfaceInfo.Boundary.Count} " +
        $"TriangleCount: {surfaceInfo.Triangles.Count} " +
        $"FirstTriangle: {(surfaceInfo.Triangles.Count > 0 ? $"{surfaceInfo.Points[surfaceInfo.Triangles[0]]} - {surfaceInfo.Points[surfaceInfo.Triangles[1]]} - {surfaceInfo.Points[surfaceInfo.Triangles[2]]}" : "no triangles")}");

      GeneratePondingImage(surfaceInfo, zipLocalPath, options);
      GenerateDrainageViolationsImage(surfaceInfo, zipLocalPath, options);
    }

    private bool GeneratePondingImage(ISurfaceInfo surfaceInfo, string zipLocalPath, HydroOptions options)
    {
      Log.LogInformation($"{nameof(GeneratePondingImage)} resolution: {options.Resolution} levelCount: {options.Levels}");
      var imageFilename = Path.Combine(new[] { zipLocalPath, "Ponding.png" });

      BitmapSource bitmap = null;
      try
      {
        bitmap = surfaceInfo.GeneratePondMap(options.Resolution, options.Levels, null, null);
      }
      catch (Exception e)
      {
        var errorMessage = $"{nameof(GeneratePondingImage)} {HydroErrorCodesProvider.FirstNameWithOffset(11)}";
        Log.LogError(e, errorMessage);
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 11, errorMessage1: e.Message);
      }

      if (bitmap == null)
      {
        Log.LogError($"{nameof(GeneratePondingImage)} {HydroErrorCodesProvider.FirstNameWithOffset(12)}");
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 12);
      }

      SaveBitmap(bitmap, imageFilename);

      Log.LogInformation($"{nameof(GeneratePondingImage)} targetImageFile: {imageFilename}");
      return true;
    }

    private bool GenerateDrainageViolationsImage(ISurfaceInfo surfaceInfo, string zipLocalPath, HydroOptions options)
    {
      Log.LogInformation($"{nameof(GenerateDrainageViolationsImage)} resolution: {options.Resolution} levelCount: {options.Levels}");
      var imageFilename = Path.Combine(new[] { zipLocalPath, "DrainageViolations.png" });

      BitmapSource bitmap = null;
      try
      {
        bitmap = surfaceInfo.GenerateDrainageViolationsMap(options.Resolution,
          options.MinSlope, options.MaxSlope,
          //options.Boundary, options.InclusionZones, options.ExclusionZones,
          null, null, null,
          options.VortexViolationColor, options.MaxSlopeViolationColor,
          options.NoViolationColorDark, options.NoViolationColorMid, options.NoViolationColorLight,
          options.MinSlopeViolationColor);
      }
      catch (Exception e)
      {
        var errorMessage = $"{nameof(GeneratePondingImage)} {HydroErrorCodesProvider.FirstNameWithOffset(14)}";
        Log.LogError(e, errorMessage);
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 14, errorMessage1: e.Message);
      }

      if (bitmap == null)
      {
        Log.LogError($"{nameof(GeneratePondingImage)} {HydroErrorCodesProvider.FirstNameWithOffset(15)}");
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 15);
      }

      SaveBitmap(bitmap, imageFilename);

      Log.LogInformation($"{nameof(GenerateDrainageViolationsImage)} targetImageFile: {imageFilename}");
      return true;
    }

    private void SaveBitmap(BitmapSource bitmap, string targetFilenameAndPath)
    {
      if (File.Exists(targetFilenameAndPath))
        File.Delete(targetFilenameAndPath);

      var pngBitmapEncoder = new PngBitmapEncoder();
      pngBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmap));
      using (var stream = (Stream)File.Create(targetFilenameAndPath))
        pngBitmapEncoder.Save(stream);

      if (!File.Exists(targetFilenameAndPath))
      {
        Log.LogError($"{nameof(SaveBitmap)}  {HydroErrorCodesProvider.FirstNameWithOffset(13)} targetFileName: {targetFilenameAndPath} ");
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 13);
      }
      Log.LogInformation($"{nameof(SaveBitmap)} saved image: targetFileName: {targetFilenameAndPath}");
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }

  }
}
#endif
