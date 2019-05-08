using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Tests.Common;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;

/*
Arguments for building project #5, Dimensions:
5 "J:\PP\Construction\Office software\SiteVision Office\Test Files\VisionLink Data\Dimensions 2012\Dimensions2012-Model 381\Model 381"

Arguments for building project #6, Christchurch Southern Motorway:
6 "J:\PP\Construction\Office software\SiteVision Office\Test Files\VisionLink Data\Southern Motorway\TAYLORS COMP"
*/

namespace VSS.TRex.Tools.TagfileSubmitter
{
  public class Processor
  {
    private static ILogger Log = Logging.Logger.CreateLogger<Program>();

    // Singleton request object for submitting TAG files. Creating these is relatively slow and support concurrent operations.
    private SubmitTAGFileRequest submitTAGFileRequest;
    private ProcessTAGFileRequest processTAGFileRequest;

    private int tAGFileCount = 0;

    public Guid[] ExtraProjectGuids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

    public Guid AssetOverride = Guid.Empty;


    public void SubmitSingleTAGFile(Guid projectID, Guid assetID, string fileName)
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

    public void ProcessSingleTAGFile(Guid projectID, string fileName)
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
          AssetUID = machineID,
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

    public void ProcessTAGFiles(Guid projectID, string[] files)
    {
      // Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);
      Guid machineID = AssetOverride == Guid.Empty ? Guid.NewGuid() : AssetOverride;

      processTAGFileRequest = processTAGFileRequest ?? new ProcessTAGFileRequest();
      ProcessTAGFileRequestArgument arg = new ProcessTAGFileRequestArgument
      {
        ProjectID = projectID,
        AssetUID = machineID,
        TAGFiles = new List<ProcessTAGFileRequestFileItem>()
      };

      foreach (string file in files)
      {
        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
        {
          byte[] bytes = new byte[fs.Length];
          fs.Read(bytes, 0, bytes.Length);

          arg.TAGFiles.Add(new ProcessTAGFileRequestFileItem { FileName = Path.GetFileName(file), TagFileContent = bytes, IsJohnDoe = false });
        }
      }

      processTAGFileRequest.Execute(arg);
    }

    public void SubmitTAGFiles(Guid projectID, string[] files)
    {
      //   Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);
      Guid machineID = AssetOverride == Guid.Empty ? Guid.NewGuid() : AssetOverride;
      foreach (string file in files)
        SubmitSingleTAGFile(projectID, machineID, file);
    }

    public void CollectTAGFilesInFolder(string folder, List<string> fileNamesFromFolders)
    {
      // If it is a single file, just include it

      if (File.Exists(folder))
      {
        fileNamesFromFolders.Add(folder);
      }
      else
      {
        foreach (string f in Directory.GetDirectories(folder))
          CollectTAGFilesInFolder(f, fileNamesFromFolders);

        fileNamesFromFolders.AddRange(Directory.GetFiles(folder, "*.tag"));
      }
    }

    public void ProcessSortedTAGFilesInFolder(Guid projectID, string folder)
    {
      var fileNamesFromFolders = new List<string>();
      CollectTAGFilesInFolder(folder, fileNamesFromFolders);

      fileNamesFromFolders.Sort(new TAGFileNameComparer());

      SubmitTAGFiles(projectID, fileNamesFromFolders.ToArray());
    }

    public void ProcessTAGFilesInFolder(Guid projectID, string folder)
    {
      // If it is a single file, just process it
      if (File.Exists(folder))
      {
        // ProcessTAGFiles(projectID, new string[] { folder });
        SubmitTAGFiles(projectID, new[] { folder });
      }
      else
      {
        string[] folders = Directory.GetDirectories(folder);
        foreach (string f in folders)
        {
          ProcessTAGFilesInFolder(projectID, f);
        }

        // ProcessTAGFiles(projectID, Directory.GetFiles(folder, "*.tag"));
        SubmitTAGFiles(projectID, Directory.GetFiles(folder, "*.tag"));
      }
    }

    public void ProcessMachine333TAGFiles(Guid projectID)
    {
      ProcessSortedTAGFilesInFolder(projectID, TestCommonConsts.TestDataFilePath() + "TAGFiles\\Machine333");
    }

    public void ProcessMachine10101TAGFiles(Guid projectID)
    {
      ProcessSortedTAGFilesInFolder(projectID, TestCommonConsts.TestDataFilePath() + "TAGFiles\\Machine10101");
    }
  }
}
