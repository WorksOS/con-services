using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class AssetDeviceHistoryCreate : Activity
  {
    public const string ASSET_DEVICE_HISTORY_NULL_MESSAGE = @"Creation of AssetDeviceHistory came back null for unknown reason.";
    public const string FAILURE_MESSAGE = @"Failed to create AssetDeviceHistory for IBKey: {0} and SerialNumber {1}.";
    public const string SUCCESS_MESSAGE = @"Created AssetDeviceHistory for Device ID: {0}, Asset ID: {1}, OwnerBSSID : {2}, Start UTC: {3}, and End UTC: {4}.";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<AssetDeviceContext>();

      Require.IsNotNull(context.IBAsset, "AssetDeviceContext.IBAsset");
      Require.IsNotNull(context.IBDevice, "AssetDeviceContext.IBDevice");

      IList<AssetDeviceHistory> history = new List<AssetDeviceHistory>();

      // Warning - this is complicated mayhem.
      // Proceed with caution...

      try
      {

        var assetDeviceHistory = Services.AssetDeviceHistory().CreateAssetDeviceHistory(context.Asset.AssetId,
                                                                      context.Asset.DeviceId,
                                                                      context.Asset.Device.OwnerBssId,
                                                                      context.Asset.InsertUtc.Value);

        if (assetDeviceHistory == null)
          return Error(ASSET_DEVICE_HISTORY_NULL_MESSAGE);

        //// Device Replacement
        //// -	 Create a History record using 
        ////  o	IB Asset’s AssetID, 
        ////  o	IB Asset’s installed Device ID, 
        ////  o	IB Asset’s installed Device’s OwnerBSSID, 
        ////  o	IB Asset’s InsertUTC as StartUTC

        //if (context.IsDeviceReplacement())
        //{
        //  var assetDeviceHistory = Services.Devices().CreateAssetDeviceHistory(
        //    context.Asset.AssetId, 
        //    context.Asset.DeviceId, 
        //    context.Asset.DeviceOwner.BssId, 
        //    context.Asset.InsertUtc.Value);

        //  if (assetDeviceHistory == null)
        //    return Error(ASSET_DEVICE_HISTORY_NULL_MESSAGE);

        //  history.Add(assetDeviceHistory);
        //}

        //// Device Transfer
        //// - Create a History record using 
        ////  o	IB Asset’s AssetID, 
        ////  o	IB Asset’s installed Device ID, 
        ////  o	IB Asset’s installed Device’s OwnerBSSID, 
        ////  o	IB Asset’s InsertUTC as StartUTC

        //else if(context.IsDeviceTransfer())
        //{
        //  var assetDeviceHistoryOne = Services.Devices().CreateAssetDeviceHistory(
        //    context.Asset.AssetId,
        //    context.Asset.DeviceId,
        //    context.Asset.DeviceOwner.BssId,
        //    context.Asset.InsertUtc.Value);

        //  if (assetDeviceHistoryOne == null)
        //    return Error(ASSET_DEVICE_HISTORY_NULL_MESSAGE);

        //  history.Add(assetDeviceHistoryOne);

        //  // If the IB Device is being transferred off of an 
        //  // Asset other than the IB Asset
        //  // - Create a History record using 
        //  //  o	IB Device’s installed on Asset's AssetID 
        //  //  o	IB Device's ID, 
        //  //  o	IB Device's OwnerBSSID
        //  //  o	IB Device's installed on Asset’s InsertUTC as StartUTC

        //  if(context.Device.AssetExists && context.Asset.Exists && context.Device.AssetId != context.Asset.AssetId)
        //  {
        //    var assetDeviceHistoryTwo = Services.Devices().CreateAssetDeviceHistory(
        //      context.Device.AssetId,
        //      context.Device.Id,
        //      context.Device.OwnerBssId,
        //      context.Device.Asset.InsertUtc.Value);

        //    if (assetDeviceHistoryTwo == null)
        //      return Error(ASSET_DEVICE_HISTORY_NULL_MESSAGE);

        //    history.Add(assetDeviceHistoryTwo);
        //  }
        //}

        //// Ownership Transfer
        //// - Create a History record using 
        ////  o	IB Asset's AssetID
        ////  o	IB Device's ID
        ////  o	IB Device's OwnerBSSID
        ////  o	IB Device's installed on Asset’s InsertUTC as StartUTC

        //else if(context.IsOwnershipTransfer())
        //{
        //  var assetDeviceHistory = Services.Devices().CreateAssetDeviceHistory(
        //    context.Asset.AssetId,
        //    context.Device.Id,
        //    context.Device.OwnerBssId,
        //    context.Device.Asset.InsertUtc.Value);

        //  if (assetDeviceHistory == null)
        //    return Error(ASSET_DEVICE_HISTORY_NULL_MESSAGE);

        //  history.Add(assetDeviceHistory);
        //}
      }
      catch (Exception ex)
      {
        return Exception(ex, FAILURE_MESSAGE, context.IBDevice.IbKey, context.IBAsset.SerialNumber);
      }

      if (history.Count == 0)
        return Warning("No AssetDeviceHistory created, the activity was executed in error. Please check the workflow logic.");

      foreach (var assetDeviceHistory in history)
      {
        AddSummary(SUCCESS_MESSAGE, assetDeviceHistory.fk_DeviceID,
          assetDeviceHistory.fk_AssetID, assetDeviceHistory.OwnerBSSID,
          assetDeviceHistory.StartUTC, assetDeviceHistory.EndUTC);
      }
      return Success();
    }
  }
}
