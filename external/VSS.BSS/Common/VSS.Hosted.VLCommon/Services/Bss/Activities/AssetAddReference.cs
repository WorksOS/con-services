using System;
using System.Text;
using VSS.Hosted.VLCommon.Services.Interfaces;

namespace VSS.Hosted.VLCommon.Bss
{
  public class AssetAddReference : Activity
  {
    private const long StoreId = (long) StoreEnum.CAT;
    public const string SuccessMessage = @"Created AssetReference with StoreID: {0} Alias: {1} Value: {2} UID: {3}.";
    public const string SuccessNotCreatingMessage = @"Not creating AssetReference for StoreID: {0} Alias: {1} Value: {2} UID: {3} - IBKey starts with '-'";
    public const string FailureMessage = @"Failed to create AssetReference with StoreID: {0} Alias: {1} Value: {2} UID: {3}.  Message: {4}";

    public override ActivityResult Execute(Inputs inputs)
    {
      var context = inputs.Get<AssetDeviceContext>();
      var addBssReference = inputs.Get<IBssReference>();

      try
      {
        if ((!String.IsNullOrEmpty(context.Device.IbKey)) && (context.Device.IbKey.StartsWith("-")))
        {
          return Success(SuccessNotCreatingMessage, StoreId, GetAlias(), GetValue(context.Asset),
            context.Asset.AssetUID.HasValue ? context.Asset.AssetUID.Value : Guid.Empty);
        }

        Services.Assets()
          .AddAssetReference(addBssReference, StoreId, GetAlias(), GetValue(context.Asset),
            context.Asset.AssetUID.HasValue ? context.Asset.AssetUID.Value : Guid.Empty);
      }
      catch (Exception ex)
      {
        return Notify(ex, FailureMessage, StoreId, GetAlias(), GetValue(context.Asset),
          context.Asset.AssetUID.HasValue ? context.Asset.AssetUID.Value : Guid.Empty, ex.Message);
      }

      return Success(SuccessMessage, StoreId, GetAlias(), GetValue(context.Asset),
            context.Asset.AssetUID.HasValue ? context.Asset.AssetUID.Value : Guid.Empty);
    }

    private string GetAlias()
    {
      return "MakeCode_SN";
    }

    private string GetValue(AssetDto asset)
    {
      return new StringBuilder().Append(asset.MakeCode).Append("_").Append(asset.SerialNumber).ToString();
    }
  }
}