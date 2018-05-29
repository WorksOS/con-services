using System;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Tests.netcore.TestFixtures;
using Xunit;

namespace RaptorClassLibrary.Tests.netcore.Pipelines
{
  public class RequestAnalyserTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_RequestAnalyser_Creation()
    {
      RequestAnalyser analyser = new RequestAnalyser();

      Assert.NotNull(analyser);
      Assert.NotNull(analyser.ProdDataMask);
      Assert.NotNull(analyser.SurveydSurfaceOnlyMask);
    }

    [Fact]
    public void Test_RequestAnalyser_DefaultConstruction_Execute_Fails()
    {
      RequestAnalyser analyser = new RequestAnalyser();

      Assert.Throws<ArgumentException>(() => analyser.Execute());
    }

    [Fact]
    public void Test_RequestAnalyser_DefaultConstruction_CountOfSubgridsThatWillBeSubmitted_Fails()
    {
      RequestAnalyser analyser = new RequestAnalyser();

      Assert.Throws<ArgumentException>(() => analyser.CountOfSubgridsThatWillBeSubmitted());
    }

    [Fact]
    public void Test_RequestAnalyser_NullOrInvertedSpatialExtents_YieldsNoSubGrids()
    {
      var prodDataExistenceMap = new SubGridTreeSubGridExistenceBitMask();
      var PipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(null)
      {
        FilterSet = new FilterSet(new[] {new CombinedFilter()}),
        ProdDataExistenceMap = prodDataExistenceMap,
        OverallExistenceMap = prodDataExistenceMap
      };

      RequestAnalyser analyser = new RequestAnalyser(PipeLine, BoundingWorldExtent3D.Null());

      Assert.Equal(0, analyser.CountOfSubgridsThatWillBeSubmitted());

      analyser = new RequestAnalyser(PipeLine, BoundingWorldExtent3D.Inverted());

      Assert.Equal(0, analyser.CountOfSubgridsThatWillBeSubmitted());
    }

    [Fact]
    public void Test_RequestAnalyser_EmptyExistenceMap_YieldsNoSubGrids()
    {
      var prodDataExistenceMap = new SubGridTreeSubGridExistenceBitMask();
      var PipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(null)
      {
        FilterSet = new FilterSet(new[] {new CombinedFilter()}),
        ProdDataExistenceMap = prodDataExistenceMap,
        OverallExistenceMap = prodDataExistenceMap
      };

      RequestAnalyser analyser = new RequestAnalyser(PipeLine, BoundingWorldExtent3D.Full());

      Assert.Equal(0, analyser.CountOfSubgridsThatWillBeSubmitted());
    }

    [Fact]
    public void Test_RequestAnalyser_SingleSubGridInExistenceMap_FullWorldExtents_YieldsOneSubGrid()
    {
      var prodDataExistenceMap = new SubGridTreeSubGridExistenceBitMask {[100, 100] = true};

      var PipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(null)
      {
        FilterSet = new FilterSet(new[] {new CombinedFilter()}),
        ProdDataExistenceMap = prodDataExistenceMap,
        OverallExistenceMap = prodDataExistenceMap
      };

      RequestAnalyser analyser = new RequestAnalyser(PipeLine, BoundingWorldExtent3D.Full());

      Assert.Equal(1, analyser.CountOfSubgridsThatWillBeSubmitted());
    }

    [Fact]
    public void Test_RequestAnalyser_SingleSubGridInExistenceMap_IntersectingFilterRestriction_YieldsOneSubGrid()
    {
      var prodDataExistenceMap = new SubGridTreeSubGridExistenceBitMask();
      prodDataExistenceMap.CalculateIndexOfCellContainingPosition(50, 50, out uint cellX, out uint cellY);
      prodDataExistenceMap[cellX, cellY] = true;

      var PipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(null)
      {
        FilterSet = new FilterSet(new[]
          {new CombinedFilter {SpatialFilter = {IsSpatial = true, Fence = new Fence(0, 0, 100, 100)}}}),
        ProdDataExistenceMap = prodDataExistenceMap,
        OverallExistenceMap = prodDataExistenceMap
      };

      RequestAnalyser analyser = new RequestAnalyser(PipeLine, new BoundingWorldExtent3D(0, 0, 1000, 1000));

      Assert.Equal(1, analyser.CountOfSubgridsThatWillBeSubmitted());
    }

    [Fact]
    public void Test_RequestAnalyser_SingleSubGridInExistenceMap_NonIntersectingFilterRestriction_YieldsNoSubGrids()
    {
      var prodDataExistenceMap = new SubGridTreeSubGridExistenceBitMask();
      prodDataExistenceMap.CalculateIndexOfCellContainingPosition(1000, 1000, out uint cellX, out uint cellY);
      prodDataExistenceMap[cellX, cellY] = true;

      var PipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(null)
      {
        FilterSet = new FilterSet(new[]
          {new CombinedFilter {SpatialFilter = {IsSpatial = true, Fence = new Fence(0, 0, 100, 100)}}}),
        ProdDataExistenceMap = prodDataExistenceMap,
        OverallExistenceMap = prodDataExistenceMap
      };

      RequestAnalyser analyser = new RequestAnalyser(PipeLine, BoundingWorldExtent3D.Inverted());

      Assert.Equal(0, analyser.CountOfSubgridsThatWillBeSubmitted());
    }

    [Fact]
    public void Test_RequestAnalyser_ManySubGridsInExistenceMap_CountIsCorrect()
    {
      var prodDataExistenceMap = new SubGridTreeSubGridExistenceBitMask();

      for (uint i = 0; i < 100; i++)
      for (uint j = 0; j < 100; j++)
        prodDataExistenceMap[i, j] = true;

      var PipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(null)
      {
        FilterSet = new FilterSet(new[] {new CombinedFilter()}),
        ProdDataExistenceMap = prodDataExistenceMap,
        OverallExistenceMap = prodDataExistenceMap
      };

      RequestAnalyser analyser = new RequestAnalyser(PipeLine, BoundingWorldExtent3D.Inverted());

      Assert.Equal(10000, analyser.CountOfSubgridsThatWillBeSubmitted());
    }

    [Theory]
    [InlineData(0, 10, 10, 10)]
    [InlineData(999, 10, 10, 10)]
    [InlineData(1000, 10, 0, 0)]
    [InlineData(1001, 10, 0, 0)]
    public void Test_RequestAnalyser_ManySubGridsInExistenceMap_CountOfSubgridsInPageRequestIsCorrect(int pageNumber, int pageSize, int expectedCount, int numberInRequest)
    {
      var prodDataExistenceMap = new SubGridTreeSubGridExistenceBitMask();

      for (uint i = 0; i < 100; i++)
      for (uint j = 0; j < 100; j++)
        prodDataExistenceMap[i, j] = true;

      var PipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(null)
      {
        FilterSet = new FilterSet(new[] {new CombinedFilter()}),
        ProdDataExistenceMap = prodDataExistenceMap,
        OverallExistenceMap = prodDataExistenceMap
      };

      RequestAnalyser analyser = new RequestAnalyser(PipeLine, BoundingWorldExtent3D.Inverted())
      {
        SubmitSinglePageOfRequests = true,
        SinglePageRequestNumber = pageNumber,
        SinglePageRequestSize = pageSize
      };

      Assert.True(expectedCount == analyser.CountOfSubgridsThatWillBeSubmitted(),$"CountOfSubgridsThatWillBeSubmitted() not {expectedCount}, = {analyser.CountOfSubgridsThatWillBeSubmitted()}");
      Assert.True(numberInRequest == analyser.TotalNumberOfSubgridsToRequest, $"analyser.TotalNumberOfSubgridsToRequest not {numberInRequest}, = {analyser.TotalNumberOfSubgridsToRequest}");
    }
  }
}
