using System;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.ComputeFuncs;
using VSS.TRex.CellDatum.GridFabric.Requests;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.Events.Models;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.ComputeFuncs;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.OverrideEvents
{
  [UnitTestCoveredRequest(RequestType = typeof(OverrideEventRequest))]
  public class OverrideEventRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<OverrideEventComputeFunc, OverrideEventRequestArgument, OverrideEventResponse>();

    [Fact]
    public void Test_OverrideEventRequest_Creation()
    {
      var request = new OverrideEventRequest();
      Assert.NotNull(request);
    }

 
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Test_OverrideEventRequest_MissingSiteModel(bool remove)
    {
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(remove, siteModel, DateTime.UtcNow);
      arg.ProjectID = Guid.NewGuid();
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.False(response.Success);
      Assert.Equal($"Failed to locate site model {arg.ProjectID}", response.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Test_OverrideEventRequest_MissingAsset(bool remove)
    {
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(remove, siteModel, DateTime.UtcNow);
      arg.AssetID = Guid.NewGuid();
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.False(response.Success);
      Assert.Equal($"Failed to locate machine {arg.AssetID}", response.Message);
    }

    #region Add Override Tests
    [Fact]
    public async Task Test_OverrideEventRequest_MissingOverrides()
    {
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(false, siteModel, DateTime.UtcNow);
      arg.MachineDesignName = null;
      arg.LayerID = null;
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.False(response.Success);
      Assert.Equal("Missing override values", response.Message);
    }

    [Fact]
    public async Task Test_OverrideEventRequest_InvalidDateRange()
    {
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(false, siteModel, DateTime.UtcNow);
      arg.StartUTC = arg.EndUTC;
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.False(response.Success);
      Assert.Equal($"Invalid date range. Start:{arg.StartUTC} End:{arg.EndUTC}", response.Message);
    }

    [Fact]
    public async Task Test_OverrideEventRequest_NoTargetsToOverride()
    {
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(false, siteModel, DateTime.UtcNow);
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.False(response.Success);
      Assert.Equal("No target values found to override", response.Message);
    }

    [Theory]
    [InlineData(-100, -70)]
    [InlineData(-70, -50)]
    [InlineData(-80, -70)]
    [InlineData(-100, -50)]
    [InlineData(-90, -60)]
    public async Task Test_OverrideEventRequest_OverlappingLayerOverrides(int startMins, int endMins)
    {
      AddApplicationGridRouting();

      var refDate = DateTime.UtcNow;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.PutValueAtDate(refDate.AddMinutes(startMins), new OverrideEvent<ushort>(refDate.AddMinutes(endMins), 2));

      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(false, siteModel, refDate);
      arg.MachineDesignName = null;
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.False(response.Success);
      Assert.Equal($"Layer override failed event date validation {arg.StartUTC}-{arg.EndUTC}", response.Message);
    }

    [Theory]
    [InlineData(-100, -70)]
    [InlineData(-70, -50)]
    [InlineData(-80, -70)]
    [InlineData(-100, -50)]
    [InlineData(-90, -60)]
    public async Task Test_OverrideEventRequest_OverlappingDesignOverrides(int startMins, int endMins)
    {
      AddApplicationGridRouting();

      var refDate = DateTime.UtcNow;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.PutValueAtDate(refDate.AddMinutes(startMins), new OverrideEvent<int>(refDate.AddMinutes(endMins), 2));

      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(false, siteModel, refDate);
      arg.LayerID = null;
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.False(response.Success);
      Assert.Equal($"Design override failed event date validation {arg.StartUTC}-{arg.EndUTC}", response.Message);
    }

    [Theory]
    [InlineData(-120, -90)]
    [InlineData(-60, -50)]
    public async Task Test_OverrideEventRequest_WithSingleExistingOverride(int startMins, int endMins)
    {
      AddApplicationGridRouting();

      var refDate = DateTime.UtcNow;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.PutValueAtDate(refDate.AddMinutes(startMins), new OverrideEvent<int>(refDate.AddMinutes(endMins), 2));
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.PutValueAtDate(refDate.AddMinutes(startMins), new OverrideEvent<ushort>(refDate.AddMinutes(endMins), 2));

      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(false, siteModel, refDate);
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.True(response.Success);

      Assert.Equal(2, siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.Count());
      Assert.Equal(2, siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.Count());
    }

    [Fact]
    public async Task Test_OverrideEventRequest_WithMultipleExistingOverrides()
    {
      AddApplicationGridRouting();

      var refDate = DateTime.UtcNow;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.PutValueAtDate(refDate.AddMinutes(-120), new OverrideEvent<int>(refDate.AddMinutes(-90), 2));
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.PutValueAtDate(refDate.AddMinutes(-60), new OverrideEvent<int>(refDate.AddMinutes(-50), 4));
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.PutValueAtDate(refDate.AddMinutes(-120), new OverrideEvent<ushort>(refDate.AddMinutes(-90), 2));
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.PutValueAtDate(refDate.AddMinutes(-60), new OverrideEvent<ushort>(refDate.AddMinutes(-50), 4));

      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(false, siteModel, refDate);
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.True(response.Success);

      Assert.Equal(3, siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.Count());
      Assert.Equal(3, siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.Count());
    }

    [Fact]
    public async Task Test_OverrideEventRequest_NoExistingOverrides()
    {
      AddApplicationGridRouting();

      var refDate = DateTime.UtcNow;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);

      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(false, siteModel, refDate);
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.True(response.Success);

      Assert.Equal(1, siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.Count());
      Assert.Equal(1, siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.Count());

    }
    #endregion

    #region Remove Override Tests
    [Fact]
    public async Task Test_OverrideEventRequest_OverrideNotFound()
    {
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(true, siteModel, DateTime.UtcNow);
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.False(response.Success);
      Assert.Equal("No override event(s) found to remove", response.Message);
    }

    [Fact]
    public async Task Test_OverrideEventRequest_RemoveSingleDesignOverride()
    {
      AddApplicationGridRouting();

      var refDate = DateTime.UtcNow;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.PutValueAtDate(refDate.AddMinutes(-90), new OverrideEvent<ushort>(refDate.AddMinutes(-60), 2));
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.PutValueAtDate(refDate.AddMinutes(-90), new OverrideEvent<int>(refDate.AddMinutes(-60), 2));

      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(true, siteModel, refDate);
      arg.LayerID = null;
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.True(response.Success);

      Assert.Equal(1, siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.Count());
      Assert.Equal(0, siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.Count());
    }

    [Fact]
    public async Task Test_OverrideEventRequest_RemoveSingleLayerOverride()
    {
      AddApplicationGridRouting();

      var refDate = DateTime.UtcNow;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.PutValueAtDate(refDate.AddMinutes(-90), new OverrideEvent<ushort>(refDate.AddMinutes(-60), 2));
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.PutValueAtDate(refDate.AddMinutes(-90), new OverrideEvent<int>(refDate.AddMinutes(-60), 2));

      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(true, siteModel, refDate);
      arg.MachineDesignName = null;
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.True(response.Success);

      Assert.Equal(0, siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.Count());
      Assert.Equal(1, siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.Count());
    }

    [Fact]
    public async Task Test_OverrideEventRequest_RemoveSingleOverride()
    {
      AddApplicationGridRouting();

      var refDate = DateTime.UtcNow;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.PutValueAtDate(refDate.AddMinutes(-90), new OverrideEvent<ushort>(refDate.AddMinutes(-60), 2));
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.PutValueAtDate(refDate.AddMinutes(-90), new OverrideEvent<int>(refDate.AddMinutes(-60), 2));

      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(true, siteModel, refDate);
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.True(response.Success);

      Assert.Equal(0, siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.Count());
      Assert.Equal(0, siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.Count());
    }

    [Fact]
    public async Task Test_OverrideEventRequest_RemoveMultipleOverridesForSingleAsset()
    {
      AddApplicationGridRouting();

      var refDate = DateTime.UtcNow;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.PutValueAtDate(refDate.AddMinutes(-90), new OverrideEvent<ushort>(refDate.AddMinutes(-60), 2));
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.PutValueAtDate(refDate.AddMinutes(-120), new OverrideEvent<ushort>(refDate.AddMinutes(-100), 4));
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.PutValueAtDate(refDate.AddMinutes(-90), new OverrideEvent<int>(refDate.AddMinutes(-60), 2));
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.PutValueAtDate(refDate.AddMinutes(-120), new OverrideEvent<int>(refDate.AddMinutes(-100), 4));

      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(true, siteModel, refDate);
      arg.StartUTC = Consts.MIN_DATETIME_AS_UTC;
      arg.EndUTC = Consts.MAX_DATETIME_AS_UTC;
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.True(response.Success);

      Assert.Equal(0, siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.Count());
      Assert.Equal(0, siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.Count());
    }

    [Fact]
    public async Task Test_OverrideEventRequest_RemoveMultipleOverridesForProject()
    {
      AddApplicationGridRouting();

      var refDate = DateTime.UtcNow;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      siteModel.Machines.CreateNew("Excavator", "", MachineType.Excavator, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.PutValueAtDate(refDate.AddMinutes(-60), new OverrideEvent<ushort>(refDate.AddMinutes(-50), 2));
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.PutValueAtDate(refDate.AddMinutes(-60), new OverrideEvent<int>(refDate.AddMinutes(-50), 2));
      var excavator = siteModel.Machines.Locate("Excavator", false);
      siteModel.MachinesTargetValues[excavator.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[excavator.InternalSiteModelMachineIndex].LayerOverrideEvents.PutValueAtDate(refDate.AddMinutes(-120), new OverrideEvent<ushort>(refDate.AddMinutes(-100), 4));
      siteModel.MachinesTargetValues[excavator.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(refDate.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[excavator.InternalSiteModelMachineIndex].DesignOverrideEvents.PutValueAtDate(refDate.AddMinutes(-120), new OverrideEvent<int>(refDate.AddMinutes(-100), 4));

      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(true, siteModel, refDate);
      arg.AssetID = Guid.Empty;
      arg.StartUTC = Consts.MIN_DATETIME_AS_UTC;
      arg.EndUTC = Consts.MAX_DATETIME_AS_UTC;

      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.True(response.Success);

      Assert.Equal(0, siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.Count());
      Assert.Equal(0, siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.Count());
      Assert.Equal(0, siteModel.MachinesTargetValues[excavator.InternalSiteModelMachineIndex].LayerOverrideEvents.Count());
      Assert.Equal(0, siteModel.MachinesTargetValues[excavator.InternalSiteModelMachineIndex].DesignOverrideEvents.Count());
    }

    #endregion

    private OverrideEventRequestArgument CreateOverrideEventRequestArgument(bool undo, ISiteModel siteModel, DateTime refDate)
    {
      var assetUid = siteModel.Machines.Locate("Bulldozer", false).ID;
      return new OverrideEventRequestArgument(undo, siteModel.ID, assetUid, refDate.AddMinutes(-90), refDate.AddMinutes(-60), "Design 1", 1);
    }
  }
}
