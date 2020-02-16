using System.Linq;
using System.Transactions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Nighthawk.EntityModels;
using System.Collections.Generic;
using System;

namespace UnitTests
{
  [TestClass()]
  public class HierarchySaverTest : ServerAPITestBase
  {
    [TestMethod()]
    public void SaveDevicesNormalCase()
    {
      XElement customers = new XElement("Customers");
      XElement cust1 = new XElement("Customer");
      cust1.SetAttributeValue("Name", "JASON_TEST_CUST_1");
      cust1.SetAttributeValue("BSSID", "-JD100");
      cust1.SetAttributeValue("ExternalCustomerID", "100");
      cust1.SetAttributeValue("fk_CustomerTypeID", 2);
      customers.Add(cust1);

      XElement assets = new XElement("Assets");
      XElement asset1 = new XElement("Asset");
      asset1.SetAttributeValue("CustomerBSSID", "-JD100");
      asset1.SetAttributeValue("SerialNumber", "SNJ100");
      asset1.SetAttributeValue("SNNumeric", "100");
      asset1.SetAttributeValue("MakeCode", "CAT");
      asset1.SetAttributeValue("ManufactureYear", "1980");
      asset1.SetAttributeValue("IBKey", "-111111");
      assets.Add(asset1);
      XElement asset2 = new XElement("Asset");
      asset2.SetAttributeValue("CustomerBSSID", "-JD100");
      asset2.SetAttributeValue("SerialNumber", "SNJ200");
      asset2.SetAttributeValue("SNNumeric", "200");
      asset2.SetAttributeValue("MakeCode", "CAT");
      asset2.SetAttributeValue("ManufactureYear", "1981");
      assets.Add(asset2);

      XElement devices = new XElement("Devices");
      XElement device1 = new XElement("Device");
      device1.SetAttributeValue("CustomerBSSID", "-JD100");
      device1.SetAttributeValue("AssetSN", "SNJ100");
      device1.SetAttributeValue("AssetMakeCode", "CAT");
      device1.SetAttributeValue("GpsDeviceID", "JSD100X100");
      device1.SetAttributeValue("fk_DeviceTypeID", ((int)DeviceTypeEnum.MTS522523).ToString() );
      device1.SetAttributeValue("ExternalID", "EXTERNAL100100");
      devices.Add(device1);
      XElement device2 = new XElement("Device");
      device2.SetAttributeValue("CustomerBSSID", "-JD100");
      device2.SetAttributeValue("AssetSN", "SNJ200");
      device2.SetAttributeValue("AssetMakeCode", "CAT");
      device2.SetAttributeValue("GpsDeviceID", "JSD200X200");
      device2.SetAttributeValue("fk_DeviceTypeID", ((int)DeviceTypeEnum.MTS522523).ToString());
      device2.SetAttributeValue("ExternalID", string.Empty);
      devices.Add(device2);

      XElement hierachy = new XElement("ROOT", customers, assets, devices);

      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, this.TransactionOptions))
      {
        HierarchySaver.Save(hierachy);
        HierarchySaver.Save(hierachy); // save twice on purpose to make sure dupes aren't inshurted

        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          int deviceCount = (from dds in ctx.DeviceReadOnly where dds.Customer.BSSID == "-JD100" && dds.Active select 1).Count();
          Assert.AreEqual(2, deviceCount, "Expect two devices to be created");

          int dt = (int)DeviceTypeEnum.MTS522523;
          Device d1 = (from dds in ctx.DeviceReadOnly 
                       where dds.Customer.BSSID == "-JD100" && 
                             dds.GpsDeviceID == "JSD100X100" && 
                             dds.DeviceType.ID == dt &&
                             dds.Active &&
                             dds.Asset.SerialNumberVIN == "SNJ100" &&
                             dds.Asset.Make == "CAT" &&
                             dds.Asset.Active select dds).FirstOrDefault();
          Assert.IsNotNull(d1, "Can't finding device1");
          Assert.AreEqual("EXTERNAL100100", d1.ExternalDeviceID, "External device ID not saved");

          Device d2 = (from dds in ctx.DeviceReadOnly
                       where dds.Customer.BSSID == "-JD100" &&
                             dds.GpsDeviceID == "JSD200X200" &&
                             dds.DeviceType.ID == dt &&
                             dds.Active &&
                             dds.Asset.SerialNumberVIN == "SNJ200" &&
                             dds.Asset.Make == "CAT" &&
                             dds.Asset.Active
                       select dds).FirstOrDefault();
          Assert.IsNotNull(d2, "Can't finding device2");
          Assert.AreEqual(string.Empty, d2.ExternalDeviceID, "External device ID not saved 2");
          
        }
      }
    }

    [TestMethod()]
    public void SaveDevicesDeviceSwap()
    {
      // Device initially belongs to cust1, on asset1
      XElement customers = new XElement("Customers");
      XElement cust1 = new XElement("Customer");
      cust1.SetAttributeValue("Name", "JASON_TEST_CUST_1");
      cust1.SetAttributeValue("BSSID", "-JD100");
      cust1.SetAttributeValue("ExternalCustomerID", "100");
      cust1.SetAttributeValue("fk_CustomerTypeID", 2);
      customers.Add(cust1);

      XElement assets = new XElement("Assets");
      XElement asset1 = new XElement("Asset");
      asset1.SetAttributeValue("CustomerBSSID", "-JD100");
      asset1.SetAttributeValue("SerialNumber", "SNJ100");
      asset1.SetAttributeValue("SNNumeric", "100");
      asset1.SetAttributeValue("MakeCode", "CAT");
      asset1.SetAttributeValue("ManufactureYear", "1980");
      asset1.SetAttributeValue("IBKey", "-111111");
      assets.Add(asset1);
      XElement asset0 = new XElement("Asset");
      asset0.SetAttributeValue("CustomerBSSID", "-JD100");
      asset0.SetAttributeValue("SerialNumber", "SNJ300");
      asset0.SetAttributeValue("SNNumeric", "300");
      asset0.SetAttributeValue("MakeCode", "CAT");
      asset0.SetAttributeValue("ManufactureYear", "1980");
      asset0.SetAttributeValue("IBKey", "-333333");
      assets.Add(asset0);

      XElement devices = new XElement("Devices");
      XElement device1 = new XElement("Device");
      device1.SetAttributeValue("CustomerBSSID", "-JD100");
      device1.SetAttributeValue("AssetSN", "SNJ100");
      device1.SetAttributeValue("AssetMakeCode", "CAT");
      device1.SetAttributeValue("GpsDeviceID", "JSD100X100");
      device1.SetAttributeValue("fk_DeviceTypeID", ((int)DeviceTypeEnum.MTS522523).ToString());
      device1.SetAttributeValue("ExternalID", "EXTERNAL_TEST");
      devices.Add(device1);
      XElement device0 = new XElement("Device");
      device0.SetAttributeValue("CustomerBSSID", "-JD100");
      device0.SetAttributeValue("AssetSN", "SNJ300");
      device0.SetAttributeValue("AssetMakeCode", "CAT");
      device0.SetAttributeValue("GpsDeviceID", "JSD300X300");
      device0.SetAttributeValue("fk_DeviceTypeID", ((int)DeviceTypeEnum.MTS522523).ToString());
      device0.SetAttributeValue("ExternalID", "EXTERNAL_TEST");
      devices.Add(device0);

      XElement hierachy = new XElement("ROOT", customers, assets, devices);

      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, this.TransactionOptions))
      {
        HierarchySaver.Save(hierachy);

        // Now device SN JSD100X100 belongs to cust2, on asset2
        customers = new XElement("Customers");
        XElement cust2 = new XElement("Customer");
        cust2.SetAttributeValue("Name", "JASON_TEST_CUST_2");
        cust2.SetAttributeValue("BSSID", "-JD200");
        cust2.SetAttributeValue("ExternalCustomerID", "200");
        cust2.SetAttributeValue("fk_CustomerTypeID", 2);
        customers.Add(cust2);

        assets = new XElement("Assets");
        XElement asset2 = new XElement("Asset");
        asset2.SetAttributeValue("CustomerBSSID", "-JD200");
        asset2.SetAttributeValue("SerialNumber", "SNJ200");
        asset2.SetAttributeValue("SNNumeric", "200");
        asset2.SetAttributeValue("MakeCode", "CAT");
        asset2.SetAttributeValue("ManufactureYear", "1981");
        assets.Add(asset2);

        devices = new XElement("Devices");
        XElement device2 = new XElement("Device");
        device2.SetAttributeValue("CustomerBSSID", "-JD200");
        device2.SetAttributeValue("AssetSN", "SNJ200");
        device2.SetAttributeValue("AssetMakeCode", "CAT");
        device2.SetAttributeValue("GpsDeviceID", "JSD100X100");
        device2.SetAttributeValue("fk_DeviceTypeID", ((int)DeviceTypeEnum.MTS522523).ToString());
        device2.SetAttributeValue("ExternalID", "EXTERNAL_TEST");
        devices.Add(device2);

        hierachy = new XElement("ROOT", customers, assets, devices);
        HierarchySaver.Save(hierachy);

        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          int dt = (int)DeviceTypeEnum.MTS522523;

          List<Device> deviceList = (from dds in ctx.DeviceReadOnly
                                  where dds.ExternalDeviceID == "EXTERNAL_TEST" && dds.DeviceType.ID == dt
                                  select dds).ToList();
          Assert.IsNotNull(deviceList, "Aye?");
          Assert.AreEqual(3, deviceList.Count, "Expect three devices");

          bool d0Active = (from d in deviceList where d.GpsDeviceID == "JSD300X300" && d.Active select 1).Count() == 1;
          Assert.IsTrue(d0Active, "Expect device0 to remain Active");
          bool d1Active = (from d in deviceList where d.GpsDeviceID == "JSD100X100" && d.Active select 1).Count() == 1;
          Assert.IsTrue(d1Active, "Expect d1 to be Active (on some asset)");
          bool d1InActive = (from d in deviceList where d.GpsDeviceID == "JSD100X100" && !d.Active select 1).Count() == 1;
          Assert.IsTrue(d1InActive, "Expect to find d1 InActive");

          Device d0 = (from d in ctx.DeviceReadOnly
                       where d.GpsDeviceID == "JSD300X300" && d.Active && d.Asset.SerialNumberVIN == "SNJ300" && d.Customer.BSSID == "-JD100"
                       select d).FirstOrDefault();
          Assert.IsNotNull(d0, "Expect device0 to be on it's original asset");

          Device d1 = (from d in ctx.DeviceReadOnly
                       where d.GpsDeviceID == "JSD100X100" && d.Active && d.Asset.SerialNumberVIN == "SNJ200" && d.Customer.BSSID == "-JD200"
                       select d).FirstOrDefault();
          Assert.IsNotNull(d1, "Expect device1 to now be on the second asset");


        }
      }
    }

    [TestMethod()]
    public void SaveDevicesDeviceSwap2()
    {
      // Asset initially has device D1
      XElement customers = new XElement("Customers");
      XElement cust1 = new XElement("Customer");
      cust1.SetAttributeValue("Name", "JASON_TEST_CUST_1");
      cust1.SetAttributeValue("BSSID", "-JD100");
      cust1.SetAttributeValue("ExternalCustomerID", "100");
      cust1.SetAttributeValue("fk_CustomerTypeID", 2);
      customers.Add(cust1);

      XElement assets = new XElement("Assets");
      XElement asset1 = new XElement("Asset");
      asset1.SetAttributeValue("CustomerBSSID", "-JD100");
      asset1.SetAttributeValue("SerialNumber", "SNJ100");
      asset1.SetAttributeValue("SNNumeric", "100");
      asset1.SetAttributeValue("MakeCode", "CAT");
      asset1.SetAttributeValue("ManufactureYear", "1980");
      asset1.SetAttributeValue("IBKey", "-111111");
      assets.Add(asset1);

      XElement devices = new XElement("Devices");
      XElement device1 = new XElement("Device");
      device1.SetAttributeValue("CustomerBSSID", "-JD100");
      device1.SetAttributeValue("AssetSN", "SNJ100");
      device1.SetAttributeValue("AssetMakeCode", "CAT");
      device1.SetAttributeValue("GpsDeviceID", "JSD100X100");
      device1.SetAttributeValue("fk_DeviceTypeID", ((int)DeviceTypeEnum.MTS522523).ToString());
      device1.SetAttributeValue("ExternalID", "EXTERNAL_TEST");
      devices.Add(device1);

      XElement hierachy = new XElement("ROOT", customers, assets, devices);

      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, this.TransactionOptions))
      {
        HierarchySaver.Save(hierachy);

        // Now asset is given device D2
        devices = new XElement("Devices");
        XElement device2 = new XElement("Device");
        device2.SetAttributeValue("CustomerBSSID", "-JD100");
        device2.SetAttributeValue("AssetSN", "SNJ100");
        device2.SetAttributeValue("AssetMakeCode", "CAT");
        device2.SetAttributeValue("GpsDeviceID", "JSD200X200");
        device2.SetAttributeValue("fk_DeviceTypeID", ((int)DeviceTypeEnum.MTS522523).ToString());
        device2.SetAttributeValue("ExternalID", "EXTERNAL_TEST");
        devices.Add(device2);

        hierachy = new XElement("ROOT", customers, assets, devices);
        HierarchySaver.Save(hierachy);

        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          int dt = (int)DeviceTypeEnum.MTS522523;
          List<Device> deviceList = (from dds in ctx.DeviceReadOnly
                                     where dds.ExternalDeviceID == "EXTERNAL_TEST" && dds.DeviceType.ID == dt
                                     select dds).ToList();
          Assert.IsNotNull(deviceList, "Aye?");
          Assert.AreEqual(2, deviceList.Count, "Expect two devices, one active, one not");

          bool d1Active = (from d in deviceList where d.GpsDeviceID == "JSD100X100" && d.Active select 1).Count() == 1;
          Assert.IsFalse(d1Active, "Expect d1 to be InActive");
          bool d2Active = (from d in deviceList where d.GpsDeviceID == "JSD200X200" && d.Active select 1).Count() == 1;
          Assert.IsTrue(d2Active, "Expect d2 to be Active");

          Device activeD = (from d in ctx.DeviceReadOnly
                            where d.GpsDeviceID == "JSD200X200" && d.Active && d.Asset.SerialNumberVIN == "SNJ100" && d.Customer.BSSID == "-JD100"
                            select d).FirstOrDefault();
          Assert.IsNotNull(activeD, "Expect d2 to be the active device on the asset");


        }
      }
    }

    [TestMethod()]
    public void SaveDevicesNoAsset()
    {
      XElement customers = new XElement("Customers");
      XElement cust1 = new XElement("Customer");
      cust1.SetAttributeValue("Name", "JASON_TEST_CUST_1");
      cust1.SetAttributeValue("BSSID", "-JD100");
      cust1.SetAttributeValue("ExternalCustomerID", "100");
      cust1.SetAttributeValue("fk_CustomerTypeID", 2);
      customers.Add(cust1);

      XElement devices = new XElement("Devices");
      XElement device1 = new XElement("Device");
      device1.SetAttributeValue("CustomerBSSID", "-JD100");
      device1.SetAttributeValue("AssetSN", "SNJ100");
      device1.SetAttributeValue("AssetMakeCode", "CAT");
      device1.SetAttributeValue("GpsDeviceID", "JSD100X100");
      device1.SetAttributeValue("fk_DeviceTypeID", ((int)DeviceTypeEnum.MTS522523).ToString());
      device1.SetAttributeValue("ExternalID", "EXTERNAL_TEST");
      devices.Add(device1);

      XElement hierachy = new XElement("ROOT", customers, devices);

      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, this.TransactionOptions))
      {
        HierarchySaver.Save(hierachy);

        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          int dt = (int)DeviceTypeEnum.MTS522523;
          bool deviceFound = (from dds in ctx.DeviceReadOnly where dds.GpsDeviceID == "JSD100X100" && dds.DeviceType.ID == dt select 1).Any();
          Assert.IsFalse(deviceFound, "Dont expect device to be created because there was no asset");
        }
      }
    }

    [TestMethod()]
    public void SaveDevicesNoCustomer()
    {
      XElement assets = new XElement("Assets");
      XElement asset1 = new XElement("Asset");
      asset1.SetAttributeValue("CustomerBSSID", "-JD100");
      asset1.SetAttributeValue("SerialNumber", "SNJ100");
      asset1.SetAttributeValue("SNNumeric", "100");
      asset1.SetAttributeValue("MakeCode", "CAT");
      asset1.SetAttributeValue("ManufactureYear", "1980");
      asset1.SetAttributeValue("IBKey", "-111111");
      assets.Add(asset1);

      XElement devices = new XElement("Devices");
      XElement device1 = new XElement("Device");
      device1.SetAttributeValue("CustomerBSSID", "-JD100");
      device1.SetAttributeValue("AssetSN", "SNJ100");
      device1.SetAttributeValue("AssetMakeCode", "CAT");
      device1.SetAttributeValue("GpsDeviceID", "JSD100X100");
      device1.SetAttributeValue("fk_DeviceTypeID", ((int)DeviceTypeEnum.MTS522523).ToString());
      device1.SetAttributeValue("ExternalID", "EXTERNAL100100");
      devices.Add(device1);

      XElement hierachy = new XElement("ROOT", assets, devices);

      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, this.TransactionOptions))
      {
        HierarchySaver.Save(hierachy);

        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          int dt = (int)DeviceTypeEnum.MTS522523;
          bool deviceFound = (from dds in ctx.DeviceReadOnly where dds.GpsDeviceID == "JSD100X100" && dds.DeviceType.ID == dt select 1).Any();
          Assert.IsFalse(deviceFound, "Dont expect device to be created because there was no asset");
        }
      }
    }

    [TestMethod()]
    public void SaveAssets()
    {
      XElement hierachy = new XElement("ROOT");

      XElement customers = new XElement("Customers");

      XElement cust1 = new XElement("Customer");
      cust1.SetAttributeValue("Name", "JASON_TEST_CUST_1");
      cust1.SetAttributeValue("BSSID", "-JD100");
      cust1.SetAttributeValue("ExternalCustomerID", "100");
      cust1.SetAttributeValue("fk_CustomerTypeID", 2);

      customers.Add(cust1);
      hierachy.Add(customers);

      XElement asset1 = new XElement("Asset");
      asset1.SetAttributeValue("CustomerBSSID", "-JD100");
      asset1.SetAttributeValue("SerialNumber", "SNJ100");
      asset1.SetAttributeValue("SNNumeric", "100");
      asset1.SetAttributeValue("MakeCode", "CAT");
      asset1.SetAttributeValue("ManufactureYear", "1980");
      asset1.SetAttributeValue("IBKey", "-111111");

      XElement invalidAsset = new XElement("Asset");
      invalidAsset.SetAttributeValue("CustomerBSSID", "-JD???");
      invalidAsset.SetAttributeValue("SerialNumber", "SNJ100");
      invalidAsset.SetAttributeValue("SNNumeric", "100");
      invalidAsset.SetAttributeValue("MakeCode", "CAT");
      invalidAsset.SetAttributeValue("ManufactureYear", "1980");
      invalidAsset.SetAttributeValue("IBKey", "-111111");

      XElement assets = new XElement("Assets");
      assets.Add(asset1);
      assets.Add(invalidAsset);

      hierachy.Add(assets);

      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, this.TransactionOptions))
      {
        HierarchySaver.Save(hierachy);
        HierarchySaver.Save(hierachy); // save twice on purpose to make sure dupes aren't inshurted

        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          int assetCount = (from aa in ctx.AssetReadOnly where aa.Make == "CAT" && aa.SerialNumberVIN == "SNJ100" select 1).Count();
          Assert.AreEqual(1, assetCount, "Expect only first asset to get created 'cause second asset is invalid (unknown customer)");

          Asset a1 = (from aas in ctx.AssetReadOnly where aas.Make == "CAT" && aas.SerialNumberVIN == "SNJ100" select aas).FirstOrDefault();
          Assert.AreEqual("SNJ100", a1.SerialNumberVIN, "Wrong SN");
          Assert.AreEqual("CAT", a1.Make, "Wrong make");
          Assert.AreEqual<int>(1980, a1.ManufactureYear.Value, "Wrong manu year");
          Assert.AreEqual("ZSNJ100", a1.Name, "Wrong name");

//          bool hasAssetUtil = (from aus in ctx.AssetUtilizationReadOnly where aus.Asset.ID == a1.ID select 1).Any();
//          Assert.IsTrue(hasAssetUtil, "Asset util not created");

        }
      }
    }

    [TestMethod()]
    public void SaveSubscriptions()
    {
      XElement customers = new XElement("Customers");

      XElement cust1 = new XElement("Customer");
      cust1.SetAttributeValue("Name", "JASON_TEST_CUST_1");
      cust1.SetAttributeValue("BSSID", "-JD100");
      cust1.SetAttributeValue("ExternalCustomerID", "100");
      cust1.SetAttributeValue("fk_CustomerTypeID", 2);
      customers.Add(cust1);

      XElement asset1 = new XElement("Asset");
      asset1.SetAttributeValue("CustomerBSSID", "-JD100");
      asset1.SetAttributeValue("SerialNumber", "SNJ100");
      asset1.SetAttributeValue("SNNumeric", "100");
      asset1.SetAttributeValue("MakeCode", "CAT");
      asset1.SetAttributeValue("ManufactureYear", "1980");
      asset1.SetAttributeValue("IBKey", "-111111");
      XElement assets = new XElement("Assets");
      assets.Add(asset1);

      XElement subscriptions = new XElement("Subscriptions");
      XElement subs1 = new XElement("Subscription");
      subs1.SetAttributeValue("ExternalCustomerID", "100");
      subs1.SetAttributeValue("AssetSN", "SNJ100");
      subs1.SetAttributeValue("AssetMakeCode", "CAT");
      subs1.SetAttributeValue("BSSPlanLineID", "LINEID_1");
      subs1.SetAttributeValue("fk_ServicePlanID", ((int)ServiceTypeEnum.VLCORE).ToString());
      subs1.SetAttributeValue("StartUTC", new DateTime(2010,1,1));
      //subs1.SetAttributeValue("EndUTC", ""); NULL
      subscriptions.Add(subs1);
      XElement subs2 = new XElement("Subscription");
      subs2.SetAttributeValue("ExternalCustomerID", "100");
      subs2.SetAttributeValue("AssetSN", "SNJ100");
      subs2.SetAttributeValue("AssetMakeCode", "CAT");
      subs2.SetAttributeValue("BSSPlanLineID", "LINEID_2");
      subs2.SetAttributeValue("fk_ServicePlanID", ((int)ServiceTypeEnum.CATHEALTH).ToString());
      subs2.SetAttributeValue("StartUTC", new DateTime(2010, 1, 2));
      //subs2.SetAttributeValue("EndUTC", ""); NULL
      subscriptions.Add(subs2);
      XElement subs3 = new XElement("Subscription");
      subs3.SetAttributeValue("ExternalCustomerID", "Complete Unknown");
      subs3.SetAttributeValue("AssetSN", "SNJ100");
      subs3.SetAttributeValue("AssetMakeCode", "CAT");
      subs3.SetAttributeValue("BSSPlanLineID", "LINEID_3");
      subs3.SetAttributeValue("fk_ServicePlanID", ((int)ServiceTypeEnum.CATHEALTH).ToString());
      subs3.SetAttributeValue("StartUTC", new DateTime(2010, 1, 1));
      //subs3.SetAttributeValue("EndUTC", ""); NULL
      subscriptions.Add(subs3);
      XElement subs4 = new XElement("Subscription");
      subs4.SetAttributeValue("ExternalCustomerID", "100");
      subs4.SetAttributeValue("AssetSN", "whodat?");
      subs4.SetAttributeValue("AssetMakeCode", "CAT");
      subs4.SetAttributeValue("BSSPlanLineID", "LINEID_4");
      subs4.SetAttributeValue("fk_ServicePlanID", ((int)ServiceTypeEnum.CATHEALTH).ToString());
      subs4.SetAttributeValue("StartUTC", new DateTime(2010, 1, 1));
      //subs4.SetAttributeValue("EndUTC", ""); NULL
      subscriptions.Add(subs4);

      XElement hierachy = new XElement("ROOT", customers, assets, subscriptions);

      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, this.TransactionOptions))
      {
        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          long maxSubsID = (from allSubs in ctx.AssetSubscriptionReadOnly orderby allSubs.ID descending select allSubs.ID).FirstOrDefault();

          HierarchySaver.Save(hierachy);
          HierarchySaver.Save(hierachy); // save twice on purpose to make sure dupes aren't inshurted

          int subsCount = (from ss in ctx.AssetSubscriptionReadOnly where ss.ID > maxSubsID select 1).Count();
          Assert.AreEqual(2, subsCount, "Expect 2 subs for the asset. The second two subs are for invalid customer or asset");

          int coreID = (int)ServiceTypeEnum.VLCORE;
          int healthID = (int)ServiceTypeEnum.CATHEALTH;
          List<AssetSubscription> subList = (from ss in ctx.AssetSubscriptionReadOnly 
                                    where ss.Asset.SerialNumberVIN == "SNJ100"
                                    orderby ss.ID ascending
                                    select ss).ToList();
          Assert.IsNotNull(subList, "Expect to find some service plans for this asset");
          Assert.AreEqual(2, subList.Count, "Should be two plans for this asset");
          AssetSubscription sub1 = subList[0];
          sub1.ServicePlanReference.Load();
          Assert.AreEqual(coreID, sub1.ServicePlan.ID, "Wrong service plan type");
          Assert.AreEqual("LINEID_1", sub1.BSSPlanLineID, "Wrong BSSPlanLineID");
          Assert.AreEqual<DateTime>(new DateTime(2010, 1, 1), sub1.StartUTC, "Wrong StartUTC");
          Assert.IsNull(sub1.EndUTC, "Wrong endUTC");

          AssetSubscription sub2 = subList[1];
          sub2.ServicePlanReference.Load();
          Assert.AreEqual(healthID, sub2.ServicePlan.ID, "Wrong service plan type 2");
          Assert.AreEqual("LINEID_2", sub2.BSSPlanLineID, "Wrong BSSPlanLineID 2");
          Assert.AreEqual<DateTime>(new DateTime(2010, 1, 2), sub2.StartUTC, "Wrong StartUTC 2");
          Assert.IsNull(sub2.EndUTC, "Wrong endUTC 2");
        }
      }
    }

    [TestMethod()]
    public void SaveSubscriptionUpdates()
    {
      XElement customers = new XElement("Customers");

      XElement cust1 = new XElement("Customer");
      cust1.SetAttributeValue("Name", "JASON_TEST_CUST_1");
      cust1.SetAttributeValue("BSSID", "-JD100");
      cust1.SetAttributeValue("ExternalCustomerID", "100");
      cust1.SetAttributeValue("fk_CustomerTypeID", 2);
      customers.Add(cust1);

      XElement asset1 = new XElement("Asset");
      asset1.SetAttributeValue("CustomerBSSID", "-JD100");
      asset1.SetAttributeValue("SerialNumber", "SNJ100");
      asset1.SetAttributeValue("SNNumeric", "100");
      asset1.SetAttributeValue("MakeCode", "CAT");
      asset1.SetAttributeValue("ManufactureYear", "1980");
      asset1.SetAttributeValue("IBKey", "-111111");
      XElement assets = new XElement("Assets");
      assets.Add(asset1);

      XElement subscriptions = new XElement("Subscriptions");
      XElement subs1 = new XElement("Subscription");
      subs1.SetAttributeValue("ExternalCustomerID", "100");
      subs1.SetAttributeValue("AssetSN", "SNJ100");
      subs1.SetAttributeValue("AssetMakeCode", "CAT");
      subs1.SetAttributeValue("BSSPlanLineID", "LINEID_1");
      subs1.SetAttributeValue("fk_ServicePlanID", ((int)ServiceTypeEnum.VLCORE).ToString());
      subs1.SetAttributeValue("StartUTC", new DateTime(2010, 1, 1));
      //subs1.SetAttributeValue("EndUTC", ""); NULL
      subscriptions.Add(subs1);
      XElement subs2 = new XElement("Subscription");
      subs2.SetAttributeValue("ExternalCustomerID", "100");
      subs2.SetAttributeValue("AssetSN", "SNJ100");
      subs2.SetAttributeValue("AssetMakeCode", "CAT");
      subs2.SetAttributeValue("BSSPlanLineID", "LINEID_2");
      subs2.SetAttributeValue("fk_ServicePlanID", ((int)ServiceTypeEnum.CATHEALTH).ToString());
      subs2.SetAttributeValue("StartUTC", new DateTime(2010, 1, 2));
      //subs2.SetAttributeValue("EndUTC", ""); NULL
      subscriptions.Add(subs2);

      XElement hierachy = new XElement("ROOT", customers, assets, subscriptions);

      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, this.TransactionOptions))
      {
        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          long maxSubsID = (from allSubs in ctx.AssetSubscriptionReadOnly orderby allSubs.ID descending select allSubs.ID).FirstOrDefault();

          HierarchySaver.Save(hierachy);

          // Update EndUTC's
          subscriptions = new XElement("Subscriptions");
          subs1 = new XElement("Subscription");
          subs1.SetAttributeValue("ExternalCustomerID", "100");
          subs1.SetAttributeValue("AssetSN", "SNJ100");
          subs1.SetAttributeValue("AssetMakeCode", "CAT");
          subs1.SetAttributeValue("BSSPlanLineID", "LINEID_1");
          subs1.SetAttributeValue("EndUTC", new DateTime(2010,2,1));
          subscriptions.Add(subs1);
          subs2 = new XElement("Subscription");
          subs2.SetAttributeValue("ExternalCustomerID", "100");
          subs2.SetAttributeValue("AssetSN", "SNJ100");
          subs2.SetAttributeValue("AssetMakeCode", "CAT");
          subs2.SetAttributeValue("BSSPlanLineID", "LINEID_2");
          subs2.SetAttributeValue("EndUTC", new DateTime(2010, 2, 2));
          subscriptions.Add(subs2);

          hierachy = new XElement("ROOT", customers, assets, subscriptions);
          HierarchySaver.Save(hierachy);

          int subsCount = (from ss in ctx.AssetSubscriptionReadOnly where ss.ID > maxSubsID select 1).Count();
          Assert.AreEqual(2, subsCount, "Expect 2 subs for the asset. The second two subs should be applied as updates");

          int coreID = (int)ServiceTypeEnum.VLCORE;
          int healthID = (int)ServiceTypeEnum.CATHEALTH;
          List<AssetSubscription> subList = (from ss in ctx.AssetSubscriptionReadOnly
                                             where ss.Asset.SerialNumberVIN == "SNJ100"
                                             orderby ss.ID ascending
                                             select ss).ToList();
          Assert.IsNotNull(subList, "Expect to find some service plans for this asset");
          Assert.AreEqual(2, subList.Count, "Should be two plans for this asset");
          AssetSubscription sub1 = subList[0];
          sub1.ServicePlanReference.Load();
          Assert.AreEqual(coreID, sub1.ServicePlan.ID, "Wrong service plan type");
          Assert.AreEqual("LINEID_1", sub1.BSSPlanLineID, "Wrong BSSPlanLineID");
          Assert.AreEqual<DateTime>(new DateTime(2010, 1, 1), sub1.StartUTC, "Wrong StartUTC");
          Assert.AreEqual<DateTime>(new DateTime(2010,2,1), sub1.EndUTC.Value, "Wrong endUTC");

          AssetSubscription sub2 = subList[1];
          sub2.ServicePlanReference.Load();
          Assert.AreEqual(healthID, sub2.ServicePlan.ID, "Wrong service plan type 2");
          Assert.AreEqual("LINEID_2", sub2.BSSPlanLineID, "Wrong BSSPlanLineID 2");
          Assert.AreEqual<DateTime>(new DateTime(2010, 1, 2), sub2.StartUTC, "Wrong StartUTC 2");
          Assert.AreEqual<DateTime>(new DateTime(2010, 2, 2), sub2.EndUTC.Value, "Wrong endUTC 2");
        }
      }
    }

    [TestMethod()]
    public void SaveAccounts()
    {
      XElement hierachy = new XElement("ROOT");

      XElement customers = new XElement("Customers");

      XElement cust1 = new XElement("Customer");
      cust1.SetAttributeValue("Name", "JASON_TEST_CUST_1");
      cust1.SetAttributeValue("BSSID", "-JD100");
      cust1.SetAttributeValue("ExternalCustomerID", "100");
      cust1.SetAttributeValue("fk_CustomerTypeID", 2);

      XElement cust2 = new XElement("Customer");
      cust2.SetAttributeValue("Name", "JASON_TEST_CUST_2");
      cust2.SetAttributeValue("BSSID", "-JD200");
      cust2.SetAttributeValue("ExternalCustomerID", "200");
      cust2.SetAttributeValue("fk_CustomerTypeID", 2);

      customers.Add(cust1);
      customers.Add(cust2);
      hierachy.Add(customers);

      XElement account = new XElement("Account");
      account.SetAttributeValue("OwningBSSID", "-JD100");
      account.SetAttributeValue("ClientBSSID", "-JD200");

      XElement invalidAccount = new XElement("Account");
      invalidAccount.SetAttributeValue("OwningBSSID", "-JD100");
      invalidAccount.SetAttributeValue("ClientBSSID", "-JDxxx");

      XElement accounts = new XElement("Accounts");
      accounts.Add(account);
      accounts.Add(invalidAccount);

      hierachy.Add(accounts);

      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, this.TransactionOptions))
      {
        HierarchySaver.Save(hierachy);
        HierarchySaver.Save(hierachy); // save twice on purpose to make sure dupes aren't inshurted

        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          int accountCount = (from acs in ctx.AccountReadOnly where acs.OwningCustomer.BSSID == "-JD100" select 1).Count();
          Assert.AreEqual(1, accountCount, "Woa. Too many accounts, mate");

          Account ac = (from acc in ctx.AccountReadOnly where acc.OwningCustomer.BSSID == "-JD100" select acc).FirstOrDefault();
          ac.OwningCustomerReference.Load();
          ac.ClientCustomerReference.Load();
          Assert.AreEqual("-JD100", ac.OwningCustomer.BSSID, "Wrong owning cust");
          Assert.AreEqual("-JD200", ac.ClientCustomer.BSSID, "Wrong client cust");
        }
      }
    }

    [TestMethod()]
    public void SaveCustomers()
    {
      XElement hierachy = new XElement("ROOT");

      XElement customers = new XElement("Customers");

      XElement cust1 = new XElement("Customer");
      cust1.SetAttributeValue("Name", "JASON_TEST_CUST_1");
      cust1.SetAttributeValue("BSSID", "-JD100");
      cust1.SetAttributeValue("ExternalCustomerID", "100");
      cust1.SetAttributeValue("fk_CustomerTypeID", 2);

      XElement cust2 = new XElement("Customer");
      cust2.SetAttributeValue("Name", "JASON_TEST_CUST_2");
      cust2.SetAttributeValue("BSSID", "-JD200");
      cust2.SetAttributeValue("ExternalCustomerID", "200");
      cust2.SetAttributeValue("fk_CustomerTypeID", 2);

      customers.Add(cust1);
      customers.Add(cust2);
      hierachy.Add(customers);

      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, this.TransactionOptions))
      {
        HierarchySaver.Save(hierachy);
        HierarchySaver.Save(hierachy); // save twice on purpose to make sure dupes aren't inshurted

        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          int cust1Count = (from custs in ctx.CustomerReadOnly where custs.BSSID == "-JD100" select 1).Count();
          int cust2Count = (from custs in ctx.CustomerReadOnly where custs.BSSID == "-JD200" select 1).Count();

          Assert.IsTrue(cust1Count == 1, "cust1 not saved");
          Assert.IsTrue(cust2Count == 1, "cust2 not saved");

          Customer custo1 = (from custs in ctx.CustomerReadOnly where custs.BSSID == "-JD100" select custs).FirstOrDefault();
          custo1.CustomerTypeReference.Load();
          Assert.AreEqual("JASON_TEST_CUST_1", custo1.Name, "Name not saved accurately");
          Assert.AreEqual("-JD100", custo1.BSSID, "BSSID not saved accurately");
          Assert.AreEqual((int)CustomerTypeEnum.Customer, custo1.CustomerType.ID, "Customer type not saved accurately");
          Assert.AreEqual("100", custo1.ExternalCustomerID, "Ext cust ID not saved accurately");

          Customer custo2 = (from custs in ctx.CustomerReadOnly where custs.BSSID == "-JD200" select custs).FirstOrDefault();
          custo2.CustomerTypeReference.Load();
          Assert.AreEqual("JASON_TEST_CUST_2", custo2.Name, "Name not saved accurately 2");
          Assert.AreEqual("-JD200", custo2.BSSID, "BSSID not saved accurately 2");
          Assert.AreEqual((int)CustomerTypeEnum.Customer, custo2.CustomerType.ID, "Customer type not saved accurately 2");
          Assert.AreEqual("200", custo2.ExternalCustomerID, "Ext cust ID not saved accurately 2");
        }
      }
    }
  }
}
