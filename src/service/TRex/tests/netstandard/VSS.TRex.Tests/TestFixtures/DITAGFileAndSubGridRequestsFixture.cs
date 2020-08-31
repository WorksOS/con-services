using System;
using System.Collections.Generic;
using System.Linq;
using CoreX.Interfaces;
using CoreX.Wrapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.MasterData.Models.Models;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.Extensions;
using VSS.TRex.Common.Models;
using VSS.TRex.CoordinateSystems.Executors;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.Pipelines;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.Factories;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGrids;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.SurveyedSurfaces.GridFabric.Requests;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.TAGFiles.Classes.Integrator;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.Types;
using Consts = VSS.TRex.Common.Consts;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DITAGFileAndSubGridRequestsFixture : DITagFileFixture, IDisposable
  {
    public DITAGFileAndSubGridRequestsFixture()
    {
      SetupFixture();
    }

    public override void ClearDynamicFixtureContent()
    {
      base.ClearDynamicFixtureContent();
    }

    public override void SetupFixture()
    {
      base.SetupFixture();

      // Provide the mocks for spatial caching
      var tRexSpatialMemoryCacheContext = new Mock<ITRexSpatialMemoryCacheContext>();

      var tRexSpatialMemoryCache = new Mock<ITRexSpatialMemoryCache>();
      tRexSpatialMemoryCache.Setup(x => x.LocateOrCreateContext(It.IsAny<Guid>(), It.IsAny<GridDataType>(), It.IsAny<string>())).Returns(tRexSpatialMemoryCacheContext.Object);

      var mockImmutableSpatialAffinityPartitionMap = new Mock<IImmutableSpatialAffinityPartitionMap>();
      mockImmutableSpatialAffinityPartitionMap.Setup(x => x.PrimaryPartitions()).Returns(Enumerable.Range(0, (int) Consts.NUMPARTITIONS_PERDATACACHE).Select(x => true).ToArray());

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<Func<ISubGridRequestor>>(factory => () => new SubGridRequestor()))
        .Build()
        // Register the factory for surface elevation requests
        .Add(x => x.AddSingleton<Func<ITRexSpatialMemoryCache, ITRexSpatialMemoryCacheContext, ISurfaceElevationPatchRequest>>((cache, context) => new SurfaceElevationPatchRequestViaLocalCompute(cache, context)))
        .Add(x => x.AddSingleton<ITRexSpatialMemoryCache>(tRexSpatialMemoryCache.Object))
        .Build()
        .Add(x => x.AddSingleton<IRequestorUtilities>(new RequestorUtilities()))
        .Add(x => x.AddSingleton<ISubGridRetrieverFactory>(new SubGridRetrieverFactory()))
        .Add(x => x.AddSingleton(ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory()))
        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new TRex.SurveyedSurfaces.SurveyedSurfaces()))

        // Register the factories for cell profiling support
        .Add(x => x.AddSingleton<IProfilerBuilderFactory<ProfileCell>>(new ProfilerBuilderFactory<ProfileCell>()))
        .Add(x => x.AddSingleton<IProfilerBuilderFactory<SummaryVolumeProfileCell>>(new ProfilerBuilderFactory<SummaryVolumeProfileCell>()))
        .Add(x => x.AddTransient<IProfilerBuilder<ProfileCell>>(factory => new ProfilerBuilder<ProfileCell>()))
        .Add(x => x.AddTransient<IProfilerBuilder<SummaryVolumeProfileCell>>(factory => new ProfilerBuilder<SummaryVolumeProfileCell>()))

        // Register the factory for the CellProfileAnalyzer for detailed cell pass/lift cell profiles
        .Add(x => x.AddTransient<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, ICellLiftBuilder, IOverrideParameters, ILiftParameters, ICellProfileAnalyzer<ProfileCell>>>(
          factory => (siteModel, pDExistenceMap, filterSet, cellLiftBuilder, overrides, liftParams) 
            => new CellProfileAnalyzer(siteModel, pDExistenceMap, filterSet, cellLiftBuilder, overrides, liftParams)))

        // Register the factory for the CellProfileAnalyzer for summary volume cell profiles
        .Add(x => x.AddTransient<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, IDesignWrapper, ICellLiftBuilder, VolumeComputationType, IOverrideParameters, ILiftParameters, ICellProfileAnalyzer<SummaryVolumeProfileCell>>>(
          factory => (siteModel, pDExistenceMap, filterSet, referenceDesignWrapper, cellLiftBuilder, volumeComputationType, overrides, liftParams) 
            => new SummaryVolumesCellProfileAnalyzer(siteModel, pDExistenceMap, filterSet, referenceDesignWrapper, cellLiftBuilder, volumeComputationType, overrides, liftParams)))

        // Register a DI factory for ImmutableSpatialAffinityPartitionMap to represent an affinity partition map with just one partition
        .Add(x => x.AddSingleton<IImmutableSpatialAffinityPartitionMap>(mockImmutableSpatialAffinityPartitionMap.Object))

        .Add(TRex.ExistenceMaps.ExistenceMaps.AddExistenceMapFactoriesToDI)

        .Add(x => x.AddTransient<IFilterSet>(factory => new FilterSet()))

        // Register the mapper between requests operating in a pipeline and the listeners responding to 
        // partial responses of sub grids
        .Add(x => x.AddSingleton<IPipelineListenerMapper>(new PipelineListenerMapper()))

        .Add(x => x.AddSingleton<ICoreXWrapper, CoreXWrapper>())
        .Complete();
    }

    /// <summary>
    /// Takes a list of TAG files and constructs an ephemeral site model that may be queried
    /// </summary>
    public static ISiteModel BuildModel(IEnumerable<string> tagFiles, out List<AggregatedDataIntegratorTask> ProcessedTasks,
      bool callTaskProcessingComplete = true,
      bool convertToImmutableRepresentation = true,
      bool treatAsJohnDoeMachines = false)
    {
      ProcessedTasks = null;

      var _tagFiles = tagFiles.ToList();

      // Create the site model and machine etc to aggregate the processed TAG file into
      var targetSiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(NewSiteModelGuid, true);
      var preTargetSiteModelId = targetSiteModel.ID;

      // Switch to mutable storage representation to allow creation of content in the site model
      targetSiteModel.SetStorageRepresentationToSupply(StorageMutability.Mutable);
      targetSiteModel.ID.Should().Be(preTargetSiteModelId);
      targetSiteModel.PrimaryStorageProxy.Mutability.Should().Be(StorageMutability.Mutable);
      targetSiteModel.PrimaryStorageProxy.ImmutableProxy.Should().NotBeNull();

      IMachine targetMachine = null;

      // Create the integrator and add the processed TAG file to its processing list
      var integrator = new AggregatedDataIntegrator();
      var pageNumber = 0;
      var pageSize = 25;
      var taskCount = 0;
      TAGFileConverter[] converters = null;

      do
      {
        // Convert TAG files using TAGFileConverters into mini-site models
        converters = _tagFiles.Skip(pageNumber * pageSize).Take(pageSize).Select(x => ReadTAGFileFullPath(x, treatAsJohnDoeMachines)).ToArray();

        var currentMachineName = string.Empty;
        for (var i = 0; i < converters.Length; i++)
        {
          //One converter per tag file. See if machine changed if using real tag files.
          var parts = _tagFiles[i].Split("--");
          var tagMachineName = parts.Length == 3 ? parts[1] : "Test Machine";
          if (!string.Equals(currentMachineName, tagMachineName, StringComparison.OrdinalIgnoreCase))
          {
            currentMachineName = tagMachineName;
            targetMachine = targetSiteModel.Machines.CreateNew(currentMachineName, "", MachineType.Dozer, DeviceTypeEnum.SNM940, treatAsJohnDoeMachines, Guid.NewGuid());
          }
          converters[i].Machine.ID = targetMachine.ID;
          integrator.AddTaskToProcessList(converters[i].SiteModel, targetSiteModel.ID, converters[i].Machines,
            converters[i].SiteModelGridAggregator, converters[i].ProcessedCellPassCount, converters[i].MachinesTargetValueChangesAggregator);

          if ((i + 1) % 10 == 0 || i == converters.Length - 1)
          {
            // Construct an integration worker and ask it to perform the integration
            ProcessedTasks = new List<AggregatedDataIntegratorTask>();
            var worker = new AggregatedDataIntegratorWorker(integrator.TasksToProcess, targetSiteModel.ID)
            {
              MaxMappedTagFilesToProcessPerAggregationEpoch = _tagFiles.Count
            };
            worker.ProcessTask(ProcessedTasks, _tagFiles.Count);

            taskCount += ProcessedTasks.Count;
          }
        }

        pageNumber++;
        converters.ForEach(x => x.Dispose());
      } while (converters.Length > 0);


      if (callTaskProcessingComplete)
        new AggregatedDataIntegratorWorker(null, targetSiteModel.ID).CompleteTaskProcessing();

      // Construct an integration worker and ask it to perform the integration
      taskCount.Should().Be(_tagFiles.Count);

      // Reacquire the target site model to ensure any notification based changes to the site model are observed
      targetSiteModel.SiteModelExtent.IsValidPlanExtent.Should().BeTrue();
      targetSiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(preTargetSiteModelId, false);
      targetSiteModel.SiteModelExtent.IsValidPlanExtent.Should().BeTrue();

      // The ISiteModels instance will supply the immutable representation of the site model, ensure it is set to mutable
      targetSiteModel.SetStorageRepresentationToSupply(StorageMutability.Mutable);

      targetSiteModel.Should().NotBe(null);
      targetSiteModel.ID.Should().Be(preTargetSiteModelId);
      targetSiteModel.PrimaryStorageProxy.Mutability.Should().Be(StorageMutability.Mutable);
      targetSiteModel.PrimaryStorageProxy.ImmutableProxy.Should().NotBeNull();

      targetSiteModel.SiteModelExtent.IsValidPlanExtent.Should().BeTrue();

      // Modify the site model to switch from the mutable to immutable cell pass representation for read requests
      if (convertToImmutableRepresentation)
        ConvertSiteModelToImmutable(targetSiteModel);

      return targetSiteModel;
    }

    public static void AddSingleCellWithPasses(ISiteModel siteModel, int cellX, int cellY, 
      IEnumerable<CellPass> passes, int expectedCellCount = -1, int expectedPassCount = -1)
    {
      // Construct the sub grid to hold the cell being tested
      var leaf = siteModel.Grid.ConstructPathToCell(cellX, cellY, SubGridPathConstructionType.CreateLeaf) as IServerLeafSubGrid;
      leaf.Should().NotBeNull();

      leaf.AllocateLeafFullPassStacks();
      leaf.CreateDefaultSegment();
      leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());
      leaf.AllocateLeafLatestPassGrid();

      // Add the leaf to the site model existence map
      siteModel.ExistenceMap[leaf.OriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel, leaf.OriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel] = true;

      var _passes = passes.ToArray();

      var subGridX = (byte)(cellX & SubGridTreeConsts.SubGridLocalKeyMask);
      var subGridY = (byte)(cellY & SubGridTreeConsts.SubGridLocalKeyMask);

      foreach (var pass in _passes)
        leaf.AddPass(subGridX, subGridY, pass);

      var cellPasses = leaf.Cells.PassesData[0].PassesData.ExtractCellPasses(subGridX, subGridY);
      if (expectedPassCount > -1)
        ((int)cellPasses.PassCount).Should().Be(expectedPassCount);

      // Assign global latest cell pass to the appropriate pass
      leaf.Directory.GlobalLatestCells[subGridX, subGridY] = cellPasses.Passes.Last();

      // Ensure the pass data existence map records the existence of a non null value in the cell
      leaf.Directory.GlobalLatestCells.PassDataExistenceMap[subGridX, subGridY] = true;

      if (expectedCellCount > -1)
      {
        // Count the number of non-null elevation cells to verify a correct setup
        long totalCells = 0;
        siteModel.Grid.Root.ScanSubGrids(siteModel.Grid.FullCellExtent(), x =>
        {
          totalCells += leaf.Directory.GlobalLatestCells.PassDataExistenceMap.CountBits();
          return true;
        });

        totalCells.Should().Be(expectedCellCount);
      }

      siteModel.SiteModelExtent.Include(siteModel.Grid.GetCellExtents(cellX, cellY));

      // Save the leaf information just created
      siteModel.Grid.SaveLeafSubGrid(leaf, siteModel.PrimaryStorageProxy, siteModel.PrimaryStorageProxy, new List<ISubGridSpatialAffinityKey>());

      siteModel.SaveToPersistentStoreForTAGFileIngest(siteModel.PrimaryStorageProxy);

      siteModel.PrimaryStorageProxy.Commit();
    }

    public static void AddMultipleCellsWithPasses(ISiteModel siteModel, int cellX, int cellY,
      List<CellPass[]> passesList, int expectedCellCount = -1, int expectedPassCount = -1)
    {
      // Construct the sub grid to hold the cell being tested
      IServerLeafSubGrid leaf = siteModel.Grid.ConstructPathToCell(cellX, cellY, SubGridPathConstructionType.CreateLeaf) as IServerLeafSubGrid;
      leaf.Should().NotBeNull();

      leaf.AllocateLeafFullPassStacks();
      leaf.CreateDefaultSegment();
      leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());
      leaf.AllocateLeafLatestPassGrid();

      // Add the leaf to the site model existence map
      siteModel.ExistenceMap[leaf.OriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel, leaf.OriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel] = true;

      long totalCells = 0;

      for (var i = 0; i < passesList.Count; i++)
      {
        //CellPass[] _passes = passes.ToArray();

        byte subGridX = (byte) (cellX & SubGridTreeConsts.SubGridLocalKeyMask);
        byte subGridY = (byte) (cellY & SubGridTreeConsts.SubGridLocalKeyMask);

        foreach (var pass in passesList[i])
          leaf.AddPass(subGridX, subGridY, pass);

        var cellPasses = leaf.Cells.PassesData[i].PassesData.ExtractCellPasses(subGridX, subGridY);
        if (expectedPassCount > -1)
          ((int)cellPasses.PassCount).Should().Be(expectedPassCount);

        // Assign global latest cell pass to the appropriate pass
        leaf.Directory.GlobalLatestCells[subGridX, subGridY] = cellPasses.Passes.Last();

        // Ensure the pass data existence map records the existence of a non null value in the cell
        leaf.Directory.GlobalLatestCells.PassDataExistenceMap[subGridX, subGridY] = true;
        
        if (expectedCellCount > -1)
        {
          // Count the number of non-null elevation cells to verify a correct setup
          siteModel.Grid.Root.ScanSubGrids(siteModel.Grid.FullCellExtent(), x =>
          {
            totalCells += leaf.Directory.GlobalLatestCells.PassDataExistenceMap.CountBits();
            return true;
          });
        }

        siteModel.SiteModelExtent.Include(siteModel.Grid.GetCellExtents(cellX, cellY));
      }

      totalCells.Should().Be(expectedCellCount);

      // Save the leaf information just created
      siteModel.Grid.SaveLeafSubGrid(leaf, siteModel.PrimaryStorageProxy, siteModel.PrimaryStorageProxy, new List<ISubGridSpatialAffinityKey>());

      siteModel.SaveToPersistentStoreForTAGFileIngest(siteModel.PrimaryStorageProxy);
    }

    public static void AddSingleSubGridWithPasses(ISiteModel siteModel, int cellX, int cellY, IEnumerable<CellPass>[,] passes)
    {
      // Construct the sub grid to hold the cell being tested
      var leaf = siteModel.Grid.ConstructPathToCell(cellX, cellY, SubGridPathConstructionType.CreateLeaf) as IServerLeafSubGrid;
      leaf.Should().NotBeNull();

      leaf.AllocateLeafFullPassStacks();
      leaf.CreateDefaultSegment();
      leaf.AllocateFullPassStacks(leaf.Directory.SegmentDirectory.First());
      leaf.AllocateLeafLatestPassGrid();

      // Add the leaf to the site model existence map
      siteModel.ExistenceMap[leaf.OriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel, leaf.OriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel] = true;

      siteModel.Grid.CountLeafSubGridsInMemory().Should().Be(1);

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        CellPass[] _passes = passes[x,y].ToArray();

        byte subGridX = (byte) ((cellX + x) & SubGridTreeConsts.SubGridLocalKeyMask);
        byte subGridY = (byte) ((cellY + y) & SubGridTreeConsts.SubGridLocalKeyMask);

        foreach (var pass in _passes)
          leaf.AddPass(subGridX, subGridY, pass);

        // Assign global latest cell pass to the appropriate pass
        leaf.Directory.GlobalLatestCells[subGridX, subGridY] = _passes.Last();

        // Ensure the pass data existence map records the existence of a non null value in the cell
        leaf.Directory.GlobalLatestCells.PassDataExistenceMap[subGridX, subGridY] = true;
      });

      var siteModelExtent = siteModel.Grid.GetCellExtents(cellX, cellY);
      siteModelExtent.Include(siteModel.Grid.GetCellExtents(cellX + SubGridTreeConsts.SubGridTreeDimension, cellY + SubGridTreeConsts.SubGridTreeDimension));
      siteModel.SiteModelExtent.Set(siteModelExtent.MinX, siteModelExtent.MinY, siteModelExtent.MaxX, siteModelExtent.MaxY);

      // Save the leaf information just created
      siteModel.Grid.SaveLeafSubGrid(leaf, siteModel.PrimaryStorageProxy, siteModel.PrimaryStorageProxy, new List<ISubGridSpatialAffinityKey>());

      siteModel.SaveToPersistentStoreForTAGFileIngest(siteModel.PrimaryStorageProxy);
    }

    /// <summary>
    /// Adds the provided coordinate system expressed as a CSIB to the site model
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="csib"></param>
    public static void AddCSIBToSiteModel(ref ISiteModel siteModel, string csib)
    {
      var executor = new AddCoordinateSystemExecutor();
      executor.Execute(siteModel.ID, csib);
    }

    /// <summary>
    /// Takes a site model and modifies its internal representation to be the immutable form
    /// </summary>
    public static void ConvertSiteModelToImmutable(ISiteModel siteModel)
    {
      siteModel.SetStorageRepresentationToSupply(StorageMutability.Immutable);
    }

    public override void Dispose()
    {
      base.Dispose();
    }
  }
}
