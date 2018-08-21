using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using Tests.Common;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.TAGFiles.Servers.Client;

/*
Arguments for building project #5, Dimensions:
5 "J:\PP\Construction\Office software\SiteVision Office\Test Files\VisionLink Data\Dimensions 2012\Dimensions2012-Model 381\Model 381"

Arguments for building project #6, Christchurch Southern Motorway:
6 "J:\PP\Construction\Office software\SiteVision Office\Test Files\VisionLink Data\Southern Motorway\TAYLORS COMP"
*/

namespace TRexTAGFileSubmittor
{
  class Program
  {
    private static ILogger Log;

    // Singleton request object for submitting TAG files. Creating these is relatively slow and support concurrent operations.
    private static SubmitTAGFileRequest submitTAGFileRequest;
    private static ProcessTAGFileRequest processTAGFileRequest;

    private static int tAGFileCount = 0;

    private static Guid[] ExtraProjectGuids = new[] {Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};

    private static Guid AssetOverride = Guid.Empty;


    public static void SubmitSingleTAGFile(Guid projectID, Guid assetID, string fileName)
    {
      submitTAGFileRequest = submitTAGFileRequest ?? new SubmitTAGFileRequest();
      SubmitTAGFileRequestArgument arg;

      using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      {
        byte[] bytes = new byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);

        arg = new SubmitTAGFileRequestArgument()
        {
          ProjectID = projectID,
          AssetID = assetID,
          TagFileContent = bytes,
          TAGFileName = Path.GetFileName(fileName)
        };
      }

      Log.LogInformation($"Submitting TAG file #{++tAGFileCount}: {fileName}");

      submitTAGFileRequest.Execute(arg);
    }

    public static void ProcessSingleTAGFile(Guid projectID, string fileName)
    {
   //   Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);
      Guid machineID = AssetOverride == Guid.Empty ? Guid.NewGuid() : AssetOverride; 

      processTAGFileRequest = processTAGFileRequest ?? new ProcessTAGFileRequest();
      ProcessTAGFileRequestArgument arg;

      using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      {
        byte[] bytes = new byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);

        arg = new ProcessTAGFileRequestArgument()
        {
          ProjectID = projectID,
          AssetID = machineID,
          TAGFiles = new List<ProcessTAGFileRequestFileItem>()
          {
            new ProcessTAGFileRequestFileItem()
            {
              FileName = Path.GetFileName(fileName),
                            TagFileContent = bytes,
                            IsJohnDoe = false
            }
          }
        };
      }

      processTAGFileRequest.Execute(arg);
    }

    public static void ProcessTAGFiles(Guid projectID, string[] files)
    {
     // Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);
      Guid machineID = AssetOverride == Guid.Empty ? Guid.NewGuid() : AssetOverride;

      processTAGFileRequest = processTAGFileRequest ?? new ProcessTAGFileRequest();
      ProcessTAGFileRequestArgument arg = new ProcessTAGFileRequestArgument
      {
        ProjectID = projectID,
        AssetID = machineID,
        TAGFiles = new List<ProcessTAGFileRequestFileItem>()
      };

      foreach (string file in files)
      {
        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
        {
          byte[] bytes = new byte[fs.Length];
          fs.Read(bytes, 0, bytes.Length);

          arg.TAGFiles.Add(new ProcessTAGFileRequestFileItem { FileName = Path.GetFileName(file), TagFileContent = bytes, IsJohnDoe = false});     
        }
      }

      processTAGFileRequest.Execute(arg);
    }

    public static void SubmitTAGFiles(Guid projectID, string[] files)
    {
      //   Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);
      Guid machineID = AssetOverride == Guid.Empty ? Guid.NewGuid() : AssetOverride;
      foreach (string file in files)
        SubmitSingleTAGFile(projectID, machineID, file);
    }

    public static void ProcessTAGFilesInFolder(Guid projectID, string folder)
    {
      // If it is a single file, just process it
      if (File.Exists(folder))
      {
        // ProcessTAGFiles(projectID, new string[] { folder });
        SubmitTAGFiles(projectID, new[] {folder});
      }
      else
      {
        string[] folders = Directory.GetDirectories(folder);
        foreach (string f in folders)
        {
          ProcessTAGFilesInFolder(projectID, f);
        }

        // ProcessTAGFiles(projectID, Directory.GetFiles(folder));
        SubmitTAGFiles(projectID, Directory.GetFiles(folder));
      }
    }

    public static void ProcessMachine333TAGFiles(Guid projectID)
    {
      ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Machine333");
    }

    public static void ProcessMachine10101TAGFiles(Guid projectID)
    {
      ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Machine10101");
    }

    private static void DependencyInjection()
    {
      DIBuilder.New().AddLogging().Complete();
    }

    static void Main(string[] args)
    {

      DependencyInjection();
      Log = VSS.TRex.Logging.Logger.CreateLogger<Program>();

      Log.LogInformation("Initialising TAG file processor");

      try
      {
        // Pull relevant arguments off the command line
        if (args.Length < 2)
        {
          Console.WriteLine("Usage: ProcessTAGFiles <ProjectID> <FolderPath>");
          return;
        }

        Guid projectID = Guid.Empty;
        string folderPath;
        try
        {
          projectID = Guid.Parse(args[0]);
          folderPath = args[1];
        }
        catch
        {
          Console.WriteLine($"Invalid project ID {args[0]} or folder path {args[1]}");
          return;
        }

        if (projectID == Guid.Empty)
        {
          return;
        }



        try
        {
          if (args.Length > 2)
            AssetOverride = Guid.Parse(args[2]);
        }
        catch
        {
          Console.WriteLine($"Invalid Asset ID {args[2]}");
          return;
        }



        // Obtain a TAGFileProcessing client server
        TAGFileProcessingClientServer TAGServer = new TAGFileProcessingClientServer();

        ProcessTAGFilesInFolder(projectID, folderPath);

        // ProcessMachine10101TAGFiles(projectID);
        // ProcessMachine333TAGFiles(projectID);

        //ProcessSingleTAGFile(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Machine10101\\2085J063SV--C01 XG 01 YANG--160804061209.tag");
        //ProcessSingleTAGFile(projectID);

        // Process all TAG files for project 4733:
        //ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 1");
        //ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 2");
        //ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 3");
        //ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 4");
      }
      finally
      {
        Console.ReadKey();
      }
    }
  }
}
