#if NET_4_7 
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using Morph.Services.Core.Interfaces;
using Morph.Services.Core.Tools;
using Newtonsoft.Json;
using SkuTester.DataModel;
using VSS.Hydrology.WebApi.Abstractions.Models;
using VSS.Hydrology.WebApi.Abstractions.Models.ResultHandling;
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

      // todoJeannie
      // 1) get boundary from Filter (designBoundary OR Geofence OR projectBoundary)
      // 2) get latestSurface from TRex using that boundary
      // 3) convert ttm to dxf mesh
      // 0) get weather for (how long?) for (this location?)

      // temporarily use this sample originalGround mesh
      var originalGroundPathAndFilename = "..\\..\\TestData\\Sample\\triangle.dxf";
      
      if (!File.Exists(originalGroundPathAndFilename))
        throw new FileNotFoundException($"{nameof(ProcessAsyncEx)} Original ground file not found {originalGroundPathAndFilename}");

       Log.LogInformation(
        $"{nameof(ProcessAsyncEx)} surface configuration loaded: designFile {originalGroundPathAndFilename} units: {(request.IsMetric ? "meters" : "us ft?")})");

      try
      {
        ISurfaceInfo surfaceInfo;

        if (StringComparer.InvariantCultureIgnoreCase.Compare(Path.GetExtension(originalGroundPathAndFilename), ".dxf") == 0)
          surfaceInfo = LandLeveling.ImportSurface(originalGroundPathAndFilename, null);
        else
          throw new ArgumentException($"{nameof(ProcessAsyncEx)} Only DXF original ground file type supported at present");

        if (surfaceInfo == null)
          throw new ArgumentException($"{nameof(ProcessAsyncEx)} Unable to create Surface from: {originalGroundPathAndFilename}");

        Log.LogInformation(
          $"{nameof(ProcessAsyncEx)} SurfaceInfo: MinElevation {surfaceInfo.MinElevation} MaxElevation {surfaceInfo.MaxElevation} " +
          $"PointCount: {surfaceInfo.Points.Count} BoundaryPointCount: {surfaceInfo.Boundary.Count} " +
          $"TriangleCount: {surfaceInfo.Triangles.Count} " +
          $"FirstTriangle: {(surfaceInfo.Triangles.Count > 0 ? $"{surfaceInfo.Points[surfaceInfo.Triangles[0]]} - {surfaceInfo.Points[surfaceInfo.Triangles[1]]} - {surfaceInfo.Points[surfaceInfo.Triangles[2]]}" : "no triangles")}");

        GenerateWithoutSketchup(surfaceInfo, originalGroundPathAndFilename, request.Resolution);
      }
      catch (Exception e)
      {
        Log.LogError(e, $"{nameof(ProcessAsyncEx)}Surface import failed");
        throw e;
      }
      
      return new PondingResult(String.Empty);
    }

    private bool GenerateWithoutSketchup(ISurfaceInfo surfaceInfo, string originalGroundPathAndFilename,
      double resolution, int levelCount = 10 )
    {
      Log.LogInformation($"{nameof(GenerateWithoutSketchup)} Generating without sketchup");

      string targetPondingFilenameAndPath = Path.Combine(Path.GetDirectoryName(originalGroundPathAndFilename),
        string.Format($"{Path.GetFileNameWithoutExtension(originalGroundPathAndFilename)}-{"originalGround"}-pondmap.png"));

      var pondMap = surfaceInfo.GeneratePondMap(resolution, levelCount, null, null);
      if (pondMap == null)
        throw new ArgumentException(
          $"{nameof(GenerateWithoutSketchup)} Unable to create pond map: resolution: {resolution} levelCount: {levelCount}");

      SaveBitmap(pondMap, targetPondingFilenameAndPath);

      if (!File.Exists(targetPondingFilenameAndPath))
        throw new FileNotFoundException(
          $"{nameof(GenerateWithoutSketchup)} Ponding map not found {targetPondingFilenameAndPath}");

      Log.LogInformation($"{nameof(GenerateWithoutSketchup)} targetPondingFile: {targetPondingFilenameAndPath}");
      return true;
    }

    private void SaveBitmap(BitmapSource pondMap, string targetFilenameAndPath)
    {
      var pngBitmapEncoder = new PngBitmapEncoder();
      pngBitmapEncoder.Frames.Add(BitmapFrame.Create(pondMap));
      using (var stream = (Stream) File.Create(targetFilenameAndPath))
        pngBitmapEncoder.Save(stream);
    }

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

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }

  }
}
#endif
