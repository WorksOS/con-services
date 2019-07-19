using System;
using FluentAssertions;
using Force.DeepCloner;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Pipelines
{
  public class RequestAnalyserTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_RequestAnalyser_Creation()
    {
      RequestAnalyser analyser = new RequestAnalyser();

      Assert.NotNull(analyser);
      Assert.NotNull(analyser.ProdDataMask);
      Assert.NotNull(analyser.SurveyedSurfaceOnlyMask);
    }

    [Fact]
    public void Test_RequestAnalyser_DefaultConstruction_Execute_Fails_Synchrous()
    {
      var analyser = new RequestAnalyser();

      Action act = () => analyser.Execute();
      act.Should().Throw<ArgumentException>().WithMessage("*No owning pipeline*");
    }

    [Fact]
    public void Test_RequestAnalyser_DefaultConstruction_Execute_Fails()
    {
      var analyser = new RequestAnalyser();

      Action act = () => analyser.Execute();
      act.Should().Throw<ArgumentException>().WithMessage("*No owning pipeline*");
    }

    [Fact]
    public void Test_RequestAnalyser_WithPipelineAndNoFilters_Execute_Fails()
    {
      var pipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(null)
      {
        FilterSet = null
      };

      RequestAnalyser analyser = new RequestAnalyser
      {
        Pipeline = pipeLine
      };

      Action act = () => analyser.Execute();
      act.Should().Throw<ArgumentException>().WithMessage("*No filters in pipeline*");
    }

    [Fact]
    public void Test_RequestAnalyser_WithPipelineAndNoProductionDataExistanceMap_Execute_Fails()
    {
      var pipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(null)
      {
        FilterSet = new FilterSet(new CombinedFilter()),
        ProdDataExistenceMap = null
      };

      RequestAnalyser analyser = new RequestAnalyser
      {
        Pipeline = pipeLine
      };

      Action act = () => analyser.Execute();
      act.Should().Throw<ArgumentException>().WithMessage("*Production Data Existence Map should have been specified*");
    }

    [Fact]
    public void Test_RequestAnalyser_DefaultConstruction_CountOfSubgridsThatWillBeSubmitted_Fails()
    {
      RequestAnalyser analyser = new RequestAnalyser();

      Assert.Throws<ArgumentException>(() => analyser.CountOfSubGridsThatWillBeSubmitted());
    }

    [Fact]
    public void Test_RequestAnalyser_NullOrInvertedSpatialExtents_YieldsNoSubGrids()
    {
      var prodDataExistenceMap = new SubGridTreeSubGridExistenceBitMask();
      var PipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(null)
      {
        FilterSet = new FilterSet(new CombinedFilter()),
        ProdDataExistenceMap = prodDataExistenceMap,
        OverallExistenceMap = prodDataExistenceMap
      };

      RequestAnalyser analyser = new RequestAnalyser(PipeLine, BoundingWorldExtent3D.Null());

      Assert.Equal(0, analyser.CountOfSubGridsThatWillBeSubmitted());

      analyser = new RequestAnalyser(PipeLine, BoundingWorldExtent3D.Inverted());

      Assert.Equal(0, analyser.CountOfSubGridsThatWillBeSubmitted());
    }

    [Fact]
    public void Test_RequestAnalyser_EmptyExistenceMap_YieldsNoSubGrids()
    {
      var prodDataExistenceMap = new SubGridTreeSubGridExistenceBitMask();
      var PipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(null)
      {
        FilterSet = new FilterSet(new CombinedFilter()),
        ProdDataExistenceMap = prodDataExistenceMap,
        OverallExistenceMap = prodDataExistenceMap
      };

      RequestAnalyser analyser = new RequestAnalyser(PipeLine, BoundingWorldExtent3D.Full());

      Assert.Equal(0, analyser.CountOfSubGridsThatWillBeSubmitted());
    }

    [Fact]
    public void Test_RequestAnalyser_SingleSubGridInExistenceMap_FullWorldExtents_YieldsOneSubGrid()
    {
      var prodDataExistenceMap = new SubGridTreeSubGridExistenceBitMask {[100, 100] = true};

      var PipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(null)
      {
        FilterSet = new FilterSet(new CombinedFilter()),
        ProdDataExistenceMap = prodDataExistenceMap,
        OverallExistenceMap = prodDataExistenceMap
      };

      RequestAnalyser analyser = new RequestAnalyser(PipeLine, BoundingWorldExtent3D.Full());

      Assert.Equal(1, analyser.CountOfSubGridsThatWillBeSubmitted());
    }

    [Fact]
    public void Test_RequestAnalyser_SingleSubGridInExistenceMap_IntersectingFilterRestriction_YieldsOneSubGrid()
    {
      var prodDataExistenceMap = new SubGridTreeSubGridExistenceBitMask();
      prodDataExistenceMap.CalculateIndexOfCellContainingPosition(50, 50, out int cellX, out int cellY);
      prodDataExistenceMap[cellX, cellY] = true;

      var PipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(null)
      {
        FilterSet = new FilterSet(new CombinedFilter {SpatialFilter = {IsSpatial = true, Fence = new Fence(0, 0, 100, 100)}}),
        ProdDataExistenceMap = prodDataExistenceMap,
        OverallExistenceMap = prodDataExistenceMap
      };

      RequestAnalyser analyser = new RequestAnalyser(PipeLine, new BoundingWorldExtent3D(0, 0, 1000, 1000));

      Assert.Equal(1, analyser.CountOfSubGridsThatWillBeSubmitted());
    }

    [Fact]
    public void Test_RequestAnalyser_SingleSubGridInExistenceMap_NonIntersectingFilterRestriction_YieldsNoSubGrids()
    {
      var prodDataExistenceMap = new SubGridTreeSubGridExistenceBitMask();
      prodDataExistenceMap.CalculateIndexOfCellContainingPosition(1000, 1000, out int cellX, out int cellY);
      prodDataExistenceMap[cellX, cellY] = true;

      var PipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(null)
      {
        FilterSet = new FilterSet(new CombinedFilter {SpatialFilter = {IsSpatial = true, Fence = new Fence(0, 0, 100, 100)}}),
        ProdDataExistenceMap = prodDataExistenceMap,
        OverallExistenceMap = prodDataExistenceMap
      };

      RequestAnalyser analyser = new RequestAnalyser(PipeLine, BoundingWorldExtent3D.Inverted());

      Assert.Equal(0, analyser.CountOfSubGridsThatWillBeSubmitted());
    }

    [Fact]
    public void Test_RequestAnalyser_ManySubGridsInExistenceMap_CountIsCorrect()
    {
      var prodDataExistenceMap = new SubGridTreeSubGridExistenceBitMask();

      for (int i = 0; i < 100; i++)
      for (int j = 0; j < 100; j++)
        prodDataExistenceMap[i, j] = true;

      var PipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(null)
      {
        FilterSet = new FilterSet(new CombinedFilter()),
        ProdDataExistenceMap = prodDataExistenceMap,
        OverallExistenceMap = prodDataExistenceMap
      };

      RequestAnalyser analyser = new RequestAnalyser(PipeLine, BoundingWorldExtent3D.Inverted());

      Assert.Equal(10000, analyser.CountOfSubGridsThatWillBeSubmitted());
    }

    [Theory]
    [InlineData(0, 10, 10, 10)]
    [InlineData(999, 10, 10, 10)]
    [InlineData(1000, 10, 0, 0)]
    [InlineData(1001, 10, 0, 0)]
    public void Test_RequestAnalyser_ManySubGridsInExistenceMap_CountOfSubgridsInPageRequestIsCorrect(int pageNumber, int pageSize, int expectedCount, int numberInRequest)
    {
      var prodDataExistenceMap = new SubGridTreeSubGridExistenceBitMask();

      for (int i = 0; i < 100; i++)
      for (int j = 0; j < 100; j++)
        prodDataExistenceMap[i, j] = true;

      var PipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(null)
      {
        FilterSet = new FilterSet(new CombinedFilter()),
        ProdDataExistenceMap = prodDataExistenceMap,
        OverallExistenceMap = prodDataExistenceMap
      };

      RequestAnalyser analyser = new RequestAnalyser(PipeLine, BoundingWorldExtent3D.Inverted())
      {
        SubmitSinglePageOfRequests = true,
        SinglePageRequestNumber = pageNumber,
        SinglePageRequestSize = pageSize
      };

      Assert.True(expectedCount == analyser.CountOfSubGridsThatWillBeSubmitted(),$"CountOfSubGridsThatWillBeSubmitted() not {expectedCount}, = {analyser.CountOfSubGridsThatWillBeSubmitted()}");
      Assert.True(numberInRequest == analyser.TotalNumberOfSubGridsToRequest, $"analyser.TotalNumberOfSubGridsToRequest not {numberInRequest}, = {analyser.TotalNumberOfSubGridsToRequest}");
    }
  }
}
