using System;
using System.Collections.Generic;
using VSS.UnitTest.Common.Contexts;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
  public class PMIntervalAssetBuilder 
  {
    #region PMIntervalAsset Fields

    private Asset _asset;
    private PMInterval _defaultInterval;
    private PMInterval _customInterval;

    #endregion

    public PMIntervalAssetBuilder Asset(Asset asset)
    {
      _asset = asset;
      return this;
    }
    public PMIntervalAssetBuilder DefaultPMInterval(PMInterval pmInterval)
    {
      _defaultInterval = pmInterval;
      return this;
    }
    public PMIntervalAssetBuilder CustomPMInterval(PMInterval pmInterval)
    {
      _customInterval = pmInterval;
      return this;
    }
    
    public PMIntervalAsset Build()
    {
      PMIntervalAsset intervalAsset =  new PMIntervalAsset();

      intervalAsset.fk_AssetID = _asset.AssetID;
      intervalAsset.fk_DefaultPMIntervalID = _defaultInterval != null ? _defaultInterval.ID : (long?)null;
      intervalAsset.fk_PMIntervalID = _customInterval.ID;

      return intervalAsset;
    }
    public PMIntervalAsset Save()
    {
      PMIntervalAsset intervalAsset = Build();

      ContextContainer.Current.OpContext.PMIntervalAsset.AddObject(intervalAsset);
      ContextContainer.Current.OpContext.SaveChanges();

      return intervalAsset;
    }
  }
}
