using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Microsoft.Extensions.Logging;
using log4netExtensions;
using VSS.GenericConfiguration;
using VSS.Asset.Data;
using VSS.Asset.Data.Models;
using System.Linq;
using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.Masterdata;
using MasterDataConsumer;
using VSS.Project.Data;
using VSS.Customer.Data;
using VSS.Device.Data;
using VSS.Geofence.Data;
using VSS.Project.Service.Repositories;

namespace RepositoryTests
{
  [TestClass]
  public class AssetRepositoryTests
  {

    IServiceProvider serviceProvider = null;
    AssetRepository assetContext = null;

    [TestInitialize]
    public void Init()
    {
      string loggerRepoName = "UnitTestLogTest";
      var logPath = System.IO.Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceProvider = new ServiceCollection()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddLogging()
        .AddSingleton<ILoggerFactory>(loggerFactory)
        .AddSingleton<IRepositoryFactory, RepositoryFactory>()
        .AddTransient<IRepository<IAssetEvent>, AssetRepository>()
        .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
        .AddTransient<IRepository<IDeviceEvent>, DeviceRepository>()
        .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
        .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()          
        .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>()
        .BuildServiceProvider();

      var retrievedloggerFactory = serviceProvider.GetService<ILoggerFactory>();
      Assert.IsNotNull(retrievedloggerFactory);

      // assetContext = new AssetRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
      assetContext = serviceProvider.GetRequiredService<IRepositoryFactory>().GetRepository<IAssetEvent>() as AssetRepository;
    }

    /// <summary>
    /// Happy path i.e. asset doesn't exist already.
    /// </summary>
    [TestMethod]
    public void CreateAsset_HappyPath()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
      var assetEvent = new CreateAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        AssetName = "AnAssetName",
        LegacyAssetId = 33334444,
        SerialNumber = "S6T00561",
        MakeCode = "J82", // looks like we only get the code, not the full desc 'JLG INDUSTRIES, INC'
        Model = "D6RXL",
        AssetType = "TRACK TYPE TRACTORS",
        IconKey = 23,
        EquipmentVIN = "3HSJGTKT8GN012463",
        ModelYear = null, // is this Year of Manufacture?
        OwningCustomerUID = Guid.NewGuid(),
        ActionUTC = firstCreatedUTC
      };

      var asset = new Asset
      {
        AssetUID = assetEvent.AssetUID.ToString(),
        Name = assetEvent.AssetName,
        LegacyAssetID = assetEvent.LegacyAssetId,
        SerialNumber = assetEvent.SerialNumber,
        MakeCode = assetEvent.MakeCode,
        Model = assetEvent.Model,
        AssetType = assetEvent.AssetType,
        IconKey = assetEvent.IconKey,
        OwningCustomerUID = assetEvent.OwningCustomerUID.ToString(),
        IsDeleted = false,
        LastActionedUtc = assetEvent.ActionUTC
      };

      assetContext.InRollbackTransaction<object>(o =>
      {
        var g = assetContext.GetAsset(asset.AssetUID);
        g.Wait();
        Assert.IsNull(g.Result, "Asset shouldn't be there yet");

        var s = assetContext.StoreEvent(assetEvent);
        s.Wait();
        Assert.AreEqual(1, s.Result, "Asset event not written");

        g = assetContext.GetAsset(asset.AssetUID);
        g.Wait();
        Assert.IsNotNull(g.Result, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(asset, g.Result, "Asset details are incorrect from AssetRepo");
        return null;
      });
    }

    /// <summary>
    /// Happy path i.e. asset doesn't exist already. Includes ProductFamily   
    /// </summary>
    [TestMethod]
    public void CreateAssetFilterSingleProductFamily_HappyPath()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
      var assetEvent1 = new CreateAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        AssetName = "AnAssetName",
        LegacyAssetId = 33334444,
        SerialNumber = "S6T00561",
        MakeCode = "J82", // looks like we only get the code, not the full desc 'JLG INDUSTRIES, INC'
        Model = "D6RXL",
        AssetType = "TRACK TYPE TRACTORS",
        IconKey = 23,
        EquipmentVIN = "3HSJGTKT8GN012463",
        ModelYear = null, // is this Year of Manufacture?
        ActionUTC = firstCreatedUTC
      };

      var assetEvent2 = new CreateAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        AssetName = "AnAssetName",
        LegacyAssetId = 33334444,
        SerialNumber = "S6T00561",
        MakeCode = "J82", // looks like we only get the code, not the full desc 'JLG INDUSTRIES, INC'
        Model = "D6RXL",
        AssetType = "DRUM TYPE TRACTORS",
        IconKey = 23,
        EquipmentVIN = "3HSJGTKT8GN012463",
        ModelYear = null, // is this Year of Manufacture?
        ActionUTC = firstCreatedUTC
      };


      var asset1 = new Asset
      {
        AssetUID = assetEvent1.AssetUID.ToString(),
        Name = assetEvent1.AssetName,
        LegacyAssetID = assetEvent1.LegacyAssetId,
        SerialNumber = assetEvent1.SerialNumber,
        MakeCode = assetEvent1.MakeCode,
        Model = assetEvent1.Model,
        IconKey = assetEvent1.IconKey,
        AssetType = assetEvent1.AssetType,
        OwningCustomerUID = "",
        IsDeleted = false,
        LastActionedUtc = assetEvent1.ActionUTC
      };

      var asset2 = new Asset
      {
        AssetUID = assetEvent2.AssetUID.ToString(),
        Name = assetEvent2.AssetName,
        LegacyAssetID = assetEvent2.LegacyAssetId,
        SerialNumber = assetEvent2.SerialNumber,
        MakeCode = assetEvent2.MakeCode,
        Model = assetEvent2.Model,
        IconKey = assetEvent2.IconKey,
        AssetType = assetEvent2.AssetType,
        OwningCustomerUID = "",
        IsDeleted = false,
        LastActionedUtc = assetEvent2.ActionUTC
      };

      assetContext.InRollbackTransaction<object>(o =>
      {
        var s = assetContext.StoreEvent(assetEvent1);
        s.Wait();
        Assert.AreEqual(1, s.Result, "Asset event not written");

        s = assetContext.StoreEvent(assetEvent2);
        s.Wait();
        Assert.AreEqual(1, s.Result, "Asset event not written");


        var g = assetContext.GetAssets(new[] { "DRUM TYPE TRACTORS" });
        g.Wait();
        Assert.IsNotNull(g.Result, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(asset2, g.Result.FirstOrDefault(), "Asset details are incorrect from AssetRepo");
        return null;
      });
    }

    [TestMethod]
    public void CreateAssetFilterMultipleProductFamily_HappyPath()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
      var assetEvent1 = new CreateAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        AssetName = "AnAssetName",
        LegacyAssetId = 33334444,
        SerialNumber = "S6T00561",
        MakeCode = "J82", // looks like we only get the code, not the full desc 'JLG INDUSTRIES, INC'
        Model = "D6RXL",
        AssetType = "TRACK TYPE TRACTORS1",
        IconKey = 23,
        EquipmentVIN = "3HSJGTKT8GN012463",
        ModelYear = null, // is this Yea
      };

      var assetEvent2 = new CreateAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        AssetName = "AnAssetName",
        LegacyAssetId = 33334444,
        SerialNumber = "S6T00561",
        MakeCode = "J82", // looks like we only get the code, not the full desc 'JLG INDUSTRIES, INC'
        Model = "D6RXL",
        AssetType = "DRUM TYPE TRACTORS1",
        IconKey = 23,
        EquipmentVIN = "3HSJGTKT8GN012463",
        ModelYear = null, // is this Year of Manufacture?
        ActionUTC = firstCreatedUTC
      };


      var asset1 = new Asset
      {
        AssetUID = assetEvent1.AssetUID.ToString(),
        Name = assetEvent1.AssetName,
        SerialNumber = assetEvent1.SerialNumber,
        MakeCode = assetEvent1.MakeCode,
        Model = assetEvent1.Model,
        IconKey = assetEvent1.IconKey,
        AssetType = assetEvent1.AssetType,
        IsDeleted = false,
        LastActionedUtc = assetEvent1.ActionUTC
      };

      var asset2 = new Asset
      {
        AssetUID = assetEvent2.AssetUID.ToString(),
        Name = assetEvent2.AssetName,
        SerialNumber = assetEvent2.SerialNumber,
        MakeCode = assetEvent2.MakeCode,
        Model = assetEvent2.Model,
        IconKey = assetEvent2.IconKey,
        AssetType = assetEvent2.AssetType,
        IsDeleted = false,
        LastActionedUtc = assetEvent2.ActionUTC
      };

      assetContext.InRollbackTransaction<object>(o =>
      {

        var s = assetContext.StoreEvent(assetEvent1);
        s.Wait();
        Assert.AreEqual(1, s.Result, "Asset event not written");

        s = assetContext.StoreEvent(assetEvent2);
        s.Wait();
        Assert.AreEqual(1, s.Result, "Asset event not written");


        var g = assetContext.GetAssets(new[] { "DRUM TYPE TRACTORS1", "TRACK TYPE TRACTORS1" });
        g.Wait();
        Assert.IsNotNull(g.Result, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(2, g.Result.Count(), "Asset details are incorrect from AssetRepo");
        return null;
      });
    }

    [TestMethod]
    public void CreateAssetProductFamily_CaseInsensitiveQuery()
    {
      var assetEvent1 = new CreateAssetEvent()
      { AssetUID = Guid.NewGuid(), AssetType = "TRACK TYPE TRACTORS1" };

      var assetEvent2 = new CreateAssetEvent()
      { AssetUID = Guid.NewGuid(), AssetType = "DRUM TYPE TRACTORS1" };

      assetContext.InRollbackTransaction<object>(o =>
      {
        var s = assetContext.StoreEvent(assetEvent1).Result;
        s = assetContext.StoreEvent(assetEvent2).Result;

        var g = assetContext.GetAssets(new[] { "DRUM TYPE Tractors1", "track type Tractors1" });
        g.Wait();
        Assert.IsNotNull(g.Result, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(2, g.Result.Count(), "Asset count is incorrect from AssetRepo");
        return null;
      });
    }

    [TestMethod]
    public void CreateAssetProductFamily_CaseInsensitiveDB()
    {
      var assetEvent1 = new CreateAssetEvent()
      { AssetUID = Guid.NewGuid(), AssetType = "track type Tractors1" };

      var assetEvent2 = new CreateAssetEvent()
      { AssetUID = Guid.NewGuid(), AssetType = "DRUM TYPE Tractors1" };

      assetContext.InRollbackTransaction<object>(o =>
      {
        var s = assetContext.StoreEvent(assetEvent1).Result;
        s = assetContext.StoreEvent(assetEvent2).Result;

        var g = assetContext.GetAssets(new[] { "DRUM TYPE TRACTORS1", "TRACK TYPE TRACTORS1" });
        g.Wait();
        Assert.IsNotNull(g.Result, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(2, g.Result.Count(), "Asset count is incorrect from AssetRepo");
        return null;
      });
    }

    [TestMethod]
    public void CreateAssetProductFamily_DefaultIsUnassigned()
    {
      var assetEvent1 = new CreateAssetEvent()
      { AssetUID = Guid.NewGuid(), AssetType = "Unassigned" };
      var assetEvent2 = new CreateAssetEvent()
      { AssetUID = Guid.NewGuid(), AssetType = "" };
      var assetEvent3 = new CreateAssetEvent()
      { AssetUID = Guid.NewGuid(), AssetType = null };

      assetContext.InRollbackTransaction<object>(o =>
      {
        var s = assetContext.StoreEvent(assetEvent1).Result;
        s = assetContext.StoreEvent(assetEvent2).Result;
        s = assetContext.StoreEvent(assetEvent3).Result;

        var g = assetContext.GetAssets(new[] { "UNASSIGNED" });
        g.Wait();
        Assert.IsNotNull(g.Result, "Unable to retrieve Asset from AssetRepo");
        Assert.IsTrue((g.Result.Count() >= 3), "Asset count is incorrect from AssetRepo");
        return null;
      });
    }

    [TestMethod]
    public void UpdateAssetProductFamily_DefaultIsUnassigned()
    {
      var assetEvent = new CreateAssetEvent()
      { AssetUID = Guid.NewGuid(), AssetType = "Track type tractor2" };

      assetContext.InRollbackTransaction<object>(o =>
      {
        var s = assetContext.StoreEvent(assetEvent).Result;
        assetEvent.AssetType = null;
        s = assetContext.StoreEvent(assetEvent).Result;

        var g = assetContext.GetAssets(new[] { "Unassigned" });
        g.Wait();
        Assert.IsNotNull(g.Result, "Unable to retrieve Asset from AssetRepo");
        // can't do == 1 until we can filter by e.g. customer
        Assert.IsTrue((g.Result.Count() >= 1), "Asset count is incorrect from AssetRepo");
        return null;
      });
    }

    // again happy path but most columns are blank. Should we store "" or null?
    [TestMethod]
    public void CreateAsset_MinimalData()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
      var assetEvent = new CreateAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        AssetName = null,
        LegacyAssetId = 33334444,
        SerialNumber = "S6T00561",
        MakeCode = "LTCEL",
        Model = "",
        AssetType = "",
        IconKey = null,
        EquipmentVIN = "",
        ModelYear = null,
        ActionUTC = firstCreatedUTC
      };

      var asset = new Asset
      {
        AssetUID = assetEvent.AssetUID.ToString(),
        Name = assetEvent.AssetName,
        LegacyAssetID = assetEvent.LegacyAssetId,
        SerialNumber = assetEvent.SerialNumber,
        MakeCode = assetEvent.MakeCode,
        Model = assetEvent.Model,
        IconKey = assetEvent.IconKey,
        AssetType = "Unassigned",
        OwningCustomerUID = "",
        IsDeleted = false,
        LastActionedUtc = assetEvent.ActionUTC
      };

      assetContext.InRollbackTransaction<object>(o =>
      {
        var s = assetContext.StoreEvent(assetEvent);
        s.Wait();
        Assert.AreEqual(1, s.Result, "Asset event not written");

        var g = assetContext.GetAsset(asset.AssetUID);
        g.Wait();
        Assert.IsNotNull(g.Result, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(asset, g.Result, "Asset details are incorrect from AssetRepo");
        return null;
      });
    }

    /// <summary>
    /// CreateAssetEvent already applied ie. has same ActionUTC
    /// update columns in case new fields imported or error occured first time
    /// </summary>
    [TestMethod]
    public void CreateAsset_ExistsFromMasterDataCreate()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
      var assetEventOriginal = new CreateAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        AssetName = null,
        LegacyAssetId = 33334444,
        SerialNumber = "S6T00561",
        MakeCode = "J82",
        Model = "D6RXL",
        AssetType = "TRACK TYPE TRACTORS",
        IconKey = 23,
        EquipmentVIN = "3HSJGTKT8GN012463",
        ModelYear = null,
        ActionUTC = firstCreatedUTC
      };

      var assetEventLater = new CreateAssetEvent()
      {
        AssetUID = assetEventOriginal.AssetUID,
        AssetName = "AnAssetName",
        LegacyAssetId = 33334444,
        SerialNumber = "S6T00561",
        MakeCode = "J82",
        Model = "D6RXL",
        AssetType = "TRACK TYPE TRACTORS",
        IconKey = 23,
        ModelYear = null, // is this Year of Manufacture?
        ActionUTC = firstCreatedUTC
      };

      var asset = new Asset
      {
        AssetUID = assetEventLater.AssetUID.ToString(),
        Name = null,
        LegacyAssetID = assetEventLater.LegacyAssetId,
        SerialNumber = assetEventLater.SerialNumber,
        MakeCode = assetEventLater.MakeCode,
        Model = assetEventLater.Model,
        AssetType = assetEventLater.AssetType,
        IconKey = assetEventLater.IconKey,
        OwningCustomerUID = "",
        IsDeleted = false,
        LastActionedUtc = assetEventLater.ActionUTC
      };

      assetContext.InRollbackTransaction<object>(o =>
      {
        var s = assetContext.StoreEvent(assetEventOriginal);
        s.Wait();
        var g = assetContext.GetAsset(asset.AssetUID);
        g.Wait();
        Assert.AreEqual(asset, g.Result, "Unable to retrieve Asset from AssetRepo");

        s = assetContext.StoreEvent(assetEventLater);
        s.Wait();

        asset.Name = assetEventLater.AssetName; // this should be updated now
        g = assetContext.GetAsset(asset.AssetUID);
        g.Wait();
        Assert.IsNotNull(g.Result, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(asset, g.Result, "Asset details are incorrect from AssetRepo");
        return null;
      });
    }

    /// <summary>
    /// ActionUTC is set already to an earlier date
    /// this could have been from a Create or Update - does it matter?
    /// </summary>
    [TestMethod]
    public void UpdateAsset_HappyPath()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
      var assetEventCreate = new CreateAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        AssetName = "AnAssetName",
        LegacyAssetId = 33334444,
        SerialNumber = "S6T00561",
        MakeCode = "J82",
        Model = "D6RXL",
        AssetType = "TRACK TYPE TRACTORS",
        IconKey = 23,
        EquipmentVIN = null,
        ModelYear = null,
        ActionUTC = firstCreatedUTC
      };

      var assetEventUpdate = new UpdateAssetEvent()
      {
        AssetUID = assetEventCreate.AssetUID,
        AssetName = "AnAssetName changed",
        LegacyAssetId = 33334444,
        Model = "D6RXL changed",
        AssetType = "TRACK TYPE TRACTORS",
        IconKey = 11,
        EquipmentVIN = null,
        ModelYear = null,
        ActionUTC = firstCreatedUTC.AddMinutes(10)
      };

      var assetFinal = new Asset
      {
        AssetUID = assetEventCreate.AssetUID.ToString(),
        Name = assetEventUpdate.AssetName,
        LegacyAssetID = assetEventUpdate.LegacyAssetId.Value,
        SerialNumber = assetEventCreate.SerialNumber,
        MakeCode = assetEventCreate.MakeCode,
        Model = assetEventUpdate.Model,
        IconKey = assetEventUpdate.IconKey,
        AssetType = assetEventUpdate.AssetType,
        OwningCustomerUID = "",
        IsDeleted = false,
        LastActionedUtc = assetEventUpdate.ActionUTC
      };

      assetContext.InRollbackTransaction<object>(o =>
      {
        var s = assetContext.StoreEvent(assetEventCreate);
        s.Wait();
        s = assetContext.StoreEvent(assetEventUpdate);
        s.Wait();

        var g = assetContext.GetAsset(assetFinal.AssetUID);
        g.Wait();
        Assert.IsNotNull(g.Result, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(assetFinal, g.Result, "Asset details are incorrect from AssetRepo");
        return null;
      });
    }

    /// <summary>
    /// Asset exists, with a later ActionUTC
    /// Potentially asset has already had an Update applied
    /// </summary>
    [TestMethod]
    public void UpdateAsset_ExistsFromMoreRecentUpdate()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
      var assetEventCreate = new CreateAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        AssetName = "AnAssetName",
        LegacyAssetId = 33334444,
        SerialNumber = "S6T00561",
        MakeCode = "J82",
        Model = "D6RXL",
        AssetType = "TRACK TYPE TRACTORS",
        IconKey = 23,
        EquipmentVIN = null,
        ModelYear = null,
        ActionUTC = firstCreatedUTC
      };

      var assetEventUpdateEarlier = new UpdateAssetEvent()
      {
        AssetUID = assetEventCreate.AssetUID,
        AssetName = "AnAssetName changed",
        LegacyAssetId = 33334444,
        Model = "D6RXL changed",
        AssetType = "TRACK TYPE TRACTORS",
        IconKey = 11,
        EquipmentVIN = null,
        ModelYear = null,
        ActionUTC = firstCreatedUTC.AddMinutes(10)
      };

      var assetEventUpdateLater = new UpdateAssetEvent()
      {
        AssetUID = assetEventCreate.AssetUID,
        AssetName = "AnAssetName changed even later",
        LegacyAssetId = 33334444,
        Model = "D6RXL even later",
        AssetType = "TRACK TYPE TRACTORS changed",
        IconKey = 10,
        EquipmentVIN = null,
        ModelYear = null,
        ActionUTC = firstCreatedUTC.AddMinutes(20)
      };

      var assetFinal = new Asset
      {
        AssetUID = assetEventCreate.AssetUID.ToString(),
        Name = assetEventUpdateLater.AssetName,
        LegacyAssetID = assetEventUpdateLater.LegacyAssetId.Value,
        SerialNumber = assetEventCreate.SerialNumber,
        MakeCode = assetEventCreate.MakeCode,
        Model = assetEventUpdateLater.Model,
        IconKey = assetEventUpdateLater.IconKey,
        AssetType = assetEventUpdateLater.AssetType,
        OwningCustomerUID = "",
        IsDeleted = false,
        LastActionedUtc = assetEventUpdateLater.ActionUTC
      };

      assetContext.InRollbackTransaction<object>(o =>
      {
        var s = assetContext.StoreEvent(assetEventCreate);
        s.Wait();
        s = assetContext.StoreEvent(assetEventUpdateLater);
        s.Wait();
        s = assetContext.StoreEvent(assetEventUpdateEarlier);
        s.Wait();

        var g = assetContext.GetAsset(assetFinal.AssetUID);
        g.Wait();
        Assert.IsNotNull(g.Result, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(assetFinal, g.Result, "Asset details are incorrect from AssetRepo");
        return null;
      });
    }

    /// <summary>
    /// Asset exists, with a later ActionUTC
    /// Potentially asset has already had an Update applied
    /// update only columns NOT in an update - in case createAssetEvent was never applied
    /// </summary>
    [TestMethod]
    public void CreateAsset_ExistsFromMasterDataUpdate()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
      var assetEventCreate = new CreateAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        AssetName = "AnAssetName",
        LegacyAssetId = 33334444,
        SerialNumber = "S6T00561",
        MakeCode = "J82",
        Model = "D6RXL",
        AssetType = "TRACK TYPE TRACTORS",
        IconKey = 23,
        EquipmentVIN = null,
        ModelYear = null,
        ActionUTC = firstCreatedUTC
      };

      var assetEventUpdate = new UpdateAssetEvent()
      {
        AssetUID = assetEventCreate.AssetUID,
        AssetName = "AnAssetName changed",
        LegacyAssetId = 55554444,
        Model = "D6RXL changed",
        AssetType = "TRACK TYPE TRACTORS changed",
        IconKey = 11,
        EquipmentVIN = null,
        ModelYear = null,
        ActionUTC = firstCreatedUTC.AddMinutes(10)
      };

      var assetFinal = new Asset
      {
        AssetUID = assetEventCreate.AssetUID.ToString(),
        Name = assetEventUpdate.AssetName,
        LegacyAssetID = assetEventUpdate.LegacyAssetId.Value,
        SerialNumber = assetEventCreate.SerialNumber,
        MakeCode = assetEventCreate.MakeCode,
        Model = assetEventUpdate.Model,
        IconKey = assetEventUpdate.IconKey,
        AssetType = assetEventUpdate.AssetType,
        OwningCustomerUID = "",
        IsDeleted = false,
        LastActionedUtc = assetEventUpdate.ActionUTC
      };

      assetContext.InRollbackTransaction<object>(o =>
      {
        var s = assetContext.StoreEvent(assetEventUpdate);
        s.Wait();
        s = assetContext.StoreEvent(assetEventCreate);
        s.Wait();

        var g = assetContext.GetAsset(assetFinal.AssetUID);
        g.Wait();
        Assert.IsNotNull(g.Result, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(assetFinal, g.Result, "Asset details are incorrect from AssetRepo");
        return null;
      });
    }

    /// <summary>
    /// asset exists
    /// </summary>
    [TestMethod]
    public void DeleteAsset_HappyPath()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
      var assetEventCreate = new CreateAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        AssetName = "AnAssetName",
        LegacyAssetId = 33334444,
        SerialNumber = "S6T00561",
        MakeCode = "J82",
        Model = "D6RXL",
        AssetType = "TRACK TYPE TRACTORS",
        IconKey = 23,
        EquipmentVIN = null,
        ModelYear = null,
        ActionUTC = firstCreatedUTC
      };

      var assetEventDelete = new DeleteAssetEvent()
      {
        AssetUID = assetEventCreate.AssetUID,
        ActionUTC = firstCreatedUTC.AddMinutes(10)
      };

      var assetFinal = new Asset
      {
        AssetUID = assetEventCreate.AssetUID.ToString(),
        Name = assetEventCreate.AssetName,
        LegacyAssetID = assetEventCreate.LegacyAssetId,
        SerialNumber = assetEventCreate.SerialNumber,
        MakeCode = assetEventCreate.MakeCode,
        Model = assetEventCreate.Model,
        IconKey = assetEventCreate.IconKey,
        AssetType = assetEventCreate.AssetType,
        OwningCustomerUID = "",
        IsDeleted = true,
        LastActionedUtc = assetEventDelete.ActionUTC
      };

      assetContext.InRollbackTransaction<object>(o =>
      {
        var s = assetContext.StoreEvent(assetEventCreate);
        s.Wait();
        s = assetContext.StoreEvent(assetEventDelete);
        s.Wait();

        var g = assetContext.GetAsset(assetFinal.AssetUID);
        g.Wait();
        Assert.IsNull(g.Result, "Should not be able to retrieve a deleted Asset");

        var l = assetContext.GetAllAssetsInternal();
        l.Wait();
        Assert.IsNotNull(l.Result, "Unable to retrieve any Assets from AssetRepo");
        Assert.IsTrue(((List<Asset>)l.Result).Contains(assetFinal), "Unable to retrieve Asset from AssetRepo");
        return null;
      });
    }

    /// <summary>
    ///  asset doesn't exist
    ///  hmmm what to do, create one as 'deleted'
    ///    or ignore it?
    /// </summary>
    [TestMethod]
    public void DeleteAsset_AssetDoesntExist()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);

      var assetEventDelete = new DeleteAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        ActionUTC = firstCreatedUTC
      };

      var assetFinal = new Asset
      {
        AssetUID = assetEventDelete.AssetUID.ToString(),
        Name = null,
        SerialNumber = null,
        MakeCode = null,
        Model = null,
        IconKey = null,
        AssetType = "Unassigned",
        IsDeleted = true,
        LastActionedUtc = assetEventDelete.ActionUTC
      };

      assetContext.InRollbackTransaction<object>(o =>
      {
        var s = assetContext.StoreEvent(assetEventDelete);
        s.Wait();
        Assert.AreEqual(1, s.Result, "Asset event not written");

        var g = assetContext.GetAsset(assetFinal.AssetUID);
        g.Wait();
        Assert.IsNull(g.Result, "Should not be able to retrieve a deleted Asset");

        var l = assetContext.GetAllAssetsInternal();
        l.Wait();
        Assert.IsNotNull(l.Result, "Unable to retrieve any Assets from AssetRepo");
        Assert.IsTrue(((List<Asset>)l.Result).Contains(assetFinal), "Unable to retrieve Asset from AssetRepo");
        return null;
      });
    }

    /// <summary>
    ///  An Update is received but asset was deleted prior to that ActionUTC
    ///  Ignore update
    /// </summary>
    [TestMethod]
    public void CreateAsset_AssetIsDeleted()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);

      var assetEventCreate = new CreateAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        AssetName = "AnAssetName",
        LegacyAssetId = 33334444,
        SerialNumber = "S6T00561",
        MakeCode = "J82",
        Model = "D6RXL",
        AssetType = "TRACK TYPE TRACTORS",
        IconKey = 23,
        EquipmentVIN = null,
        ModelYear = null,
        ActionUTC = firstCreatedUTC
      };

      var assetEventDelete = new DeleteAssetEvent()
      {
        AssetUID = assetEventCreate.AssetUID,
        ActionUTC = firstCreatedUTC.AddMinutes(10)
      };

      var assetFinal = new Asset
      {
        AssetUID = assetEventDelete.AssetUID.ToString(),
        Name = null,
        SerialNumber = null,
        MakeCode = null,
        Model = null,
        IconKey = null,
        AssetType = "Unassigned",
        IsDeleted = true,
        LastActionedUtc = assetEventDelete.ActionUTC
      };

      assetContext.InRollbackTransaction<object>(o =>
      {
        var s = assetContext.StoreEvent(assetEventDelete);
        s.Wait();
        s = assetContext.StoreEvent(assetEventCreate);
        s.Wait();

        var g = assetContext.GetAsset(assetFinal.AssetUID);
        g.Wait();
        Assert.IsNull(g.Result, "Should not be able to retrieve a deleted Asset");

        var l = assetContext.GetAllAssetsInternal();
        l.Wait();
        Assert.IsNotNull(l.Result, "Unable to retrieve any Assets from AssetRepo");
        Assert.IsTrue(((List<Asset>)l.Result).Contains(assetFinal), "Unable to retrieve Asset from AssetRepo");
        return null;
      });
    }


    /// <summary>
    ///  A Create is received but asset was deleted prior to that ActionUTC
    ///  Ignore create
    /// </summary>
    [TestMethod]
    public void UpdateAsset_AssetIsDeleted()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);

      var assetEventUpdate = new UpdateAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        AssetName = "AnAssetName",
        LegacyAssetId = 33334444,
        Model = "D6RXL",
        AssetType = "TRACK TYPE TRACTORS",
        IconKey = 23,
        EquipmentVIN = null,
        ModelYear = null,
        ActionUTC = firstCreatedUTC
      };

      var assetEventDelete = new DeleteAssetEvent()
      {
        AssetUID = assetEventUpdate.AssetUID,
        ActionUTC = firstCreatedUTC.AddMinutes(10)
      };

      var assetFinal = new Asset
      {
        AssetUID = assetEventDelete.AssetUID.ToString(),
        Name = null,
        SerialNumber = null,
        MakeCode = null,
        Model = null,
        IconKey = null,
        AssetType = "Unassigned",
        IsDeleted = true,
        LastActionedUtc = assetEventDelete.ActionUTC
      };

      assetContext.InRollbackTransaction<object>(o =>
      {
        var s = assetContext.StoreEvent(assetEventDelete);
        s.Wait();
        s = assetContext.StoreEvent(assetEventUpdate);
        s.Wait();

        var g = assetContext.GetAsset(assetFinal.AssetUID);
        g.Wait();
        Assert.IsNull(g.Result, "Should not be able to retrieve a deleted Asset");

        var l = assetContext.GetAllAssetsInternal();
        l.Wait();
        Assert.IsNotNull(l.Result, "Unable to retrieve any Assets from AssetRepo");
        Assert.IsTrue(((List<Asset>)l.Result).Contains(assetFinal), "Unable to retrieve Asset from AssetRepo");
        return null;
      });
    }


    //#region AssetCount // todo this is for VUP
    ///// <summary>
    /////  Invalid group, only ProductFamily allowed at present
    ///// </summary>
    //[TestMethod]
    //public void AssetCount_InvalidGroup()
    //{
    //  var assetEvent = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = Guid.NewGuid().ToString() };

    //  assetContext.InRollbackTransaction<object>(o =>
    //  {
    //    var s = assetContext.StoreEvent(assetEvent).Result;

    //    var l = assetContext.GetAssetCount(AssetCountGrouping.GeoFence, null).Result;
    //    Assert.AreEqual(0, l.Count(), "Invalid Group");
    //    return null;
    //  });
    //}

    ///// <summary>
    /////  One or none of group/ProductFamily are allowed
    ///// </summary>
    //[TestMethod]
    //public void AssetCount_BothGroupAndFilter()
    //{
    //  var assetEvent = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = Guid.NewGuid().ToString() };

    //  string[] productFamily = new string[] { assetEvent.AssetType };

    //  var assetContext = new AssetRepository(ConfigSettings.GetConnectionString("VSPDB"));
    //  assetContext.InRollbackTransaction<object>(o =>
    //  {
    //    var s = assetContext.StoreAsset(assetEvent).Result;

    //    var l = assetContext.GetAssetCount(AssetCountGrouping.ProductFamily, productFamily).Result;
    //    Assert.AreEqual(0, l.Count(), "Invalid Group/family combination");
    //    return null;
    //  });
    //}

    ///// <summary>
    /////  ProductFamily filter, at least one exists
    ///// </summary>
    //[TestMethod]
    //public void AssetCount_Group_NoFilters_OneFamilyExists()
    //{
    //  var assetEvent = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = Guid.NewGuid().ToString() };

    //  var assetContext = new AssetRepository(ConfigSettings.GetConnectionString("VSPDB"));
    //  assetContext.InRollbackTransaction<object>(o =>
    //  {
    //    var s = assetContext.StoreAsset(assetEvent).Result;

    //    var l = assetContext.GetAssetCount(AssetCountGrouping.ProductFamily, null).Result;
    //    Assert.IsTrue(l.Count() >= 1, "Should be at least the 1 ProductFamily from above");
    //    Assert.IsNotNull(((List<CategoryCount>)l).FirstOrDefault(x => x.CountOf == assetEvent.AssetType), "Unable to retrieve CategoryCount");
    //    Assert.AreEqual(1, ((List<CategoryCount>)l).FirstOrDefault(x => x.CountOf == assetEvent.AssetType).Count, "Should be asset from above");
    //    return null;
    //  });
    //}

    ///// <summary>
    /////  ProductFamily filter, at least two exists
    ///// </summary>
    //[TestMethod]
    //public void AssetCount_Group_NoFilters_TwoFamiliesExist()
    //{
    //  var assetEvent1 = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = Guid.NewGuid().ToString() };
    //  var assetEvent2 = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = Guid.NewGuid().ToString() };

    //  var assetContext = new AssetRepository(ConfigSettings.GetConnectionString("VSPDB"));
    //  assetContext.InRollbackTransaction<object>(o =>
    //  {
    //    var s = assetContext.StoreAsset(assetEvent1).Result;
    //    s = assetContext.StoreAsset(assetEvent2).Result;

    //    var l = assetContext.GetAssetCount(AssetCountGrouping.ProductFamily, null).Result;
    //    Assert.IsTrue(l.Count() >= 2, "Should be at least the 2 ProductFamilies from above");
    //    Assert.IsNotNull(((List<CategoryCount>)l).FirstOrDefault(x => x.CountOf == assetEvent1.AssetType), "Unable to retrieve CategoryCounto");
    //    Assert.AreEqual(1, ((List<CategoryCount>)l).FirstOrDefault(x => x.CountOf == assetEvent1.AssetType).Count, "Should be 1 asset from above");
    //    Assert.IsNotNull(((List<CategoryCount>)l).FirstOrDefault(x => x.CountOf == assetEvent2.AssetType), "Unable to retrieve CategoryCount");
    //    Assert.AreEqual(1, ((List<CategoryCount>)l).FirstOrDefault(x => x.CountOf == assetEvent2.AssetType).Count, "Should be 1 asset from above");
    //    return null;
    //  });
    //}


    ///// <summary>
    /////  temporarily ignored as this may pick up data from other exceptance test in the DB.
    /////     Should be resolved when we add other filters e.g. Customer
    /////     
    /////  ProductFamily filter, no assets exist
    /////     will return an empty list
    ///// </summary>
    //[TestMethod]
    //[Ignore]
    //public void AssetCount_Group_NoFilters_NoAssetsExists()
    //{
    //  var assetContext = new AssetRepository(ConfigSettings.GetConnectionString("VSPDB"));
    //  assetContext.InRollbackTransaction<object>(o =>
    //  {
    //    var l = assetContext.GetAssetCount(AssetCountGrouping.ProductFamily, null).Result;
    //    Assert.AreEqual(0, l.Count(), "There are no existing product families (eventually include other search criteria) so nothing to group");
    //    return null;
    //  });
    //}

    ///// <summary>
    ///// temporarily ignored as this may pick up data from other exceptance test in the DB.
    /////     Should be resolved when we add other filters e.g. Customer
    /////     
    /////  ProductFamily filter, only a deleted asset exists
    /////     will return an empty list
    ///// </summary>
    //[TestMethod]
    //[Ignore]
    //public void AssetCount_Group_NoFilters_OneDeletedAssetExists()
    //{
    //  var assetEvent = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = Guid.NewGuid().ToString() };
    //  var assetEventDeleted = new DeleteAssetEvent()
    //  { AssetUID = assetEvent.AssetUID };

    //  var assetContext = new AssetRepository(ConfigSettings.GetConnectionString("VSPDB"));
    //  assetContext.InRollbackTransaction<object>(o =>
    //  {
    //    var s = assetContext.StoreAsset(assetEvent).Result;
    //    s = assetContext.StoreAsset(assetEventDeleted).Result;

    //    var l = assetContext.GetAssetCount(AssetCountGrouping.ProductFamily, null).Result;
    //    Assert.AreEqual(0, l.Count(), "There are no existing product families (eventually include other search criteria) so nothing to group");
    //    return null;
    //  });
    //}


    ///// <summary>
    /////  ProductFamily filter, at least one exists
    ///// </summary>
    //[TestMethod]
    //public void AssetCount_NoGroup_OneFilter_AssetExists()
    //{
    //  var assetEvent = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = Guid.NewGuid().ToString() };

    //  string[] productFamily = new string[] { assetEvent.AssetType };

    //  var assetContext = new AssetRepository(ConfigSettings.GetConnectionString("VSPDB"));
    //  assetContext.InRollbackTransaction<object>(o =>
    //  {
    //    var s = assetContext.StoreAsset(assetEvent).Result;

    //    var l = assetContext.GetAssetCount(null, productFamily).Result;
    //    Assert.AreEqual(1, l.Count(), "Should be count of assets in productFamilies from above");
    //    Assert.AreEqual("All Assets", l[0].CountOf, "Unable to retrieve CategoryCount");
    //    Assert.AreEqual(1, l[0].Count, "Should be asset from above");
    //    return null;
    //  });
    //}

    ///// <summary>
    /////  ProductFamily filter1, one has asset the other doesn't
    ///// </summary>
    //[TestMethod]
    //public void AssetCount_NoGroup_TwoFilters_OneHasAssets()
    //{
    //  var assetEvent = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = Guid.NewGuid().ToString() };

    //  string[] productFamily = new string[] { assetEvent.AssetType, Guid.NewGuid().ToString() };

    //  var assetContext = new AssetRepository(ConfigSettings.GetConnectionString("VSPDB"));
    //  assetContext.InRollbackTransaction<object>(o =>
    //  {
    //    var s = assetContext.StoreAsset(assetEvent).Result;

    //    var l = assetContext.GetAssetCount(null, productFamily).Result;
    //    Assert.AreEqual(1, l.Count(), "Should be count of assets in productFamilies from above");
    //    Assert.AreEqual("All Assets", l[0].CountOf, "Unable to retrieve CategoryCount");
    //    Assert.AreEqual(1, l[0].Count, "Should be asset from above");
    //    return null;
    //  });
    //}

    ///// <summary>
    /////  ProductFamily filter, no assets exist for it
    ///// </summary>
    //[TestMethod]
    //public void AssetCount_NoGroup_OneFilter_NoAssetExists()
    //{
    //  var assetEvent = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = Guid.NewGuid().ToString() };

    //  string[] productFamily = new string[] { Guid.NewGuid().ToString() };

    //  var assetContext = new AssetRepository(ConfigSettings.GetConnectionString("VSPDB"));
    //  assetContext.InRollbackTransaction<object>(o =>
    //  {
    //    var s = assetContext.StoreAsset(assetEvent).Result;

    //    var l = assetContext.GetAssetCount(null, productFamily).Result;
    //    Assert.AreEqual(1, l.Count(), "Should be count of assets in productFamilies from above");
    //    Assert.AreEqual("All Assets", l[0].CountOf, "Unable to retrieve CategoryCount");
    //    Assert.AreEqual(0, l[0].Count, "Should be no assets");
    //    return null;
    //  });
    //}

    ///// <summary>
    /////  ProductFamily filter, only 1 deleted asset exists
    /////     return empty list
    ///// </summary>
    //[TestMethod]
    //public void AssetCount_NoGroup_OneFilter_OneDeletedAssetExists()
    //{
    //  var assetEvent = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = Guid.NewGuid().ToString() };
    //  var assetEventDeleted = new DeleteAssetEvent()
    //  { AssetUID = assetEvent.AssetUID };

    //  string[] productFamily = new string[] { assetEvent.AssetType };

    //  var assetContext = new AssetRepository(ConfigSettings.GetConnectionString("VSPDB"));
    //  assetContext.InRollbackTransaction<object>(o =>
    //  {
    //    var s = assetContext.StoreAsset(assetEvent).Result;
    //    s = assetContext.StoreAsset(assetEventDeleted).Result;

    //    var l = assetContext.GetAssetCount(null, productFamily).Result;
    //    Assert.AreEqual(1, l.Count(), "Should be count of assets in productFamilies from above");
    //    Assert.AreEqual("All Assets", l[0].CountOf, "Unable to retrieve CategoryCount");
    //    Assert.AreEqual(0, l[0].Count, "Should be no assets");
    //    return null;
    //  });
    //}


    ///// <summary>
    /////  No grouping or filters i.e. count AllAssets
    ///// </summary>
    //[TestMethod]
    //public void AssetCount_NoGrouping_NoFilters_TwoTypesExists()
    //{
    //  var assetEvent1 = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = Guid.NewGuid().ToString() };
    //  var assetEvent2 = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = Guid.NewGuid().ToString() };

    //  var assetContext = new AssetRepository(ConfigSettings.GetConnectionString("VSPDB"));
    //  assetContext.InRollbackTransaction<object>(o =>
    //  {
    //    var s = assetContext.StoreAsset(assetEvent1).Result;
    //    s = assetContext.StoreAsset(assetEvent2).Result;

    //    var l = assetContext.GetAssetCount(null, null).Result;
    //    Assert.AreEqual(1, l.Count(), "Should be 1 assetCount");
    //    Assert.IsTrue((l[0].Count >= 2), "Should be at least the 2 assets in total");
    //    Assert.AreEqual("All Assets", l[0].CountOf, "Text should be All Assets count");
    //    return null;
    //  });
    //}

    ///// <summary>
    /////  temporarily ignored partial as this may pick up data from other exceptance test in the DB.
    /////     Should be resolved when we add other filters e.g. Customer
    ///// 
    /////  No grouping or filters. there are no assets. i.e. count AllAssets should == 0
    ///// </summary>
    //[TestMethod]
    //public void AssetCount_NoGrouping_NoFilters_NoAssetsExists()
    //{
    //  var assetContext = new AssetRepository(ConfigSettings.GetConnectionString("VSPDB"));
    //  assetContext.InRollbackTransaction<object>(o =>
    //  {
    //    var l = assetContext.GetAssetCount(null, null).Result;
    //    Assert.AreEqual(1, l.Count(), "Should be 1 assetCount");
    //    // temp ignore  Assert.AreEqual(0, l[0].Count, "Should be no assets in total");
    //    Assert.AreEqual("All Assets", l[0].CountOf, "Text should be All Assets count");
    //    return null;
    //  });
    //}

    ///// <summary>
    /////  temporarily ignored partial as this may pick up data from other exceptance test in the DB.
    /////     Should be resolved when we add other filters e.g. Customer
    /////  
    /////  No grouping or filters. there are only deleted assets. 
    /////     return All Assets count = 0
    ///// </summary>
    //[TestMethod]
    //public void AssetCount_NoGrouping_NoFilters_OneDeletedAssetExists()
    //{
    //  var assetEvent = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = Guid.NewGuid().ToString() };
    //  var assetEventDeleted = new DeleteAssetEvent()
    //  { AssetUID = assetEvent.AssetUID };

    //  var assetContext = new AssetRepository(ConfigSettings.GetConnectionString("VSPDB"));
    //  assetContext.InRollbackTransaction<object>(o =>
    //  {
    //    var s = assetContext.StoreAsset(assetEvent).Result;
    //    s = assetContext.StoreAsset(assetEventDeleted).Result;

    //    var l = assetContext.GetAssetCount(null, null).Result;
    //    Assert.AreEqual(1, l.Count(), "Should be 1 assetCount");
    //    // temp ignore Assert.AreEqual(0, l[0].Count, "Should be no assets in total");
    //    Assert.AreEqual("All Assets", l[0].CountOf, "Text should be All Assets count");
    //    return null;
    //  });
    //}


    ///// <summary>
    /////  Product family filter: Unassigned
    /////     null/spaces AssetType are stored as "Unassigned"
    ///// </summary>
    //[TestMethod]
    //public void AssetCount_NoGrouping_UnassignedFilter()
    //{
    //  var assetEvent1 = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = "Unassigned" };
    //  var assetEvent2 = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = "" };
    //  var assetEvent3 = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = null };

    //  string[] productFamily = new string[] { "Unassigned", Guid.NewGuid().ToString() };

    //  var assetContext = new AssetRepository(ConfigSettings.GetConnectionString("VSPDB"));
    //  assetContext.InRollbackTransaction<object>(o =>
    //  {
    //    var s = assetContext.StoreAsset(assetEvent1).Result;
    //    s = assetContext.StoreAsset(assetEvent2).Result;
    //    s = assetContext.StoreAsset(assetEvent3).Result;

    //    var l = assetContext.GetAssetCount(null, productFamily).Result;
    //    Assert.AreEqual(1, l.Count(), "Should be count of assets in productFamilies from above");
    //    Assert.AreEqual("All Assets", l[0].CountOf, "Unable to retrieve CategoryCount");
    //    // temp ignore till other filters available. should be == 3
    //    Assert.IsTrue((l[0].Count >= 3), "Should be assets from above");
    //    return null;
    //  });
    //}

    ///// <summary>
    /////  Product family filter: Unassigned
    ///// </summary>
    //[TestMethod]
    //public void AssetCount_CaseInsensitive()
    //{
    //  // don't allow null/spaces to be stored into AssetType
    //  // change it to  "Unassigned"
    //  var assetEvent1 = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = Guid.NewGuid().ToString().ToUpper() };
    //  var assetEvent2 = new CreateAssetEvent()
    //  { AssetUID = Guid.NewGuid(), AssetType = Guid.NewGuid().ToString().ToUpper() };

    //  string[] productFamily = new string[] { assetEvent1.AssetType.ToLower(), assetEvent2.AssetType.ToLower() };

    //  var assetContext = new AssetRepository(ConfigSettings.GetConnectionString("VSPDB"));
    //  assetContext.InRollbackTransaction<object>(o =>
    //  {
    //    var s = assetContext.StoreAsset(assetEvent1).Result;
    //    s = assetContext.StoreAsset(assetEvent2).Result;

    //    var l = assetContext.GetAssetCount(null, productFamily).Result;
    //    Assert.AreEqual(1, l.Count(), "Should be count of assets in productFamilies from above");
    //    Assert.AreEqual("All Assets", l[0].CountOf, "Unable to retrieve CategoryCount");
    //    Assert.AreEqual(2, l[0].Count, "Should be assets from above");
    //    return null;
    //  });
    //}

    //#endregion

  }

}

