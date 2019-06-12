using System;
using System.Linq;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.ComputeFuncs;
using VSS.TRex.CellDatum.GridFabric.Requests;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;
using Consts = VSS.TRex.Common.Consts;

namespace VSS.TRex.Tests.CellDatum.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(CellDatumRequest_ClusterCompute))]
  [UnitTestCoveredRequest(RequestType = typeof(CellDatumRequest_ApplicationService))]
  public class CellDatumRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private const string DIMENSIONS_2012_DC_CSIB = "QM0G000ZHC4000000000800BY7SN2W0EYST640036P3P1SV09C1G61CZZKJC976CNB295K7W7G30DA30A1N74ZJH1831E5V0CHJ60W295GMWT3E95154T3A85H5CRK9D94PJM1P9Q6R30E1C1E4Q173W9XDE923XGGHN8JR37B6RESPQ3ZHWW6YV5PFDGCTZYPWDSJEFE1G2THV3VAZVN28ECXY7ZNBYANFEG452TZZ3X2Q1GCYM8EWCRVGKWD5KANKTXA1MV0YWKRBKBAZYVXXJRM70WKCN2X1CX96TVXKFRW92YJBT5ZCFSVM37ZD5HKVFYYYMJVS05KA6TXFY6ZE4H6NQX8J3VAX79TTF82VPSV1KVR8W9V7BM1N3MEY5QHACSFNCK7VWPNY52RXGC1G9BPBS1QWA7ZVM6T2E0WMDY7P6CXJ68RB4CHJCDSVR6000047S29YVT08000";

    private void AddDesignProfilerGridRouting()
    {
      //This is specific to cell datum i.e. what the cell datum cluster compute will call in the design profiler
      IgniteMock.AddApplicationGridRouting<CalculateDesignElevationSpotComputeFunc, CalculateDesignElevationSpotArgument, double>();
    }

    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<CellDatumRequestComputeFunc_ApplicationService, CellDatumRequestArgument_ApplicationService, CellDatumResponse_ApplicationService>();

    private void AddClusterComputeGridRouting() => IgniteMock.AddClusterComputeSpatialAffinityGridRouting<CellDatumRequestComputeFunc_ClusterCompute, CellDatumRequestArgument_ClusterCompute, CellDatumResponse_ClusterCompute>();

  
    private ISiteModel BuildModelForSingleCellDatum(DateTime baseTime, bool heightOnly = false)
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;
      siteModel.MachinesTargetValues[bulldozerMachineIndex].VibrationStateEvents.PutValueAtDate(Consts.MIN_DATETIME_AS_UTC, VibrationState.On);

      var cellPasses = Enumerable.Range(1, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = 1.0f + x * 0.5f,
          PassType = PassType.Front,
          CCV = heightOnly ? CellPassConsts.NullCCV : (short)(10 + 10 * x),
          MachineSpeed = heightOnly ? Consts.NullMachineSpeed : (ushort)(650 + x),
          MDP = heightOnly ? CellPassConsts.NullMDP : (short)(20 + 20 * x),
          MaterialTemperature = heightOnly ? CellPassConsts.NullMaterialTemperatureValue : (ushort)(1000 + x)
        }).ToArray();

      //The subgrid tree extents are 1 << 30 or ~ 1 billion.
      //The default origin offset (SubGridTreeConsts.DefaultIndexOriginOffset) is ~500 million.
      //So we are placing the cell at the world origin (N/E) and default cell size of 0.34 metres
      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      //Add the machine targets for summaries
      var minUTCDate = Consts.MIN_DATETIME_AS_UTC;
      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetPassCountStateEvents.PutValueAtDate(minUTCDate, 10);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetCCVStateEvents.PutValueAtDate(minUTCDate, 220);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetMDPStateEvents.PutValueAtDate(minUTCDate, 880);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetMinMaterialTemperature.PutValueAtDate(minUTCDate, 900);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetMaxMaterialTemperature.PutValueAtDate(minUTCDate, 1200);

      return siteModel;
    }

    #region Cluster Compute
    private CellDatumRequestArgument_ClusterCompute CreateCellDatumRequestArgument_ClusterCompute(ISiteModel siteModel, DesignOffset referenceDesign, DisplayMode mode)
    {
      //The single cell is at world origin
      var coords = new XYZ(0.1, 0.1);
      siteModel.Grid.CalculateIndexOfCellContainingPosition(coords.X, coords.Y, out int OTGCellX, out int OTGCellY);

      return new CellDatumRequestArgument_ClusterCompute
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        Mode = mode,
        NEECoords = coords,
        OTGCellX = OTGCellX,
        OTGCellY = OTGCellY,
        ReferenceDesign = referenceDesign
      };
    }

    [Fact]
    public void Test_CellDatumRequest_ClusterCompute_Creation()
    {
      var request = new CellDatumRequest_ClusterCompute();
      request.Should().NotBeNull();
    }

    [Fact]
    public void Test_CellDatumRequest_ClusterCompute_Execute_EmptySiteModel()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CellDatumRequest_ClusterCompute();

      var response = request.Execute(CreateCellDatumRequestArgument_ClusterCompute(siteModel, new DesignOffset(), DisplayMode.Height), new SubGridSpatialAffinityKey());

      response.Should().NotBeNull();
      Assert.Equal(CellDatumReturnCode.NoValueFound, response.ReturnCode);
    }

    [Theory]
    [InlineData(DisplayMode.PassCount, 10)]
    [InlineData(DisplayMode.PassCountSummary, 100.0)]
    [InlineData(DisplayMode.CCV, 110)]
    [InlineData(DisplayMode.CCVPercent, 50.0)]
    [InlineData(DisplayMode.CCVSummary, 50.0)]
    [InlineData(DisplayMode.CCVPercentSummary, 50.0)]
    [InlineData(DisplayMode.CCVPercentChange, 10.0)]
    [InlineData(DisplayMode.MDP, 220)]
    [InlineData(DisplayMode.MDPPercent, 25.0)]
    [InlineData(DisplayMode.MDPPercentSummary, 25.0)]
    [InlineData(DisplayMode.MDPSummary, 25.0)]
    [InlineData(DisplayMode.Height, 6.0)]
    [InlineData(DisplayMode.TemperatureDetail, 101.0)]
    [InlineData(DisplayMode.TemperatureSummary, 101.0)]
    [InlineData(DisplayMode.MachineSpeed, 660)]
    [InlineData(DisplayMode.CutFill, 3.5)]//1.5 offset from 5
    public void Test_CellDatumRequest_ClusterCompute_Execute_SingleCellSiteModelLastPass(DisplayMode mode, double expectedValue)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();

      var baseTime = DateTime.UtcNow;
      var siteModel = BuildModelForSingleCellDatum(baseTime);
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutOrigin(ref siteModel, 1.0f);
      var referenceDesign = new DesignOffset(designUid, 1.5);
      var request = new CellDatumRequest_ClusterCompute();
      var arg = CreateCellDatumRequestArgument_ClusterCompute(siteModel, referenceDesign, mode);
      var response = request.Execute(arg, new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER, arg.ProjectID, arg.OTGCellX, arg.OTGCellY));

      response.Should().NotBeNull();
      Assert.Equal(CellDatumReturnCode.ValueFound, response.ReturnCode);
      Assert.Equal(expectedValue, response.Value);
      Assert.Equal(baseTime.AddMinutes(10), response.TimeStampUTC);
    }

    [Theory]
    [InlineData(DisplayMode.PassCount)]
    [InlineData(DisplayMode.PassCountSummary)]
    [InlineData(DisplayMode.CCV)]
    [InlineData(DisplayMode.CCVPercent)]
    [InlineData(DisplayMode.CCVPercentSummary)]
    [InlineData(DisplayMode.CCVPercentChange)]
    [InlineData(DisplayMode.MDP)]
    [InlineData(DisplayMode.MDPPercent)]
    [InlineData(DisplayMode.MDPPercentSummary)]
    [InlineData(DisplayMode.MDPSummary)]
    [InlineData(DisplayMode.Height)]
    [InlineData(DisplayMode.TemperatureDetail)]
    [InlineData(DisplayMode.TemperatureSummary)]
    [InlineData(DisplayMode.MachineSpeed)]
    [InlineData(DisplayMode.CutFill)] 
    public void Test_CellDatumRequest_ClusterCompute_Execute_SingleCellSiteModelMinimalValues(DisplayMode mode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();

      var baseTime = DateTime.UtcNow;
      var siteModel = BuildModelForSingleCellDatum(baseTime, true);
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutOrigin(ref siteModel, 1.0f);
      var referenceDesign = new DesignOffset(designUid, 0);
      var request = new CellDatumRequest_ClusterCompute();
      var arg = CreateCellDatumRequestArgument_ClusterCompute(siteModel, referenceDesign, mode);
      var response = request.Execute(arg, new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER, arg.ProjectID, arg.OTGCellX, arg.OTGCellY));

      response.Should().NotBeNull();
      //Only elevation and pass count
      var expected = CellDatumReturnCode.NoValueFound;
      switch (mode)
      {
        case DisplayMode.Height:
        case DisplayMode.CutFill:
        case DisplayMode.PassCount:
        case DisplayMode.PassCountSummary:
          expected = CellDatumReturnCode.ValueFound;
          break;
      }
      Assert.Equal(expected, response.ReturnCode);
    }

    [Fact]
    public void Test_CellDatumRequest_ClusterCompute_Execute_MissingDesign()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CellDatumRequest_ClusterCompute();

      Assert.Throws<ArgumentException>(() => request.Execute(CreateCellDatumRequestArgument_ClusterCompute(siteModel, new DesignOffset(Guid.NewGuid(), -0.5), DisplayMode.Height), new SubGridSpatialAffinityKey()));
    }

    [Fact]
    public void Test_CellDatumRequest_ClusterCompute_Execute_MissingSiteModel()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var arg = CreateCellDatumRequestArgument_ClusterCompute(siteModel, new DesignOffset(), DisplayMode.Height);
      arg.ProjectID = Guid.NewGuid();

      var request = new CellDatumRequest_ClusterCompute();
      var response = request.Execute(arg, new SubGridSpatialAffinityKey());

      response.Should().NotBeNull();
      Assert.Equal(CellDatumReturnCode.UnexpectedError, response.ReturnCode);
    }
    #endregion

    #region Application Service
    private CellDatumRequestArgument_ApplicationService CreateCellDatumRequestArgument_ApplicationService(ISiteModel siteModel, DesignOffset referenceDesign, DisplayMode mode)
    {
      //The single cell is at world origin
      var coords = new XYZ(0.1, 0.1, 0);

      return new CellDatumRequestArgument_ApplicationService
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        Mode = mode,
        Point = coords,
        ReferenceDesign = referenceDesign,
        CoordsAreGrid = true
      };
    }

    [Fact]
    public void Test_CellDatumRequest_ApplicationService_Creation()
    {
      var request = new CellDatumRequest_ApplicationService();
      request.Should().NotBeNull();
    }

    [Fact]
    public void Test_CellDatumRequest_ApplicationService_Execute_EmptySiteModel()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new CellDatumRequest_ApplicationService();
      var response = request.Execute(CreateCellDatumRequestArgument_ApplicationService(siteModel, new DesignOffset(), DisplayMode.Height));

      response.Should().NotBeNull();
      Assert.Equal(CellDatumReturnCode.NoValueFound, response.ReturnCode);
    }

    [Theory]
    [InlineData(DisplayMode.PassCount, 10)]
    [InlineData(DisplayMode.PassCountSummary, 100.0)]
    [InlineData(DisplayMode.CCV, 110)]
    [InlineData(DisplayMode.CCVPercent, 50.0)]
    [InlineData(DisplayMode.CCVSummary, 50.0)]
    [InlineData(DisplayMode.CCVPercentSummary, 50.0)]
    [InlineData(DisplayMode.CCVPercentChange, 10.0)]
    [InlineData(DisplayMode.MDP, 220)]
    [InlineData(DisplayMode.MDPPercent, 25.0)]
    [InlineData(DisplayMode.MDPPercentSummary, 25.0)]
    [InlineData(DisplayMode.MDPSummary, 25.0)]
    [InlineData(DisplayMode.Height, 6.0)]
    [InlineData(DisplayMode.TemperatureDetail, 101.0)]
    [InlineData(DisplayMode.TemperatureSummary, 101.0)]
    [InlineData(DisplayMode.MachineSpeed, 660)]
    [InlineData(DisplayMode.CutFill, 3.5)]//1.5 offset from 5
    public void Test_CellDatumRequest_ApplicationService_Execute_SingleCellSiteModelLastPass(DisplayMode mode, double expectedValue)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();

      var baseTime = DateTime.UtcNow;
      var siteModel = BuildModelForSingleCellDatum(baseTime);
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutOrigin(ref siteModel, 1.0f);
      var referenceDesign = new DesignOffset(designUid, 1.5);

      var request = new CellDatumRequest_ApplicationService();
      var response = request.Execute(CreateCellDatumRequestArgument_ApplicationService(siteModel, referenceDesign, mode));

      response.Should().NotBeNull();
      Assert.Equal(CellDatumReturnCode.ValueFound, response.ReturnCode);
      Assert.Equal(expectedValue, response.Value);
      Assert.Equal(baseTime.AddMinutes(10), response.TimeStampUTC);
    }

    [Fact(Skip = "Skip until coreX is available")]
    public void Test_CellDatumRequest_ApplicationService_Execute_SingleCellSiteModel_LLH()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();

      var baseTime = DateTime.UtcNow;
      var siteModel = BuildModelForSingleCellDatum(baseTime);

      DITAGFileAndSubGridRequestsWithIgniteFixture.AddCSIBToSiteModel(ref siteModel, DIMENSIONS_2012_DC_CSIB);
      siteModel.CSIB().Should().Be(DIMENSIONS_2012_DC_CSIB);

      var arg = CreateCellDatumRequestArgument_ApplicationService(siteModel, new DesignOffset(), DisplayMode.Height);
      arg.Point = ConvertCoordinates.NEEToLLH(siteModel.CSIB(), arg.Point);
      arg.CoordsAreGrid = false;

      var request = new CellDatumRequest_ApplicationService();
      var response = request.Execute(arg);

      response.Should().NotBeNull();
      Assert.Equal(CellDatumReturnCode.ValueFound, response.ReturnCode);
      Assert.Equal(6.0, response.Value);
      Assert.Equal(baseTime.AddMinutes(10), response.TimeStampUTC);
    }

    [Fact]
    public void Test_CellDatumRequest_ApplicationService_Execute_SingleCellSiteModel_Outside()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();

      var baseTime = DateTime.UtcNow;
      var siteModel = BuildModelForSingleCellDatum(baseTime);

      var arg = CreateCellDatumRequestArgument_ApplicationService(siteModel, new DesignOffset(), DisplayMode.Height);
      arg.Point = new XYZ(123456, 123456);

      var request = new CellDatumRequest_ApplicationService();
      var response = request.Execute(arg);

      response.Should().NotBeNull();
      Assert.Equal(CellDatumReturnCode.NoValueFound, response.ReturnCode);
    }

    [Fact]
    public void Test_CellDatumRequest_ApplicationService_Execute_MissingSiteModel()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var arg = CreateCellDatumRequestArgument_ApplicationService(siteModel, new DesignOffset(), DisplayMode.Height);
      arg.ProjectID = Guid.NewGuid();

      var request = new CellDatumRequest_ApplicationService();
      var response = request.Execute(arg);

      response.Should().NotBeNull();
      Assert.Equal(CellDatumReturnCode.UnexpectedError, response.ReturnCode);

    }
    #endregion

  }
}

