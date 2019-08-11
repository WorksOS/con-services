using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Tools.ProjectExtractor
{
  public class Injector
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<Extractor>();

    private readonly ISiteModel _siteModel;
    private readonly string _projectInputPath;

    /// <summary>
    /// Given a stream containing the serialised content of a type of 'file' in TRex, return a deserialised object representing it
    /// </summary>
    /// <param name="streamType"></param>
    /// <param name="MS"></param>
    /// <returns></returns>
    private object ConstructTRexObjectFromStream(FileSystemStreamType streamType, MemoryStream MS)
    {
      switch (streamType)
      {
        case FileSystemStreamType.SubGridSegment:
          break;
        case FileSystemStreamType.SubGridDirectory:
          break;
        case FileSystemStreamType.Events:
          break;
        case FileSystemStreamType.ProductionDataXML:
          break;
        case FileSystemStreamType.SubGridExistenceMap:
          break;
        case FileSystemStreamType.CoordinateSystemCSIB:
          return Encoding.ASCII.GetString(MS.ToArray());
        case FileSystemStreamType.SurveyedSurfaces:
          break;
        case FileSystemStreamType.Designs:
          break;
        case FileSystemStreamType.Machines:
          break;
        case FileSystemStreamType.MachineDesigns:
          break;
        case FileSystemStreamType.MachineDesignNames:
          break;
        case FileSystemStreamType.ProofingRuns:
          break;
        case FileSystemStreamType.Alignments:
          break;
        case FileSystemStreamType.SubGridVersionMap:
          break;
        case FileSystemStreamType.SiteModelMachineElevationChangeMap:
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(streamType), streamType, null);
      }

      return null;
    }

    public Injector(ISiteModel siteModel, string projectInputPath)
    {
      _siteModel = siteModel;
      _projectInputPath = projectInputPath;
    }

    public void InjectSiteModelFile(string fileName, FileSystemStreamType streamType)
    {
      using (var MS = new MemoryStream(File.ReadAllBytes(Path.Combine(_projectInputPath, fileName))))
      {
        if (MS.Length == 0)
        {
          Log.LogInformation($"Stream read for file {fileName} of type {streamType} is empty");
          Console.WriteLine($"Stream read for file {fileName} of type {streamType} is empty");
          return;
        }

        object obj = ConstructTRexObjectFromStream(streamType, MS);

        if (obj == null)
        {
          Log.LogInformation($"Failed to deserialise file {fileName} of type {streamType}, or stream is null");
          Console.WriteLine($"Failed to deserialise file {fileName} of type {streamType}, or stream is null");

          return;
        }

        // Write the deserialised object into the TRex project. This will write the content in to the mutable store, the act of
        // writing this element will also project the mutable object into the immutable form and inject it into the immutable grid.
        var writeResult = _siteModel.PrimaryStorageProxy.WriteStreamToPersistentStore(_siteModel.ID, fileName, streamType, MS, obj);

        if (writeResult != FileSystemErrorStatus.OK)
        {
          Log.LogInformation($"Failed to write file {fileName} of type {streamType}, (writeResult = {writeResult})");
          Console.WriteLine($"Failed to write file {fileName} of type {streamType}, (writeResult = {writeResult})");
        }
      }
    }
  }
}
