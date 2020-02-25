using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.DI;
using VSS.ConfigurationStore;
using VSS.TRex.Common.Utilities;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.TAGFiles.Servers.Client;

/*
Arguments for building project #5, Dimensions:
5 "J:\PP\Construction\Office software\SiteVision Office\Test Files\VisionLink Data\Dimensions 2012\Dimensions2012-Model 381\Model 381"

Arguments for building project #6, Christchurch Southern Motorway:
6 "J:\PP\Construction\Office software\SiteVision Office\Test Files\VisionLink Data\Southern Motorway\TAYLORS COMP"
*/

namespace VSS.TRex.Tools.TagfileSubmitter
{

  public class Program
  {
    private static ILogger Log = Logging.Logger.CreateLogger<Program>();

    private static void DependencyInjection()
    {
      DIBuilder.New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(TRexGridFactory.AddGridFactoriesToDI)
        .Build()
        .Add(x => x.AddSingleton(new TAGFileProcessingClientServer()))
        .Complete();
    }

    public static void Main(string[] args)
    {
      DependencyInjection();

      try
      {
        var processor = new Processor();

        // Make sure all our assemblies are loaded...
        AssembliesHelper.LoadAllAssembliesForExecutingContext();

        Log.LogInformation("Initialising TAG file processor");

        try
        {
          // Pull relevant arguments off the command line
          if (args.Length < 2)
          {
            Console.WriteLine("Usage: ProcessTAGFiles <ProjectUID> <FolderPath>");
            Console.ReadKey();
            return;
          }

          Guid projectId;
          string folderPath;
          try
          {
            projectId = Guid.Parse(args[0]);
            folderPath = args[1];
          }
          catch
          {
            Console.WriteLine($"Invalid project ID {args[0]} or folder path {args[1]}");
            Console.ReadKey();
            return;
          }

          if (projectId == Guid.Empty)
          {
            return;
          }

          try
          {
            if (args.Length > 2)
              processor.AssetOverride = Guid.Parse(args[2]);
          }
          catch
          {
            Console.WriteLine($"Invalid Asset ID {args[2]}");
            return;
          }

          try
          {
            processor.ProcessSortedTAGFilesInFolder(projectId, folderPath);
          }
          catch (Exception e)
          {
            Console.WriteLine($"Exception: {e}");
          }

          // ProcessMachine10101TAGFiles(projectID);
          // ProcessMachine333TAGFiles(projectID);

          //ProcessSingleTAGFile(projectID, TestConsts.TestDataFilePath() + "TAGFiles\\Machine10101\\2085J063SV--C01 XG 01 YANG--160804061209.tag");
          //ProcessSingleTAGFile(projectID);

          // Process all TAG files for project 4733:
          //ProcessTAGFilesInFolder(projectID, TestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 1");
          //ProcessTAGFilesInFolder(projectID, TestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 2");
          //ProcessTAGFilesInFolder(projectID, TestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 3");
          //ProcessTAGFilesInFolder(projectID, TestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 4");
        }
        finally
        {
          DIContext.Obtain<ITRexGridFactory>()?.StopGrids();
        }
      }
      finally
      {
        // Reinstate with a command line flag if required in future
        //Console.WriteLine("TAG file submission complete. Press a key...");
        //Console.ReadKey();
      }
    }
  }
}
