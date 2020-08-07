using System;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGrids
{
  public class SubGridRequestsWithIntendedCachingTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    private ISiteModel BuildModelForSubGridRequest()
    {
      var baseTime = DateTime.UtcNow;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      CellPass[,][] cellPasses = new CellPass[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension][];

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        cellPasses[x, y] = Enumerable.Range(0, 1).Select(p =>
          new CellPass
          {
            Height = 1 + x + y,
            InternalSiteModelMachineIndex = bulldozerMachineIndex,
            Time = baseTime.AddMinutes(p),
            PassType = PassType.Front
          }).ToArray();
      });

      DITAGFileAndSubGridRequestsFixture.AddSingleSubGridWithPasses(siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);

      return siteModel;
    }

    [Fact]
    public void SubGridForCachine_IgnoresFilterMask_WithNoOverrideMaskRetriction()
    {
      var siteModel = BuildModelForSubGridRequest();

      var retriever = new SubGridRetriever(siteModel,
        GridDataType.Height,
        siteModel.PrimaryStorageProxy,
        new CombinedFilter(),
        new CellPassAttributeFilterProcessingAnnex(),
        false, // Has override mask
        BoundingIntegerExtent2D.Inverted(),
        true, // prepareGridForCacheStorageIfNoSieving
        1000,
        AreaControlSet.CreateAreaControlSet(),
        new FilteredValuePopulationControl(),
        siteModel.ExistenceMap,
        new OverrideParameters(),
        new LiftParameters()
        );

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height);

      var result = retriever.RetrieveSubGrid(clientGrid, SubGridTreeBitmapSubGridBits.FullMask, out var seiveFilterInUse,
        () => 
        {
          clientGrid.FilterMap.Clear();
          clientGrid.FilterMap[0, 0] = true;

          clientGrid.ProdDataMap.Fill();

          return ServerRequestResult.NoError; 
        });

      result.Should().Be(ServerRequestResult.NoError);
      seiveFilterInUse.Should().BeFalse();
      clientGrid.CountNonNullCells().Should().Be(1);
    }

    [Fact]
    public void SubGridForCachine_IgnoresFilterMask_WithFullOverrideMaskRetriction()
    {
      var siteModel = BuildModelForSubGridRequest();

      var retriever = new SubGridRetriever(siteModel,
        GridDataType.Height,
        siteModel.PrimaryStorageProxy,
        new CombinedFilter(),
        new CellPassAttributeFilterProcessingAnnex(),
        true, // Has override mask
        BoundingIntegerExtent2D.Inverted(),
        true, // prepareGridForCacheStorageIfNoSieving
        1000,
        AreaControlSet.CreateAreaControlSet(),
        new FilteredValuePopulationControl(),
        siteModel.ExistenceMap,
        new OverrideParameters(),
        new LiftParameters()
        );

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height);

      var result = retriever.RetrieveSubGrid(clientGrid, SubGridTreeBitmapSubGridBits.FullMask, out var seiveFilterInUse,
        () =>
        {
          clientGrid.FilterMap.Clear();
          clientGrid.FilterMap[0, 0] = true;

          clientGrid.ProdDataMap.Fill();

          return ServerRequestResult.NoError;
        });

      result.Should().Be(ServerRequestResult.NoError);
      seiveFilterInUse.Should().BeFalse();
      clientGrid.CountNonNullCells().Should().Be(1);
    }


    [Fact]
    public void SubGridForCachine_IgnoresFilterMask_WithPartialNonOverlappingOverrideMaskRetriction()
    {
      var siteModel = BuildModelForSubGridRequest();

      var retriever = new SubGridRetriever(siteModel,
        GridDataType.Height,
        siteModel.PrimaryStorageProxy,
        new CombinedFilter(),
        new CellPassAttributeFilterProcessingAnnex(),
        true, // Has override mask
        BoundingIntegerExtent2D.Inverted(),
        true, // prepareGridForCacheStorageIfNoSieving
        1000,
        AreaControlSet.CreateAreaControlSet(),
        new FilteredValuePopulationControl(),
        siteModel.ExistenceMap,
        new OverrideParameters(),
        new LiftParameters()
        );

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height);

      var overrideMask = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
      overrideMask[10, 10] = true; // This does not overlap the filter but should still return a result

      var result = retriever.RetrieveSubGrid(clientGrid, overrideMask, out var seiveFilterInUse,
        () =>
        {
          clientGrid.FilterMap.Clear();
          clientGrid.FilterMap[0, 0] = true;

          clientGrid.ProdDataMap.Fill();

          return ServerRequestResult.NoError;
        });

      result.Should().Be(ServerRequestResult.NoError);
      seiveFilterInUse.Should().BeFalse();
      clientGrid.CountNonNullCells().Should().Be(1);
    }
  }
}
