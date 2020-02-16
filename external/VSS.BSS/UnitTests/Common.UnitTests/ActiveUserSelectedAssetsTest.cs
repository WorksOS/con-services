using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests
{
  [TestClass]
  public class ActiveUserSelectedAssetsTest : UnitTestBase
  {
    [TestMethod]
    [DatabaseTest]
    public void SaveFirstSelection()
    {
      var s1 = TestData.EssentialsMTS521;
      var s2 = TestData.EssentialsMTS522;
      SessionContext sesh = Helpers.Sessions.GetContextFor(TestData.CustomerUserActiveUser, true, false);
      int existingCount = (from selection in Ctx.OpContext.ActiveUserAssetSelectionReadOnly
                           where selection.fk_ActiveUserID == sesh.ActiveUserID
                           select selection.fk_ActiveUserID).Count();
      Assert.AreEqual(0, existingCount, "Test should start off with no assets selected for this active user");

      List<long> newSelection = new List<long> { TestData.TestAssetMTS521.AssetID, TestData.TestAssetMTS522.AssetID };
      ActiveUserSelectedAssetsAccess.Save(sesh.ActiveUserID, newSelection);

      ValidateSelection(sesh, newSelection);
    }

    [TestMethod]
    [DatabaseTest]
    public void SaveNewEqualsExisting()
    {
      var s1 = TestData.EssentialsMTS521;
      var s2 = TestData.EssentialsMTS522;
      SessionContext sesh = Helpers.Sessions.GetContextFor(TestData.CustomerUserActiveUser, true, true);
      var existing = (from selection in Ctx.OpContext.ActiveUserAssetSelectionReadOnly
                      where selection.fk_ActiveUserID == sesh.ActiveUserID
                      select selection.fk_AssetID).ToList();
      Assert.IsTrue(existing.Count > 0, "Test should start off with some assets selected for this active user");

      List<long> newSelection = new List<long>(existing);
      ActiveUserSelectedAssetsAccess.Save(sesh.ActiveUserID, newSelection);

      ValidateSelection(sesh, newSelection);
    }

    [TestMethod]
    [DatabaseTest]
    public void SaveNewReplacesExisting()
    {
      var s1 = TestData.EssentialsMTS521;
      var s2 = TestData.EssentialsMTS522;
      SessionContext sesh = Helpers.Sessions.GetContextFor(TestData.CustomerUserActiveUser, true, true);
      var existing = (from selection in Ctx.OpContext.ActiveUserAssetSelectionReadOnly
                      where selection.fk_ActiveUserID == sesh.ActiveUserID
                      select selection.fk_AssetID).ToList();
      Assert.IsTrue(existing.Count > 0, "Test should start off with some assets selected for this active user");

      Asset newAsset = Entity.Asset.WithDevice(Entity.Device.TrimTrac.OwnerBssId(TestData.TestAccount.BSSID).Save()).WithCoreService().SyncWithRpt().Save();
      sesh = Helpers.Sessions.GetContextFor(TestData.CustomerUserActiveUser, true);

      List<long> newSelection = new List<long>{newAsset.AssetID};
      Helpers.WorkingSet.Select(TestData.CustomerUserActiveUser, newSelection);

      ValidateSelection(sesh, newSelection);
    }

    [TestMethod]
    [DatabaseTest]
    public void SaveNewOverlapsExisting()
    {
      var s1 = TestData.EssentialsMTS521;
      var s2 = TestData.EssentialsMTS522;
      SessionContext sesh = Helpers.Sessions.GetContextFor(TestData.CustomerUserActiveUser, true, true);
      var existing = (from selection in Ctx.OpContext.ActiveUserAssetSelectionReadOnly
                      where selection.fk_ActiveUserID == sesh.ActiveUserID
                      select selection.fk_AssetID).ToList();
      Assert.IsTrue(existing.Count > 0, "Test should start off with some assets selected for this active user");

      Asset newAsset = Entity.Asset.WithDevice(Entity.Device.TrimTrac.OwnerBssId(TestData.TestAccount.BSSID).Save()).WithCoreService().SyncWithRpt().Save();
      sesh = Helpers.Sessions.GetContextFor(TestData.CustomerUserActiveUser,true);

      List<long> newSelection = new List<long> { newAsset.AssetID, TestData.TestAssetMTS522.AssetID };
      Helpers.WorkingSet.Select(TestData.CustomerUserActiveUser,newSelection);

      ValidateSelection(sesh, newSelection);
    }

    [TestMethod]
    [DatabaseTest]
    public void SaveNewOverlapsExistingAndDeselecteds()
    {
      var s1 = TestData.EssentialsMTS521;
      var s2 = TestData.EssentialsMTS522;
      var s3 = TestData.EssentialsMTS523;
      SessionContext sesh = Helpers.Sessions.GetContextFor(TestData.CustomerUserActiveUser, true);

      List<long> firstSelection = new List<long> { TestData.TestAssetMTS522.AssetID };
      Helpers.WorkingSet.Select(TestData.CustomerUserActiveUser, firstSelection);
      ValidateSelection(sesh, firstSelection);

      List<long> secondSelection = new List<long> { TestData.TestAssetMTS521.AssetID };
      Helpers.WorkingSet.Select(TestData.CustomerUserActiveUser, secondSelection);
      ValidateSelection(sesh, secondSelection);

      List<long> thirdSelection = new List<long> { TestData.TestAssetMTS521.AssetID, TestData.TestAssetMTS522.AssetID };
      Helpers.WorkingSet.Select(TestData.CustomerUserActiveUser, thirdSelection);
      ValidateSelection(sesh, thirdSelection);
    }

    [TestMethod]
    [DatabaseTest]
    public void SaveEmptySelection()
    {
      var s1 = TestData.EssentialsMTS521;
      var s2 = TestData.EssentialsMTS522;
      SessionContext sesh = Helpers.Sessions.GetContextFor(TestData.CustomerUserActiveUser, true, true);
      var existing = (from selection in Ctx.OpContext.ActiveUserAssetSelectionReadOnly
                      where selection.fk_ActiveUserID == sesh.ActiveUserID
                      select selection.fk_AssetID).ToList();
      Assert.IsTrue(existing.Count > 0, "Test should start off with some assets selected for this active user");

      List<long> newSelection = new List<long>();
      ActiveUserSelectedAssetsAccess.Save(sesh.ActiveUserID, newSelection);

      ValidateSelection(sesh, newSelection);
    }


    #region Privates
    private void ValidateSelection(SessionContext sesh, List<long> newSelection)
    {
      List<long> matches = (from selection in Ctx.OpContext.ActiveUserAssetSelectionReadOnly
                            where selection.fk_ActiveUserID == sesh.ActiveUserID &&
                                   newSelection.Contains(selection.fk_AssetID)
                            orderby selection.fk_AssetID
                            select selection.fk_AssetID).ToList();
      Assert.AreEqual(newSelection.Count, matches.Count, "Expect there to be an exact match in the number of items in the ActiveUserAssetSelection and the provided selection");

      newSelection = newSelection.OrderBy(of => of).ToList();
      for (int i = 0; i < matches.Count; i++)
      {
        Assert.AreEqual(newSelection[i], matches[i], "Expect assetIDs in ActiveUserAssetSelection to match. Mismatch at index " + i.ToString());
      }

      List<long> rptMatches = (from ws in Ctx.RptContext.vw_WorkingSet
                               where ws.ifk_ActiveUserID == sesh.ActiveUserID
                               orderby ws.fk_DimAssetID
                               select ws.fk_DimAssetID).ToList();
      Assert.AreEqual(newSelection.Count, rptMatches.Count, "Expect there to be an exact match in the number of items in the NH_RPT.WorkingSet and the provided selection");
      for (int i = 0; i < rptMatches.Count; i++)
      {
        Assert.AreEqual(newSelection[i], rptMatches[i], "Expect assetIDs in NH_RPT..WorkingSet to match. Mismatch at index " + i.ToString());
      }
    }

    private void ValidateProjectIDSelection(SessionContext sesh, long projID, List<long> matchingProjectAssets)
    {
      var items = (from aws in Ctx.OpContext.ActiveUserAssetSelectionReadOnly
                   where aws.fk_ProjectID == projID && aws.fk_ActiveUserID == sesh.ActiveUserID
                   orderby aws.fk_AssetID
                   select aws.fk_AssetID).ToList();
      Assert.AreEqual(matchingProjectAssets.Count, items.Count, "Mismatch in count of number of expected assets with projectID, va actual");

      matchingProjectAssets = matchingProjectAssets.OrderBy(of => of).ToList();
      for (int i = 0; i < items.Count; i++)
      {
        Assert.AreEqual(matchingProjectAssets[i], items[i], "Expect projectID to be set on all assetIDs in match. Problem at index " + i.ToString());
      }
    }

    #endregion
  }
}
