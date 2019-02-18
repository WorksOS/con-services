using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Cells;
using VSS.TRex.DI;
using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Exports.Patches
{
  public class PatchRequestTests: IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    public ISiteModel NewEmptyModel()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      _ = siteModel.Machines.CreateNew("Bulldozer", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      return siteModel;
    }

    private void AddApplicationGridRouting() => DITAGFileAndSubGridRequestsWithIgniteFixture.AddApplicationGridRouting<PatchRequestComputeFunc, PatchRequestArgument, PatchRequestResponse>();

    private void AddClusterComputeGridRouting() => DITAGFileAndSubGridRequestsWithIgniteFixture.AddClusterComputeGridRouting<SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();

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

      siteModel = NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = baseHeight + x * heightIncrement,
          PassType = PassType.Front
        });

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Count());
    }

    private PatchRequestArgument SimplePatchRequestArgument(Guid projectUid)
    {
      return new PatchRequestArgument
      {
        DataPatchNumber = 0,
        DataPatchSize = 10,
        Filters = new FilterSet(new CombinedFilter()),
        Mode = DisplayMode.Height,
        ProjectID = projectUid,
        ReferenceDesignUID = Guid.Empty,
        TRexNodeID = "'Test_PatchRequest_Execute_EmptySiteModel"
      };
    }

    [Fact]
    public void Test_PatchRequest_Execute_EmptySiteModel()
    {
      AddApplicationGridRouting();

      var siteModel = NewEmptyModel();
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

      var corePath = Path.Combine("TestData", "Export", "Patches");
      var tagFiles = new[]
      {
        Path.Combine(corePath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var request = new PatchRequest();
      var response = request.Execute(SimplePatchRequestArgument(siteModel.ID));

      response.Should().NotBeNull();
      response.SubGrids.Should().NotBeNull();
      response.SubGrids.Count.Should().Be(10);
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
    }
  }
}
