using System;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.ComputeFuncs;
using VSS.TRex.CellDatum.GridFabric.Requests;
using VSS.TRex.CellDatum.GridFabric.Responses;
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

    [Fact]
    public async Task Test_OverrideEventRequest_MissingSiteModel()
    {
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(false, siteModel);
      arg.ProjectID = Guid.NewGuid();
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.False(response.Success);
      Assert.Equal($"Failed to locate site model {arg.ProjectID}", response.Message);
    }

    [Fact]
    public async Task Test_OverrideEventRequest_MissingAsset()
    {
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(false, siteModel);
      arg.AssetID = Guid.NewGuid();
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.False(response.Success);
      Assert.Equal($"Failed to locate machine {arg.AssetID}", response.Message);
    }

    [Fact]
    public async Task Test_OverrideEventRequest_MissingOverrides()
    {
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(false, siteModel);
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
      var arg = CreateOverrideEventRequestArgument(false, siteModel);
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
      var arg = CreateOverrideEventRequestArgument(false, siteModel);
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

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].LayerOverrideEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-90), new OverrideEvent<ushort>(DateTime.UtcNow.AddMinutes(-60), 2));

      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(false, siteModel);
      arg.StartUTC = DateTime.UtcNow.AddMinutes(startMins);
      arg.EndUTC = DateTime.UtcNow.AddMinutes(endMins);
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

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-150), 1);
      siteModel.MachinesTargetValues[bulldozer.InternalSiteModelMachineIndex].DesignOverrideEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-90), new OverrideEvent<int>(DateTime.UtcNow.AddMinutes(-60), 2));

      var request = new OverrideEventRequest();
      var arg = CreateOverrideEventRequestArgument(false, siteModel);
      arg.LayerID = null;
      arg.StartUTC = DateTime.UtcNow.AddMinutes(startMins);
      arg.EndUTC = DateTime.UtcNow.AddMinutes(endMins);

      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      Assert.False(response.Success);
      Assert.Equal($"Design override failed event date validation {arg.StartUTC}-{arg.EndUTC}", response.Message);
    }

    //TODO:
    //Valid add overrides (layer & design & combo)
    //a) one machine event, no existing overrides - add override
    //b) one machine event, one existing override - add override: before after
    //c) one machine event, two existing overrides - add override: before, after, between

    //Remove overrides
    //No site model found
    //No asset found
    //Valid - remove layer override only for one asset
    //Valid - remove design override only for one asset
    //Valid - remove design & layer override for one asset
    //Valid - remove all layer overrides for one asset
    //Valid - remove all design overrides for one asset
    //Valid - remove all layer & design overrides for one asset
    //Valid - remove all layer overrides for multiple assets (i.e. for project - no asset uid)
    //Valid - remove all design overrides for multiple assets (i.e. for project - no asset uid)
    //Valid - remove all layer & design overrides for multiple assets (i.e. for project - no asset uid)



    private OverrideEventRequestArgument CreateOverrideEventRequestArgument(bool undo, ISiteModel siteModel)
    {
      var assetUid = siteModel.Machines.Locate("Bulldozer", false).ID;
      return new OverrideEventRequestArgument(undo, siteModel.ID, assetUid, DateTime.UtcNow.AddMinutes(-90), DateTime.UtcNow.AddMinutes(-60), "Design 1", 1);
    }
  }
}
