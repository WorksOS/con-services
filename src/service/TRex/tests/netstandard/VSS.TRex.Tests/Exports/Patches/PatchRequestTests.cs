using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Cells;
using VSS.TRex.Designs.Models;
using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Exports.Patches
{
  [UnitTestCoveredRequest(RequestType = typeof(PatchRequest))]
  public class PatchRequestTests: IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<PatchRequestComputeFunc, PatchRequestArgument, PatchRequestResponse>();

    private void AddClusterComputeGridRouting() => IgniteMock.AddClusterComputeGridRouting<SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();

    [Fact]
    public void Test_PatchRequest_Creation()
    {
      var request = new PatchRequest();

      request.Should().NotBeNull();
    }

    private void BuildModelForSingleCellPatch(out ISiteModel siteModel, float heightIncrement)
    {
      var baseTime = DateTime.UtcNow;
      var baseHeight = 1.0f;

      siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = baseHeight + x * heightIncrement,
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);
    }

    private PatchRequestArgument SimplePatchRequestArgument(Guid projectUid)
    {
      return new PatchRequestArgument
      {
        DataPatchNumber = 0,
        DataPatchSize = 100,
        Filters = new FilterSet(new CombinedFilter()),
        Mode = DisplayMode.Height,
        ProjectID = projectUid,
        ReferenceDesign = new DesignOffset(),
        TRexNodeID = "'Test_PatchRequest_Execute_EmptySiteModel"
      };
    }

    [Fact]
    public void Test_PatchRequest_Execute_EmptySiteModel()
    {
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new PatchRequest();

      var response = request.Execute(SimplePatchRequestArgument(siteModel.ID));

      response.Should().NotBeNull();
      response.SubGrids.Should().BeNull();
    }

    [Fact]
    public void Test_PatchRequest_Execute_SingleTAGFileSiteModel()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var request = new PatchRequest();
      var response = request.Execute(SimplePatchRequestArgument(siteModel.ID));

      response.Should().NotBeNull();
      response.SubGrids.Should().NotBeNull();
      response.SubGrids.Count.Should().Be(12);

      response.SubGrids.ForEach(x => x.Should().BeOfType<ClientHeightAndTimeLeafSubGrid>());

      int nonNullCellCount = 0;
      response.SubGrids.ForEach(x => nonNullCellCount += ((ClientHeightAndTimeLeafSubGrid)x).CountNonNullCells());
      nonNullCellCount.Should().Be(3054);
    }

    [Fact]
    public void Test_PatchRequest_Execute_SingleCellSiteModel()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      BuildModelForSingleCellPatch(out var siteModel, 0.5f);

      var request = new PatchRequest();
      var response = request.Execute(SimplePatchRequestArgument(siteModel.ID));

      response.Should().NotBeNull();
      response.SubGrids.Should().NotBeNull();
      response.SubGrids.Count.Should().Be(1);
      response.SubGrids[0].CountNonNullCells().Should().Be(1);
      response.SubGrids[0].Should().BeOfType<ClientHeightAndTimeLeafSubGrid>();
      ((ClientHeightAndTimeLeafSubGrid)response.SubGrids[0]).Cells[0, 0].Should().BeApproximately(5.5f, 0.000001f);
    }

    [Fact]
    public void ExecuteAndConvertToResult()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      BuildModelForSingleCellPatch(out var siteModel, 0.5f);

      var request = new PatchRequest();
      var result = request.ExecuteAndConvertToResult(SimplePatchRequestArgument(siteModel.ID));

      result.Should().NotBeNull();
      result.Patch.Should().NotBeNull();
      result.Patch.Length.Should().Be(1);

      result.Patch[0].ElevationOrigin.Should().Be(5.5f);
      result.Patch[0].Data[0, 0].ElevationOffset.Should().Be(0);
    }

    [Fact]
    public void PatchResult_ConstructResultData()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      BuildModelForSingleCellPatch(out var siteModel, 0.5f);

      var request = new PatchRequest();
      var result = request.ExecuteAndConvertToResult(SimplePatchRequestArgument(siteModel.ID));

      var bytes = result.ConstructResultData();
      bytes.Should().NotBeNull();
    }
  }
}
