using System;
using System.Linq;
using Apache.Ignite.Core.Compute;
using FluentAssertions;
using VSS.TRex.Analytics.CCAStatistics;
using VSS.TRex.Analytics.CCAStatistics.GridFabric;
using VSS.TRex.Cells;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CCAStatistics.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(CCAStatisticsRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(CCAStatisticsRequest_ClusterCompute))]
  public class CCAStatisticsRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private ISiteModel NewEmptyModel()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      _ = siteModel.Machines.CreateNew("Bulldozer", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      return siteModel;
    }

    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<IComputeFunc<CCAStatisticsArgument, CCAStatisticsResponse>, CCAStatisticsArgument, CCAStatisticsResponse>();

    private void AddClusterComputeGridRouting() => IgniteMock.AddClusterComputeGridRouting<IComputeFunc<CCAStatisticsArgument, CCAStatisticsResponse>, CCAStatisticsArgument, CCAStatisticsResponse>();

    private CCAStatisticsArgument SimpleCCAStatisticsArgument(ISiteModel siteModel, ushort targetMin, ushort targetMax)
    {
      return new CCAStatisticsArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter())
      };
    }

    private void BuildModelForSingleCellCCA(out ISiteModel siteModel, float heightIncrement)
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
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
    }

    [Fact]
    public void Test_SummaryCCAStatistics_Creation()
    {
      var operation = new CCAStatisticsOperation();

      operation.Should().NotBeNull();
    }
  }
}
