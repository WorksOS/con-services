using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Models
{
  [TestClass]
  public class MachineControllerResultTests
  {
    [TestMethod]
    public void MachineDesignsResult_single()
    {
      var designId = 44;
      var designName = "The NameOf Design";
      var machineDesignsResult = new MachineDesignsResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod(designName, designId, 55, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-1), Guid.NewGuid())
        }
      );
      Assert.AreEqual(1, machineDesignsResult.AssetOnDesignPeriods.Count, "invalid design period count");
      Assert.AreEqual(designId, machineDesignsResult.AssetOnDesignPeriods[0].OnMachineDesignId, "invalid design id");
      Assert.AreEqual(designName, machineDesignsResult.AssetOnDesignPeriods[0].OnMachineDesignName, "invalid design name");
    }

    [TestMethod]
    public void MachineDesignsResult_empty()
    {
      var machineDesignsResult = new MachineDesignsResult(new List<AssetOnDesignPeriod>());
      Assert.AreEqual(0, machineDesignsResult.AssetOnDesignPeriods.Count, "invalid design period count");
    }

    [TestMethod]
    public void MachineDesignDetails_single()
    {
      var assetId = 56;
      var machineName = "the machine name";
      var isJohnDoe = false;
      var assetUid = Guid.NewGuid();
      var designId = 44;
      var designName = "The NameOf Design";
      var machineDesignsResult = new MachineDesignDetails
      (
        assetId, machineName, isJohnDoe,
        new [] { new AssetOnDesignPeriod(designName, designId, assetId, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-1), assetUid)},
        assetUid
      );
      Assert.AreEqual(1, machineDesignsResult.AssetOnDesignPeriods.Count, "invalid design period count");
      Assert.AreEqual(designId, machineDesignsResult.AssetOnDesignPeriods[0].OnMachineDesignId, "invalid design id");
      Assert.AreEqual(designName, machineDesignsResult.AssetOnDesignPeriods[0].OnMachineDesignName, "invalid design name");
    }

    [TestMethod]
    public void MachineDesignDetails_empty()
    {
      var assetId = 56;
      var machineName = "the machine name";
      var isJohnDoe = false;
      var assetUid = Guid.NewGuid();
      var machineDesignsResult = new MachineDesignDetails
      (
        assetId, machineName, isJohnDoe,
        new AssetOnDesignPeriod [0],
        assetUid
      );
      Assert.AreEqual(0, machineDesignsResult.AssetOnDesignPeriods.Count, "invalid design period count");
    }
  }
}
