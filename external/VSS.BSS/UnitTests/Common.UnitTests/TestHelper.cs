using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
  public static class TestHelper
  {
    /*
    public static PLStatusMessage GetPLStatusMessage(long msgID, byte sourceID)
    {
      using (NH_RPT ctx = ObjectContextFactory.NewNHContext<NH_RPT>())
      {
        return GetPLStatusMessage(msgID, sourceID, ctx);
      }
    }

    public static PLStatusMessage GetPLStatusMessage(long msgID, byte sourceID, NH_RPT ctx)
    {
      return (
        from s in ctx.PLStatusMessageReadOnly.Include("DimAsset")
        where s.MsgID == msgID
        && (s.MsgSrcID == sourceID)
        select s
      ).First();
    }

    public static void AssertPLStatusMessage(long msgID, byte sourceID, InboundDeviceEventUtilization utilEvent, long assetID)
    {
      using (NH_RPT ctx = ObjectContextFactory.NewNHContext<NH_RPT>())
      {
        AssertPLStatusMessage(ctx, msgID, sourceID, utilEvent, assetID);
      }
      
    }

    public static void AssertPLStatusMessage(NH_RPT ctx, long msgID, byte sourceID, InboundDeviceEventUtilization utilEvent, long assetID)
    {
      PLStatusMessage status = GetPLStatusMessage(msgID, sourceID, ctx);
      Assert.IsNotNull(status, "PLStatusMessage Is Null.");

      AssertCommonPLFields(utilEvent, assetID, status);

      Assert.AreEqual(utilEvent.RuntimeSeconds, utilEvent.IsOn ? status.TotalRuntimeSeconds : status.IdleRuntimeSeconds, "PLStatusMessage.RuntimeSeconds doesn't match expected.");
    }

    public static void AssertPLStatusMessage(long msgID, byte sourceID, InboundDeviceEventFuel fuelEvent, long assetID)
    {
      using (NH_RPT ctx = ObjectContextFactory.NewNHContext<NH_RPT>())
      {
        AssertPLStatusMessage(ctx, msgID, sourceID, fuelEvent, assetID);
      }
    }

    public static void AssertPLStatusMessage(NH_RPT ctx, long msgID, byte sourceID, InboundDeviceEventFuel fuelEvent, long assetID)
    {
      PLStatusMessage status = GetPLStatusMessage(msgID, sourceID, ctx);
      Assert.IsNotNull(status, "PLStatusMessage Is Null.");

      AssertCommonPLFields(fuelEvent, assetID, status);

      Assert.AreEqual(fuelEvent.IdleConsumptionGallons, status.IdleConsumptionGallons, "PLStatusMessage.IdleConsumptionGallons doesn't match expected.");
      Assert.AreEqual(fuelEvent.TotalConsumptionGallons, status.TotalConsumptionGallons, "PLStatusMessage.TotalConsumptionGallons doesn't match expected.");
      Assert.AreEqual(fuelEvent.MaxRPMFuelGallons, status.MaxRPMFuelGallons, "PLStatusMessage.MaxRPMFuelGallons doesn't match expected.");
      Assert.AreEqual(fuelEvent.PercentRemaining, status.PercentRemaining, "PLStatusMessage.PercentRemaining doesn't match expected.");
    }

    private static void AssertCommonPLFields(InboundDeviceEvent deviceEvent, long assetID, PLStatusMessage status)
    {
      Assert.AreEqual(deviceEvent.ExternalCorrelationID, Convert.ToString(status.MsgID));
      Assert.AreEqual(deviceEvent.ExternalSourceID, status.MsgSrcID, "Incorrect Message Source ID");
      Assert.AreEqual(assetID, status.DimAsset.ID, "PLStatusMessage.AssetID doesn't match expected.");
      AssertDateTime(deviceEvent.EventUTC, status.EventUTC);
      AssertDateTime(deviceEvent.ReceivedUTC, status.ReceivedUTC);

      if (deviceEvent.Position != null && deviceEvent.Position.IsValid)
      {
        AssertPLMessagePosition(status, deviceEvent.Position);
      }
    }

    private static void AssertPLMessagePosition(PLStatusMessage status, InboundDeviceInfoPosition positionInfo)
    {
      Assert.AreEqual(positionInfo.Latitude, status.Latitude.Value, .001, "PLStatusMessage.Latitude doesn't match expected.");
      Assert.AreEqual(positionInfo.Longitude, status.Longitude.Value, .001, "PLStatusMessage.Longitude doesn't match expected.");
    }
    */
    public static void AssertDateTime(DateTime? expected, DateTime? actual)
    {
      AssertDateTime(expected, actual, 3);
    }

    public static double AbsDiffInMilliseconds(DateTime expected, DateTime actual)
    {
      return Math.Abs(expected.Subtract(actual).TotalMilliseconds);
    }

    public static void AssertDateTime(DateTime? expected, DateTime? actual, int msTolerance)
    {
      Assert.IsTrue((actual.HasValue && expected.HasValue) || (!actual.HasValue && !expected.HasValue));
      if (expected.HasValue && actual.HasValue)
      {
        double msDiff = AbsDiffInMilliseconds(expected.Value, actual.Value);
        Assert.IsTrue(msDiff <= msTolerance, string.Format("Timedate difference of {0} ticks violates tolerance of {1} milliseconds", msDiff, msTolerance));
      }
    }

    /*
    public static void AssertPLStatusMessage(NH_RPT rptCtx, List<InboundDeviceEventUtilization> events, long assetID)
    {
      if (events == null || events.Count == 0)
        return;

      foreach (InboundDeviceEventUtilization item in events)
      {
        Assert.IsNotNull(item.ExternalCorrelationID);
        Assert.AreEqual((byte)1, item.ExternalSourceID);
        long msgID = long.Parse(item.ExternalCorrelationID);

        AssertPLStatusMessage(rptCtx, msgID, item.ExternalSourceID, item, assetID);
      }
    }

    public static void AssertPLStatusMessage(NH_RPT rptCtx, List<InboundDeviceEventFuel> events, long assetID)
    {
      if (events == null || events.Count == 0)
        return;

      foreach (InboundDeviceEventFuel item in events)
      {
        Assert.IsNotNull(item.ExternalCorrelationID);
        long msgID = long.Parse(item.ExternalCorrelationID);

        AssertPLStatusMessage(rptCtx, msgID, item.ExternalSourceID, item, assetID);
      }
    }

    public static void AssertPLStatusMessage(NH_RPT ctx, PLStatusMessage expectedStatus)
    {
      PLStatusMessage actualStatus = GetPLStatusMessage(expectedStatus.MsgID, expectedStatus.MsgSrcID, ctx);
      Assert.IsNotNull(actualStatus, "PLStatusMessage Is Null.");

      Assert.AreEqual(expectedStatus.IdleConsumptionGallons, actualStatus.IdleConsumptionGallons, "PLStatusMessage.IdleConsumptionGallons doesn't match expected.");
      Assert.AreEqual(expectedStatus.TotalConsumptionGallons, actualStatus.TotalConsumptionGallons, "PLStatusMessage.TotalConsumptionGallons doesn't match expected.");
      Assert.AreEqual(expectedStatus.MaxRPMFuelGallons, actualStatus.MaxRPMFuelGallons, "PLStatusMessage.MaxRPMFuelGallons doesn't match expected.");
      Assert.AreEqual(expectedStatus.PercentRemaining, actualStatus.PercentRemaining, "PLStatusMessage.PercentRemaining doesn't match expected.");

      Assert.AreEqual(expectedStatus.MsgID, actualStatus.MsgID);

      bool latValuesAreDifferent = (actualStatus.Latitude.HasValue ^ expectedStatus.Latitude.HasValue);
      Assert.IsFalse(latValuesAreDifferent, "PLStatusmessageStatus: one of the latitudes doesn't have a value while the other doesn't");

      bool lngValuesAreDifferent = (actualStatus.Longitude.HasValue ^ expectedStatus.Longitude.HasValue);
      Assert.IsFalse(lngValuesAreDifferent, "PLStatusMessageStatus: one of the longitudes has a value while the other doesn't");

      if (expectedStatus.Latitude.HasValue && expectedStatus.Longitude.HasValue)
      {
        Assert.AreEqual(expectedStatus.Latitude.Value, actualStatus.Latitude.Value, .001, "PLStatusMessage.Latitude doesn't match expected.");
        Assert.AreEqual(expectedStatus.Longitude.Value, actualStatus.Longitude.Value, .001, "PLStatusMessage.Longitude doesn't match expected.");
      }
    }
     */
  }
}
