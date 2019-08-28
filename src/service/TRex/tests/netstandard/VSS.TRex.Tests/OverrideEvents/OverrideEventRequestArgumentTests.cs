using System;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using Xunit;

namespace VSS.TRex.Tests.OverrideEvents
{
  public class OverrideEventRequestArgumentTests
  {
    [Fact]
    public void Test_OverrideEventRequestArgument_Creation()
    {
      var arg = new OverrideEventRequestArgument();
      Assert.NotNull(arg);
    }

    [Fact]
    public void Test_OverrideEventRequestArgument_CreationWithArgs()
    {
      var undo = false;
      var siteModelID = Guid.NewGuid();
      var assetID = Guid.NewGuid();
      var startUTC = DateTime.UtcNow.AddMinutes(-10);
      var endUTC = DateTime.UtcNow.AddMinutes(-1);
      var machineDesignName = "Southern Motorway 1.4";
      var layerID = (ushort)2;

      var arg = new OverrideEventRequestArgument(undo, siteModelID, assetID, startUTC, endUTC, machineDesignName, layerID);
      Assert.NotNull(arg);
      Assert.Equal(undo, arg.Undo);
      Assert.Equal(siteModelID, arg.ProjectID);
      Assert.Equal(assetID, arg.AssetID);
      Assert.Equal(startUTC, arg.StartUTC);
      Assert.Equal(endUTC, arg.EndUTC);
      Assert.Equal(machineDesignName, arg.MachineDesignName);
      Assert.Equal(layerID, arg.LayerID);
    }

  }
}
