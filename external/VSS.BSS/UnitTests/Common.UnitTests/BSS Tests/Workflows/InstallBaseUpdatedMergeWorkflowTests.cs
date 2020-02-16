using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class InstallBaseUpdatedMergeWorkflowTests : BssUnitTestBase
  {
    private ServiceViewAPITestHelper _scenarioHelper;
    public ServiceViewAPITestHelper ScenarioHelper
    {
      get { return _scenarioHelper ?? (_scenarioHelper = new ServiceViewAPITestHelper()); }
    }

    [DatabaseTest]
    [TestMethod]
    public void IBKeyDoesNotExist_ResultIsFalseWithSummary()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var owner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var differentOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();

      var message = BSS.IBUpdatedMerge
        .PartNumber(partNumber)
        .OwnerBssId(differentOwner.BSSID).Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success, "Success should be false.");
      TestHelper.AssertBssFailureCode(result, BssFailureCode.IbKeyDoesNotExist);
    }

    [DatabaseTest]
    [TestMethod]
    public void GPSDeviceIDIsNotAssociatedWithIBKey_ResultIsFalseWithSummary()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var owner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var differentOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS522.OwnerBssId(owner.BSSID).IbKey(IdGen.StringId()).Save();

      var message = BSS.IBUpdatedMerge
        .PartNumber(partNumber)
        .IBKey(device.IBKey)
        .GpsDeviceId(IdGen.StringId())
        .OwnerBssId(differentOwner.BSSID).Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success, "Success should be false.");
      TestHelper.AssertBssFailureCode(result, BssFailureCode.GpsDeviceIdInvalid);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceOwnerBssIdAndIBOwnerBssIDAreNotDifferent_ResultIsFalseWithSummary()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var owner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS522.OwnerBssId(owner.BSSID).IbKey(IdGen.StringId()).Save();

      var message = BSS.IBUpdatedMerge
        .PartNumber(partNumber)
        .IBKey(device.IBKey)
        .GpsDeviceId(device.GpsDeviceID)
        .OwnerBssId(owner.BSSID).Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success, "Success should be false.");
      TestHelper.AssertBssFailureCode(result, BssFailureCode.DeviceOwnerUnchanged);
    }

    [DatabaseTest]
    [TestMethod]
    public void DeviceTransfer_ResultIsFalseWithSummary()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var owner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var differentOwner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS522.OwnerBssId(owner.BSSID).IbKey(IdGen.StringId()).Save();
      var asset = Entity.Asset.Save();

      var message = BSS.IBUpdatedMerge
        .PartNumber(partNumber)
        .IBKey(device.IBKey)
        .GpsDeviceId(device.GpsDeviceID)
        .EquipmentSN(asset.SerialNumberVIN)
        .MakeCode(asset.fk_MakeCode)
        .OwnerBssId(differentOwner.BSSID).Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success, "Success should be false.");
      TestHelper.AssertBssFailureCode(result, BssFailureCode.DeviceXferAndOwnershipXfer);
    }

    [DatabaseTest]
    [TestMethod]
    public void OldOwnerAndNewOwnerDifferentCustomerTypes_ResultIsFalseWithSummary()
    {
      var partNumber = TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522);
      var owner = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();
      var differentOwner = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.MTS522.OwnerBssId(owner.BSSID).IbKey(IdGen.StringId()).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var message = BSS.IBUpdatedMerge
        .PartNumber(partNumber)
        .IBKey(device.IBKey)
        .GpsDeviceId(device.GpsDeviceID)
        .EquipmentSN(asset.SerialNumberVIN)
        .MakeCode(asset.fk_MakeCode)
        .OwnerBssId(differentOwner.BSSID).Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success, "Success should be false.");
      TestHelper.AssertBssFailureCode(result, BssFailureCode.MergeXferDiffCustomerType);
    }

    [DatabaseTest]
    [TestMethod]
    public void MergeAccounts_ParentDealersInDifferentNetworks_ResultIsFalseWithSummary()
    {
      ScenarioHelper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_OneSitechDealer();

      ScenarioHelper.SetupSeries522WithService(ScenarioHelper.Account, ServiceTypeEnum.Essentials, DateTime.UtcNow.AddMonths(-1)).Save();

      var message = BSS.IBUpdatedMerge
        .PartNumber(TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522))
        .IBKey(ScenarioHelper.Series522.IBKey)
        .GpsDeviceId(ScenarioHelper.Series522.GpsDeviceID)
        .EquipmentSN(ScenarioHelper.AssetSeries522.SerialNumberVIN)
        .MakeCode(ScenarioHelper.AssetSeries522.fk_MakeCode)
        .OwnerBssId(ScenarioHelper.Account2.BSSID) // Update Ownership to Account2
        .Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success, "Success should be false.");
      TestHelper.AssertBssFailureCode(result, BssFailureCode.MergeXferDiffDealerNetwork);
    }

    [DatabaseTest]
    [TestMethod]
    public void MergeDealers_DifferentNetworks_ResultIsFalseWithSummary()
    {
      ScenarioHelper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_OneSitechDealer();

      ScenarioHelper.SetupSeries522WithService(ScenarioHelper.Dealer, ServiceTypeEnum.Essentials, DateTime.UtcNow.AddMonths(-1)).Save();

      var message = BSS.IBUpdatedMerge
        .PartNumber(TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522))
        .IBKey(ScenarioHelper.Series522.IBKey)
        .GpsDeviceId(ScenarioHelper.Series522.GpsDeviceID)
        .EquipmentSN(ScenarioHelper.AssetSeries522.SerialNumberVIN)
        .MakeCode(ScenarioHelper.AssetSeries522.fk_MakeCode)
        .OwnerBssId(ScenarioHelper.Dealer2.BSSID) // Update Ownership to Account2
        .Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsFalse(result.Success, "Success should be false.");
      TestHelper.AssertBssFailureCode(result, BssFailureCode.MergeXferDiffDealerNetwork);
    }

    [DatabaseTest]
    [TestMethod]
    public void MergeAccounts_SameDealerParent_SameCustomerParent_Success()
    {
      ScenarioHelper.SetupHierarchy_OneCustomerHierarchy_OneCatDealer_CatParentDealer();

      var ownerVisibilityDate = DateTime.UtcNow.AddMonths(-2);
      var service = ScenarioHelper.SetupSeries522WithService(ScenarioHelper.Account, ServiceTypeEnum.Essentials, ownerVisibilityDate);
      service.WithView(x => x.ForAsset(ScenarioHelper.AssetSeries522).ForCustomer(ScenarioHelper.CatCorp))
        .WithView(x => x.ForAsset(ScenarioHelper.AssetSeries522).ForCustomer(ScenarioHelper.ParentDealer))
        .WithView(x => x.ForAsset(ScenarioHelper.AssetSeries522).ForCustomer(ScenarioHelper.Dealer))
        .WithView(x => x.ForAsset(ScenarioHelper.AssetSeries522).ForCustomer(ScenarioHelper.ParentCustomer).StartsOn(ownerVisibilityDate))
        .WithView(x => x.ForAsset(ScenarioHelper.AssetSeries522).ForCustomer(ScenarioHelper.Customer).StartsOn(ownerVisibilityDate))
        .Save();

      var message = BSS.IBUpdatedMerge
        .PartNumber(TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522))
        .IBKey(ScenarioHelper.Series522.IBKey)
        .GpsDeviceId(ScenarioHelper.Series522.GpsDeviceID)
        .EquipmentSN(ScenarioHelper.AssetSeries522.SerialNumberVIN)
        .MakeCode(ScenarioHelper.AssetSeries522.fk_MakeCode)
        .OwnerBssId(ScenarioHelper.Account2.BSSID) // Update Ownership to Account2
        .Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success, "Success should be true.");

      var assetViews = (from sv in Ctx.OpContext.ServiceViewReadOnly
                        where sv.fk_AssetID == ScenarioHelper.AssetSeries522.AssetID
                        select sv).ToList();

      var terminatedViews = assetViews.Where(x => x.EndKeyDate == DateTime.Parse(message.ActionUTC).KeyDate()).ToList();
      var createdViews = assetViews.Where(x => x.EndKeyDate == 99991231).ToList();

      Assert.AreEqual(5, terminatedViews.Count, "Terminated view count not equal.");
      Assert.AreEqual(5, createdViews.Count, "Created view count not equal.");

      Assert.AreEqual(1, terminatedViews.Count(x => x.fk_CustomerID == ScenarioHelper.CatCorp.ID));
      Assert.AreEqual(1, terminatedViews.Count(x => x.fk_CustomerID == ScenarioHelper.ParentDealer.ID));
      Assert.AreEqual(1, terminatedViews.Count(x => x.fk_CustomerID == ScenarioHelper.Dealer.ID));
      Assert.AreEqual(1, terminatedViews.Count(x => x.fk_CustomerID == ScenarioHelper.ParentCustomer.ID));
      Assert.AreEqual(1, terminatedViews.Count(x => x.fk_CustomerID == ScenarioHelper.Customer.ID));

      var transferKeyDate = DateTime.Parse(message.ActionUTC).KeyDate();
      Assert.AreEqual(1, createdViews.Count(x => x.fk_CustomerID == ScenarioHelper.CatCorp.ID && x.StartKeyDate == transferKeyDate));
      Assert.AreEqual(1, createdViews.Count(x => x.fk_CustomerID == ScenarioHelper.ParentDealer.ID && x.StartKeyDate == transferKeyDate));
      Assert.AreEqual(1, createdViews.Count(x => x.fk_CustomerID == ScenarioHelper.Dealer.ID && x.StartKeyDate == transferKeyDate));
      Assert.AreEqual(1, createdViews.Count(x => x.fk_CustomerID == ScenarioHelper.ParentCustomer.ID && x.StartKeyDate == transferKeyDate));
      Assert.AreEqual(1, createdViews.Count(x => x.fk_CustomerID == ScenarioHelper.Customer.ID && x.StartKeyDate == transferKeyDate));
    }

    [DatabaseTest]
    [TestMethod]
    public void MergeAccounts_SameDealerParent_DifferentCustomerParent_Success()
    {
      ScenarioHelper.SetupHierarchy_TwoCustomerHierarchies_OneCatDealer();

      var service = ScenarioHelper.SetupSeries522WithService(ScenarioHelper.Account, ServiceTypeEnum.Essentials, DateTime.UtcNow.AddMonths(-1))
        .WithView(x => x.ForAsset(ScenarioHelper.AssetSeries522).ForCustomer(ScenarioHelper.CatCorp))
        .WithView(x => x.ForAsset(ScenarioHelper.AssetSeries522).ForCustomer(ScenarioHelper.Dealer))
        .WithView(x => x.ForAsset(ScenarioHelper.AssetSeries522).ForCustomer(ScenarioHelper.Customer))
        .Save();

      var message = BSS.IBUpdatedMerge
        .PartNumber(TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522))
        .IBKey(ScenarioHelper.Series522.IBKey)
        .GpsDeviceId(ScenarioHelper.Series522.GpsDeviceID)
        .EquipmentSN(ScenarioHelper.AssetSeries522.SerialNumberVIN)
        .MakeCode(ScenarioHelper.AssetSeries522.fk_MakeCode)
        .OwnerBssId(ScenarioHelper.Account2.BSSID) 
        .Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success, "Success should be true.");

      var assetViews = (from sv in Ctx.OpContext.ServiceViewReadOnly
                        where sv.fk_AssetID == ScenarioHelper.AssetSeries522.AssetID
                        select sv).ToList();

      var terminatedViews = assetViews.Where(x => x.EndKeyDate == DateTime.Parse(message.ActionUTC).KeyDate()).ToList();
      var createdViews = assetViews.Where(x => x.EndKeyDate == 99991231).ToList();

      Assert.AreEqual(3, terminatedViews.Count, "Terminated view count not equal.");
      Assert.AreEqual(3, createdViews.Count, "Created view count not equal.");

      Assert.AreEqual(1, terminatedViews.Count(x => x.fk_CustomerID == ScenarioHelper.CatCorp.ID));
      Assert.AreEqual(1, terminatedViews.Count(x => x.fk_CustomerID == ScenarioHelper.Dealer.ID));
      Assert.AreEqual(1, terminatedViews.Count(x => x.fk_CustomerID == ScenarioHelper.Customer.ID));

      var transferKeyDate = DateTime.Parse(message.ActionUTC).KeyDate();
      
      Assert.AreEqual(1, createdViews.Count(x => x.fk_CustomerID == ScenarioHelper.CatCorp.ID && x.StartKeyDate == transferKeyDate));
      Assert.AreEqual(1, createdViews.Count(x => x.fk_CustomerID == ScenarioHelper.Dealer.ID && x.StartKeyDate == transferKeyDate));
      // ServiceView started at Service.OwnerVisibilityKeyDate to transfer history
      var historyStartKeyDate = service.OwnerVisibilityKeyDate;
      Assert.AreEqual(1, createdViews.Count(x => x.fk_CustomerID == ScenarioHelper.Customer2.ID && x.StartKeyDate == historyStartKeyDate));
    }

    [DatabaseTest]
    [TestMethod]
    public void MergeAccounts_DifferntDealerParents_SameCustomerParent_Success()
    {
      ScenarioHelper.SetupHierarchy_OneCustomerHierarchy_TwoCatDealers();

      var ownerVisibilityDate = DateTime.UtcNow.AddMonths(-1);
      var service = ScenarioHelper.SetupSeries522WithService(ScenarioHelper.Account, ServiceTypeEnum.Essentials, ownerVisibilityDate)
        .WithView(x => x.ForAsset(ScenarioHelper.AssetSeries522).ForCustomer(ScenarioHelper.CatCorp))
        .WithView(x => x.ForAsset(ScenarioHelper.AssetSeries522).ForCustomer(ScenarioHelper.Dealer))
        .WithView(x => x.ForAsset(ScenarioHelper.AssetSeries522).ForCustomer(ScenarioHelper.Customer).StartsOn(ownerVisibilityDate))
        .Save();

      var message = BSS.IBUpdatedMerge
        .PartNumber(TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522))
        .IBKey(ScenarioHelper.Series522.IBKey)
        .GpsDeviceId(ScenarioHelper.Series522.GpsDeviceID)
        .EquipmentSN(ScenarioHelper.AssetSeries522.SerialNumberVIN)
        .MakeCode(ScenarioHelper.AssetSeries522.fk_MakeCode)
        .OwnerBssId(ScenarioHelper.Account2.BSSID) 
        .Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success, "Success should be true.");

      var assetViews = (from sv in Ctx.OpContext.ServiceViewReadOnly
                        where sv.fk_AssetID == ScenarioHelper.AssetSeries522.AssetID
                        select sv).ToList();

      var terminatedViews = assetViews.Where(x => x.EndKeyDate == DateTime.Parse(message.ActionUTC).KeyDate()).ToList();
      var createdViews = assetViews.Where(x => x.EndKeyDate == 99991231).ToList();

      Assert.AreEqual(3, terminatedViews.Count, "Terminated view count not equal.");
      Assert.AreEqual(3, createdViews.Count, "Created view count not equal.");

      Assert.AreEqual(1, terminatedViews.Count(x => x.fk_CustomerID == ScenarioHelper.CatCorp.ID));
      Assert.AreEqual(1, terminatedViews.Count(x => x.fk_CustomerID == ScenarioHelper.Dealer.ID));
      Assert.AreEqual(1, terminatedViews.Count(x => x.fk_CustomerID == ScenarioHelper.Customer.ID));

      var transferKeyDate = DateTime.Parse(message.ActionUTC).KeyDate();
      var newViewStartDate = service.ActivationKeyDate.FromKeyDate().AddMonths(-13).KeyDate();
      Assert.AreEqual(1, createdViews.Count(x => x.fk_CustomerID == ScenarioHelper.CatCorp.ID && x.StartKeyDate == transferKeyDate));
      Assert.AreEqual(1, createdViews.Count(x => x.fk_CustomerID == ScenarioHelper.Customer.ID && x.StartKeyDate == transferKeyDate));
      Assert.AreEqual(1, createdViews.Count(x => x.fk_CustomerID == ScenarioHelper.Dealer2.ID && x.StartKeyDate == newViewStartDate));
   
    }

    [DatabaseTest]
    [TestMethod]
    public void MergeAccounts_DifferntDealerParents_DifferentCustomerParents_Success()
    {
      ScenarioHelper.SetupHierarchy_TwoCustomerHierarchies_TwoCatDealers();

      var service = ScenarioHelper.SetupSeries522WithService(ScenarioHelper.Account, ServiceTypeEnum.Essentials, DateTime.UtcNow.AddMonths(-1))
        .WithView(x => x.ForAsset(ScenarioHelper.AssetSeries522).ForCustomer(ScenarioHelper.CatCorp))
        .WithView(x => x.ForAsset(ScenarioHelper.AssetSeries522).ForCustomer(ScenarioHelper.Dealer))
        .WithView(x => x.ForAsset(ScenarioHelper.AssetSeries522).ForCustomer(ScenarioHelper.Customer))
        .Save();

      var message = BSS.IBUpdatedMerge
        .PartNumber(TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522))
        .IBKey(ScenarioHelper.Series522.IBKey)
        .GpsDeviceId(ScenarioHelper.Series522.GpsDeviceID)
        .EquipmentSN(ScenarioHelper.AssetSeries522.SerialNumberVIN)
        .MakeCode(ScenarioHelper.AssetSeries522.fk_MakeCode)
        .OwnerBssId(ScenarioHelper.Account2.BSSID) 
        .Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success, "Success should be true.");

      var assetViews = (from sv in Ctx.OpContext.ServiceViewReadOnly
                        where sv.fk_AssetID == ScenarioHelper.AssetSeries522.AssetID
                        select sv).ToList();

      var terminatedViews = assetViews.Where(x => x.EndKeyDate == DateTime.Parse(message.ActionUTC).KeyDate()).ToList();
      var createdViews = assetViews.Where(x => x.EndKeyDate == 99991231).ToList();

      Assert.AreEqual(3, terminatedViews.Count, "Terminated view count not equal.");
      Assert.AreEqual(3, createdViews.Count, "Created view count not equal.");

      Assert.AreEqual(1, terminatedViews.Count(x => x.fk_CustomerID == ScenarioHelper.CatCorp.ID));
      Assert.AreEqual(1, terminatedViews.Count(x => x.fk_CustomerID == ScenarioHelper.Dealer.ID));
      Assert.AreEqual(1, terminatedViews.Count(x => x.fk_CustomerID == ScenarioHelper.Customer.ID));

      var transferKeyDate = DateTime.Parse(message.ActionUTC).KeyDate();

      Assert.AreEqual(1, createdViews.Count(x => x.fk_CustomerID == ScenarioHelper.CatCorp.ID && x.StartKeyDate == transferKeyDate));

      Assert.AreEqual(1, createdViews.Count(x => x.fk_CustomerID == ScenarioHelper.Customer2.ID && x.StartKeyDate < transferKeyDate));
      Assert.AreEqual(1, createdViews.Count(x => x.fk_CustomerID == ScenarioHelper.Dealer2.ID && x.StartKeyDate < transferKeyDate));

    }

    [DatabaseTest]
    [TestMethod]
    public void MergeDealers_SameNetwork_Success()
    {
      ScenarioHelper.SetupHierarchy_TwoCustomerHierarchies_TwoCatDealers();

      var service = ScenarioHelper.SetupSeries522WithService(ScenarioHelper.Dealer, ServiceTypeEnum.Essentials, DateTime.UtcNow.AddMonths(-1))
        .WithView(x => x.ForAsset(ScenarioHelper.AssetSeries522).ForCustomer(ScenarioHelper.CatCorp))
        .WithView(x => x.ForAsset(ScenarioHelper.AssetSeries522).ForCustomer(ScenarioHelper.Dealer))
        .Save();

      var message = BSS.IBUpdatedMerge
        .PartNumber(TestHelper.GetPartNumberByDeviceType(DeviceTypeEnum.Series522))
        .IBKey(ScenarioHelper.Series522.IBKey)
        .GpsDeviceId(ScenarioHelper.Series522.GpsDeviceID)
        .EquipmentSN(ScenarioHelper.AssetSeries522.SerialNumberVIN)
        .MakeCode(ScenarioHelper.AssetSeries522.fk_MakeCode)
        .OwnerBssId(ScenarioHelper.Dealer2.BSSID) // Update Ownership to Dealer2
        .Build();

      WorkflowResult result = TestHelper.ExecuteWorkflow(message);

      Assert.IsTrue(result.Success, "Success should be true.");

      var assetViews = (from sv in Ctx.OpContext.ServiceViewReadOnly
                        where sv.fk_AssetID == ScenarioHelper.AssetSeries522.AssetID
                        select sv).ToList();

      var terminatedViews = assetViews.Where(x => x.EndKeyDate == DateTime.Parse(message.ActionUTC).KeyDate()).ToList();
      var createdViews = assetViews.Where(x => x.EndKeyDate == 99991231).ToList();

      Assert.AreEqual(2, terminatedViews.Count, "Terminated view count not equal.");
      Assert.AreEqual(2, createdViews.Count, "Created view count not equal.");

      Assert.AreEqual(1, terminatedViews.Count(x => x.fk_CustomerID == ScenarioHelper.CatCorp.ID));
      Assert.AreEqual(1, terminatedViews.Count(x => x.fk_CustomerID == ScenarioHelper.Dealer.ID));

      var transferKeyDate = DateTime.Parse(message.ActionUTC).KeyDate();
      Assert.AreEqual(1, createdViews.Count(x => x.fk_CustomerID == ScenarioHelper.CatCorp.ID && x.StartKeyDate == transferKeyDate));
      Assert.AreEqual(1, createdViews.Count(x => x.fk_CustomerID == ScenarioHelper.Dealer2.ID && x.StartKeyDate < transferKeyDate));
    }

  }
}
