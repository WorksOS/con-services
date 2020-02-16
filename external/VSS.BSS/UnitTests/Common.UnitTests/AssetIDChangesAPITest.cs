using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;
using Microsoft.Web.Services;
using Microsoft.ServiceModel.Web;

namespace UnitTests
{
  [TestClass()]
  public class AssetIDChangesAPITest : UnitTestBase
  {
    
    private void AddAssetAliases()
    {
      Asset testAsset = TestData.TestAssetPL121;
      Customer testCust = TestData.TestCustomer;
      User testUser = TestData.TestCustomerAdminUser;
      AssetAlias alias = new AssetAlias();
      alias.DealerAccountCode = "TestAccountCode";
      alias.fk_AssetID = testAsset.AssetID;
      alias.fk_CustomerID = testCust.ID;
      alias.fk_UserID = testUser.ID;
      alias.IBKey = "TestKey";
      alias.InsertUTC = DateTime.Parse("2013-01-16T12:00:00Z");
      alias.Name = "NewAsset1";
      alias.NetworkCustomerCode = "TestCustCode";
      alias.NetworkDealerCode = "TestDealerCode";
      alias.OwnerBSSID = testCust.BSSID;
      Ctx.OpContext.AssetAlias.AddObject(alias);
      AssetAlias alias2 = new AssetAlias();
      alias2.DealerAccountCode = "TestAccountCode2";
      alias2.fk_AssetID = testAsset.AssetID;
      alias2.fk_CustomerID = testCust.ID;
      alias2.fk_UserID = testUser.ID;
      alias2.IBKey = "TestKey2";
      alias2.InsertUTC = DateTime.Parse("2013-01-17T12:00:00Z");
      alias2.Name = "NewAsset2";
      alias2.NetworkCustomerCode = "TestCustCode2";
      alias2.NetworkDealerCode = "TestDealerCode2";
      alias2.OwnerBSSID = testCust.BSSID;
      Ctx.OpContext.AssetAlias.AddObject(alias2);
      AssetAlias alias3 = new AssetAlias();
      alias3.DealerAccountCode = "TestAccountCode3";
      alias3.fk_AssetID = testAsset.AssetID;
      alias3.fk_CustomerID = testCust.ID;
      alias3.fk_UserID = testUser.ID;
      alias3.IBKey = "TestKey3";
      alias3.InsertUTC = DateTime.Parse("2013-01-18T12:00:00Z");
      alias3.Name = "NewAsset3";
      alias3.NetworkCustomerCode = "TestCustCode3";
      alias3.NetworkDealerCode = "TestDealerCode3";
      alias3.OwnerBSSID = testCust.BSSID;
      Ctx.OpContext.AssetAlias.AddObject(alias3);
      AssetAlias alias4 = new AssetAlias();
      alias4.DealerAccountCode = "TestAccountCode4";
      alias4.fk_AssetID = testAsset.AssetID;
      alias4.fk_CustomerID = testCust.ID;
      alias4.fk_UserID = testUser.ID;
      alias4.IBKey = "TestKey4";
      alias4.InsertUTC = DateTime.Parse("2013-01-18T12:30:00Z");
      alias4.Name = "NewAsset4";
      alias4.NetworkCustomerCode = "TestCustCode4";
      alias4.NetworkDealerCode = "TestDealerCode4";
      alias4.OwnerBSSID = testCust.BSSID;
      Ctx.OpContext.AssetAlias.AddObject(alias4);
      Ctx.OpContext.SaveChanges(); 
    }
    [TestMethod()]
    [DatabaseTest()]
    public void AssetIDChangesTest_with4()
    {
      AddAssetAliases();   
      AssetIDChanges result = AssetIDChangesAPI.GetAssetIDChanges(DateTime.Parse("2013-01-16T12:00:00Z"));
      Assert.IsNotNull(result, "Result cannot be null");
      Assert.AreEqual(4, result.AssetInfo.Count, "Invalid result - Edit Asset ID Failed");
    }
    [TestMethod()]
    [DatabaseTest()]
    public void AssetIDChangesTest_with3()
    {
      AddAssetAliases();
      AssetIDChanges result = AssetIDChangesAPI.GetAssetIDChanges(DateTime.Parse("2013-01-17T12:00:00Z"));
      Assert.IsNotNull(result, "Result cannot be null");
      Assert.AreEqual(3, result.AssetInfo.Count, "Invalid result - Edit Asset ID Failed");
      Assert.AreNotEqual("NewAsset1", result.AssetInfo[0].AssetName, "Invalid result set - GetAssetIDChanges failed");
      Assert.AreNotEqual("NewAsset1", result.AssetInfo[1].AssetName, "Invalid result set - GetAssetIDChanges failed");
      Assert.AreNotEqual("NewAsset1", result.AssetInfo[2].AssetName, "Invalid result set - GetAssetIDChanges failed");
    }
    [TestMethod()]
    [DatabaseTest()]
    public void AssetIDChangesTest_withNoresults()
    {
      AddAssetAliases();
      AssetIDChanges result = AssetIDChangesAPI.GetAssetIDChanges(DateTime.Parse("2013-01-19T12:00:00Z"));
      Assert.IsNotNull(result, "Result cannot be null");
      Assert.AreEqual(0, result.AssetInfo.Count, "Invalid result - Edit Asset ID Failed");
    }
  }
}
