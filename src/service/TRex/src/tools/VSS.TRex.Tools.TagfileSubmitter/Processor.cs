using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tests.Common;
using VSS.TRex.TAGFiles.Classes;
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

    public Guid AssetOverride = Guid.Empty;


    public Task SubmitSingleTAGFile(Guid projectID, Guid assetID, string fileName)
    {
      submitTAGFileRequest = submitTAGFileRequest ?? new SubmitTAGFileRequest();
      SubmitTAGFileRequestArgument arg;

      using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      {
        byte[] bytes = new byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);

        arg = new SubmitTAGFileRequestArgument
        {
          ProjectID = projectID,
          AssetID = assetID,
          TagFileContent = bytes,
          TAGFileName = Path.GetFileName(fileName)
        };
      }

      Log.LogInformation($"Submitting TAG file #{++tAGFileCount}: {fileName}");

      return submitTAGFileRequest.ExecuteAsync(arg);
    }

    public Task ProcessSingleTAGFile(Guid projectID, string fileName)
    {
      //   Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);
      var machineID = AssetOverride == Guid.Empty ? Guid.NewGuid() : AssetOverride;

      processTAGFileRequest = processTAGFileRequest ?? new ProcessTAGFileRequest();
      ProcessTAGFileRequestArgument arg;

      using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      {
        byte[] bytes = new byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);

        arg = new ProcessTAGFileRequestArgument
        {
          ProjectID = projectID,
          AssetUID = machineID,
          TAGFiles = new List<ProcessTAGFileRequestFileItem>
          {
            new ProcessTAGFileRequestFileItem
            {
              FileName = Path.GetFileName(fileName),
                            TagFileContent = bytes,
                            IsJohnDoe = false
            }
          }
        };
      }

      return processTAGFileRequest.ExecuteAsync(arg);
    }

    public Task ProcessTAGFiles(Guid projectID, string[] files)
    {
      // Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);
      var machineID = AssetOverride == Guid.Empty ? Guid.NewGuid() : AssetOverride;

      processTAGFileRequest = processTAGFileRequest ?? new ProcessTAGFileRequest();
      var arg = new ProcessTAGFileRequestArgument
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

      return processTAGFileRequest.ExecuteAsync(arg);
    }

    public void SubmitTAGFiles(Guid projectID, List<string> files)
    {
      //   Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);
      var machineID = AssetOverride == Guid.Empty ? Guid.NewGuid() : AssetOverride;

      var taskList = new List<Task>();

      foreach (string file in files)
        taskList.Add(SubmitSingleTAGFile(projectID, machineID, file));

      Task.WhenAll(taskList);
    }

    public void CollectTAGFilesInFolder(string folder, List<List<string>> fileNamesFromFolders)
    {
      // If it is a single file, just include it

      if (File.Exists(folder))
      {
        fileNamesFromFolders.Add(new List<string>{folder});
      }
      else
      {
        foreach (string f in Directory.GetDirectories(folder))
          CollectTAGFilesInFolder(f, fileNamesFromFolders);

        fileNamesFromFolders.Add(Directory.GetFiles(folder, "*.tag").ToList());
      }
    }

    public void ProcessSortedTAGFilesInFolder(Guid projectID, string folder)
    {
      var fileNamesFromFolders = new List<List<string>>();
      CollectTAGFilesInFolder(folder, fileNamesFromFolders);

      fileNamesFromFolders.ForEach(x => x.Sort(new TAGFileNameComparer()));

      fileNamesFromFolders.ForEach(x => SubmitTAGFiles(projectID, x));
    }

    public void ProcessTAGFilesInFolder(Guid projectID, string folder)
    {
      // If it is a single file, just process it
      if (File.Exists(folder))
      {
        // ProcessTAGFiles(projectID, new string[] { folder });
        SubmitTAGFiles(projectID, new List<string> { folder });
      }
      else
      {
        string[] folders = Directory.GetDirectories(folder);
        foreach (string f in folders)
        {
          ProcessTAGFilesInFolder(projectID, f);
        }

        // ProcessTAGFiles(projectID, Directory.GetFiles(folder, "*.tag"));
        SubmitTAGFiles(projectID, Directory.GetFiles(folder, "*.tag").ToList());
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
