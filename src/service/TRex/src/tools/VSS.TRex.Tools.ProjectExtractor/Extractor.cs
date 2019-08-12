using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.Alignments;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Designs;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.Machines;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Tools.ProjectExtractor
{
  public class Extractor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<Extractor>();

    private readonly ISiteModel _siteModel;
    private readonly string _projectOutputPath;

    public Extractor(ISiteModel siteModel, string projectOutputPath)
    {
      _siteModel = siteModel;
      _projectOutputPath = projectOutputPath;
    }

    public void ExtractAll()
    {
      ExtractSiteModelCoreMetaData();
      ExtractCoordinateSystem();
      ExtractMachines();
      ExtractExistenceMap();
      ExtractEventData();
      ExtractSpatialData();
      ExtractChangeMaps();
      ExtractSiteModelDesigns();
      ExtractMachineSiteModelDesigns();
      ExtractSiteModelMachineDesignNames();
      ExtractSurveyedSurfaces();
      ExtractAlignments();
      ExtractProofingRuns();
    }

    public void ExtractEventData()
    {
      foreach (var machine in _siteModel.Machines)
      {
        var basePath = Path.Combine(_projectOutputPath, "Events", machine.ID.ToString());
        Directory.CreateDirectory(basePath);

        foreach (var evtList in ProductionEventLists.ProductionEventTypeValues)
        {
          var eventsFileName = ProductionEvents.EventChangeListPersistantFileName(machine.InternalSiteModelMachineIndex, evtList);
          var readResult = _siteModel.PrimaryStorageProxy.ReadStreamFromPersistentStore(_siteModel.ID, eventsFileName, FileSystemStreamType.Events, out MemoryStream MS);

          if (readResult != FileSystemErrorStatus.OK || MS == null)
          {
            Log.LogError($"Failed to read directory stream for {eventsFileName} with error {readResult}, or read stream is null");
            Console.WriteLine($"Failed to read directory stream for {eventsFileName} with error {readResult}, or read stream is null");
            continue;
          }

          using (MS)
          {
            File.WriteAllBytes(Path.Combine(basePath, eventsFileName), MS.ToArray());
          }
        }
      }
    }

    public void ExtractSpatialData()
    {
      //  overallMap.ScanAllSetBitsAsSubGridAddresses(address => subGridGrouper.IntegrateSubGridGroup(result.ConstructPathToCell(address.X, address.Y, SubGridPathConstructionType.CreateLeaf) as IServerLeafSubGrid));

      // First write out the subGrid directory stream

      var basePath = Path.Combine(_projectOutputPath, "Spatial");
      Directory.CreateDirectory(basePath);

      _siteModel.ExistenceMap.ScanAllSetBitsAsSubGridAddresses(address =>
      {
        var fileName = ServerSubGridTree.GetLeafSubGridFullFileName(address);
        var FSError = _siteModel.PrimaryStorageProxy.ReadSpatialStreamFromPersistentStore(_siteModel.ID, fileName, address.X, address.Y, -1, -1, 1,
          FileSystemStreamType.SubGridDirectory, out var MS);

        if (FSError != FileSystemErrorStatus.OK || MS == null)
        {
          Log.LogError($"Failed to read directory stream for {fileName} with error {FSError}, or read stream is null");
          Console.WriteLine($"Failed to read directory stream for {fileName} with error {FSError}, or read stream is null");
          return;
        }

        using (MS)
        {
          File.WriteAllBytes(Path.Combine(basePath, fileName), MS.ToArray());

          // Write out all segment streams for the subGrid

          using (var subGrid = new ServerSubGridTreeLeaf())
          {
            subGrid.SetIsMutable(true);
            
            MS.Position = 0;
            if (subGrid.LoadDirectoryFromStream(MS))
            {
              subGrid.Directory.SegmentDirectory.ForEach(segment =>
              {
                var segmentFileName = segment.FileName(address.X, address.Y);
                var FSErrorSegment = _siteModel.PrimaryStorageProxy.ReadSpatialStreamFromPersistentStore
                (_siteModel.ID, segmentFileName, address.X, address.Y, segment.StartTime.Ticks, segment.EndTime.Ticks, segment.Version,
                  FileSystemStreamType.SubGridDirectory, out var MSSegment);

                if (FSErrorSegment != FileSystemErrorStatus.OK)
                {
                  Log.LogError($"Failed to read segment stream for {segmentFileName} with error {FSErrorSegment}");
                  Console.WriteLine($"Failed to read segment stream for {segmentFileName} with error {FSErrorSegment}");
                  return;
                }

                using (MSSegment)
                {
                  File.WriteAllBytes(Path.Combine(basePath, segmentFileName), MSSegment.ToArray());
                }
              });
            }
            else
            {
              Log.LogError($"Failed to read directory stream for {fileName}");
            }
          }
        }
      });
    }

    public void ExtractSiteModelFile(string fileName, FileSystemStreamType streamType)
    {
      var readResult = _siteModel.PrimaryStorageProxy.ReadStreamFromPersistentStore(_siteModel.ID, fileName, streamType, out var MS);

      if (readResult != FileSystemErrorStatus.OK || MS == null)
      {
        Log.LogInformation($"Failed to read file {fileName} of type {streamType}, (readResult = {readResult}), or stream is null");
        Console.WriteLine($"Failed to read file {fileName} of type {streamType}, (readResult = {readResult}), or stream is null");
        Console.WriteLine($"Failed to read existence map (readResult = {readResult}), or stream is null");
      }
      else
      {
        using (MS)
        {
          var basePath = Path.Combine(_projectOutputPath);
          Directory.CreateDirectory(basePath);

          File.WriteAllBytes(Path.Combine(basePath, fileName), MS.ToArray());
        }
      }
    }

    public void ExtractProofingRuns() => ExtractSiteModelFile(SiteProofingRunList.PROOFING_RUN_LIST_STREAM_NAME, FileSystemStreamType.ProofingRuns);

    public void ExtractAlignments() => ExtractSiteModelFile(AlignmentManager.ALIGNMENTS_STREAM_NAME, FileSystemStreamType.Alignments);

    public void ExtractSurveyedSurfaces() => ExtractSiteModelFile(SurveyedSurfaceManager.SURVEYED_SURFACE_STREAM_NAME, FileSystemStreamType.SurveyedSurfaces);

    public void ExtractSiteModelDesigns() => ExtractSiteModelFile(DesignManager.DESIGNS_STREAM_NAME, FileSystemStreamType.Designs);

    public void ExtractMachineSiteModelDesigns() => ExtractSiteModelFile(SiteModelDesignList.LIST_STREAM_NAME, FileSystemStreamType.MachineDesigns);

    public void ExtractSiteModelMachineDesignNames() => ExtractSiteModelFile(SiteModelMachineDesignList.MACHINE_DESIGN_LIST_STREAM_NAME, FileSystemStreamType.MachineDesignNames);

    public void ExtractExistenceMap() => ExtractSiteModelFile(SiteModel.kSubGridExistenceMapFileName, FileSystemStreamType.SubGridExistenceMap);

    public void ExtractSiteModelCoreMetaData() => ExtractSiteModelFile(SiteModel.kSiteModelXMLFileName, FileSystemStreamType.ProductionDataXML);

    public void ExtractMachines() => ExtractSiteModelFile(MachinesList.MACHINES_LIST_STREAM_NAME, FileSystemStreamType.Machines);

    public void ExtractCoordinateSystem() => ExtractSiteModelFile(CoordinateSystemConsts.kCoordinateSystemCSIBStorageKeyName, FileSystemStreamType.CoordinateSystemCSIB);

    public void ExtractChangeMaps()
    {
      var proxyStorageCache = DIContext.Obtain<ISiteModels>().PrimaryImmutableStorageProxy.ProjectMachineCache(FileSystemStreamType.SiteModelMachineElevationChangeMap);

      foreach (var machine in _siteModel.Machines)
      {
        try
        {
          var changeMap = proxyStorageCache.Get(new SiteModelMachineAffinityKey(_siteModel.ID, machine.ID, FileSystemStreamType.SiteModelMachineElevationChangeMap));

          var basePath = Path.Combine(_projectOutputPath, "ChangeMaps");
          Directory.CreateDirectory(basePath);

          File.WriteAllBytes(Path.Combine(basePath, machine.ID.ToString()), changeMap.Bytes);
        }
        catch (KeyNotFoundException)
        {
          // No change map for machine - continue on
        }
      }
    }
  }
}
