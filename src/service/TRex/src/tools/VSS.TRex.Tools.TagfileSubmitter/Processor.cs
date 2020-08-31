using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.TAGFiles.Models;

/*
Arguments for building project #5, Dimensions:
5 "J:\PP\Construction\Office software\SiteVision Office\Test Files\VisionLink Data\Dimensions 2012\Dimensions2012-Model 381\Model 381"

Arguments for building project #6, Christchurch Southern Motorway:
6 "J:\PP\Construction\Office software\SiteVision Office\Test Files\VisionLink Data\Southern Motorway\TAYLORS COMP"
*/

namespace VSS.TRex.Tools.TagfileSubmitter
{
  public static class TestCommonConsts
  {
    public static string TestDataFilePath() => "C:\\Dev\\VSS.TRex\\TAGFiles.Tests\\TestData\\";
  }

  public class Processor
  {
    private static ILogger Log = Logging.Logger.CreateLogger<Program>();

    // Singleton request object for submitting TAG files. Creating these is relatively slow and support concurrent operations.
    private SubmitTAGFileRequest _submitTagFileRequest;
    private ProcessTAGFileRequest _processTagFileRequest;

    private int _tagFileCount;

    public Guid AssetOverride = Guid.Empty;

    /// <summary>
    /// Return a delay in milliseconds to be enforced after each TAg file. Different delays can be returned to recognise the relative size
    /// of certain files
    /// </summary>
    private int OriginSourceSubmissionDelay(TAGFileOriginSource originSource)
    {
      return originSource switch
      {
        TAGFileOriginSource.LegacyTAGFileSource => 10,
        TAGFileOriginSource.VolvoMachineAssistCompactionCSV => 200,
        TAGFileOriginSource.VolvoMachineAssistEarthworksCSV => 200,
        _ => throw new NotImplementedException()
      };
    }

    public Task SubmitSingleTAGFile(Guid projectId, Guid assetId, string fileName, bool treatAsJohnDoe, TAGFileOriginSource originSource)
    {
      _submitTagFileRequest ??= new SubmitTAGFileRequest();
      SubmitTAGFileRequestArgument arg;

      using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      {
        var bytes = new byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);

        arg = new SubmitTAGFileRequestArgument
        {
          ProjectID = projectId,
          AssetID = assetId,
          TagFileContent = bytes,
          TAGFileName = Path.GetFileName(fileName),
          TreatAsJohnDoe = treatAsJohnDoe,
          SubmissionFlags = originSource == TAGFileOriginSource.LegacyTAGFileSource ? TAGFileSubmissionFlags.AddToArchive : 0,
          OriginSource = originSource
        };
      }

      Log.LogInformation($"Submitting TAG file #{++_tagFileCount}: {fileName} to asset {assetId}");

      Task.Delay(OriginSourceSubmissionDelay(originSource)).WaitAndUnwrapException();

      return _submitTagFileRequest.ExecuteAsync(arg);
    }

    public Task ProcessSingleTAGFile(Guid projectId, string fileName, Guid assetId, bool treatAsJohnDoe)
    {
      //   Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);
      var machineId = AssetOverride == Guid.Empty ? assetId : AssetOverride;

      _processTagFileRequest ??= new ProcessTAGFileRequest();
      ProcessTAGFileRequestArgument arg;

      using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      {
        var bytes = new byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);

        arg = new ProcessTAGFileRequestArgument
        {
          ProjectID = projectId,
          TAGFiles = new List<ProcessTAGFileRequestFileItem>
          {
            new ProcessTAGFileRequestFileItem
            {
              FileName = Path.GetFileName(fileName),
                            TagFileContent = bytes,
                            AssetId = machineId,
                            IsJohnDoe = treatAsJohnDoe,
                            SubmissionFlags = TAGFileSubmissionFlags.AddToArchive,
                            OriginSource = TAGFileOriginSource.LegacyTAGFileSource
            }
          }
        };
      }

      return _processTagFileRequest.ExecuteAsync(arg);
    }

    public Task ProcessTAGFiles(Guid projectId, string[] files)
    {
      // Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);
      var machineId = AssetOverride == Guid.Empty ? Guid.NewGuid() : AssetOverride;

      _processTagFileRequest ??= new ProcessTAGFileRequest();
      var arg = new ProcessTAGFileRequestArgument
      {
        ProjectID = projectId,
        TAGFiles = new List<ProcessTAGFileRequestFileItem>()
      };

      foreach (var file in files)
      {
        using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
        {
          var bytes = new byte[fs.Length];
          fs.Read(bytes, 0, bytes.Length);

          arg.TAGFiles.Add(new ProcessTAGFileRequestFileItem 
          { 
            FileName = Path.GetFileName(file), 
            TagFileContent = bytes, 
            AssetId = machineId, 
            IsJohnDoe = false,
            SubmissionFlags = TAGFileSubmissionFlags.AddToArchive,
            OriginSource = TAGFileOriginSource.LegacyTAGFileSource
          });
        }
      }

      return _processTagFileRequest.ExecuteAsync(arg);
    }

    public void SubmitTAGFiles(Guid projectId, List<string> files, bool treatAsJohnDoe, TAGFileOriginSource originSource)
    {
      // Assemble list of unique machines from the TAG file names using the hardware serial number to distinguish them
      var machineGuids = files.Select(x => x.Split('-')[0]).Distinct().ToDictionary(k => k, v => Guid.NewGuid());
      var taskList = new List<Task>();

      Log.LogInformation($"{machineGuids.Count} separate assets being submitted");

      foreach (var file in files)
      {
        var machineId = AssetOverride == Guid.Empty ? machineGuids[file.Split('-')[0]] : AssetOverride;
        taskList.Add(SubmitSingleTAGFile(projectId, machineId, file, treatAsJohnDoe, originSource));
      }

      try
      {
        Task.WhenAll(taskList).Wait();
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception submitting TAG files");
      }
    }

    public void CollectTAGFilesInFolder(string folder, List<List<string>> fileNamesFromFolders, string fileSkeleton)
    {
      // If it is a single file, just include it

      if (File.Exists(folder))
      {
        fileNamesFromFolders.Add(new List<string>{folder});
      }
      else
      {
        foreach (var f in Directory.GetDirectories(folder))
          CollectTAGFilesInFolder(f, fileNamesFromFolders, fileSkeleton);

        fileNamesFromFolders.Add(Directory.GetFiles(folder, fileSkeleton).ToList());
      }
    }

    public void ProcessSortedTAGFilesInFolder(Guid projectId, string folder, bool treatAsJohnDoe, TAGFileOriginSource originSource, string fileSkeleton)
    {
      var fileNamesFromFolders = new List<List<string>>();
      CollectTAGFilesInFolder(folder, fileNamesFromFolders, fileSkeleton);

      var combinedList = new List<string>();
      fileNamesFromFolders.ForEach(x => combinedList.AddRange(x));
      combinedList.Sort(new TAGFileNameComparer());
      SubmitTAGFiles(projectId, combinedList, treatAsJohnDoe, originSource);

//      fileNamesFromFolders.ForEach(x => x.Sort(new TAGFileNameComparer()));
//      fileNamesFromFolders.ForEach(x => SubmitTAGFiles(projectId, x));
    }

    public void ProcessTAGFilesInFolder(Guid projectId, string folder, bool treatAsJohnDoe, TAGFileOriginSource originSource, string fileSkeleton)
    {
      // If it is a single file, just process it
      if (File.Exists(folder))
      {
        // ProcessTAGFiles(projectID, new string[] { folder });
        SubmitTAGFiles(projectId, new List<string> { folder }, treatAsJohnDoe, originSource);
      }
      else
      {
        var folders = Directory.GetDirectories(folder);
        foreach (var f in folders)
        {
          ProcessTAGFilesInFolder(projectId, f, treatAsJohnDoe, originSource, fileSkeleton);
        }

        // ProcessTAGFiles(projectID, Directory.GetFiles(folder, "*.tag"));
        SubmitTAGFiles(projectId, Directory.GetFiles(folder, fileSkeleton).ToList(), treatAsJohnDoe, originSource);
      }
    }

    public void ProcessMachine333TAGFiles(Guid projectId, bool treatAsJohnDoe)
    {
      ProcessSortedTAGFilesInFolder(projectId, TestCommonConsts.TestDataFilePath() + "TAGFiles\\Machine333", treatAsJohnDoe, TAGFileOriginSource.LegacyTAGFileSource, "*.tag");
    }

    public void ProcessMachine10101TAGFiles(Guid projectId, bool treatAsJohnDoe)
    {
      ProcessSortedTAGFilesInFolder(projectId, TestCommonConsts.TestDataFilePath() + "TAGFiles\\Machine10101", treatAsJohnDoe, TAGFileOriginSource.LegacyTAGFileSource, "*.tag");
    }
  }
}
