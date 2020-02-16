using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.NH_OPMockObjectSet;
using VSS.UnitTest.Common;
using VSS.UnitTest.Common.Contexts;

namespace UnitTests
{
  [TestClass]
  public class EquipmentAPITest : UnitTestBase
  {
    [DatabaseTest]
    [TestMethod]
    public void CreateAddsDefaultStoreId()
    {
      API.Equipment.Create(Ctx.OpContext, "TestAsset", "Cat", "TestAsset", 0, DeviceTypeEnum.MANUALDEVICE, "Truck", "Test", 1999, Guid.NewGuid());

      long storeId = (from a in Ctx.OpContext.AssetReadOnly where a.SerialNumberVIN == "TestAsset" select a.fk_StoreID).FirstOrDefault();

      Assert.AreEqual(1, storeId);
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateAddsStoreId()
    {
      var store = new Store();
      store.Name = "TestStore";
      store.Description = "TestStore";
      Ctx.OpContext.Store.AddObject(store);
      Ctx.OpContext.SaveChanges();
      API.Equipment.Create(Ctx.OpContext, "TestAsset", "Cat", "TestAsset", 0, DeviceTypeEnum.MANUALDEVICE, "Truck", "Test", 1999, Guid.NewGuid(), null, store.ID);

      long storeId = (from a in Ctx.OpContext.AssetReadOnly where a.SerialNumberVIN == "TestAsset" select a.fk_StoreID).FirstOrDefault();

      Assert.AreEqual(store.ID, storeId);
    }

    

    [DatabaseTest]
    [TestMethod()]
    public void GetOemPMSalesModel_Test()
    {
      PMSalesModel model = Entity.PMSalesModel.SerialNumberPrefix("UTA").StartRange(1).EndRange(99999).ExternalID(777).Save();
      PMSalesModel model2 = Entity.PMSalesModel.SerialNumberPrefix(string.Empty).ExternalID(123).MakeModel("CIH", "MyModel").Save();
 
      //Set up some assets: asset1 & asset2 is CAT model, asset3 is non-CAT or rather no match in SalesModel and asset4 is blank model
      TestData.TestAssetPL321.Device.GpsDeviceID = "UTAAAA";
      TestData.TestAssetMTS521.Device.GpsDeviceID = "UTABBB";
      TestData.TestAssetMTS522.Device.GpsDeviceID = "UTACBA";
      TestData.TestAssetMTS523.Device.GpsDeviceID = "UTAFED";
      
      TestData.TestAssetPL321.Model = "Acme";
      TestData.TestAssetMTS521.Model = "Acme";
      TestData.TestAssetMTS522.Model = "Acme";
      TestData.TestAssetMTS523.Model = string.Empty;
      
      TestData.TestAssetPL321.SerialNumberVIN = "UTA00108";
      TestData.TestAssetMTS521.SerialNumberVIN = "UTA00109";
      TestData.TestAssetMTS522.SerialNumberVIN = "test1";
      TestData.TestAssetMTS523.SerialNumberVIN = "test2";
      
      Asset caseAsset = Entity.Asset.WithDevice(TestData.TestSNM940).MakeCode("CIH").ModelName("MyModel").SerialNumberVin("TESTDATA_ASSET_SNM940").SyncWithRpt().Save();

      Ctx.OpContext.SaveChanges();

      long pmSalesModelID1 = API.Equipment.GetOemPMSalesModelID(Ctx.OpContext, TestData.TestAssetPL321.AssetID);
      Assert.AreEqual(model.ID, pmSalesModelID1, "Wrong sales model for asset1");

      long pmSalesModelID2 = API.Equipment.GetOemPMSalesModelID(Ctx.OpContext, TestData.TestAssetMTS521.AssetID);
      Assert.AreEqual(model.ID, pmSalesModelID2, "Wrong sales model for asset2");

      long pmSalesModelID3 = API.Equipment.GetOemPMSalesModelID(Ctx.OpContext, TestData.TestAssetMTS522.AssetID);
      Assert.AreEqual(EquipmentAPI.DEFAULT_SALES_MODEL_ID, pmSalesModelID3, "Wrong sales model for asset3");

      long pmSalesModelID4 = API.Equipment.GetOemPMSalesModelID(Ctx.OpContext, TestData.TestAssetMTS523.AssetID);
      Assert.AreEqual(EquipmentAPI.DEFAULT_SALES_MODEL_ID, pmSalesModelID4, "Wrong sales model for asset4");

      long pmSalesModelID5 = API.Equipment.GetOemPMSalesModelID(Ctx.OpContext, caseAsset.AssetID);
      Assert.AreEqual(model2.ID, pmSalesModelID5, "Wrong sales model for asset5");

    }

    [DatabaseTest]
    [TestMethod]
    public void Create_SuccessCreatingAsset()
    {
      var target = new EquipmentAPI();
      var expected = CreateAssetHelper(target);

      var actual = (Ctx.OpContext.AssetReadOnly.Where(a => a.AssetID == expected.AssetID)).SingleOrDefault();
      var pmIntervalInstances = (Ctx.OpContext.PMIntervalInstanceReadOnly.Where(a => a.fk_AssetID == expected.AssetID).Select(k => k)).ToList();
      AssetAssertHelper(expected, actual);

      // Check for VL default intervals in PMIntervalInstance table
      Assert.AreEqual(4, pmIntervalInstances.Count, "There should be 4 VL default interval instances for this asset");
    }
    
    [DatabaseTest]
    [TestMethod]
    public void Create_SuccessAssociateDeviceTest()
    {
      var target = new EquipmentAPI();
      var customer = TestData.TestCustomer;
      var asset = CreateAssetHelper(target);

      var device = API.Device.CreateDevice(Ctx.OpContext, "-1234712874362183746", customer.BSSID,
                                           "GPS_Device_45", DeviceTypeEnum.Series521, TimeSpan.FromMinutes(5),
                                           TimeSpan.FromHours(2), TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10), true);
      device.Asset.Add(asset);
      var result = Ctx.OpContext.SaveChanges();
      var actual = (Ctx.OpContext.DeviceReadOnly.Where(d => d.ID == device.ID)
                                                .Select(d => d.Asset)
                                                .SingleOrDefault()).ToList();

      Assert.AreEqual(1, actual.Count, "Only one asset should be associated with the device.");
      Assert.AreEqual(asset.AssetID, actual[0].AssetID, "Asset ids do not match.");

    }

    

    [DatabaseTest]
    [TestMethod]
    public void CreateAsset_SuccessWheelTractorScrapperDualEngine()
    {
        var target = new EquipmentAPI();
        var testAsset1 = target.Create(Ctx.OpContext, "TEST_ASSET1", "CAT", "SERIAL_NUMBER_1", TestData.TestPL321.ID,
                                       (DeviceTypeEnum)TestData.TestPL321.fk_DeviceTypeID, "WHEEL TRACTOR SCRAPERS", "627B", 1999, Guid.NewGuid());
        Assert.IsTrue(testAsset1.fk_ModelVariant == 2);
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateAsset_UpdateAsset_ModelAndProductFamilyInDataBase_AssetDoesNotGetUpdated_Success()
    {
      //Add Sales Model
      var productFamily = Entity.ProductFamily.Save();
      var salesModel = Entity.SalesModel.ForProductFamily(productFamily).Save();
      var assetSerialNumber = string.Format("{0}00123", salesModel.SerialNumberPrefix);

      var target = new EquipmentAPI();
      // create a Caterpillar asset
      var testAsset = target.Create(Ctx.OpContext, "TEST_ASSET1", "CAT", assetSerialNumber, TestData.TestPL321.ID,
                                     (DeviceTypeEnum)TestData.TestPL321.fk_DeviceTypeID, productFamily.Description, salesModel.Description, 1999, Guid.NewGuid(), "VIN");
      var fieldsToUpdate = new List<Param>
      {
        new Param {Name = "Name", Value = "NEW_NAME"},
        new Param {Name = "ProductFamilyName", Value = "NEW_PRODUCT_FAMILY_NAME"},
        new Param {Name = "Model", Value = "NEW_MODEL"},
        new Param {Name = "EquipmentVIN", Value = "NEW_VIN"}
      };
      target.Update(Ctx.OpContext, testAsset.AssetID, fieldsToUpdate);
      Asset assetFromDB = (from a in Ctx.OpContext.Asset where a.AssetID == testAsset.AssetID select a).SingleOrDefault();
      // verify these fields were updated
      Assert.AreEqual("NEW_NAME", assetFromDB.Name, "Wrong name");
      Assert.AreEqual("NEW_VIN", assetFromDB.EquipmentVIN, "Wrong equipment VIN");
      // verify these fields were not updated
      Assert.AreEqual(productFamily.Description, assetFromDB.ProductFamilyName, "Product family should not have been updated");
      Assert.AreEqual(salesModel.Description, assetFromDB.Model, "Model should not have been updated");
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateAsset_UpdateAsset_ModelAndProductFamilyNotInDataBase_AssetDoesGetUpdated_Success()
    {
      var target = new EquipmentAPI();
      // create a Caterpillar asset
      var testAsset = target.Create(Ctx.OpContext, "TEST_ASSET1", "CAT", "SERIAL_NUMBER_1", TestData.TestPL321.ID,
                                     (DeviceTypeEnum)TestData.TestPL321.fk_DeviceTypeID, "WHEEL TRACTOR SCRAPERS", "627B", 1999, Guid.NewGuid(), "VIN");
      List<Param> fieldsToUpdate = new List<Param>();
      fieldsToUpdate.Add(new Param { Name = "Name", Value = "NEW_NAME"});
      fieldsToUpdate.Add(new Param { Name = "ProductFamilyName", Value = "NEW_PRODUCT_FAMILY_NAME"});
      fieldsToUpdate.Add(new Param { Name = "Model", Value = "NEW_MODEL" });
      fieldsToUpdate.Add(new Param { Name = "EquipmentVIN", Value = "NEW_VIN" });
      target.Update(Ctx.OpContext, testAsset.AssetID, fieldsToUpdate);
      Asset assetFromDB = (from a in Ctx.OpContext.Asset where a.AssetID == testAsset.AssetID select a).SingleOrDefault();
      // verify these fields were updated
      Assert.AreEqual("NEW_NAME", assetFromDB.Name, "Wrong name");
      Assert.AreEqual("NEW_VIN", assetFromDB.EquipmentVIN, "Wrong equipment VIN");
      // verify these fields were not updated
      Assert.AreEqual("NEW_PRODUCT_FAMILY_NAME", assetFromDB.ProductFamilyName, "Product family should have been updated");
      Assert.AreEqual("NEW_MODEL", assetFromDB.Model, "Model should have been updated");
    }
    
    [DatabaseTest]
    [TestMethod]
    public void UpdateWorkingDefinition_Success()
    {
      var target = new EquipmentAPI();
      var testAsset = TestData.TestAssetMTS521;
      Entity.AssetWorkingDefinition.ForAsset(testAsset).Save();

      bool success = target.UpdateWorkingDefinition(Ctx.OpContext, testAsset.AssetID, WorkDefinitionEnum.MovementandSensorEvents, 1, false);
      Assert.IsTrue(success, "Expect update to succeed, first time");

      var asSavedToDb = (from au in Ctx.OpContext.AssetWorkingDefinitionReadOnly
                         where au.fk_AssetID == testAsset.AssetID
                              && au.SensorNumber == 1
                              && au.SensorStartIsOn == false
                         select au).FirstOrDefault();

      Assert.IsNotNull(asSavedToDb, "Looks like we got a problem fellas - asset util not saved to DBdoobey");
      Assert.AreEqual((int)WorkDefinitionEnum.MovementandSensorEvents, asSavedToDb.fk_WorkDefinitionID, "Working defn not saved");

      success = target.UpdateWorkingDefinition(Ctx.OpContext, testAsset.AssetID, WorkDefinitionEnum.SensorEvents, 2, true);
      Assert.IsTrue(success, "Update existing should work");
      asSavedToDb = (from au in Ctx.OpContext.AssetWorkingDefinitionReadOnly
                     where au.fk_AssetID == testAsset.AssetID
                           && au.SensorNumber == 2
                           && au.SensorStartIsOn
                     select au).FirstOrDefault();
      Assert.IsNotNull(asSavedToDb, "Looks like we got a problem fellas - asset util not saved to DBdoobey");
      Assert.AreEqual((int)WorkDefinitionEnum.SensorEvents, asSavedToDb.fk_WorkDefinitionID, "Working defn not saved");
    }

    [TestMethod]
    [DatabaseTest]
    public void RemovedDisableStoppedNotificationMeterDelta_17309_Test()
    {
      var target = new EquipmentAPI();
      var testAsset = TestData.TestAssetMTS522;
      Entity.AssetWorkingDefinition.ForAsset(testAsset).Save();

      bool success = target.UpdateWorkingDefinition(Ctx.OpContext, testAsset.AssetID, WorkDefinitionEnum.MeterDelta, 1, false);
      Assert.IsTrue(success, "Expect update to succeed, first time");
      
      using(INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
      {
          var mtsOutMsg = (from o in opCtx1.MTSOutReadOnly
                         where
                           o.SerialNumber == TestData.TestMTS522.GpsDeviceID && o.PacketID == 0x03 && o.TypeID == 0x15 &&
                           o.SubTypeID == 0x04
                         select o).FirstOrDefault();
        Assert.IsNull(mtsOutMsg, "there should be no stopped notification message");
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void RemovedDisableStoppedNotificationSensorOnly_17309_Test()
    {
      var target = new EquipmentAPI();
      var testAsset = TestData.TestAssetMTS522;
      Entity.AssetWorkingDefinition.ForAsset(testAsset).Save();

      bool success = target.UpdateWorkingDefinition(Ctx.OpContext, testAsset.AssetID, WorkDefinitionEnum.SensorEvents, 1, false);
      Assert.IsTrue(success, "Expect update to succeed, first time");

      using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
      {
          var mtsOutMsg = (from o in opCtx1.MTSOutReadOnly
                         where
                           o.SerialNumber == TestData.TestMTS522.GpsDeviceID && o.PacketID == 0x03 && o.TypeID == 0x15 &&
                           o.SubTypeID == 0x04
                         select o).FirstOrDefault();
        Assert.IsNull(mtsOutMsg, "there should be no stopped notification message");
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void RemovedDisableStoppedNotificationMovement_17309_Test()
    {
      var target = new EquipmentAPI();
      var testAsset = TestData.TestAssetMTS522;
      Entity.AssetWorkingDefinition.ForAsset(testAsset).Save();

      bool success = target.UpdateWorkingDefinition(Ctx.OpContext, testAsset.AssetID, WorkDefinitionEnum.MovementEvents, 1, false);
      Assert.IsTrue(success, "Expect update to succeed, first time");

      using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
      {
          var mtsOutMsg = (from o in opCtx1.MTSOutReadOnly
                         where
                           o.SerialNumber == TestData.TestMTS522.GpsDeviceID && o.PacketID == 0x03 && o.TypeID == 0x15 &&
                           o.SubTypeID == 0x04
                         select o).FirstOrDefault();
        Assert.IsNull(mtsOutMsg, "there should be no stopped notification message");
      }
    }
    
    [DatabaseTest]
    [TestMethod]
    public void Create_SuccessBug12057()
    {
      var target = new EquipmentAPI();
      var testAsset1 = target.Create(Ctx.OpContext, "TEST_ASSET", "TTT", "SERIAL_NUMBER", TestData.TestMTS521.ID,
                                     (DeviceTypeEnum)TestData.TestMTS521.fk_DeviceTypeID, "FAMILY_DESC", "MODEL_DESC", 1999, Guid.NewGuid());
      var assetBurnRates = (Ctx.OpContext.AssetBurnRatesReadOnly.Where(a=>a.fk_AssetID == testAsset1.AssetID)).FirstOrDefault();
      Assert.IsNull(assetBurnRates.EstimatedIdleBurnRateGallonsPerHour);
      Assert.IsNull(assetBurnRates.EstimatedWorkingBurnRateGallonsPerHour);
    }

    
    [TestMethod]
    [DatabaseTest]
    public void Create_SuccessPL641AndPLE641()
    {
      var target = new EquipmentAPI();
      var testAsset1 = target.Create(Ctx.OpContext, "TEST_ASSET1", "CAT", "SN1111", TestData.TestPL641.ID,
                                     (DeviceTypeEnum)TestData.TestPL641.fk_DeviceTypeID, "FAMILY_DESC", "MODEL_DESC", 1999, Guid.NewGuid());
      var testAsset2 = target.Create(Ctx.OpContext, "TEST_ASSET2", "CAT", "SN1112", TestData.TestPLE641.ID,
                                     (DeviceTypeEnum)TestData.TestPLE641.fk_DeviceTypeID, "FAMILY_DESC", "MODEL_DESC", 1999, Guid.NewGuid());
      Assert.IsNotNull(testAsset1,"Asset creation failed for PL641");
      Assert.IsNotNull(testAsset2, "Asset creation failed for PLE641");
    }

    [TestMethod]
    [DatabaseTest]
    public void Create_SuccessDCM300()
    {
      var target = new EquipmentAPI();
      var testAsset1 = target.Create(Ctx.OpContext, "TEST_ASSET1", "CAT", "SN0000", TestData.TestDCM300.ID,
                                     (DeviceTypeEnum)TestData.TestDCM300.fk_DeviceTypeID, "FAMILY_DESC", "MODEL_DESC", 1999, Guid.NewGuid());

      var asSavedToDb = (from au in Ctx.OpContext.AssetWorkingDefinitionReadOnly
                         where au.fk_AssetID == testAsset1.AssetID
                         select au).FirstOrDefault();
      Assert.AreEqual((int)WorkDefinitionEnum.MovementEvents, asSavedToDb.fk_WorkDefinitionID, "Working defn not saved");

      Assert.IsNotNull(testAsset1, "Asset creation failed for PLE641");
    }

    [DatabaseTest]
    [TestMethod]
    public void GetAssetDefaultIconIDTest()
    {
      var target = new EquipmentAPI();
      var asset = CreateAssetHelper(target);

      var actual = (Ctx.OpContext.AssetReadOnly.Where(a => a.AssetID == asset.AssetID)).FirstOrDefault();

      Assert.AreEqual((int) IconEnum.Default, actual.IconID, "CAT Asset has incorrect default icon.");
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateAssetIconTest()
    {
      var target = new EquipmentAPI();
      var asset = CreateAssetHelper(target);
      asset.IconID = (int)IconEnum.Skidders;

      var result = Ctx.OpContext.SaveChanges();

      var actual = (Ctx.OpContext.AssetReadOnly.Where(a => a.AssetID == asset.AssetID)).FirstOrDefault();

      Assert.AreEqual((int)IconEnum.Skidders, actual.IconID, "Asset has incorrect icon.");
    }

   
    // Test stored procedure onboarding API
    [TestMethod()]
    [DatabaseTest()]
    public void GetAssetsDueForOnboardingProcess_3()
    {
      Asset a = TestData.TestAssetMTS522;

      StoredProcDefinition sproc = new StoredProcDefinition("NH_OP", true, "SELECT * FROM dbo.tvf_GetAssetsDueForOnboardingProcess(1)");
      
      System.Data.SqlClient.SqlDataReader rdr = SqlAccessMethods.ExecuteReader(sproc);

      List<long> list = new List<long>();
      while (rdr.Read())
      {
        list.Add(rdr.GetInt64(0));
      }

      Assert.AreEqual(1, list.Count, "Asset should be due for OEMData processing");
      Assert.AreEqual(a.AssetID, list[0], "Expected MY asset.");
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateCASEAsset_ProductFamilyGetUpdated_Success()
    {
      var target = new EquipmentAPI();
      // create a CASE asset
      var testAsset = target.Create(Ctx.OpContext, "CASE_ASSET", "CIH", "SERIAL_NUMBER_1", TestData.TestMTS521.ID,
                                     (DeviceTypeEnum)TestData.TestMTS521.fk_DeviceTypeID, null, "Puma Tractor (T4A)", 1999, Guid.NewGuid(), "VIN");
      
      Asset assetFromDB = (from a in Ctx.OpContext.Asset where a.AssetID == testAsset.AssetID select a).SingleOrDefault();
      // verify these fields were updated
      Assert.AreEqual("AGRICULTURAL TRACTORS", assetFromDB.ProductFamilyName, "Wrong product family name");      
    }
    [DatabaseTest]
    [TestMethod]
    public void GetCorrectIcon_Success()
    {
      var target = new EquipmentAPI();
      var icon = Entity.Icon.Description("TrackTypeTractors").Save();      
      var salesModel = Entity.SalesModel.ModelCode("D6H").SerialNumberPrefix("6CF").StartRange(1).EndRange(4014).Description(
          "D6H").ForIcon(icon).Save();
      
      // create a CASE asset
      var testAsset = target.Create(Ctx.OpContext, "CASE_ASSET", "CIH", "SERIAL_NUMBER_1", TestData.TestMTS521.ID,
                                     (DeviceTypeEnum)TestData.TestMTS521.fk_DeviceTypeID, null, "D6H", 1999, Guid.NewGuid(), "VIN");

      Asset assetFromDB = (from a in Ctx.OpContext.Asset where a.AssetID == testAsset.AssetID select a).SingleOrDefault();
      Assert.AreEqual(icon.ID, assetFromDB.IconID, "Icons Do not Match");
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateNHAsset_ProductFamilyGetUpdated_Success()
    {
      var target = new EquipmentAPI();
      // create a CASE asset
      var testAsset = target.Create(Ctx.OpContext, "NH_ASSET", "NH", "SERIAL_NUMBER_1", TestData.TestMTS521.ID,
                                     (DeviceTypeEnum)TestData.TestMTS521.fk_DeviceTypeID, null, "FR SPFH", 1999, Guid.NewGuid(), "VIN");

      Asset assetFromDB = (from a in Ctx.OpContext.Asset where a.AssetID == testAsset.AssetID select a).SingleOrDefault();
      // verify these fields were updated
      Assert.AreEqual("FORAGE HARVESTER", assetFromDB.ProductFamilyName, "Wrong product family name");
    }

    [TestMethod]
    public void UpdateAssetModel_CATStrategy_SalesModelFound_UpdateModel()
    {
      //Add Sales Model
      var productFamily = Entity.ProductFamily.Name("OHT").Description("OFF HIGHWAY TRUCKS").Save();
      var salesModel = Entity.SalesModel.ModelCode("795FAC").SerialNumberPrefix("ERM").StartRange(1).EndRange(99999).Description(
          "795FAC").ForProductFamily(productFamily).Save();
      var device = Entity.Device.PL321.GpsDeviceId("DQCAT0152967Q1").DeviceState(DeviceStateEnum.Subscribed).OwnerBssId(TestData.TestDealer.BSSID).Save();
      var asset =
        Entity.Asset.Name("2330").SerialNumberVin("ERM00279").MakeCode("CAT").ModelName("795F").ManufactureYear(2013).
          InsertUtc(DateTime.UtcNow).UpdateUtc(DateTime.UtcNow).ProductFamily("OFF HIGHWAY TRUCKS").IconID(15).
          WithDevice(device).IsMetric(false).Save();
      var modifiedProperties = new List<Param> {new Param {Name = "Model", Value = "795FAC"}};
      API.Equipment.Update(Ctx.OpContext, asset.AssetID, modifiedProperties);
      Assert.AreEqual(salesModel.ModelCode, asset.Model);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateAssetVINUsingAssetUid()
    {
      var device = Entity.Device.PL321.GpsDeviceId("DQCAT0152967Q1").DeviceState(DeviceStateEnum.Subscribed).OwnerBssId(TestData.TestDealer.BSSID).Save();
      var asset =
        Entity.Asset.Name("2330").SerialNumberVin("ERM00279").MakeCode("CAT").ModelName("795F").ManufactureYear(2013).
          InsertUtc(DateTime.UtcNow).UpdateUtc(DateTime.UtcNow).AssetUid(Guid.NewGuid()).EquipmentVIN("777559").ProductFamily("OFF HIGHWAY TRUCKS").IconID(15).
          WithDevice(device).IsMetric(false).Save();
      var modifiedProperties = new List<Param> { new Param { Name = "EquipmentVIN", Value = "123456" } };
      API.Equipment.UpdateByAssetUid(Ctx.OpContext, asset.AssetUID.Value, modifiedProperties);

      Asset assetFromDB = (from a in Ctx.OpContext.Asset where a.AssetID == asset.AssetID select a).SingleOrDefault();
      Assert.AreEqual("123456", assetFromDB.EquipmentVIN);
    }

    [TestMethod]
    public void UpdateAssetModel_CATStrategy_SalesModelFound_IgnoreModifiedModel()
    {
      //Add Sales Model
      var productFamily = Entity.ProductFamily.Name("OHT").Description("OFF HIGHWAY TRUCKS").Save();
      var salesModel = Entity.SalesModel.ModelCode("795FAC").SerialNumberPrefix("ERM").StartRange(1).EndRange(99999).Description(
          "795FAC").ForProductFamily(productFamily).Save();
      var device = Entity.Device.PL321.GpsDeviceId("DQCAT0152967Q1").DeviceState(DeviceStateEnum.Subscribed).OwnerBssId(TestData.TestDealer.BSSID).Save();
      var asset =
        Entity.Asset.Name("2330").SerialNumberVin("ERM00279").MakeCode("CAT").ModelName("795FAC").ManufactureYear(2013).
          InsertUtc(DateTime.UtcNow).UpdateUtc(DateTime.UtcNow).ProductFamily("OFF HIGHWAY TRUCKS").IconID(15).
          WithDevice(device).IsMetric(false).Save();
      var modifiedProperties = new List<Param> { new Param { Name = "Model", Value = "123F" } };
      API.Equipment.Update(Ctx.OpContext, asset.AssetID, modifiedProperties);
      Assert.AreEqual(salesModel.ModelCode, asset.Model);
    }

    [TestMethod]
    public void UpdateAssetModel_CATStrategy_SalesModelNotFound_UseModifiedModelAndProductFamily()
    {
      //Add Sales Model
      var productFamily = Entity.ProductFamily
                            .Name("OHT")
                            .Description("OFF HIGHWAY TRUCKS").Save();

      var salesModel = Entity.SalesModel
                        .ModelCode("795FAC")
                        .SerialNumberPrefix("ERM")
                        .StartRange(1)
                        .EndRange(99999)
                        .Description("795FAC")
                        .ForProductFamily(productFamily).Save();

      var device = Entity.Device.PL321
                    .GpsDeviceId("DQCAT0152967Q1")
                    .DeviceState(DeviceStateEnum.Subscribed)
                    .OwnerBssId(TestData.TestDealer.BSSID).Save();

      var asset = Entity.Asset
                    .Name("2330")
                    .SerialNumberVin("XXX00279") // Prefix XXX will not be found in SalesModel
                    .MakeCode("CAT")
                    .ModelName("795FAC")
                    .ManufactureYear(2013)
                    .InsertUtc(DateTime.UtcNow)
                    .UpdateUtc(DateTime.UtcNow)
                    .ProductFamily("OFF HIGHWAY TRUCKS")
                    .IconID(15)
                    .WithDevice(device)
                    .IsMetric(false).Save();

      var modifiedProperties = new List<Param> 
      { 
        new Param { Name = "Model", Value = "modifiedModelValue" },
        new Param { Name = "ProductFamilyName", Value = "modifiedProductFamilyNameValue" }
      };

      API.Equipment.Update(Ctx.OpContext, asset.AssetID, modifiedProperties);

      Assert.AreEqual(modifiedProperties[0].Value, asset.Model);
      Assert.AreEqual(modifiedProperties.Where(p => p.Name == "Model").First().Value, asset.Model);
      Assert.AreEqual(modifiedProperties.Where(p => p.Name == "ProductFamilyName").First().Value, asset.ProductFamilyName);
    }

    [TestMethod]
    public void UpdateAssetModel_NullStrategy_NoModelUpdate()
    {
      //Add Sales Model
      var productFamily = Entity.ProductFamily.Name("OHT").Description("OFF HIGHWAY TRUCKS").Save();
      var salesModel = Entity.SalesModel.ModelCode("795FAC").SerialNumberPrefix("ERM").StartRange(1).EndRange(99999).Description(
          "795FAC").ForProductFamily(productFamily).Save();
      var device = Entity.Device.PL321.GpsDeviceId("DQCAT0152967Q1").DeviceState(DeviceStateEnum.Subscribed).OwnerBssId(TestData.TestDealer.BSSID).Save();
      var asset =
        Entity.Asset.Name("2330").SerialNumberVin("ERM00279").MakeCode("House").ModelName("795F").ManufactureYear(2013).
          InsertUtc(DateTime.UtcNow).UpdateUtc(DateTime.UtcNow).ProductFamily("OFF HIGHWAY TRUCKS").IconID(15).
          WithDevice(device).IsMetric(false).Save();
      var modifiedProperties = new List<Param> { new Param { Name = "Model", Value = "795FAC" } };
      API.Equipment.Update(Ctx.OpContext, asset.AssetID, modifiedProperties);
      Assert.AreEqual(salesModel.ModelCode, asset.Model);
    }



    #region Implementation

    
    private void AssertAssetExpectedHoursProjected(AssetExpectedRuntimeHoursProjected aehp, ExpectedRuntimeHours expHours, string context)
    {
      Assert.AreEqual(expHours.Sun, aehp.HoursSun, "Wrong sunday hours " + context);
      Assert.AreEqual(expHours.Mon, aehp.HoursMon, "Wrong mon hours " + context);
      Assert.AreEqual(expHours.Tue, aehp.HoursTue, "Wrong tue hours " + context);
      Assert.AreEqual(expHours.Wed, aehp.HoursWed, "Wrong wed hours " + context);
      Assert.AreEqual(expHours.Thu, aehp.HoursThu, "Wrong thu hours " + context);
      Assert.AreEqual(expHours.Fri, aehp.HoursFri, "Wrong fri hours " + context);
      Assert.AreEqual(expHours.Sat, aehp.HoursSat, "Wrong sat hours " + context);
    }

    private void AssertAssetBurnRates(AssetBurnRates abr, double? idleBurnRate, string context)
    {
      Assert.AreEqual(idleBurnRate, abr.EstimatedIdleBurnRateGallonsPerHour, "Burn rate wrong " + context);
    }

    private void AssertAssetWorkingDefinition(AssetWorkingDefinition awd, WorkDefinitionEnum workDefn, string context)
    {
      Assert.AreEqual((int)workDefn, awd.fk_WorkDefinitionID, "Wrong work def " + context);
    }

    private Asset CreateAssetHelper(EquipmentAPI target)
    {
      var datetimeTicks = DateTime.UtcNow.Ticks;
      var name = "testasset" + datetimeTicks;
      const string makeCode = "CAT";
      var serialNumber = "SN1000" + datetimeTicks;
      var familyDescription = "familyDesc" + datetimeTicks;
      const int year = 2010;
      var modelDescription = "ModelDescription" + datetimeTicks;

      var device = TestData.TestSNM940;

      return target.Create(Ctx.OpContext, name, makeCode, serialNumber, device.ID,
        (DeviceTypeEnum)device.fk_DeviceTypeID, familyDescription, modelDescription, year, Guid.NewGuid());
    }


    #endregion

    

    #region ASSERT HELPER METHODS

    private static void AssetAssertHelper(Asset expected, Asset actual)
    {
      Assert.IsNotNull(expected, "Expected asset is null.");
      Assert.IsNotNull(actual, "Actual asset is null.");

      Assert.AreEqual(expected.AssetID, actual.AssetID, "Asset Ids do not match.");
      Assert.AreEqual(expected.fk_DeviceID, actual.fk_DeviceID, "Asset Device Ids do not match.");
      Assert.AreEqual(expected.fk_MakeCode, actual.fk_MakeCode, "Asset Make codes do not match.");
      Assert.AreEqual(expected.IconID, actual.IconID, "Asset IconIDs do not match.");
      Assert.AreEqual(expected.ManufactureYear, actual.ManufactureYear, "Asset ManufactureYear do not match.");
      Assert.AreEqual(expected.Model, actual.Model, "Asset Model do not match.");
      Assert.AreEqual(expected.Name, actual.Name, "Asset Name do not match.");
      Assert.AreEqual(expected.ProductFamilyName, actual.ProductFamilyName, "Asset ProductFamilyName do not match.");
      Assert.AreEqual(expected.SerialNumberVIN, actual.SerialNumberVIN, "Asset SerialNumberVIN do not match.");
    }

    public static void ExpectedRuntimeHoursProjectedAreEqual(ExpectedRuntimeHours expected, AssetExpectedRuntimeHoursProjected actual)
    {
      Assert.AreEqual(expected.Sun, (double)actual.HoursSun, "Sun  expected hours don't match.");
      Assert.AreEqual(expected.Mon, (double)actual.HoursMon, "Mon  expected hours don't match.");
      Assert.AreEqual(expected.Tue, (double)actual.HoursTue, "Tue  expected hours don't match.");
      Assert.AreEqual(expected.Wed, (double)actual.HoursWed, "Wed  expected hours don't match.");
      Assert.AreEqual(expected.Thu, (double)actual.HoursThu, "Thu  expected hours don't match.");
      Assert.AreEqual(expected.Fri, (double)actual.HoursFri, "Fri  expected hours don't match.");
      Assert.AreEqual(expected.Sat, (double)actual.HoursSat, "Sat  expected hours don't match.");
    }

    
    
    public static void ExpectedRuntimeHoursProjectedAreEqual(AssetExpectedRuntimeHoursProjected expectedHoursExpected, AssetExpectedRuntimeHoursProjected expectedHoursActual)
    {
      Assert.AreEqual(expectedHoursExpected.HoursSun, expectedHoursActual.HoursSun, "Sun  expected hours don't match.");
      Assert.AreEqual(expectedHoursExpected.HoursMon, expectedHoursActual.HoursMon, "Mon  expected hours don't match.");
      Assert.AreEqual(expectedHoursExpected.HoursTue, expectedHoursActual.HoursTue, "Tue  expected hours don't match.");
      Assert.AreEqual(expectedHoursExpected.HoursWed, expectedHoursActual.HoursWed, "Wed  expected hours don't match.");
      Assert.AreEqual(expectedHoursExpected.HoursThu, expectedHoursActual.HoursThu, "Thu  expected hours don't match.");
      Assert.AreEqual(expectedHoursExpected.HoursFri, expectedHoursActual.HoursFri, "Fri  expected hours don't match.");
      Assert.AreEqual(expectedHoursExpected.HoursSat, expectedHoursActual.HoursSat, "Sat  expected hours don't match.");
    }  

    public static void BurnRatesAreEqual(AssetBurnRates burnRatesExpected, AssetBurnRates burnRatesActual)
    {
      Assert.AreEqual(burnRatesExpected.EstimatedIdleBurnRateGallonsPerHour, burnRatesActual.EstimatedIdleBurnRateGallonsPerHour);
      Assert.AreEqual(burnRatesExpected.EstimatedWorkingBurnRateGallonsPerHour, burnRatesActual.EstimatedWorkingBurnRateGallonsPerHour);
    }

    public static void WorkingDefsAreEqual(AssetWorkingDefinition workingDefExpected, AssetWorkingDefinition workingDefActual)
    {
      Assert.AreEqual(workingDefExpected.fk_WorkDefinitionID, workingDefActual.fk_WorkDefinitionID);
      Assert.AreEqual(workingDefExpected.SensorNumber, workingDefActual.SensorNumber);
      Assert.AreEqual(workingDefExpected.SensorStartIsOn, workingDefActual.SensorStartIsOn);
    }

    public static void AssetsAreEqual(Asset expectedAsset, Asset actualAsset)
    {
      Assert.AreEqual(expectedAsset.fk_DeviceID, actualAsset.fk_DeviceID, "Device doesn't match");
      Assert.AreEqual(expectedAsset.fk_MakeCode, actualAsset.fk_MakeCode, "Make doesn't match");
      Assert.AreEqual(expectedAsset.Name, actualAsset.Name, "Name doesn't match");
      Assert.AreEqual(expectedAsset.SerialNumberVIN, actualAsset.SerialNumberVIN, "SerialNumberVIN doesn't match");
      Assert.AreEqual(expectedAsset.ManufactureYear, actualAsset.ManufactureYear, "ManufactureYear doesn't match");
      Assert.AreEqual(expectedAsset.ProductFamilyName, actualAsset.ProductFamilyName, "ProductFamilyName doesn't match");
    }

    #endregion


    [TestMethod]
    public void AssociateAssetDeviceTest_Success()
    {
        Mock<INH_OP> _mockNhOpContext = new Mock<INH_OP>(); ;
        EquipmentAPI equipmentAPI = new EquipmentAPI();
        MockObjectSet<Asset> assetData = new MockObjectSet<Asset>();
        assetData.AddObject(new Asset()
        {
            AssetID = 2,
            fk_DeviceID = 0,
            AssetUID = new Guid("69f13aec-a7b9-4ec8-b884-8cf0830bc664")
        });

        MockObjectSet<Device> deviceData = new MockObjectSet<Device>();
        deviceData.AddObject(new Device()
        {
            ID = 123,
            DeviceUID = new Guid("ad388952-f7d0-4282-a92f-7ffccf428ded")
        });

        _mockNhOpContext.SetupGet(o => o.Asset).Returns(assetData);
        _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

        _mockNhOpContext.SetupGet(o => o.Device).Returns(deviceData);
        _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

        var result = equipmentAPI.AssociateAssetDevice(_mockNhOpContext.Object, 2, 123);        
        var updatedDeviceId =_mockNhOpContext.Object.Asset.FirstOrDefault().fk_DeviceID;
        Assert.AreEqual(123, updatedDeviceId);
        _mockNhOpContext.Verify(o => o.SaveChanges(), Times.Exactly(1));
        Assert.AreEqual(true, result);
    }
    
    [TestMethod]
    public void CreateAssetDeviceHistory_HistoryForAssetDoesNotExist_ReturnsNewAssetDeviceHistory()
    {
      var device = Entity.Device.MTS522.Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var adh = API.Equipment.CreateAssetDeviceHistory(Ctx.OpContext, asset.AssetID, device.ID, device.OwnerBSSID, asset.InsertUTC.Value);

      Assert.AreEqual(asset.AssetID, adh.fk_AssetID, "AssetId not equal.");
      Assert.AreEqual(device.ID, adh.fk_DeviceID, "DeviceId not equal.");
      Assert.AreEqual(device.OwnerBSSID, adh.OwnerBSSID, "OwnerBssId not equal.");
      Assert.AreEqual(asset.InsertUTC, adh.StartUTC, "InsertUtc not equal.");
    }

    [TestMethod]
    public void CreateAssetDeviceHistory_HistoryForAssetExists_AssetDeviceHistoryStartUtcIsExistingEndUtc()
    {
      var endDate = DateTime.UtcNow.AddDays(-10);
      var device = Entity.Device.MTS522.Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var existingAdh = new AssetDeviceHistory
      {
        fk_AssetID = asset.AssetID,
        fk_DeviceID = device.ID,
        StartUTC = DateTime.UtcNow.AddDays(-11),
        EndUTC = endDate
      };
      Ctx.OpContext.AssetDeviceHistory.AddObject(existingAdh);
      Ctx.OpContext.SaveChanges();

      var adh = API.Equipment.CreateAssetDeviceHistory(Ctx.OpContext, asset.AssetID, device.ID, device.OwnerBSSID, asset.InsertUTC.Value);

      Assert.AreEqual(asset.AssetID, adh.fk_AssetID, "AssetId not equal.");
      Assert.AreEqual(device.ID, adh.fk_DeviceID, "DeviceId not equal.");
      Assert.AreEqual(device.OwnerBSSID, adh.OwnerBSSID, "OwnerBssId not equal.");
      Assert.AreEqual(endDate, adh.StartUTC, "startUtc is not existing end date.");
    }

    [TestMethod]
    public void GetAssetDeviceHistory_HistoryForAssetExists_ReturnsLatestEntry()
    {
      DateTime now = DateTime.UtcNow;
      var device = Entity.Device.MTS522.Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      for (int i = 0; i < 2; ++i)
      {
        var existingAdh = new AssetDeviceHistory
        {
          fk_AssetID = asset.AssetID,
          fk_DeviceID = device.ID,
          StartUTC = now.AddDays(-10 + i),
          EndUTC = now.AddDays(-10 + i + 5),
          OwnerBSSID = "Owner" + i
        };
        Ctx.OpContext.AssetDeviceHistory.AddObject(existingAdh);
      }
      Ctx.OpContext.SaveChanges();
      var latestAdh = API.Equipment.GetAssetDeviceHistory(Ctx.OpContext, asset.AssetID);
      Assert.AreEqual("Owner1", latestAdh.OwnerBSSID, "Latest assetDeviceHistory was not fetched");
    }

    [TestMethod]
    public void AssociateAssetDeviceTest_Failure()
    {
        Mock<INH_OP> _mockNhOpContext = new Mock<INH_OP>(); ;
        EquipmentAPI equipmentAPI = new EquipmentAPI();
        MockObjectSet<Asset> assetData = new MockObjectSet<Asset>();
        assetData.AddObject(new Asset()
        {
            AssetID = 2,
            fk_DeviceID = 0,
            AssetUID = new Guid("69f13aec-a7b9-4ec8-b884-8cf0830bc664")
        });

        MockObjectSet<Device> deviceData = new MockObjectSet<Device>();
        deviceData.AddObject(new Device()
        {
            ID = 123,
            DeviceUID = new Guid("ad388952-f7d0-4282-a92f-7ffccf428ded")
        });

        _mockNhOpContext.SetupGet(o => o.Asset).Returns(assetData);        
        _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(0);

        var result = equipmentAPI.AssociateAssetDevice(_mockNhOpContext.Object, 2, 123);        
        _mockNhOpContext.Verify(o => o.SaveChanges(), Times.Exactly(1));
        Assert.AreEqual(false, result);
    }

    [TestMethod]
    public void DisassociateAssetDeviceTest_Success()
    {
      Mock<INH_OP> _mockNhOpContext = new Mock<INH_OP>(); ;
      EquipmentAPI equipmentAPI = new EquipmentAPI();
      MockObjectSet<Asset> assetData = new MockObjectSet<Asset>();
      assetData.AddObject(new Asset()
      {
        AssetID = 2,
        fk_DeviceID = 123,
        AssetUID = new Guid("69f13aec-a7b9-4ec8-b884-8cf0830bc664")
      });

      MockObjectSet<Device> deviceData = new MockObjectSet<Device>();
      deviceData.AddObject(new Device()
      {
        ID = 123,
        DeviceUID = new Guid("ad388952-f7d0-4282-a92f-7ffccf428ded")
      });

      _mockNhOpContext.SetupGet(o => o.Asset).Returns(assetData);
      _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

      _mockNhOpContext.SetupGet(o => o.Device).Returns(deviceData);
      _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);
      var result = equipmentAPI.DisassociateAssetDevice(_mockNhOpContext.Object, 2, 123);
      var updatedDeviceId = _mockNhOpContext.Object.Asset.FirstOrDefault().fk_DeviceID;
      Assert.AreEqual(0, updatedDeviceId);
      _mockNhOpContext.Verify(o => o.SaveChanges(), Times.Exactly(1));
      Assert.AreEqual(true, result);
    }

    [TestMethod]
    public void DisassociateAssetDeviceTest_Failure()
    {
      Mock<INH_OP> _mockNhOpContext = new Mock<INH_OP>();
      EquipmentAPI equipmentAPI = new EquipmentAPI();
      MockObjectSet<Asset> assetData = new MockObjectSet<Asset>();
      assetData.AddObject(new Asset()
      {
        AssetID = 2,
        fk_DeviceID = 123,
        AssetUID = new Guid("69f13aec-a7b9-4ec8-b884-8cf0830bc664")
      });

      MockObjectSet<Device> deviceData = new MockObjectSet<Device>();
      deviceData.AddObject(new Device()
      {
        ID = 123,
        DeviceUID = new Guid("ad388952-f7d0-4282-a92f-7ffccf428ded")
      });

      _mockNhOpContext.SetupGet(o => o.Asset).Returns(assetData);
      _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(0);

      var result = equipmentAPI.DisassociateAssetDevice(_mockNhOpContext.Object, 2, 123);
      _mockNhOpContext.Verify(o => o.SaveChanges(), Times.Exactly(1));
      Assert.AreEqual(false, result);
    }

    [TestMethod]
    [DatabaseTest]
    public void GetDeviceId_Success()
    {
      var device = Entity.Device.MTS522.GpsDeviceId("12345").Save();
      var asset = Entity.Asset.MakeCode("CAT").SerialNumberVin("TEST").WithDevice(device).Save();

      var gpsDeviceId = API.Equipment.GetDeviceId("CAT", "TEST");
      Assert.AreEqual("12345", gpsDeviceId);
    }

    [TestMethod]
    [DatabaseTest]
    public void GetDeviceId_Failure()
    {
      var device = Entity.Device.MTS522.GpsDeviceId("12345").Save();
      var asset = Entity.Asset.MakeCode("CAT").SerialNumberVin("TEST").WithDevice(device).Save();

      var gpsDeviceId = API.Equipment.GetDeviceId("CAT", "TEST1");
      Assert.IsNull(gpsDeviceId);
    }

    [TestMethod]
    public void ReleaseAssetTest_Success()
    {
      var assetId = 42;
      var deviceId = 43;
      var storeId = 44;
      var assetGuid = Guid.NewGuid();
      var oldUpdateUtc = DateTime.UtcNow.AddMinutes(-1);
      
      Mock<INH_OP> _mockNhOpContext = new Mock<INH_OP>();
      EquipmentAPI equipmentAPI = new EquipmentAPI();

      MockObjectSet<Asset> assetData = new MockObjectSet<Asset>();
      assetData.AddObject(new Asset()
      {
        AssetID = assetId,
        AssetUID = assetGuid,
        fk_DeviceID = deviceId,
        fk_StoreID = storeId,
        UpdateUTC = oldUpdateUtc
      });
      _mockNhOpContext.SetupGet(o => o.Asset).Returns(assetData);

      MockObjectSet<Device> deviceData = new MockObjectSet<Device>();
      deviceData.AddObject(new Device()
      {
        ID = deviceId,
        fk_DeviceStateID = (int)DeviceStateEnum.Subscribed,
        UpdateUTC = oldUpdateUtc
      });
      _mockNhOpContext.SetupGet(o => o.Device).Returns(deviceData);

      MockObjectSet<DailyReport> dailyReport = new MockObjectSet<DailyReport>();
      _mockNhOpContext.SetupGet(o => o.DailyReport).Returns(dailyReport);
      _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

      var result = equipmentAPI.ReleaseAsset(_mockNhOpContext.Object, assetGuid);

      _mockNhOpContext.VerifyGet(o => o.Device, Times.Exactly(1));
      _mockNhOpContext.Verify(o => o.SaveChanges(), Times.Exactly(1));

      var updatedAsset = _mockNhOpContext.Object.Asset.Where(a => a.AssetID == assetId).FirstOrDefault();
      var updatedDevice = _mockNhOpContext.Object.Device.Where(d => d.ID == deviceId).FirstOrDefault();

      Assert.IsTrue(result);
      Assert.IsNotNull(updatedAsset);
      Assert.AreEqual(0, updatedAsset.fk_StoreID);
      Assert.IsTrue(updatedAsset.UpdateUTC > oldUpdateUtc);
      Assert.IsNotNull(updatedDevice);
      Assert.AreEqual((int)DeviceStateEnum.Provisioned, updatedDevice.fk_DeviceStateID);
      Assert.IsTrue(updatedDevice.UpdateUTC > oldUpdateUtc);
    }

    [TestMethod]
    public void ReleaseAssetTest_Success_NoDevice()
    {
      var assetId = 42;
      var storeId = 44;
      var assetGuid = Guid.NewGuid();
      var oldUpdateUtc = DateTime.UtcNow.AddMinutes(-1);

      Mock<INH_OP> _mockNhOpContext = new Mock<INH_OP>();
      EquipmentAPI equipmentAPI = new EquipmentAPI();

      MockObjectSet<Asset> assetData = new MockObjectSet<Asset>();
      assetData.AddObject(new Asset()
      {
        AssetID = assetId,
        AssetUID = assetGuid,
        fk_DeviceID = 0,
        fk_StoreID = storeId,
        UpdateUTC = oldUpdateUtc
      });
      _mockNhOpContext.SetupGet(o => o.Asset).Returns(assetData);
      _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

      var result = equipmentAPI.ReleaseAsset(_mockNhOpContext.Object, assetGuid);

      _mockNhOpContext.VerifyGet(o => o.Device, Times.Never());
      _mockNhOpContext.Verify(o => o.SaveChanges(), Times.Exactly(1));

      var updatedAsset = _mockNhOpContext.Object.Asset.Where(a => a.AssetID == assetId).FirstOrDefault();

      Assert.IsTrue(result);
      Assert.IsNotNull(updatedAsset);
      Assert.AreEqual(0, updatedAsset.fk_StoreID);
      Assert.IsTrue(updatedAsset.UpdateUTC > oldUpdateUtc);
    }
  }
}
