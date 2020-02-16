using System;
using System.Linq;
using VSS.UnitTest.Common.Contexts;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
  public class AssetBurnRatesBuilder 
  {
    #region AssetBurnRates Fields

    private long _assetId;
    private Asset _asset;
    private double? _estimatedIdleBurnRateGallonsPerHour = null;
    private double? _estimatedWorkingBurnRateGallonsPerHour = null;
    private DateTime _updateUtc = DateTime.UtcNow;

    #endregion
    public virtual AssetBurnRatesBuilder EstimatedIdleBurnRateGallonsPerHour(double? estimatedIdleBurnRateGallonsPerHour)
    {
      _estimatedIdleBurnRateGallonsPerHour = estimatedIdleBurnRateGallonsPerHour;
      return this;
    }
    public virtual AssetBurnRatesBuilder EstimatedWorkingBurnRateGallonsPerHour(double? estimatedWorkingBurnRateGallonsPerHour)
    {
      _estimatedWorkingBurnRateGallonsPerHour = estimatedWorkingBurnRateGallonsPerHour;
      return this;
    }
    public virtual AssetBurnRatesBuilder UpdateUtc(DateTime updateUtc)
    {
      _updateUtc = updateUtc;
      return this;
    }
    public virtual AssetBurnRatesBuilder ForAsset(long assetId)
    {
      _assetId = assetId;
      return this;
    }
    public virtual AssetBurnRatesBuilder ForAsset(Asset asset)
    {
      _asset = asset;
      _assetId = asset.AssetID;
      return this;
    }
    public virtual AssetBurnRatesBuilder SyncWithRpt()
    {
      return this;
    }

    private AssetBurnRates Build() 
    {
      var assetBurnRates = new AssetBurnRates();

      assetBurnRates.EstimatedIdleBurnRateGallonsPerHour = _estimatedIdleBurnRateGallonsPerHour;
      assetBurnRates.EstimatedWorkingBurnRateGallonsPerHour = _estimatedWorkingBurnRateGallonsPerHour;
      assetBurnRates.UpdateUTC = _updateUtc;

      if (_asset != null)
        assetBurnRates.fk_AssetID = _asset.AssetID;
      else
        assetBurnRates.fk_AssetID = _assetId;

      return assetBurnRates;
    }

    public virtual AssetBurnRates Save() 
    {
      AssetBurnRates assetBurnRatesBuilt = Build();
      AssetBurnRates assetBurnRates = (from aehh in ContextContainer.Current.OpContext.AssetBurnRates
                                                                             where aehh.fk_AssetID == _assetId
                                                                             select aehh).FirstOrDefault();

      if (assetBurnRates != null)
      {
        assetBurnRates.EstimatedIdleBurnRateGallonsPerHour = assetBurnRatesBuilt.EstimatedIdleBurnRateGallonsPerHour;
        assetBurnRates.EstimatedWorkingBurnRateGallonsPerHour = assetBurnRatesBuilt.EstimatedWorkingBurnRateGallonsPerHour;
        assetBurnRates.UpdateUTC = assetBurnRatesBuilt.UpdateUTC;
      }
      else
      {
        assetBurnRates = assetBurnRatesBuilt;
        ContextContainer.Current.OpContext.AssetBurnRates.AddObject(assetBurnRates);
      }
      ContextContainer.Current.OpContext.SaveChanges();
      return assetBurnRates;
    }
  }
}
