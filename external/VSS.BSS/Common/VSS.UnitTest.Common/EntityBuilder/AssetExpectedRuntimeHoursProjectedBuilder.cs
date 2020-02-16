using System;
using System.Linq;
using VSS.UnitTest.Common.Contexts;
using VSS.Hosted.VLCommon;


namespace VSS.UnitTest.Common.EntityBuilder
{
  public class AssetExpectedRuntimeHoursProjectedBuilder 
  {
    #region AssetExpectedRuntimeProjected Fields

    private long _assetId;
    private Asset _asset;
    private double _hoursSun = 0;
    private double _hoursMon = 8;
    private double _hoursTue = 8;
    private double _hoursWed = 8;
    private double _hoursThu = 8;
    private double _hoursFri = 8;
    private double _hoursSat = 0;
    private DateTime _updateUtc = DateTime.UtcNow;

    #endregion

    public virtual AssetExpectedRuntimeHoursProjectedBuilder HoursSun(double hoursSun)
    {
      _hoursSun = hoursSun;
      return this;
    }
    public virtual AssetExpectedRuntimeHoursProjectedBuilder HoursMon(double hoursMon)
    {
      _hoursMon = hoursMon;
      return this;
    }
    public virtual AssetExpectedRuntimeHoursProjectedBuilder HoursTue(double hoursTue)
    {
      _hoursTue = hoursTue;
      return this;
    }
    public virtual AssetExpectedRuntimeHoursProjectedBuilder HoursWed(double hoursWed)
    {
      _hoursWed = hoursWed;
      return this;
    }
    public virtual AssetExpectedRuntimeHoursProjectedBuilder HoursThu(double hoursThu)
    {
      _hoursThu = hoursThu;
      return this;
    }
    public virtual AssetExpectedRuntimeHoursProjectedBuilder HoursFri(double hoursFri)
    {
      _hoursFri = hoursFri;
      return this;
    }
    public virtual AssetExpectedRuntimeHoursProjectedBuilder HoursSat(double hoursSat)
    {
      _hoursSat = hoursSat;
      return this;
    }
    public virtual AssetExpectedRuntimeHoursProjectedBuilder UpdateUtc(DateTime updateUtc)
    {
      _updateUtc = updateUtc;
      return this;
    }
    public virtual AssetExpectedRuntimeHoursProjectedBuilder ForAsset(long assetId)
    {
      _assetId = assetId;
      return this;
    }
    public virtual AssetExpectedRuntimeHoursProjectedBuilder ForAsset(Asset asset)
    {
      _asset = asset;
      _assetId = asset.AssetID;
      return this;
    }

    private AssetExpectedRuntimeHoursProjected Build() 
    {
      var assetExpectedRuntimeProjected = new AssetExpectedRuntimeHoursProjected();

      assetExpectedRuntimeProjected.HoursSun = _hoursSun;
      assetExpectedRuntimeProjected.HoursMon = _hoursMon;
      assetExpectedRuntimeProjected.HoursTue = _hoursTue;
      assetExpectedRuntimeProjected.HoursWed = _hoursWed;
      assetExpectedRuntimeProjected.HoursThu = _hoursThu;
      assetExpectedRuntimeProjected.HoursFri = _hoursFri;
      assetExpectedRuntimeProjected.HoursSat = _hoursSat;
      assetExpectedRuntimeProjected.UpdateUTC = _updateUtc;

      if (_asset != null)
        assetExpectedRuntimeProjected.fk_AssetID = _asset.AssetID;
      else
        assetExpectedRuntimeProjected.fk_AssetID = _assetId;

      return assetExpectedRuntimeProjected;
    }
    
    public virtual AssetExpectedRuntimeHoursProjected Save() 
    {
      AssetExpectedRuntimeHoursProjected assetExpectedRuntimeHoursProjectedBuilt = Build();
      AssetExpectedRuntimeHoursProjected assetExpectedRuntimeHoursProjected = (from aehh in ContextContainer.Current.OpContext.AssetExpectedRuntimeHoursProjected
                                                                             where aehh.fk_AssetID == _assetId
                                                                             select aehh).FirstOrDefault();

      if (assetExpectedRuntimeHoursProjected != null)
      {
        assetExpectedRuntimeHoursProjected.HoursSun = assetExpectedRuntimeHoursProjectedBuilt.HoursSun;
        assetExpectedRuntimeHoursProjected.HoursMon = assetExpectedRuntimeHoursProjectedBuilt.HoursMon;
        assetExpectedRuntimeHoursProjected.HoursTue = assetExpectedRuntimeHoursProjectedBuilt.HoursTue;
        assetExpectedRuntimeHoursProjected.HoursWed = assetExpectedRuntimeHoursProjectedBuilt.HoursWed;
        assetExpectedRuntimeHoursProjected.HoursThu = assetExpectedRuntimeHoursProjectedBuilt.HoursThu;
        assetExpectedRuntimeHoursProjected.HoursFri = assetExpectedRuntimeHoursProjectedBuilt.HoursFri;
        assetExpectedRuntimeHoursProjected.HoursSat = assetExpectedRuntimeHoursProjectedBuilt.HoursSat;
        assetExpectedRuntimeHoursProjected.UpdateUTC = assetExpectedRuntimeHoursProjectedBuilt.UpdateUTC;
      }
      else
      {
        assetExpectedRuntimeHoursProjected = assetExpectedRuntimeHoursProjectedBuilt;
        ContextContainer.Current.OpContext.AssetExpectedRuntimeHoursProjected.AddObject(assetExpectedRuntimeHoursProjected);
      }
      ContextContainer.Current.OpContext.SaveChanges();

      return assetExpectedRuntimeHoursProjected;
    }
  }
}
