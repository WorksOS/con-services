using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Logging;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.Types;

namespace VSS.TRex.Tools.ProjectExtractor
{
  public class Program
  {
    private static ILogger Log;

    private static void DependencyInjection()
    {
      DIBuilder.New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Build()
        .Add(TRexGridFactory.AddGridFactoriesToDI)
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels()))
        .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))
        .Complete();
    }

    static void ExtractEventData(ISiteModel siteModel, string projectOutputPath)
    {
      foreach (var machine in siteModel.Machines)
      {
        var allEventsForMachine = siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].GetEventLists();

        foreach (var evtList in allEventsForMachine)
        {
          siteModel.PrimaryStorageProxy.ReadStreamFromPersistentStore(siteModel.ID, evtList.EventChangeListPersistantFileName(), FileSystemStreamType.Events, out MemoryStream MS);

          using (MS)
          {
            File.WriteAllBytes(Path.Combine(projectOutputPath, "Events", $"Machine-{machine.ID}", evtList.EventChangeListPersistantFileName()), MS.ToArray());
          }
        }
      }
    }

    static void ExtractSpatialData(ISiteModel siteModel, string projectOutputPath)
    {
    //  overallMap.ScanAllSetBitsAsSubGridAddresses(address => subGridGrouper.IntegrateSubGridGroup(result.ConstructPathToCell(address.X, address.Y, SubGridPathConstructionType.CreateLeaf) as IServerLeafSubGrid));

    // First write out the subGrid directory stream

    siteModel.ExistenceMap.ScanAllSetBitsAsSubGridAddresses(address =>
    {
      var fileName = ServerSubGridTree.GetLeafSubGridFullFileName(address);
      var FSError = siteModel.PrimaryStorageProxy.ReadSpatialStreamFromPersistentStore(siteModel.ID, fileName, address.X, address.Y, -1, -1, 0,
        FileSystemStreamType.SubGridDirectory, out MemoryStream MS);

      if (FSError != FileSystemErrorStatus.OK)
      {
        Log.LogError($"Failed to read directory stream for {fileName} with error {FSError}");
        return;
      }

      using (MS)
      {
        File.WriteAllBytes(Path.Combine(projectOutputPath, "Spatial", fileName), MS.ToArray());
      }

      // Write out all segment streams for the subGrid

      var subGrid = new ServerSubGridTreeLeaf();
      if (subGrid.LoadDirectoryFromStream(MS))
      {
        subGrid.Directory.SegmentDirectory.ForEach(segment =>
        {
          var segmentFileName = segment.FileName(address.X, address.Y);
            var FSErrorSegment = siteModel.PrimaryStorageProxy.ReadSpatialStreamFromPersistentStore(siteModel.ID, segmentFileName, address.X, address.Y, -1, -1, 0,
            FileSystemStreamType.SubGridDirectory, out MemoryStream MSSegment);

          if (FSErrorSegment != FileSystemErrorStatus.OK)
          {
            Log.LogError($"Failed to read segment stream for {segmentFileName } with error {FSErrorSegment}");
            return;
          }

          using (MSSegment)
          {
            File.WriteAllBytes(Path.Combine(projectOutputPath, "Spatial", fileName), MSSegment.ToArray());
          }
        });
      }
      else
      {
        Log.LogError($"Failed to read directory stream for {fileName}");
      }
    });
    }

    static void ExtractExistenceMap(ISiteModel siteModel, string projectOutputPath)
    {
      var readResult = siteModel.PrimaryStorageProxy.ReadStreamFromPersistentStore(siteModel.ID, SiteModel.kSubGridExistenceMapFileName,
        FileSystemStreamType.SubGridExistenceMap, out MemoryStream MS);

      using (MS)
      {
        File.WriteAllBytes(Path.Combine(projectOutputPath, "MetaData", SiteModel.kSubGridExistenceMapFileName), MS.ToArray());
      }
    }

    static void ExtractSiteModelCoreMetaData(ISiteModel siteModel, string projectOutputPath)
    {
      var readResult = siteModel.PrimaryStorageProxy.ReadStreamFromPersistentStore(siteModel.ID, SiteModel.kSiteModelXMLFileName,
        FileSystemStreamType.ProductionDataXML, out MemoryStream MS);

      using (MS)
      {
        File.WriteAllBytes(Path.Combine(projectOutputPath, "MetaData", SiteModel.kSiteModelXMLFileName), MS.ToArray());
      }
    }

    static void Main(string[] args)
    {
      if (args.Length < 2)
        Console.WriteLine("Project Extractor: Usage: <ProjectUid> <RootProjectFolder>");

      if (!Guid.TryParse(args[0], out Guid projectUid))
      {
        Log.LogError($"Project UID {args[0]} is not a valid GUID");
        Console.WriteLine($"Project UID {args[0]} is not a valid GUID");
      }

      var RootOutputFolder = args[1];
      if (!Directory.Exists(RootOutputFolder))
      {
        Log.LogError($"Output location {RootOutputFolder} does not exist");
        Console.WriteLine($"Output location {RootOutputFolder} does not exist");
      }

      var ProjectOutputFolder = Path.Combine(RootOutputFolder, projectUid.ToString());

      DependencyInjection();

      try
      {
        Log = Logger.CreateLogger<Program>();

        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid, false);

        if (siteModel == null)
        {
          Log.LogError($"Project {projectUid} does not exist");
          Console.WriteLine($"Project {projectUid} does not exist");
          return;
        }

        siteModel.SetStorageRepresentationToSupply(StorageMutability.Mutable);

        Console.WriteLine($"Scanning project: {projectUid}");
        Console.WriteLine($"Outputting project data to: {ProjectOutputFolder}");

        ExtractSiteModelCoreMetaData(siteModel, ProjectOutputFolder);
        ExtractExistenceMap(siteModel, ProjectOutputFolder);
        ExtractEventData(siteModel, ProjectOutputFolder);
        ExtractSpatialData(siteModel, ProjectOutputFolder);
       // ExtractChangeMaps(siteModel, ProjectOutputFolder);

        Log.LogInformation($"Extraction of data for project {siteModel.ID} complete");
        Console.WriteLine($"Extraction of data for project {siteModel.ID} complete");
      }
            finally
      {
        DIContext.Obtain<ITRexGridFactory>()?.StopGrids();
      }
    }
  }
}
