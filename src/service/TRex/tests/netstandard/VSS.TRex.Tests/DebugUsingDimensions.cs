using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using VSS.TRex.Analytics.CutFillStatistics;
using VSS.TRex.Analytics.CutFillStatistics.GridFabric;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests
{
  /// <summary>
  /// Example setting up a 'unit test' using real Dimensions tag file data and Large Sites Road design so can debug TRex.
  /// This example is for cut-fill statistics.
  /// </summary>
  [UnitTestCoveredRequest(RequestType = typeof(CutFillStatisticsRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(CutFillStatisticsRequest_ClusterCompute))]
  public class DebugUsingDimensions : BaseTests<CutFillStatisticsArgument, CutFillStatisticsResponse>,
    IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {

    private void AddDesignProfilerGridRouting()
    {
      IgniteMock.Immutable.AddApplicationGridRouting<IComputeFunc<CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();
    }

    private CutFillStatisticsArgument SimpleCutFillStatisticsArgument(ISiteModel siteModel, Guid designUid, double offset)
    {
      return new CutFillStatisticsArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        ReferenceDesign = new DesignOffset(designUid, offset),
        Offsets = new double[0]
      };
    }

    [Fact (Skip = "Not a real test. Use for debugging")]
    public async Task DebugCutFillStatistics()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();
      AddDesignProfilerGridRouting();
      var tagFiles = Directory.GetFiles(Path.Combine("TestData", "TAGFiles", "Dimensions-Machine 4250986182719752"), "*.tag").ToArray();
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, Path.Combine("TestData", "Common"), "Large Sites Road - Trimble Road.ttm", false);

      var operation = new CutFillStatisticsOperation();
      var argument = SimpleCutFillStatisticsArgument(siteModel, designUid, 0);
      argument.Offsets = new[] { 0.5, 0.2, 0.1, 0.0, -0.1, -0.2, -0.5 };
      var result = await operation.ExecuteAsync(argument);
    }
  }
}
