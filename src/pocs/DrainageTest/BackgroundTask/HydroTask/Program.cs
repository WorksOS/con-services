using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.ServiceLocation;
using Morph.Services.Core.Interfaces;
using SkuTester.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Morph.Services.Core.Tools;
using Trimble.Geodetic.Math.Adjustment;
using Trimble.Vce.Data.Skp;

namespace HydroTask
{
  public class Program
  {
    static void Main(string[] args)
    {
      if (args == null || args.Length < 1)
        throw new ArgumentException("Missing command line argument");

      // ..\..\TestData\Sample\TestCase.xml
      var configFilePath = Path.GetFullPath(ParseCommandLineArguments(args)["input"]);
      if (!File.Exists(configFilePath))
        throw new ArgumentException($"Unable to locate the xml configuration file: {configFilePath}");

      var logger = (ILogger) null;
      try
      {
        var thread = new Thread(Program.CreateWpfApp);
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Thread.Sleep(100);
        new BootStrapper().Run();

        logger = ServiceLocator.Current.GetInstance<ILogger>();
        if (logger == null)
          throw new InternalErrorException("Unable to retrieve ILogger");
      
        Execute(configFilePath, logger);
      }
      catch (Exception ex)
      {
        var errorMessage = $"{nameof(Main)} Exception: {ex}";
        if (logger == null)
          Console.WriteLine(errorMessage);
        else
          logger.LogError(errorMessage, Category.Exception.ToString());
      }
      finally
      {
        Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
      }
    }

    private static void Execute(string configFilePath, ILogger logger)
    {
      var useCase = TestCase.Load(configFilePath);
      if (useCase == null)
        throw new ArgumentException("Unable to load surface configuration");
      logger.LogInfo(nameof(Execute), $"XML loaded: designFile {useCase.Surface} units: {(useCase.IsMetric ? "metres" : "us ft?")} points {(useCase.IsXYZ ? "xyz" : "nee")})");

      using (var landLevelingInstance = ServiceLocator.Current.GetInstance<ILandLeveling>())
      {
        var surfaceInfo = (ISurfaceInfo) null;
        if (StringComparer.InvariantCultureIgnoreCase.Compare(Path.GetExtension(useCase.Surface), ".dxf") == 0)
          surfaceInfo = landLevelingInstance.ImportSurface(useCase.Surface, (Action<float>) null);
        else
          throw new ArgumentException("Only DXF surface file type supported at present");

        if (surfaceInfo == null)
          throw new ArgumentException($"Unable to create Surface from: {useCase.Surface}");

        var targetPath =
          Path.ChangeExtension(
            Path.Combine(Path.GetDirectoryName(Path.GetFullPath(configFilePath)),
              Path.GetFileNameWithoutExtension(configFilePath)), "skp");
        
        using (var skuModel = new SkuModel(useCase.IsMetric))
        {
          skuModel.Name = Path.GetFileNameWithoutExtension(targetPath);
          // generate original ponding surface bitmap according to Original configuration
          //     and writes TestCase-original-pondmap.png file
          if (useCase.OriginalVisualizationTools != null)
          {
            foreach (var visualizationTool in useCase.OriginalVisualizationTools)
            {
              if (visualizationTool is PondMap)
              {
                var texture = visualizationTool.GenerateAndSaveTexture(surfaceInfo, targetPath, "original");
                var originalLayerName = "originalSurface " + visualizationTool.GetType();
                skuModel.AddSurfaceWithHorizontalTexture(surfaceInfo.Points, surfaceInfo.Triangles, originalLayerName,
                  texture, 0.75, originalLayerName, null);
              }
            }
          }

          logger.LogInfo(nameof(Execute), $"targetPath: {targetPath}");
        }

        /* todo could exclude sketchup stuff
       var Levels = 10;
       BitmapSource pondMap = surfaceInfo.GeneratePondMap(useCase.Resolution, Levels, (IEnumerable<IEnumerable<Point>>) null,
             (IEnumerable<IEnumerable<Point>>) null);
       string filename = Path.Combine(Path.GetDirectoryName(targetPath),
                       string.Format("{0}-{1}-pondmap.png", (object)Path.GetFileNameWithoutExtension(targetPath), "original"));
       SaveBitmap(pondMap, filename);
       logger.LogInfo(nameof(Execute), $"targetPath: {targetPath}");
       */

      }
    }

    private static void CreateWpfApp()
    {
      new Application() { ShutdownMode = ShutdownMode.OnExplicitShutdown }.Run();
    }

    private static IDictionary<string, string> ParseCommandLineArguments(string[] args)
    {
      var sortedList = new SortedList<string, string>(args.Length);
      foreach (var arg in args)
      {
        if (arg.StartsWith("/") || arg.StartsWith("-"))
        {
          string str1 = arg.Substring(1);
          if (str1.Length > 1)
          {
            string index2 = str1;
            string str2 = string.Empty;
            int length = str1.IndexOf(':');
            if (length > 0)
            {
              index2 = str1.Substring(0, length);
              str2 = str1.Substring(length + 1);
            }

            sortedList[index2] = str2;
          }
        }
        else
          sortedList["input"] = arg;
      }

      return sortedList;
    }
    
  }
}
