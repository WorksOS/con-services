using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using VSS.Hosted.VLCommon;

namespace UnitTests
{
  public class BssHelper
  {
    internal static string CorePlanName = "89500-00";
    internal static string ManualPlanName = "89550-00"; // Manual Maint Log
    internal static string AddOnPlanName = "89220-00"; // Cat Utilization
    internal static string RapidReportingPlanName = "89540-00"; // Rapid reporting
    internal static int NullKeyDate = DotNetExtensions.NullKeyDate;

    #region Implementation

    internal static bool IsMTSDevice(DeviceTypeEnum deviceType)
    {
      switch (deviceType)
      {
        case DeviceTypeEnum.Series521:
        case DeviceTypeEnum.Series522:
        case DeviceTypeEnum.Series523:
        case DeviceTypeEnum.SNM940:
        case DeviceTypeEnum.PL420:
        case DeviceTypeEnum.PL421:
          return true;
      }
      return false;
    }

    internal static void CreatAssetCurrentStatus(string assetSN, AssetStatusEnum assetStatusEnum)
    {
      using (INH_RPT rptContext = ObjectContextFactory.NewNHContext<INH_RPT>())
      {
        DimAsset asset= (from a in rptContext.DimAssetReadOnly where a.SerialNumberVIN == assetSN select a).FirstOrDefault();
        Assert.IsNotNull(asset, "Failed to find asset");
        AssetCurrentStatus assetCurrentStatus = rptContext.AssetCurrentStatus.SingleOrDefault(a => a.fk_DimAssetID == asset.AssetID);

        if (assetCurrentStatus == null)
        {
          assetCurrentStatus = new AssetCurrentStatus {fk_DimAssetID = asset.AssetID};
        }

        assetCurrentStatus.fk_DimAssetWorkingStateID = (int)assetStatusEnum;
        rptContext.SaveChanges();
      }
    }

     internal static void TerminateServiceForDevice(long deviceId)
    {
      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var serviceList = (from s in ctx.Service
                   where s.fk_DeviceID == deviceId
                   select s).ToList();
        foreach (var s in serviceList)
          s.CancellationKeyDate = DateTime.UtcNow.AddDays(-1).KeyDate();

        ctx.SaveChanges();

      }
    }

    internal static void ValidateAssetOwner(string assetSN, long customerId)
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        Customer customer = (from a in opCtx.AssetReadOnly
                        from c in opCtx.CustomerReadOnly
                        join d in opCtx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                        let adh = (from ad in opCtx.AssetDeviceHistoryReadOnly where ad.OwnerBSSID != "0" select ad).
                          OrderByDescending(t => t.StartUTC).Select(t => t.OwnerBSSID).Take(1).FirstOrDefault(t => t == c.BSSID)
                        where a.SerialNumberVIN == assetSN
                        && (c.BSSID == d.OwnerBSSID || (a.fk_DeviceID == 0 && adh != null && adh == c.BSSID))
                        select c).SingleOrDefault();
        Assert.IsNotNull(customer, "Customer should not be null");
        Assert.AreEqual(customer.ID, customerId,"Wrong owner");
      }
    }

    internal static void ValidateAssetFamilyAndModel(string assetSN, string expectedFamily, string expectedModel)
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        Asset asset = (from a in opCtx.AssetReadOnly where a.SerialNumberVIN == assetSN select a).SingleOrDefault();
        Assert.IsNotNull(asset, "Failed to find asset");
        Assert.AreEqual(expectedFamily, asset.ProductFamilyName, "Incorrect Product Family Name");
        Assert.AreEqual(expectedModel, asset.Model, "Incorrect asset Model");
      }
    }

    internal static void ValidatePersonalityRecord(DevicePersonality personality, PersonalityTypeEnum expectedType, string expectedValue)
    {
      Assert.AreEqual((int)expectedType, personality.fk_PersonalityTypeID, "Incorrect personality type");
      Assert.AreEqual(expectedValue, personality.Value, "Incorrect personality value");
    }

    internal static void ValidateDeviceServicePlans(string ibKey, int expectedCount)
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        Device device = (from d in opCtx.DeviceReadOnly where d.IBKey == ibKey select d).SingleOrDefault();
        Assert.IsNotNull(device, "Failed to find device");
        int utcNowKeyDate = DateTime.UtcNow.KeyDate();
        List<Service> plans = (from svc in opCtx.ServiceReadOnly
                     where svc.Device.IBKey == ibKey
                     && svc.ActivationKeyDate <= utcNowKeyDate
                     && svc.CancellationKeyDate >= utcNowKeyDate
                     select svc).ToList();
        Assert.AreEqual(expectedCount, plans.Count, "Incorrect service plan count for device");

        if (plans.Count > 0)
        {
          foreach (Service svcPlan in plans)
          {
            Assert.AreEqual(device.ID, svcPlan.fk_DeviceID, "Incorrect Device ID");
          }
        }
      }
    }

    internal static void ValidateAssetCustomerServicePlans(string assetSN, long customerId, int expectedCount)
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        long assetId = (from a in opCtx.AssetReadOnly where a.SerialNumberVIN == assetSN select a.AssetID).SingleOrDefault();
        Assert.IsNotNull(assetId, "Failed to find asset");
        int utcNowKeyDate = DateTime.UtcNow.KeyDate();
        List<ServiceView> plans = (from svc in opCtx.ServiceViewReadOnly
                     where svc.fk_CustomerID == customerId &&
                     svc.fk_AssetID == assetId &&
                     svc.StartKeyDate <= utcNowKeyDate &&
                     svc.EndKeyDate >= utcNowKeyDate
                     select svc).ToList();
        Assert.AreEqual(expectedCount, plans.Count, "Incorrect service plan count for device");
      }
    }   

    internal static void ValidateCorporateServiceViews(string ibKey, int cancelKeyDate, int expectedCorpViewCount, int expectedServiceRecordCount)
    {
      // Need a new context to query database as Unit Under Test creates a new context using session ID
      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        long deviceID = (from d in ctx.DeviceReadOnly where d.IBKey == ibKey select d.ID).SingleOrDefault();
        Assert.IsTrue(deviceID > 0, "Failed to find device for service plan");

        long assetID = (from a in ctx.AssetReadOnly where a.fk_DeviceID == deviceID select a.AssetID).SingleOrDefault();
        Assert.IsTrue(assetID > 0, "Failed to find asset for device service plan");

        int countOfCorpServiceViews = (from sv in ctx.ServiceViewReadOnly
                                       where sv.Customer.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate
                                       && sv.StartKeyDate <= cancelKeyDate 
                                       && sv.EndKeyDate >= cancelKeyDate
                                       && sv.fk_AssetID == assetID
                                       select sv.ID).Count();
        Assert.AreEqual(expectedCorpViewCount, countOfCorpServiceViews, "Count of corporate service views is not correct");

        List<Service> totalServiceRecords = (from s in ctx.ServiceReadOnly
                                   where s.fk_DeviceID == deviceID
                                   && s.ActivationKeyDate <= cancelKeyDate
                                   && s.CancellationKeyDate >= cancelKeyDate
                                   select s).ToList();
        Assert.AreEqual(expectedServiceRecordCount, totalServiceRecords.Count, "Incorrect count of service records");

        if (ibKey.StartsWith("-"))
        {
          foreach (Service s in totalServiceRecords)
          {
            Assert.IsTrue(!s.BSSLineID.StartsWith("--"), "BssLineID should not contain --");
          }
        }

      }
    }

    internal static void ValidateMTSDeviceState(string gpsDeviceID, DeviceStateEnum expectedState)
    {
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        int opDeviceState = (from d in opCtx.DeviceReadOnly where d.GpsDeviceID == gpsDeviceID select d.fk_DeviceStateID).SingleOrDefault();
        Assert.AreEqual((int)expectedState, opDeviceState, "Incorrect Op Device State");
      }
    }

    internal static void ValidatePLNotDeregistered(string gpsDeviceID)
    {
      using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
          List<PLOut> plMessages = (from m in opCtx1.PLOutReadOnly where m.ModuleCode == gpsDeviceID select m).ToList();
        foreach (PLOut msg in plMessages)
        {
          if (msg.Body == "2")
            Assert.Fail("PL Device was sent a deregistration message.");
        }        
      }
    }

    internal static void ValidatePLDeviceState(string gpsDeviceID, DeviceStateEnum expectedState)
    {
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        int opDeviceState = (from d in opCtx.DeviceReadOnly where d.GpsDeviceID == gpsDeviceID select d.fk_DeviceStateID).SingleOrDefault();
        Assert.AreEqual((int)expectedState, opDeviceState, "Incorrect Op Device State");
      }
    }

    internal static void ValidateManualDeviceState(string ibkey, DeviceStateEnum expectedState)
    {
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        int opDeviceState = (from d in opCtx.DeviceReadOnly where d.IBKey == ibkey select d.fk_DeviceStateID).SingleOrDefault();
        Assert.AreEqual((int)expectedState, opDeviceState, "Incorrect Op Device State");
      }
    }

    internal static void ValidatePLReadOnlyState(string gpsDeviceID, bool readOnly)
    {
      using (var opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
      {
          bool isReadOnly = (from m in opCtx1.PLDeviceReadOnly where m.ModuleCode == gpsDeviceID select m.IsReadOnly).SingleOrDefault();
        Assert.AreEqual(readOnly, isReadOnly, "ReadOnly flag does not match");
      }
    }

    internal static void ValidateTrimTracDeviceState(string gpsDeviceID, DeviceStateEnum expectedState, int expectedOutCount, DateTime actionUTC)
    {
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        int opDeviceState = (from d in opCtx.DeviceReadOnly where d.GpsDeviceID == gpsDeviceID select d.fk_DeviceStateID).SingleOrDefault();
        Assert.AreEqual((int)expectedState, opDeviceState, "Incorrect Op Device State");
      }
      DeviceAPI deviceApi = new DeviceAPI();
      string unitID = deviceApi.IMEI2UnitID(gpsDeviceID);
      using (var opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
      {

          int outMessages = (from m in opCtx1.TTOutReadOnly
                             where m.UnitID == unitID && m.Payload.Contains("STKA") && m.Payload.Contains("999990")
                            && m.InsertUTC >= actionUTC select m).Count();
        Assert.AreEqual(expectedOutCount, outMessages, "TT Configuration message not sent for cancel");
      }
    }

    internal static void ValidateServiceViewMove(long srcAssetID, long destAssetID, int currentKeyDate, int expectedDestSVStartDate)
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        int countOfSVForSrcAsset = (from sv in opCtx.ServiceViewReadOnly
                                    where sv.fk_AssetID == srcAssetID
                                    && sv.StartKeyDate <= currentKeyDate
                                    && sv.EndKeyDate >= currentKeyDate
                                    select sv.ID).Count();
        Assert.AreEqual(0, countOfSVForSrcAsset, "SV's for Source Asset should be terminated");

        List<ServiceView> svForDestAsset = (from sv in opCtx.ServiceViewReadOnly
                              where sv.fk_AssetID == destAssetID
                              && sv.StartKeyDate <= currentKeyDate
                              && sv.EndKeyDate >= currentKeyDate
                              select sv).ToList();
        Assert.IsTrue(svForDestAsset.Count > 0, "There should be some active SV's for Destination Asset");
        foreach (ServiceView sv in svForDestAsset)
        {
          Assert.AreEqual(expectedDestSVStartDate, sv.StartKeyDate, "StartKeyDate does not match");
          Assert.AreEqual(BssHelper.NullKeyDate, sv.EndKeyDate, "EndKeyDate does not match");
        }
      }
    }
    
    #endregion
  }   
}
