using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

namespace VSS.Hosted.VLCommon
{
  public static class DeviceTypeList
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

    private static NHMemoryCache<int, DeviceType> DeviceTypeCache;
    private static TimeSpan _cacheItemExpiration = TimeSpan.FromDays(1);

    public static TimeSpan CacheItemExpiration
    {
      get { return _cacheItemExpiration; }
      set { _cacheItemExpiration = value; }
    }

    public static void InitDeviceTypeCache()
    {
      if (DeviceTypeCache != null) return;
      DeviceTypeCache = new NHMemoryCache<int, DeviceType>("DeviceTypeFeatureSetIdCache");

      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
          var deviceTypes = (from dt in opCtx.DeviceTypeReadOnly
                             select dt).ToDictionary(t => t.ID, t => new DeviceType { ID = t.ID, Name = t.Name, fk_AppFeatureSetID = t.fk_AppFeatureSetID });

        DeviceTypeCache.SetValues(deviceTypes, DateTimeOffset.Now.Add(_cacheItemExpiration));

        Log.IfDebugFormat("Loaded {0} items in the DeviceTypeCache Cache", DeviceTypeCache.GetCount());
      }
    }

    public static void ResetCache()
    {
      DeviceTypeCache = null;
    }

    public static int GetAppFeatureSetId(int deviceType)
    {
      var dt = GetDeviceType(deviceType);
      return dt == null ? 0 : dt.fk_AppFeatureSetID;
    }

    public static DeviceType GetDeviceType(int deviceTypeId)
    {
      DeviceType dt = null;
      InitDeviceTypeCache();

      if (DeviceTypeCache.Contains(deviceTypeId))
      {
        dt = DeviceTypeCache.Get(deviceTypeId);
      }
      else
      {
          ResetCache();
          InitDeviceTypeCache();
           if (DeviceTypeCache.Contains(deviceTypeId))
           {
                dt = DeviceTypeCache.Get(deviceTypeId);
           }
           else
           {
              Log.IfInfoFormat("No DeviceType for devicetypeId {0}", deviceTypeId);
           }
        }
      return dt;
    }

    //Getting all the list of Device Types
    public static List<DeviceType> GetDeviceTypes()
    {
        List<DeviceType> deviceTypeList;

        using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
        {
            deviceTypeList = (from dt in opCtx.DeviceTypeReadOnly
                              select dt).ToList<DeviceType>();
        }
        return deviceTypeList;
    }
  }

}
