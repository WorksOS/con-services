using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.DataAccess.Views;
using VSS.UnitTest.Common;

namespace UnitTests.DataAccess_Tests
{
	[TestClass]
	public class DbViewAccessTest : UnitTestBase
	{

		[TestMethod]
		[DatabaseTest]
		public void DbViewAccess_TrmbOpsAsUserXWithActiveUserX_MatchesResultsFromView()
		{
			// Normal customer, user "mrX", has fleet of 3 assets, two selected in an active session
			Customer normalCustomer = Entity.Customer.EndCustomer.SyncWithRpt().Save();
			User mrX = Entity.User.Username("MrX").Password("mrXLovesMrsX").ForCustomer(normalCustomer).Save();
			Asset customerAsset1 = Entity.Asset.SerialNumberVin("AAA").WithDevice(Entity.Device.MTS521.Save()).SyncWithRpt().Save();
			Asset customerAsset2 = Entity.Asset.SerialNumberVin("BBB").WithDevice(Entity.Device.MTS522.Save()).SyncWithRpt().Save();
			Asset customerAsset3 = Entity.Asset.SerialNumberVin("CCC").WithDevice(Entity.Device.MTS523.Save()).SyncWithRpt().Save();
			Entity.Service.Essentials.ForDevice(customerAsset1.Device).WithView(view => view.ForAsset(customerAsset1).ForCustomer(normalCustomer)).SyncWithRpt().Save();
			Entity.Service.Essentials.ForDevice(customerAsset2.Device).WithView(view => view.ForAsset(customerAsset2).ForCustomer(normalCustomer)).SyncWithRpt().Save();
			Entity.Service.Essentials.ForDevice(customerAsset3.Device).WithView(view => view.ForAsset(customerAsset3).ForCustomer(normalCustomer)).SyncWithRpt().Save();
			ActiveUser activeMrX = Entity.ActiveUser.ForUser(mrX).Save();
			SessionContext mrXSession1 = Helpers.Sessions.GetContextFor(activeMrX, true);
			Helpers.WorkingSet.Select(activeMrX, new List<long> { customerAsset1.AssetID, customerAsset3.AssetID });

			int count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
									 where aws.fk_ActiveUserID == mrXSession1.ActiveUserID
										 && (aws.fk_AssetID == customerAsset1.AssetID || aws.fk_AssetID == customerAsset3.AssetID)
									 select aws.fk_ActiveUserID).Count();
			Assert.AreEqual(2, count, "MrX has asset 1 and 3 selected in his working set");

			var dbVA = new DbViewAccess();
			var assetIdsDbViewAccess = dbVA.GetAssetWorkingSetItems(mrXSession1.ActiveUserID);
			int assetIdsDbViewAccessCount = assetIdsDbViewAccess.Count(x => x.AssetId == customerAsset1.AssetID || x.AssetId == customerAsset3.AssetID);
			Assert.AreEqual(count, assetIdsDbViewAccessCount, "Db View Access does not match");

			int rptCount = (from ws in Ctx.RptContext.vw_WorkingSet
											where ws.ifk_ActiveUserID == mrXSession1.ActiveUserID
											 && (ws.fk_DimAssetID == customerAsset1.AssetID || ws.fk_DimAssetID == customerAsset3.AssetID)
											select 1).Count();
			Assert.AreEqual(2, rptCount, "MrX has asset 1 and 3 selected in his working set in NH_RPT also");

			var assetIdsRptDbViewAccess = dbVA.GetWorkingSetItems(mrXSession1.ActiveUserID);
			int assetIdsRptDbViewAccessCount = assetIdsRptDbViewAccess.Count(x => x.AssetId == customerAsset1.AssetID || x.AssetId == customerAsset3.AssetID);

			Assert.AreEqual(rptCount, assetIdsRptDbViewAccessCount, "Db View Access does not match");
			// Trimble Ops user does an impersonation of "MrX" = TrmbOpsAsMrX
			// TrimbleOpsAsMrX does not see MrX's working set
			// TrimbleOpsAsMrX can make their own asset selection, without affected MrX's asset selection

			SessionContext opsSession1 = Helpers.Sessions.GetContextFor(TestData.TrimbleOpsActiveUser);
			SessionContext opsAsMrX = API.Session.ImpersonatedLogin(opsSession1, mrX.Name);
			Assert.AreEqual(mrX.Name, opsAsMrX.UserName, "Impersonated user properties expected in SessionContext");
			Assert.AreEqual(normalCustomer.Name, opsAsMrX.CustomerName, "Impersonated user's customer properties are expected in SessionContext");
			ActiveUser opsAsMrXActiveUser = (from au in Ctx.OpContext.ActiveUserReadOnly
																			 where au.ID == opsAsMrX.ActiveUserID
																			 select au).Single();

			Helpers.WorkingSet.Select(opsAsMrXActiveUser, new List<long> { customerAsset2.AssetID });
			
			count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
							 where aws.fk_ActiveUserID == opsAsMrX.ActiveUserID
							 select aws.fk_ActiveUserID).Count();
			Assert.AreEqual(3, count, "Trimble Ops can see all of impersonated user's assets");

			assetIdsDbViewAccess = dbVA.GetAssetWorkingSetItems(opsAsMrX.ActiveUserID);
			Assert.AreEqual(count, assetIdsDbViewAccess.Count, "Trimble Ops count for DbViewAccess does not match");

			count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
							 where aws.fk_ActiveUserID == mrXSession1.ActiveUserID
								 && (aws.fk_AssetID == customerAsset1.AssetID || aws.fk_AssetID == customerAsset3.AssetID)
								 && aws.Selected
							 select aws.fk_ActiveUserID).Count();
			Assert.AreEqual(2, count, "MrX's asset working set is not changed by trmbOpsAsMrX");
			rptCount = (from ws in Ctx.RptContext.vw_WorkingSet
									where ws.ifk_ActiveUserID == mrXSession1.ActiveUserID
									 && (ws.fk_DimAssetID == customerAsset1.AssetID || ws.fk_DimAssetID == customerAsset3.AssetID)
									select 1).Count();
			Assert.AreEqual(2, rptCount, "MrX's asset working set is not changed by trmbOpsAsMrX, in NH_RPT also");

			assetIdsRptDbViewAccess = dbVA.GetWorkingSetItems(mrXSession1.ActiveUserID).
				Where(x => x.AssetId == customerAsset1.AssetID || x.AssetId == customerAsset3.AssetID).ToList();
			Assert.AreEqual(rptCount, assetIdsRptDbViewAccess.Count, "DbViewAccess does not match");


			count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
							 where aws.fk_ActiveUserID == opsAsMrX.ActiveUserID
								 && aws.fk_AssetID == customerAsset2.AssetID
								 && aws.Selected
							 select aws.fk_ActiveUserID).Count();
			Assert.AreEqual(1, count, "TrmbOpsAsMrX's asset working set is separate to mrX's");
			int viewAccessCount = dbVA.GetAssetWorkingSetItems(opsAsMrX.ActiveUserID, filterForSelected: true)
				.Count(x => x.AssetId == customerAsset2.AssetID);
			Assert.AreEqual(count, viewAccessCount, "DbViewAccess count does not match");

			rptCount = (from ws in Ctx.RptContext.vw_WorkingSet
									where ws.ifk_ActiveUserID == opsAsMrX.ActiveUserID
									 && ws.fk_DimAssetID == customerAsset2.AssetID
									select 1).Count();

			int rptViewAccessCount = dbVA.GetWorkingSetItems(opsAsMrX.ActiveUserID)
				.Count(x => x.AssetId == customerAsset2.AssetID);
			Assert.AreEqual(rptCount, rptViewAccessCount, "DbViewAccess count does not match");

		}

		[TestMethod]
		[DatabaseTest]
		public void DbViewAccess_DealerAsUserXWithActiveUserX_MatchesResultsFromView()
		{
			// Normal customer, user "mrX", has fleet of 3 assets, two selected in an active session
			Customer normalCustomer = Entity.Customer.EndCustomer.SyncWithRpt().Save();
			User mrX = Entity.User.Username("MrX").Password("mrXLovesMrsX").ForCustomer(normalCustomer).Save();
			Asset customerAsset1 = Entity.Asset.SerialNumberVin("AAA").WithDevice(Entity.Device.MTS521.Save()).SyncWithRpt().Save();
			Asset customerAsset2 = Entity.Asset.SerialNumberVin("BBB").WithDevice(Entity.Device.MTS522.Save()).SyncWithRpt().Save();
			Asset customerAsset3 = Entity.Asset.SerialNumberVin("CCC").WithDevice(Entity.Device.MTS523.Save()).SyncWithRpt().Save();
			Entity.Service.Essentials.ForDevice(customerAsset1.Device).WithView(view => view.ForAsset(customerAsset1).ForCustomer(normalCustomer)).SyncWithRpt().Save();
			Entity.Service.Essentials.ForDevice(customerAsset2.Device).WithView(view => view.ForAsset(customerAsset2).ForCustomer(normalCustomer)).SyncWithRpt().Save();
			Entity.Service.Essentials.ForDevice(customerAsset3.Device).WithView(view => view.ForAsset(customerAsset3).ForCustomer(normalCustomer)).SyncWithRpt().Save();
			ActiveUser activeMrX = Entity.ActiveUser.ForUser(mrX).Save();
			SessionContext mrXSession1 = Helpers.Sessions.GetContextFor(activeMrX, true);
			Helpers.WorkingSet.Select(activeMrX, new List<long> { customerAsset1.AssetID, customerAsset3.AssetID });

			int count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
									 where aws.fk_ActiveUserID == mrXSession1.ActiveUserID
										 && (aws.fk_AssetID == customerAsset1.AssetID || aws.fk_AssetID == customerAsset3.AssetID)
									 select aws.fk_ActiveUserID).Count();
			Assert.AreEqual(2, count, "MrX has asset 1 and 3 selected in his working set");

			var dbVA = new DbViewAccess();
			int assetCount = dbVA.GetAssetWorkingSetItems(mrXSession1.ActiveUserID)
				.Count(x => x.AssetId == customerAsset1.AssetID || x.AssetId == customerAsset3.AssetID);
			Assert.AreEqual(count, assetCount, "Asset counts from dbViewAccess does not match");

			int rptCount = (from ws in Ctx.RptContext.vw_WorkingSet
											where ws.ifk_ActiveUserID == mrXSession1.ActiveUserID
											 && (ws.fk_DimAssetID == customerAsset1.AssetID || ws.fk_DimAssetID == customerAsset3.AssetID)
											select 1).Count();
			Assert.AreEqual(2, rptCount, "MrX has asset 1 and 3 selected in his working set in NH_RPT also");

			int rptAssetCount = dbVA.GetWorkingSetItems(mrXSession1.ActiveUserID)
				.Count(x => x.AssetId == customerAsset1.AssetID || x.AssetId == customerAsset3.AssetID);

			Assert.AreEqual(rptCount, rptAssetCount, "Asset counts from dbViewAccess does not match");
			// MrX has relationship with Dealer, through an Account
			Customer dealer = Entity.Customer.Dealer.SyncWithRpt().Save();
			Customer account = Entity.Customer.Account.SyncWithRpt().Save();
			Entity.CustomerRelationship.Relate(dealer, account).Save();
			Entity.CustomerRelationship.Relate(normalCustomer, account).Save();
			// Dealer has views on two of the three assets
			Entity.Service.Essentials.ForDevice(customerAsset1.Device).WithView(view => view.ForAsset(customerAsset1).ForCustomer(dealer)).SyncWithRpt().Save();
			Entity.Service.Essentials.ForDevice(customerAsset2.Device).WithView(view => view.ForAsset(customerAsset2).ForCustomer(dealer)).SyncWithRpt().Save();
			User dealerUser = Entity.User.Username("DealerUser").Password("ISellBigTrucks").ForCustomer(dealer).Save();
			ActiveUser dealerAU = Entity.ActiveUser.ForUser(dealerUser).Save();
			Helpers.WorkingSet.Populate(dealerAU);

			// Dealer does an impersonation of "MrX" = DealerAsMrX
			// This does not expire MrX's session.
			// DealerAsMrX asset population is intersection of Dealers assets and MrX's assets
			// DealerAsMrX does not see MrX's working set
			// DealerAsMrX can make their own asset selection, without affected MrX's asset selection
			SessionContext dealerSession1 = Helpers.Sessions.GetContextFor(dealerAU);
			SessionContext dealerAsMrX = API.Session.ImpersonatedLogin(dealerSession1, mrX.Name);
			Assert.AreEqual(mrX.Name, dealerAsMrX.UserName, "Impersonated user properties expected in SessionContext");
			Assert.AreEqual(normalCustomer.Name, dealerAsMrX.CustomerName, "Impersonated user's customer properties are expected in SessionContext");

			ActiveUser dealerAsMrXActiveUser = (from au in Ctx.OpContext.ActiveUserReadOnly
																					where au.ID == dealerAsMrX.ActiveUserID
																					select au).Single();
			Helpers.WorkingSet.Select(dealerAsMrXActiveUser, new List<long> { customerAsset2.AssetID });
			
			count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
							 where aws.fk_ActiveUserID == dealerAsMrX.ActiveUserID
							 select aws.fk_ActiveUserID).Count();
			Assert.AreEqual(2, count, "Asset 1 and 2 are in the intersection of dealer assets with customers assets");

			assetCount = dbVA.GetAssetWorkingSetItems(dealerAsMrX.ActiveUserID).Count();
			Assert.AreEqual(count, assetCount, "Asset counts from dbViewAccess does not match");
			count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
							 where aws.fk_ActiveUserID == mrXSession1.ActiveUserID
								 && (aws.fk_AssetID == customerAsset1.AssetID || aws.fk_AssetID == customerAsset3.AssetID)
								 && aws.Selected
							 select aws.fk_ActiveUserID).Count();
			Assert.AreEqual(2, count, "MrX's asset working set is not changed by dealerAsMrX");
			assetCount = dbVA.GetAssetWorkingSetItems(mrXSession1.ActiveUserID, filterForSelected: true)
				.Count(x => x.AssetId == customerAsset1.AssetID || x.AssetId == customerAsset3.AssetID);
			Assert.AreEqual(count, assetCount, "Asset counts from dbViewAccess does not match");

			rptCount = (from ws in Ctx.RptContext.vw_WorkingSet
									where ws.ifk_ActiveUserID == mrXSession1.ActiveUserID
									 && (ws.fk_DimAssetID == customerAsset1.AssetID || ws.fk_DimAssetID == customerAsset3.AssetID)
									select 1).Count();
			Assert.AreEqual(2, rptCount, "MrX's asset working set is not changed by dealerAsMrX, in NH_RPT also");
			rptAssetCount = dbVA.GetWorkingSetItems(mrXSession1.ActiveUserID)
				.Count(x => x.AssetId == customerAsset1.AssetID || x.AssetId == customerAsset3.AssetID);
			Assert.AreEqual(rptCount, rptAssetCount, "Asset counts from dbViewAccess does not match");

			count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
							 where aws.fk_ActiveUserID == dealerAsMrX.ActiveUserID
								 && aws.fk_AssetID == customerAsset2.AssetID
								 && aws.Selected
							 select aws.fk_ActiveUserID).Count();
			Assert.AreEqual(1, count, "DealerAsMrX's asset working set is separate to mrX's");
			assetCount = dbVA.GetAssetWorkingSetItems(dealerAsMrX.ActiveUserID, filterForSelected: true)
				.Count(x => x.AssetId == customerAsset2.AssetID);
			Assert.AreEqual(count, assetCount, "Asset counts from dbViewAccess does not match");

			rptCount = (from ws in Ctx.RptContext.vw_WorkingSet
									where ws.ifk_ActiveUserID == dealerAsMrX.ActiveUserID
									 && ws.fk_DimAssetID == customerAsset2.AssetID
									select 1).Count();
			Assert.AreEqual(1, rptCount, "DealerAsMrX's asset working set is separate to mrX's, in nh_rpt also");

			rptAssetCount = dbVA.GetWorkingSetItems(dealerAsMrX.ActiveUserID)
				.Count(x => x.AssetId == customerAsset2.AssetID);

			Assert.AreEqual(rptCount, rptAssetCount, "Asset counts from dbViewAccess does not match");

		}
	}
}
