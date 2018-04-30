using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

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
      Log4NetProvider.RepoName = loggerRepoName;
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
        .AddTransient<IRepository<IFilterEvent>, FilterRepository>()
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
        ModelYear = assetEvent.ModelYear,
        AssetType = assetEvent.AssetType,
        IconKey = assetEvent.IconKey,
        EquipmentVIN = assetEvent.EquipmentVIN,
        OwningCustomerUID = assetEvent.OwningCustomerUID.ToString(),
        IsDeleted = false,
        LastActionedUtc = assetEvent.ActionUTC
      };

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {
        var g = await assetContext.GetAsset(asset.AssetUID);
        Assert.IsNull(g, "Asset shouldn't be there yet");

        var s = await assetContext.StoreEvent(assetEvent);
        Assert.AreEqual(1, s, "Asset event not written");

        g = await assetContext.GetAsset(asset.AssetUID);
        Assert.IsNotNull(g, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(asset, g, "Asset details are incorrect from AssetRepo");
        return null;
      }).Wait();
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
        EquipmentVIN = assetEvent1.EquipmentVIN,
        IsDeleted = false,
        LastActionedUtc = assetEvent2.ActionUTC
      };

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {
        var s = await assetContext.StoreEvent(assetEvent1);
        Assert.AreEqual(1, s, "Asset event not written");

        s = await assetContext.StoreEvent(assetEvent2);
        Assert.AreEqual(1, s, "Asset event not written");

        var g = await assetContext.GetAssets(new[] { "DRUM TYPE TRACTORS" });
        Assert.IsNotNull(g, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(asset2, g.FirstOrDefault(), "Asset details are incorrect from AssetRepo");
        return null;
      }).Wait();
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

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {

        var s = await assetContext.StoreEvent(assetEvent1);
        Assert.AreEqual(1, s, "Asset event not written");

        s = await assetContext.StoreEvent(assetEvent2);
        Assert.AreEqual(1, s, "Asset event not written");


        var g = await assetContext.GetAssets(new[] { "DRUM TYPE TRACTORS1", "TRACK TYPE TRACTORS1" });
        Assert.IsNotNull(g, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(2, g.Count(), "Asset details are incorrect from AssetRepo");
        return null;
      }).Wait();
    }

    [TestMethod]
    public void CreateAssetProductFamily_CaseInsensitiveQuery()
    {
      var assetEvent1 = new CreateAssetEvent()
      { AssetUID = Guid.NewGuid(), AssetType = "TRACK TYPE TRACTORS1" };

      var assetEvent2 = new CreateAssetEvent()
      { AssetUID = Guid.NewGuid(), AssetType = "DRUM TYPE TRACTORS1" };

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {
        var s = await assetContext.StoreEvent(assetEvent1);
        s = await assetContext.StoreEvent(assetEvent2);

        var g = await assetContext.GetAssets(new[] { "DRUM TYPE Tractors1", "track type Tractors1" });
        Assert.IsNotNull(g, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(2, g.Count(), "Asset count is incorrect from AssetRepo");
        return null;
      }).Wait();
    }

    [TestMethod]
    public void CreateAssetProductFamily_CaseInsensitiveDB()
    {
      var assetEvent1 = new CreateAssetEvent()
      { AssetUID = Guid.NewGuid(), AssetType = "track type Tractors1" };

      var assetEvent2 = new CreateAssetEvent()
      { AssetUID = Guid.NewGuid(), AssetType = "DRUM TYPE Tractors1" };

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {
        var s = await assetContext.StoreEvent(assetEvent1);
        s = await assetContext.StoreEvent(assetEvent2);

        var g = await assetContext.GetAssets(new[] { "DRUM TYPE TRACTORS1", "TRACK TYPE TRACTORS1" });
        Assert.IsNotNull(g, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(2, g.Count(), "Asset count is incorrect from AssetRepo");
        return null;
      }).Wait();
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

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {
        var s = await assetContext.StoreEvent(assetEvent1);
        s = await assetContext.StoreEvent(assetEvent2);
        s = await assetContext.StoreEvent(assetEvent3);

        var g = await assetContext.GetAssets(new[] { "UNASSIGNED" });
        Assert.IsNotNull(g, "Unable to retrieve Asset from AssetRepo");
        Assert.IsTrue((g.Count() >= 3), "Asset count is incorrect from AssetRepo");
        return null;
      }).Wait();
    }

    [TestMethod]
    public void UpdateAssetProductFamily_DefaultIsUnassigned()
    {
      var assetEvent = new CreateAssetEvent()
      { AssetUID = Guid.NewGuid(), AssetType = "Track type tractor2" };

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {
        var s = await assetContext.StoreEvent(assetEvent);
        assetEvent.AssetType = "Track type tractor3";
        s = await assetContext.StoreEvent(assetEvent);

        var g = await assetContext.GetAssets(new[] { "Track type tractor3" });
        Assert.IsNotNull(g, "Unable to retrieve Asset from AssetRepo");
        // can't do == 1 until we can filter by e.g. customer
        Assert.IsTrue((g.Any()), "Asset count is incorrect from AssetRepo");
        return null;
      }).Wait();
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
        OwningCustomerUID = Guid.NewGuid(),
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
        OwningCustomerUID = assetEvent.OwningCustomerUID.ToString(),
        EquipmentVIN = assetEvent.EquipmentVIN,
        ModelYear = assetEvent.ModelYear,
        IsDeleted = false,
        LastActionedUtc = assetEvent.ActionUTC
      };

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {
        var s = await assetContext.StoreEvent(assetEvent);
        Assert.AreEqual(1, s, "Asset event not written");

        var g = await assetContext.GetAsset(asset.AssetUID);
        Assert.IsNotNull(g, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(asset, g, "Asset details are incorrect from AssetRepo");
        return null;
      }).Wait();
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
        OwningCustomerUID = Guid.NewGuid(),
        ModelYear = 1880,
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
        ModelYear = 1980, 
        ActionUTC = firstCreatedUTC
      };

      var asset = new Asset
      {
        AssetUID = assetEventOriginal.AssetUID.ToString(),
        Name = null,
        LegacyAssetID = assetEventLater.LegacyAssetId,
        SerialNumber = assetEventLater.SerialNumber,
        MakeCode = assetEventLater.MakeCode,
        Model = assetEventLater.Model,
        ModelYear= assetEventOriginal.ModelYear,
        AssetType = assetEventLater.AssetType,
        IconKey = assetEventLater.IconKey,
        EquipmentVIN = assetEventOriginal.EquipmentVIN,
        OwningCustomerUID = assetEventOriginal.OwningCustomerUID.ToString(),
        IsDeleted = false,
        LastActionedUtc = assetEventLater.ActionUTC
      };

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {
        var s = await assetContext.StoreEvent(assetEventOriginal);
        var g = await assetContext.GetAsset(asset.AssetUID);
        Assert.AreEqual(asset, g, "Unable to retrieve Asset from AssetRepo");

        s = await assetContext.StoreEvent(assetEventLater);

        // these should be updated now
        asset.ModelYear = assetEventLater.ModelYear;
        asset.Name = assetEventLater.AssetName; 
        g = await assetContext.GetAsset(asset.AssetUID);
        Assert.IsNotNull(g, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(asset, g, "Asset details are incorrect from AssetRepo");
        return null;
      }).Wait();
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
        IsDeleted = false,
        LastActionedUtc = assetEventUpdate.ActionUTC
      };

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {
        var s = await assetContext.StoreEvent(assetEventCreate);
        s = await assetContext.StoreEvent(assetEventUpdate);

        var g = await assetContext.GetAsset(assetFinal.AssetUID);
        Assert.IsNotNull(g, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(assetFinal, g, "Asset details are incorrect from AssetRepo");
        return null;
      }).Wait();
    }


    /// <summary>
    /// update contains null values. should ignore those.
    /// </summary>
    [TestMethod]
    public void UpdateAsset_IgnoreNullValues()
    {
      DateTime firstCreatedUTC = new DateTime(2015, 1, 1, 2, 30, 3);
      var assetEventCreate = new CreateAssetEvent()
      {
        AssetUID = Guid.NewGuid(),
        AssetName = "AnAssetName",
        AssetType = "TRACK TYPE TRACTORS",
        EquipmentVIN = "equipVin",
        IconKey = 23,
        LegacyAssetId = 33334444,
        MakeCode = "J82",
        Model = "D6RXL",
        ModelYear = 1880,
        OwningCustomerUID = Guid.NewGuid(),
        SerialNumber = "S6T00561",       
        ActionUTC = firstCreatedUTC
      };

      var assetEventUpdate = new UpdateAssetEvent()
      {
        AssetUID = assetEventCreate.AssetUID,
        AssetName = null,
        AssetType = null,
        EquipmentVIN = null,
        IconKey = null,
        LegacyAssetId = null,        
        Model = null,
        ModelYear = null,
        OwningCustomerUID = null,       
        ActionUTC = firstCreatedUTC.AddMinutes(10)
      };

      var assetFinal = new Asset
      {
        AssetUID = assetEventCreate.AssetUID.ToString(),
        Name = assetEventCreate.AssetName,
        AssetType = assetEventCreate.AssetType,
        EquipmentVIN = assetEventCreate.EquipmentVIN,
        IconKey = assetEventCreate.IconKey,
        LegacyAssetID = assetEventCreate.LegacyAssetId,        
        MakeCode = assetEventCreate.MakeCode,
        Model = assetEventCreate.Model,
        ModelYear = assetEventCreate.ModelYear,
        OwningCustomerUID = assetEventCreate.OwningCustomerUID.ToString(),
        SerialNumber = assetEventCreate.SerialNumber,
        IsDeleted = false,
        LastActionedUtc = assetEventUpdate.ActionUTC
      };

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {
        var s = await assetContext.StoreEvent(assetEventCreate);
        s = await assetContext.StoreEvent(assetEventUpdate);

        var g = await assetContext.GetAsset(assetFinal.AssetUID);
        Assert.IsNotNull(g, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(assetFinal, g, "Asset details are incorrect from AssetRepo");
        return null;
      }).Wait();
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
        IsDeleted = false,
        LastActionedUtc = assetEventUpdateLater.ActionUTC
      };

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {
        var s = await assetContext.StoreEvent(assetEventCreate);
        s = await assetContext.StoreEvent(assetEventUpdateLater);
        s = await assetContext.StoreEvent(assetEventUpdateEarlier);

        var g = await assetContext.GetAsset(assetFinal.AssetUID);
        Assert.IsNotNull(g, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(assetFinal, g, "Asset details are incorrect from AssetRepo");
        return null;
      }).Wait();
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
        OwningCustomerUID = null,
        IsDeleted = false,
        LastActionedUtc = assetEventUpdate.ActionUTC
      };

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {
        var s = await assetContext.StoreEvent(assetEventUpdate);
        s = await assetContext.StoreEvent(assetEventCreate);

        var g = await assetContext.GetAsset(assetFinal.AssetUID);
        Assert.IsNotNull(g, "Unable to retrieve Asset from AssetRepo");
        Assert.AreEqual(assetFinal, g, "Asset details are incorrect from AssetRepo");
        return null;
      }).Wait();
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
        IsDeleted = true,
        LastActionedUtc = assetEventDelete.ActionUTC
      };

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {
        var s = await assetContext.StoreEvent(assetEventCreate);
        s = await assetContext.StoreEvent(assetEventDelete);

        var g = await assetContext.GetAsset(assetFinal.AssetUID);
        Assert.IsNull(g, "Should not be able to retrieve a deleted Asset");

        var l = await assetContext.GetAllAssetsInternal();
        Assert.IsNotNull(l, "Unable to retrieve any Assets from AssetRepo");
        Assert.IsTrue(((List<Asset>)l).Contains(assetFinal), "Unable to retrieve Asset from AssetRepo");
        return null;
      }).Wait();
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

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {
        var s = await assetContext.StoreEvent(assetEventDelete);
        Assert.AreEqual(1, s, "Asset event not written");

        var g = await assetContext.GetAsset(assetFinal.AssetUID);
        Assert.IsNull(g, "Should not be able to retrieve a deleted Asset");

        var l = await assetContext.GetAllAssetsInternal();
        Assert.IsNotNull(l, "Unable to retrieve any Assets from AssetRepo");
        Assert.IsTrue(((List<Asset>)l).Contains(assetFinal), "Unable to retrieve Asset from AssetRepo");
        return null;
      }).Wait();
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

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {
        var s = await assetContext.StoreEvent(assetEventDelete);
        s = await assetContext.StoreEvent(assetEventCreate);

        var g = await assetContext.GetAsset(assetFinal.AssetUID);
        Assert.IsNull(g, "Should not be able to retrieve a deleted Asset");

        var l = await assetContext.GetAllAssetsInternal();
        Assert.IsNotNull(l, "Unable to retrieve any Assets from AssetRepo");
        Assert.IsTrue(((List<Asset>)l).Contains(assetFinal), "Unable to retrieve Asset from AssetRepo");
        return null;
      }).Wait();
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

      assetContext.InRollbackTransactionAsync<object>(async o =>
      {
        var s = await assetContext.StoreEvent(assetEventDelete);
        s = await assetContext.StoreEvent(assetEventUpdate);

        var g = await assetContext.GetAsset(assetFinal.AssetUID);
        Assert.IsNull(g, "Should not be able to retrieve a deleted Asset");

        var l = await assetContext.GetAllAssetsInternal();
        Assert.IsNotNull(l, "Unable to retrieve any Assets from AssetRepo");
        Assert.IsTrue(((List<Asset>)l).Contains(assetFinal), "Unable to retrieve Asset from AssetRepo");
        return null;
      }).Wait();
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

