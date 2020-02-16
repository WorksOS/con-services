using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;

// ReSharper disable CheckNamespace
namespace VSS.Hosted.VLCommon
// ReSharper restore CheckNamespace
{
  public static class AssetIDCache
  {
    // Returns non-null AssetID from NH_OP.Asset, if found.
    // The AssetID is generated from a hash algorithm, so we know the AssetID, but we still need to check that
    // there is an asset in NH_OP with that AssetID

    public static long? GetAssetID(string gpsDeviceID, DeviceTypeEnum deviceType, long assetID = 0)
    {
      try
      {
        if (string.IsNullOrEmpty(gpsDeviceID) && assetID == 0)
          return null;

        var eyeDees = GetCacheItem(gpsDeviceID, deviceType, assetID);

        if (eyeDees == null || (assetID != 0 && assetID != eyeDees.AssetID))
          return null;

        return eyeDees.AssetID;
      }
      catch (Exception e)
      {
        Log.IfError("Unexpected Error Reading Asset ID from Cache", e);
        return null;
      }
    }

    public static AssetKeys GetCacheItem(string gpsDeviceID, DeviceTypeEnum deviceType, long assetID)
    {
      AssetKeys eyeDees = null;

      var ak = new AssetKeys { AssetID = assetID, GpsDeviceID = gpsDeviceID, DeviceTypeID = (int)deviceType };

      var found = false;
      if (assetID != 0)
      {
        found = _assetcache.TryGetValue(ak.AssetID, out eyeDees); //assetcache = "PL Device Cache", so if Device is found aka: has an AssetID, the Device is assumed to be a "PL Device"
        if (found && deviceType == DeviceTypeEnum.PL121
           && (eyeDees.DeviceTypeID != (int)DeviceTypeEnum.PL121
           && eyeDees.DeviceTypeID != (int)DeviceTypeEnum.PL321))// An assetID is present but if not 121/321, it must be a MTSDevice ex: PL522
          found = false;
      }
      else
      {
        if (gpsDeviceID == null) //probably not an MTS Device (corner case)
        {
          Log.IfInfoFormat("AssetIDCache.GetCacheItem: GPSDeviceID is null. AssetID={0}, deviceType={1}", assetID, deviceType);
        }
        else
        {
          //cacheByKey = "MTS Device Cache"
          found = _cacheByKey.TryGetValue(ak.Key, out eyeDees);

          if (!found && deviceType == DeviceTypeEnum.PL121)
          {
            ak.DeviceTypeID = (int)DeviceTypeEnum.PL321;
            found = _cacheByKey.TryGetValue(ak.Key, out eyeDees);
          }
        }
      }

	    if (!found)
	    {
				eyeDees = CheckDbAndAddToCache(assetID, gpsDeviceID, deviceType);
	    }

	    return eyeDees;
    }

		private static AssetKeys CheckDbAndAddToCache(long assetId, string gpsDeviceId, DeviceTypeEnum deviceType)
	  {
			AssetKeys deviceAsset = null;
			using (var ctx = ObjectContextFactory.NewNHContext<INH_OP>())
			{
				if (assetId != 0)
					deviceAsset = (from a in ctx.AssetReadOnly
												 join d in ctx.DeviceReadOnly on a.fk_DeviceID equals d.ID
												 where d.fk_DeviceStateID == (int)DeviceStateEnum.Subscribed
												 && a.AssetID == assetId
												 select new AssetKeys
												 {
													 DeviceTypeID = d.fk_DeviceTypeID,
													 GpsDeviceID = d.GpsDeviceID,
													 AssetID = a.AssetID,
													 Make = a.fk_MakeCode
												 }).FirstOrDefault();
				else if (gpsDeviceId != null && deviceType != DeviceTypeEnum.MANUALDEVICE)
				{
					deviceAsset = (from a in ctx.AssetReadOnly
												 join d in ctx.DeviceReadOnly on a.fk_DeviceID equals d.ID
												 where d.fk_DeviceStateID == (int)DeviceStateEnum.Subscribed
															 && d.GpsDeviceID == gpsDeviceId && d.fk_DeviceTypeID == (int)deviceType
												 select new AssetKeys
												 {
													 DeviceTypeID = d.fk_DeviceTypeID,
													 GpsDeviceID = d.GpsDeviceID,
													 AssetID = a.AssetID,
													 Make = a.fk_MakeCode
												 }).FirstOrDefault();
				}
			}
			if (deviceAsset != null)
			{
				lock (_cacheByKey)
				{
					_cacheByKey.TryAdd(deviceAsset.Key, deviceAsset);
				}

				lock (_assetcache)
				{
					_assetcache.TryAdd(deviceAsset.AssetID, deviceAsset);
				}
			}

			return deviceAsset;
	  }


    public static string GetMakeCode(string gpsDeviceID, DeviceTypeEnum deviceType, long assetID)
    {
      try
      {
        if (assetID == 0)
          return null;

        var asset = GetCacheItem(gpsDeviceID, deviceType, assetID);

        if (asset == null)
          return null;

        return asset.Make;
      }
      catch (Exception e)
      {
        Log.IfError("Unexpected Error Reading Device Type ID from Cache", e);
        return null;
      }
    }

    public static long? GetDeviceTypeID(string gpsDeviceID, DeviceTypeEnum deviceType, long assetID)
    {
      try
      {
        if (assetID == 0)
          return null;

        var asset = GetCacheItem(gpsDeviceID, deviceType, assetID);

        if (asset == null)
          return null;

        return asset.DeviceTypeID;
      }
      catch (Exception e)
      {
        Log.IfError("Unexpected Error Reading Device Type ID from Cache", e);
        return null;
      }
    }

    public static void Init(bool reset = false)
    {
      if (_cacheUpdateTimer == null || reset)
      {
        _cacheByKey = null;
        SetCache();
        _cacheUpdateTimer = new Timer(RefreshCache);
        _cacheUpdateTimer.Change(ConfigurationManager.AppSettings["CacheUpdateThreshold"] != null ? TimeSpan.Parse(ConfigurationManager.AppSettings["CacheUpdateThreshold"]) : DefaultCacheUpdateThreshold, TimeSpan.FromMilliseconds(-1));
      }
    }

    #region Implementation

    private static void SetCache()
    {
      try
      {
        if (_cacheByKey == null)
          _cacheByKey = new ConcurrentDictionary<string, AssetKeys>();
        if (_assetcache == null)
          _assetcache = new ConcurrentDictionary<long, AssetKeys>();
        List<AssetKeys> deviceAssets;
        using (var ctx = ObjectContextFactory.NewNHContext<INH_OP>())
        {
          deviceAssets = (from a in ctx.AssetReadOnly
                          join d in ctx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                          where d.fk_DeviceStateID == (int)DeviceStateEnum.Subscribed
                          select new AssetKeys
                                   {
                                     DeviceTypeID = d.fk_DeviceTypeID,
                                     GpsDeviceID = d.GpsDeviceID,
                                     AssetID = a.AssetID,
                                     Make = a.fk_MakeCode
                                   }).ToList();
        }
        
        var keyCache = new ConcurrentDictionary<string, AssetKeys>(deviceAssets.Where(e => !string.IsNullOrWhiteSpace(e.GpsDeviceID) && e.DeviceTypeID !=(int)DeviceTypeEnum.MANUALDEVICE).ToDictionary(f => f.Key, f => f));
        lock (_cacheByKey)
        {
          _cacheByKey = keyCache;
        }

        var aCache = new ConcurrentDictionary<long, AssetKeys>(deviceAssets.ToDictionary(f => f.AssetID, f => f));
        lock (_assetcache)
        {
          _assetcache = aCache;
        }
      }
      catch (Exception e)
      {
        Log.IfError("Could not preLoad Asset ID Cache", e);
      }
    }


    private static void RefreshCache(object sender)
    {
      try
      {
        SetCache();
        GC.Collect();
      }
      catch (Exception e)
      {
        Log.IfError("Could not Refresh Asset ID Cache", e);
      }
      finally
      {
        _cacheUpdateTimer.Change(ConfigurationManager.AppSettings["CacheUpdateThreshold"] != null ? TimeSpan.Parse(ConfigurationManager.AppSettings["CacheUpdateThreshold"]) : DefaultCacheUpdateThreshold, TimeSpan.FromMilliseconds(-1));
      }
    }

    public class AssetKeys
    {
      public int DeviceTypeID { get; set; }
      public string GpsDeviceID { get; set; }
      public long AssetID { get; set; }
      public string Make { get; set; }

      public string Key
      {
        get
        {
          if (GpsDeviceID == null)
          {
            Log.IfInfoFormat("AssetKeys.Key: GPSDeviceID is null. AssetID={0}, Make={1}, DeviceTypeID={2}", AssetID, Make, DeviceTypeID);
            throw new InvalidOperationException("GPS Device ID cannot be null");
          }
          return string.Format("{0}_{1}", GpsDeviceID.Trim(), DeviceTypeID);
        }
      }
    }
    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly TimeSpan DefaultCacheUpdateThreshold = TimeSpan.FromMinutes(3);

    private static ConcurrentDictionary<string, AssetKeys> _cacheByKey;
    private static ConcurrentDictionary<long, AssetKeys> _assetcache;
    private static Timer _cacheUpdateTimer;
    #endregion
  }
}
