using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.Machines;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.Types;

namespace VSS.TRex.Tools.ProjectExtractor
{
  public class Extractor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<Extractor>();

    private ISiteModel _siteModel;
    private string _projectOutputPath;

    public Extractor(ISiteModel siteModel, string projectOutputPath)
    {
      _siteModel = siteModel;
      _projectOutputPath = projectOutputPath;
    }

    public void ExtractEventData()
    {
      foreach (var machine in _siteModel.Machines)
      {
        var allEventsForMachine = _siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].GetEventLists();

        foreach (var evtList in allEventsForMachine)
        {
          _siteModel.PrimaryStorageProxy.ReadStreamFromPersistentStore(_siteModel.ID, evtList.EventChangeListPersistantFileName(), FileSystemStreamType.Events, out MemoryStream MS);

          using (MS)
          {
            File.WriteAllBytes(Path.Combine(_projectOutputPath, "Events", $"Machine-{machine.ID}", evtList.EventChangeListPersistantFileName()), MS.ToArray());
          }
        }
      }
    }

    public void ExtractSpatialData()
    {
      //  overallMap.ScanAllSetBitsAsSubGridAddresses(address => subGridGrouper.IntegrateSubGridGroup(result.ConstructPathToCell(address.X, address.Y, SubGridPathConstructionType.CreateLeaf) as IServerLeafSubGrid));

      // First write out the subGrid directory stream

      _siteModel.ExistenceMap.ScanAllSetBitsAsSubGridAddresses(address =>
      {
        var fileName = ServerSubGridTree.GetLeafSubGridFullFileName(address);
        var FSError = _siteModel.PrimaryStorageProxy.ReadSpatialStreamFromPersistentStore(_siteModel.ID, fileName, address.X, address.Y, -1, -1, 0,
          FileSystemStreamType.SubGridDirectory, out MemoryStream MS);

        if (FSError != FileSystemErrorStatus.OK || MS == null)
        {
          Log.LogError($"Failed to read directory stream for {fileName} with error {FSError}, or read stream is null");
          return;
        }

        using (MS)
        {
          File.WriteAllBytes(Path.Combine(_projectOutputPath, "Spatial", fileName), MS.ToArray());
        }

        // Write out all segment streams for the subGrid

        var subGrid = new ServerSubGridTreeLeaf();
        if (subGrid.LoadDirectoryFromStream(MS))
        {
          subGrid.Directory.SegmentDirectory.ForEach(segment =>
          {
            var segmentFileName = segment.FileName(address.X, address.Y);
            var FSErrorSegment = _siteModel.PrimaryStorageProxy.ReadSpatialStreamFromPersistentStore(_siteModel.ID, segmentFileName, address.X, address.Y, -1, -1, 0,
              FileSystemStreamType.SubGridDirectory, out MemoryStream MSSegment);

            if (FSErrorSegment != FileSystemErrorStatus.OK)
            {
              Log.LogError($"Failed to read segment stream for {segmentFileName} with error {FSErrorSegment}");
              return;
            }

            using (MSSegment)
            {
              File.WriteAllBytes(Path.Combine(_projectOutputPath, "Spatial", fileName), MSSegment.ToArray());
            }
          });
        }
        else
        {
          Log.LogError($"Failed to read directory stream for {fileName}");
        }
      });
    }

    public void ExtractExistenceMap()
    {
      var readResult = _siteModel.PrimaryStorageProxy.ReadStreamFromPersistentStore(_siteModel.ID, SiteModel.kSubGridExistenceMapFileName,
        FileSystemStreamType.SubGridExistenceMap, out MemoryStream MS);

      if (readResult != FileSystemErrorStatus.OK || MS == null)
      {
        Log.LogInformation($"Failed to read existence map (readResult = {readResult}), or stream is null");
        Console.WriteLine($"Failed to read existence map (readResult = {readResult}), or stream is null");
      }
      else
      {
        using (MS)
        {
          File.WriteAllBytes(Path.Combine(_projectOutputPath, "MetaData", SiteModel.kSubGridExistenceMapFileName), MS.ToArray());
        }
      }
    }

    public void ExtractSiteModelCoreMetaData()
    {
      var readResult = _siteModel.PrimaryStorageProxy.ReadStreamFromPersistentStore(_siteModel.ID, SiteModel.kSiteModelXMLFileName,
        FileSystemStreamType.ProductionDataXML, out MemoryStream MS);

      if (readResult != FileSystemErrorStatus.OK || MS == null)
      {
        Log.LogInformation($"Failed to read core project metadata (readResult = {readResult}), or stream is null");
        Console.WriteLine($"Failed to read core project metadata (readResult = {readResult}), or stream is null");
      }
      else
      {
        using (MS)
        {
          File.WriteAllBytes(Path.Combine(_projectOutputPath, "MetaData", SiteModel.kSiteModelXMLFileName), MS.ToArray());
        }
      }
    }

    public void ExtractChangeMaps()
    {
      var proxyStorageCache = DIContext.Obtain<ISiteModels>().PrimaryImmutableStorageProxy.ProjectMachineCache(FileSystemStreamType.SiteModelMachineElevationChangeMap);

      foreach (var machine in _siteModel.Machines)
      {
        try
        {
          var changeMap = proxyStorageCache.Get(new SiteModelMachineAffinityKey(_siteModel.ID, machine.ID, FileSystemStreamType.SiteModelMachineElevationChangeMap));

          File.WriteAllBytes(Path.Combine(_projectOutputPath, "ChangeMaps", $"Machine-{machine.ID}"), changeMap.Bytes);
        }
        catch (KeyNotFoundException)
        {
          // No change map for machine - continue on
        }
      }
    }

    public void ExtractMachines()
    {
      _siteModel.PrimaryStorageProxy.ReadStreamFromPersistentStore(_siteModel.ID, MachinesList.MACHINES_LIST_STREAM_NAME, FileSystemStreamType.Machines, out MemoryStream MS);

      if (MS == null)
      {
        Log.LogWarning($"No machines found for site model {_siteModel.ID}");
        Console.WriteLine($"No machines found for site model {_siteModel.ID}");
      }
      else
      {
        using (MS)
        {
          File.WriteAllBytes(Path.Combine(_projectOutputPath, "MachineList"), MS.ToArray());
        }
      }
    }

    public void ExtractCoordinateSystem()
    {
      _siteModel.PrimaryStorageProxy.ReadStreamFromPersistentStore(_siteModel.ID, MachinesList.MACHINES_LIST_STREAM_NAME, FileSystemStreamType.CoordinateSystemCSIB, out MemoryStream MS);

      if (MS == null)
      {
        Log.LogWarning($"No coordinate system found for site model {_siteModel.ID}");
        Console.WriteLine($"No coordinate system found for site model {_siteModel.ID}");
      }
      else
      {
        using (MS)
        {
          File.WriteAllBytes(Path.Combine(_projectOutputPath, "MachineList"), MS.ToArray());
        }
      }
    }
  }
}
