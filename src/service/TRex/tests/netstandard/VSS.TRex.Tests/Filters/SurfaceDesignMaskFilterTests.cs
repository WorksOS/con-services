using System;
using System.Linq;
using Apache.Ignite.Core.Compute;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Filters
{
  public class SurfaceDesignMaskFilterTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    protected void AddDesignProfilerGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting
      <CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();

    private void SetupTestIgniteRouting()
    {
      AddDesignProfilerGridRouting();
    }

    private (ISiteModel, CombinedFilter) CreateSiteModelWithSimpleDesign()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      // Create a simple design in the model to ask as the mask
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutSingleCellPoint
        (ref siteModel, 0.0f, siteModel.CellSize / 2, siteModel.CellSize / 2, siteModel.CellSize / 3);

      var filter = new CombinedFilter
      {
        SpatialFilter = new CellSpatialFilter
        {
          SurfaceDesignMaskDesignUid = designUid,
          IsDesignMask = true
        }
      };

      return (siteModel, filter);
    }

    [Fact]
    public void Creation()
    {
      var (siteModel, filter) = CreateSiteModelWithSimpleDesign();

      siteModel.Should().NotBeNull();
      filter.Should().NotBeNull();

      filter.SpatialFilter.HasSurfaceDesignMask().Should().BeTrue();
      filter.SpatialFilter.IsDesignMask.Should().BeTrue();
    }

    [Fact]
    public async void MasksOutValues_WithoutSurveyedSurfaces()
    {
      SetupTestIgniteRouting();

      var (siteModel, filter) = CreateSiteModelWithSimpleDesign();

      // Create a sub grid at the Northwest origin so that it covers the small TIN design surrounding the
      // [CellSize / 2, CellSize / 2] point

      var baseTime = DateTime.UtcNow;
      var cellPasses = new CellPass[32, 32][];

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        cellPasses[x, y] = Enumerable.Range(0, 1).Select(p =>
          new CellPass
          {
            Height = 1.0f,
            InternalSiteModelMachineIndex = siteModel.Machines[0].InternalSiteModelMachineIndex,
            Time = baseTime.AddMinutes(p),
            PassType = PassType.Front
          }).ToArray();
      });

      DITAGFileAndSubGridRequestsFixture.AddSingleSubGridWithPasses(siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);

      // Construct a requestor and ask it to retrieve the sub grid from the site model, using the filter 
      // with the surface design mask

      var utilities = DIContext.Obtain<IRequestorUtilities>();
      var requestors = utilities.ConstructRequestors(null, siteModel,
        new OverrideParameters(),
        new LiftParameters(),
        utilities.ConstructRequestorIntermediaries(siteModel, 
          new FilterSet(filter), false, GridDataType.Height),
        AreaControlSet.CreateAreaControlSet(),
        siteModel.ExistenceMap);

      requestors.Length.Should().Be(1);

      var response = await requestors[0].RequestSubGridInternal
        (new SubGridCellAddress(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset),
        true, false);

      response.requestResult.Should().Be(ServerRequestResult.NoError);
      response.clientGrid.Should().NotBeNull();

      // Ensure the filtered cell has data
      response.clientGrid.FilterMap[0, 0].Should().BeTrue();
      (response.clientGrid as IClientHeightLeafSubGrid).Cells[0, 0].Should().Be(1.0f);

      // Ensure no other cells have data
      response.clientGrid.FilterMap.CountBits().Should().Be(1);
      var subGrid = response.clientGrid as IClientHeightLeafSubGrid;
      var count = 0;
      subGrid.ForEach((x, y) => count += subGrid.Cells[x, y] == 1.0f ? 1 : 0);
      count.Should().Be(1);
    }
  }
}
