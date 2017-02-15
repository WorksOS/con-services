//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;

//namespace VSS.TagFileAuth.Service.WebApiTests.Models
//{
//  [TestClass]
//  public class AssetConfigDataTests
//  {

//    [TestMethod]
//    public void AssetConfigDataValidLoadAndDump()
//    {
//      var myConfig = new AssetConfigData
//      {
//        assetIdentifier = Guid.NewGuid().ToString(),
//        startDate = DateTime.UtcNow.AddDays(1),
//        dumpSwitchNumber = 0,
//        dumpSwitchOpen = true,
//        loadSwitchNumber = 7,
//        loadSwitchOpen = true,
//        targetCyclesPerDay = 1,
//        volumePerCycleCubicMeter =1
//      };

//      myConfig.Validate();
//    }


//    [TestMethod]
//    public void AssetConfigDataInvalidDumpSwitch()
//    {
//      var myConfig = new AssetConfigData
//      {
//        assetIdentifier = Guid.NewGuid().ToString(),
//        startDate = DateTime.UtcNow.AddDays(1),
//        dumpSwitchNumber = 8, // out of range
//        dumpSwitchOpen = true,
//        loadSwitchNumber = 1,
//        loadSwitchOpen = false,
//        targetCyclesPerDay = 1,
//        volumePerCycleCubicMeter = 1
//      };

//      Assert.ThrowsException<ServiceException>(() => myConfig.Validate());
//    }

//    [TestMethod]
//    public void AssetConfigDataInvalidLoadSwitch()
//    {
//      var myConfig = new AssetConfigData
//      {
//        assetIdentifier = Guid.NewGuid().ToString(),
//        startDate = DateTime.UtcNow.AddDays(1),
//        dumpSwitchNumber = 1,
//        dumpSwitchOpen = true,
//        loadSwitchNumber = null, // must be set
//        loadSwitchOpen = true,
//        targetCyclesPerDay = 1,
//        volumePerCycleCubicMeter = 1
//      };

//      Assert.ThrowsException<ServiceException>(() => myConfig.Validate());
//    }

//    [TestMethod]
//    public void AssetConfigDataInvalidNoSwitches()
//    {
//      var myConfig = new AssetConfigData
//      {
//        assetIdentifier = Guid.NewGuid().ToString(),
//        startDate = DateTime.UtcNow,
//        dumpSwitchNumber = null,
//        dumpSwitchOpen = null,
//        loadSwitchNumber = null,
//        loadSwitchOpen = null,
//        targetCyclesPerDay = 1,
//        volumePerCycleCubicMeter = 1
//      };

//      Assert.ThrowsException<ServiceException>(() => myConfig.Validate());
//    }

//    [TestMethod]
//    public void AssetConfigDataValidLoadOnly()
//    {
//      var myConfig = new AssetConfigData
//      {
//        assetIdentifier = Guid.NewGuid().ToString(),
//        startDate = DateTime.UtcNow.AddDays(1),
//        dumpSwitchNumber = null,
//        dumpSwitchOpen = null,
//        loadSwitchNumber = 7,
//        loadSwitchOpen = true,
//        targetCyclesPerDay = 1,
//        volumePerCycleCubicMeter = 1
//      };

//      myConfig.Validate();
//    }

//    [TestMethod]
//    public void AssetConfigDataValidDumpOnly()
//    {
//      var myConfig = new AssetConfigData
//      {
//        assetIdentifier = Guid.NewGuid().ToString(),
//        startDate = DateTime.UtcNow.AddDays(1),
//        dumpSwitchNumber = 1,
//        dumpSwitchOpen = true,
//        loadSwitchNumber = null,
//        loadSwitchOpen = null,
//        targetCyclesPerDay = 1,
//        volumePerCycleCubicMeter = 1
//      };

//      myConfig.Validate();
//    }

//    [TestMethod]
//    public void AssetConfigDataInvalidLoadSameAsDump()
//    {
//      var myConfig = new AssetConfigData
//      {
//        assetIdentifier = Guid.NewGuid().ToString(),
//        startDate = DateTime.UtcNow.AddDays(1),
//        dumpSwitchNumber = 1,
//        dumpSwitchOpen = true,
//        loadSwitchNumber = 1,
//        loadSwitchOpen = true,
//        targetCyclesPerDay = 1,
//        volumePerCycleCubicMeter = 1
//      };

//      Assert.ThrowsException<ServiceException>(() => myConfig.Validate());
//    }

//    [TestMethod]
//    public void AssetConfigDataValidSameSwitchNumberDifferentStates()
//    {
//      var myConfig = new AssetConfigData
//      {
//        assetIdentifier = Guid.NewGuid().ToString(),
//        startDate = DateTime.UtcNow.AddDays(1),
//        dumpSwitchNumber = 1,
//        dumpSwitchOpen = true,
//        loadSwitchNumber = 1,
//        loadSwitchOpen = false,
//        targetCyclesPerDay = 1,
//        volumePerCycleCubicMeter = 1
//      };

//      myConfig.Validate();
//    }

   

//  }
//}
