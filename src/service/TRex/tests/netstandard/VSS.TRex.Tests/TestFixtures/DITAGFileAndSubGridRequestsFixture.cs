using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Cells;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.Factories;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.TAGFiles.Classes.Integrator;
using VSS.TRex.Types;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Consts = VSS.TRex.Common.Consts;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DITAGFileAndSubGridRequestsFixture : DITagFileFixture
  {
    public DITAGFileAndSubGridRequestsFixture() : base()
    {
      SetupFixture();
    }

    public new void SetupFixture()
    {
      // Provide the surveyed surface request mock
      var surfaceElevationPatchRequest = new Mock<ISurfaceElevationPatchRequest>();
      surfaceElevationPatchRequest.Setup(x => x.Execute(It.IsAny<ISurfaceElevationPatchArgument>())).Returns(new ClientHeightAndTimeLeafSubGrid());

      // Provide the mocks for spatial caching
      var tRexSpatialMemoryCacheContext = new Mock<ITRexSpatialMemoryCacheContext>();

      var tRexSpatialMemoryCache = new Mock<ITRexSpatialMemoryCache>();
      tRexSpatialMemoryCache.Setup(x => x.LocateOrCreateContext(It.IsAny<Guid>(), It.IsAny<string>())).Returns(tRexSpatialMemoryCacheContext.Object);

      var mockImmutableSpatialAffinityPartitionMap = new Mock<IImmutableSpatialAffinityPartitionMap>();
      mockImmutableSpatialAffinityPartitionMap.Setup(x => x.PrimaryPartitions()).Returns(Enumerable.Range(0, (int) Consts.NUMPARTITIONS_PERDATACACHE).Select(x => true).ToArray());

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<Func<ISubGridRequestor>>(factory => () => new SubGridRequestor()))
        .Build()
        // Register the mock factory for surface elevation requests
        .Add(x => x.AddSingleton<Func<ITRexSpatialMemoryCache, ITRexSpatialMemoryCacheContext, ISurfaceElevationPatchRequest>>((cache, context) => surfaceElevationPatchRequest.Object))
        .Add(x => x.AddSingleton<ITRexSpatialMemoryCache>(tRexSpatialMemoryCache.Object))
        .Build()
        .Add(x => x.AddSingleton<IRequestorUtilities>(new RequestorUtilities()))
        .Add(x => x.AddSingleton(ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory()))
        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces.SurveyedSurfaces()))

        // Register the factories for cell profiling support
        .Add(x => x.AddSingleton<IProfilerBuilderFactory<ProfileCell>>(new ProfilerBuilderFactory<ProfileCell>()))
        .Add(x => x.AddSingleton<IProfilerBuilderFactory<SummaryVolumeProfileCell>>(new ProfilerBuilderFactory<SummaryVolumeProfileCell>()))
        .Add(x => x.AddTransient<IProfilerBuilder<ProfileCell>>(factory => new ProfilerBuilder<ProfileCell>()))
        .Add(x => x.AddTransient<IProfilerBuilder<SummaryVolumeProfileCell>>(factory => new ProfilerBuilder<SummaryVolumeProfileCell>()))

        // Register the factory for the CellProfileAnalyzer for detailed cell pass/lift cell profiles
        .Add(x => x.AddTransient<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, IDesign, ICellLiftBuilder, ICellProfileAnalyzer<ProfileCell>>>(
          factory => (siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign, cellLiftBuilder) => new CellProfileAnalyzer(siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign, cellLiftBuilder)))

        // Register the factory for the CellProfileAnalyzer for summary volume cell profiles
        .Add(x => x.AddTransient<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, IDesign, IDesign, ICellLiftBuilder, ICellProfileAnalyzer<SummaryVolumeProfileCell>>>(
          factory => (siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign, referenceDesign, cellLiftBuilder) => new SummaryVolumesCellProfileAnalyzer(siteModel, pDExistenceMap, filterSet, cellPassFilter_ElevationRangeDesign, referenceDesign, cellLiftBuilder)))

        // Register a DI factory for ImmutableSpatialAffinityPartitionMap to represent an affinity partition map with just one partition
        .Add(x => x.AddSingleton<IImmutableSpatialAffinityPartitionMap>(mockImmutableSpatialAffinityPartitionMap.Object))

        .Add(x => x.AddTransient<IAlignments>(factory => new Alignments.Alignments()))
        .Add(x => x.AddTransient<IDesigns>(factory => new TRex.Designs.Storage.Designs()))

        .Add(TRex.ExistenceMaps.ExistenceMaps.AddExistenceMapFactoriesToDI)

        .Add(x => x.AddTransient<IFilterSet>(factory => new FilterSet()))

        .Complete();
    }

    /// <summary>
    /// Takes a list of TAG files and constructs an ephemeral site model that may be queried
    /// </summary>
    /// <param name="tagFiles"></param>
    /// <param name="ProcessedTasks"></param>
    /// <returns></returns>
    public static ISiteModel BuildModel(IEnumerable<string> tagFiles, out List<AggregatedDataIntegratorTask> ProcessedTasks)
    {
      var _tagFiles = tagFiles.ToList();

      // Convert TAG files using TAGFileConverters into mini-site models
      var converters = _tagFiles.Select(DITagFileFixture.ReadTAGFileFullPath).ToArray();

      // Create the site model and machine etc to aggregate the processed TAG file into
      ISiteModel targetSiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine targetMachine = targetSiteModel.Machines.CreateNew("Test Machine", "", MachineType.Dozer, DeviceType.SNM940, false, Guid.NewGuid());

      // Create the integrator and add the processed TAG file to its processing list
      AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();

      foreach (var c in converters)
      {
        c.Machine.ID = targetMachine.ID;
        integrator.AddTaskToProcessList(c.SiteModel, targetSiteModel.ID, c.Machine, targetMachine.ID, 
          c.SiteModelGridAggregator, c.ProcessedCellPassCount, c.MachineTargetValueChangesAggregator);
      }

      // Construct an integration worker and ask it to perform the integration
      ProcessedTasks = new List<AggregatedDataIntegratorTask>();
      AggregatedDataIntegratorWorker worker = new AggregatedDataIntegratorWorker(integrator.TasksToProcess)
      {
        MaxMappedTagFilesToProcessPerAggregationEpoch = _tagFiles.Count
      };
      worker.ProcessTask(ProcessedTasks);

      targetSiteModel.Should().NotBe(null);
      ProcessedTasks.Count.Should().Be(_tagFiles.Count);

      // Cause the latest cell pass information to be created for all sub grids
      targetSiteModel.Grid.Root.ScanSubGrids(targetSiteModel.Grid.FullCellExtent(),
        leaf =>
        {
          if (leaf is IServerLeafSubGrid Leaf)
            Leaf.ComputeLatestPassInformation(true, DIContext.Obtain<ISiteModels>().StorageProxy);
          return true;
        });

      return targetSiteModel;
    }

    public static void AddSingleCellWithPasses(ISiteModel siteModel, uint cellX, uint cellY, IEnumerable<CellPass> passes, int expectedCellCount, int expectedPasssCount)
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

      siteModel.Grid.CountLeafSubGridsInMemory().Should().Be(1);

      CellPass[] _passes = passes.ToArray();

      byte subGridX = (byte)(cellX & SubGridTreeConsts.SubGridLocalKeyMask);
      byte subGridY = (byte)(cellY & SubGridTreeConsts.SubGridLocalKeyMask);

      foreach (var pass in _passes)
        leaf.AddPass(subGridX, subGridY, pass);

      var cellPasses = leaf.Cells.PassesData[0].PassesData.ExtractCellPasses(subGridX, subGridY);
      cellPasses.Length.Should().Be(expectedPasssCount);

      // Assign global latest cell pass to the appropriate pass
      leaf.Directory.GlobalLatestCells[subGridX, subGridY] = cellPasses.Last();

      // Ensure the pass data existence map records the existence of a non null value in the cell
      leaf.Directory.GlobalLatestCells.PassDataExistenceMap[subGridX, subGridY] = true;

      // Count the number of non-null elevation cells to verify a correct setup
      long totalCells = 0;
      siteModel.Grid.Root.ScanSubGrids(siteModel.Grid.FullCellExtent(), x => {
        totalCells += leaf.Directory.GlobalLatestCells.PassDataExistenceMap.CountBits();
        return true;
      });

      totalCells.Should().Be(expectedCellCount);

      var siteModelExtent = siteModel.Grid.GetCellExtents(cellX, cellY);
      siteModel.SiteModelExtent.Set(siteModelExtent.MinX, siteModelExtent.MinY, siteModelExtent.MaxX, siteModelExtent.MaxY);

      // Save the site model metadata to preserve the site model extent information across a site model change notification event
      siteModel.SaveMetadataToPersistentStore(DIContext.Obtain<ISiteModels>().StorageProxy);
    }

    public new void Dispose()
    {
      base.Dispose();
    }
  }
}
