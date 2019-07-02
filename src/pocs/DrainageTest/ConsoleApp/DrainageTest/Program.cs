using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.ServiceLocation;
using Morph.Services.Core.Interfaces;
using SkuTester.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using Morph.Services.Core.DataModel;
using Trimble.Geodetic.Math.Adjustment;

namespace DrainageTest
{
  public class Program
  {
    static void Main(string[] args)
    {
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
          throw new InternalErrorException("Unable to retrieve ILogger.");

        //  try debug arg
        //  samples are all dxf best fit:   ..\..\TestData\Sample\TestCase.xml
        //                                    should succeed and display mesh
        //                                  ..\..\TestData\Trench\TestCase.xml
        //                                    throws exception SolverDrainageGridGradientDescent
        //                                  ..\..\TestData\Lillydale\TestCase.xml 
        //                                    should succeed, however sketchup warns that it's fixing minor errors
        //                                  ..\..\TestData\BuildingPad\TestCase.xml 
        //                                    should succeed, however sketchup warns that it's fixing minor errors
        //                                  ..\..\TestData\JeannieTestSquare\TestCase.xml
        //                                    simple horz square with 1 hill and 1 pond
        //                                    throws exception "error in the application"????
        //                                   ..\..\TestData\2018_3dFace\TestCase.xml
        // Grant_DroneSurveyWithPonds
        // ..\..\TestData\AlphaDimensions2012_milling_surface5\AlphaDimensions2012_milling_surface5.xml
        // ..\..\TestData\LargeSitesRoad_TrimbleRoad_1_0\LargeSitesRoad_TrimbleRoad_1_0.xml
        args[0] = "..\\..\\TestData\\LargeSitesRoad_TrimbleRoad_Change\\LargeSitesRoad_TrimbleRoad_Change.xml";
        var stringBuilder = new StringBuilder();
        foreach (string str in args)
          stringBuilder.AppendFormat("{0} ", (object) str);
        logger.LogInfo(nameof(Main), "start: {0}", (object) stringBuilder.ToString());
        Execute(args, logger);
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

    private static void Execute(string[] args, ILogger logger)
    {
      var commandLineArgument = Program.ParseCommandLineArguments(args)["input"];
      logger.LogInfo(nameof(Execute), $"args: {commandLineArgument}");
      if (!File.Exists(commandLineArgument))
        throw new ArgumentException($"Unable to locate the xml file: {commandLineArgument}");

      var xmlPath = Path.GetFullPath(commandLineArgument);
      var useCase = TestCase.Load(xmlPath);
      if (useCase == null)
        throw new ArgumentException("Unable to load surface configuration");
      logger.LogInfo(nameof(Execute), $"XML loaded: designFile {useCase.Surface} units: {(useCase.IsMetric ? "metres" : "us ft?")} points {(useCase.IsXYZ ? "xyz" : "nee")})");

      var surface = new Surface(useCase);
      using (var landLevelingInstance = ServiceLocator.Current.GetInstance<ILandLeveling>())
      {
        var surfaceInfo = surface.GenerateSurfaceInfo(landLevelingInstance);
        if (surfaceInfo == null)
          throw new ArgumentException($"Unable to create Surface from: {useCase.Surface}");

        Design design = null;
        //design = surface.ComputeSurfaces(landLevelingInstance);
        //if (design == null)
        //  throw new ArgumentException($"Unable to create design from Surface: {useCase.Surface}");

        var sketchupFile = new SketchupFile(useCase, surfaceInfo, design);

        // may be able to get the info we need from surface._design?
        // this may only be needed for display in sketchup
        var targetPath = sketchupFile.CreateModel(xmlPath);
        Morph.Services.Core.DataModel.Utils.LaunchSketchup(targetPath);
      }
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

    private static void CreateWpfApp()
    {
      new Application() {ShutdownMode = ShutdownMode.OnExplicitShutdown}.Run();
    }
  }
}
