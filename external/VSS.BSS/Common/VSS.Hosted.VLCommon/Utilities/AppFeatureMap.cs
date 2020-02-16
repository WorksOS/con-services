using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using System.Runtime.Caching;

namespace VSS.Hosted.VLCommon
{
  public static class AppFeatureMap
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

    private static NHMemoryCache<AppFeatureSetKey, List<int>> AppFeatureSetAppFeatureCache;

    private static NHMemoryCache<AppFeatureKey, HashSet<AppFeatureSetKey>> AppFeatureAppFeatureSetCache;
    private static NHMemoryCache<int,int> DeviceTypeFeatureSetCache;

    public static void InitCaches()
    {
      if (AppFeatureSetAppFeatureCache != null && DeviceTypeFeatureSetCache != null) return;
      AppFeatureSetAppFeatureCache =
        new NHMemoryCache<AppFeatureSetKey, List<int>>("AppFeatureSetAppFeatureCache");

      AppFeatureAppFeatureSetCache = new NHMemoryCache<AppFeatureKey, HashSet<AppFeatureSetKey>>("AppFeatureAppFeatureSetCache");
      DeviceTypeFeatureSetCache = new NHMemoryCache<int, int>("DeviceTypeFeatureSetCache");
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
        var appFeatureSetAppFeature = (from afs in opCtx.AppFeatureSetAppFeatureReadOnly
                                       select afs).ToList();

        var appFeatureSetGroup =
          appFeatureSetAppFeature.GroupBy(f => new AppFeatureSetKey { FeatureSetId = f.fk_AppFeatureSetID }).
            ToDictionary(t => t.Key, t => t.Select(x => x.fk_AppFeatureID).ToList());

        var appFeatureGroup = new Dictionary<AppFeatureKey, HashSet<AppFeatureSetKey>>();

        foreach (var fsGroup in appFeatureSetGroup)
        {
          foreach (var featureId in fsGroup.Value)
          {
            var featureKey = new AppFeatureKey { FeatureId = featureId };
            if (!appFeatureGroup.ContainsKey(featureKey))
            {
              appFeatureGroup.Add(featureKey, new HashSet<AppFeatureSetKey>() { fsGroup.Key });
            }
            else
            {
              appFeatureGroup[featureKey].Add(fsGroup.Key);
            }
          }
        }

        var deviceType = (from dt in opCtx.DeviceTypeReadOnly select new { DeviceId = dt.ID, FeatureSetId = dt.fk_AppFeatureSetID }).ToList();
        var deviceTypeFeatureSet = new Dictionary<int, int>();
        foreach (var item in deviceType)
        {
          deviceTypeFeatureSet.Add(item.DeviceId, item.FeatureSetId);
        }
                
        AppFeatureSetAppFeatureCache.SetValuesNoCacheExpiry(appFeatureSetGroup);
        AppFeatureAppFeatureSetCache.SetValuesNoCacheExpiry(appFeatureGroup);
        DeviceTypeFeatureSetCache.SetValuesNoCacheExpiry(deviceTypeFeatureSet);

        Log.IfDebugFormat("Loaded {0} items in the AppFeaturesetAppFeatureCache Cache", AppFeatureSetAppFeatureCache.GetCount());
      }
    }

    public static void ResetCache()
    {
      AppFeatureSetAppFeatureCache = null;
      AppFeatureAppFeatureSetCache = null;
    }

    public static bool DoesDeviceTypeSupportFeature(int deviceTypeId, AppFeatureEnum appFeature)
    {
      //Passing AppFeatureSet Id 
      int featureSetId;
      List<int> featureSetList = new List<int>();
      InitCaches();      
      if (!DeviceTypeFeatureSetCache.Contains(deviceTypeId))
      {
        using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
        {
          featureSetList = (from dt in opCtx.DeviceTypeReadOnly
                            where dt.ID == deviceTypeId
                            select dt.fk_AppFeatureSetID).ToList();

          if (featureSetList.Any())
          {
            DeviceTypeFeatureSetCache.Set(deviceTypeId, featureSetList.FirstOrDefault(),ObjectCache.InfiniteAbsoluteExpiration);
          }
          else
          {
            Log.IfInfoFormat("No AppFeatureset for DeviceId {0}", deviceTypeId);
          }
        }
      }
      return DoesFeatureSetSupportsFeature(DeviceTypeFeatureSetCache.Get(deviceTypeId), appFeature);
    }



    public static bool DoesFeatureSetSupportsFeature(int appFeatureSetId, AppFeatureEnum appFeatureId)
    {
      var appFeatureSetList = GetFeatureList(appFeatureSetId);
      Log.IfDebugFormat("DoesFeatureSetSupportsFeature {2} appFeatureSetId {0} appFeatureId {1}", appFeatureSetId, (int)appFeatureId, appFeatureSetList.Contains((int)appFeatureId));
      return appFeatureSetList.Contains((int)appFeatureId);
    }

    public static List<int> GetFeatureSetsThatSupportFeature(AppFeatureEnum appFeatureEnum)
    {
      InitCaches();

      List<int> featureSetsList;
      int appFeatureId = (int)appFeatureEnum;
      AppFeatureKey feature = new AppFeatureKey { FeatureId = appFeatureId };

      if (AppFeatureAppFeatureSetCache.Contains(feature))
      {
        featureSetsList = AppFeatureAppFeatureSetCache.Get(feature).Select(x => x.FeatureSetId).ToList();
      }
      else
      {
        using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
        {
          featureSetsList = (from afs in opCtx.AppFeatureSetAppFeatureReadOnly
                             where afs.fk_AppFeatureID == appFeatureId
                             select afs.fk_AppFeatureSetID).ToList();

          if (featureSetsList.Any())
          {
            var featureSetsHashSet = new HashSet<AppFeatureSetKey>(featureSetsList.Select(x => new AppFeatureSetKey { FeatureSetId = x }));
            AppFeatureAppFeatureSetCache.Set(feature, featureSetsHashSet,ObjectCache.InfiniteAbsoluteExpiration);
          }
          else
          {
            Log.IfInfoFormat("No AppFeaturesetAppFeature for AppFeatureId {0}", appFeatureId);
          }
        }
      }

      return featureSetsList;
    }

    public static List<int> GetFeatureList(int appFeatureSetId)
    {
      InitCaches();
      List<int> features;
      AppFeatureSetKey appFeatureSet = new AppFeatureSetKey { FeatureSetId = appFeatureSetId };

      if (AppFeatureSetAppFeatureCache.Contains(appFeatureSet))
      {
        features = AppFeatureSetAppFeatureCache.Get(appFeatureSet);
      }
      else
      {
        using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
        {
          features = (from afs in opCtx.AppFeatureSetAppFeatureReadOnly
                      where afs.fk_AppFeatureSetID == appFeatureSetId
                      select afs.fk_AppFeatureID).ToList();

          if (features.Count > 0)
          {
            AppFeatureSetAppFeatureCache.Set(appFeatureSet, features, ObjectCache.InfiniteAbsoluteExpiration);
          }
          else
          {
            Log.IfInfoFormat("No AppFeaturesetAppFeature for AppFeatureSetId {0}", appFeatureSetId);
          }
        }
      }
      return features;
    }

    public class AppFeatureSetKey
    {
      public int FeatureSetId;

      public override bool Equals(object obj)
      {
        AppFeatureSetKey featureSetKey = obj as AppFeatureSetKey;
        if (featureSetKey != null)
        {
          return FeatureSetId == featureSetKey.FeatureSetId;
        }
        return false;
      }

      public override int GetHashCode()
      {
        return FeatureSetId.GetHashCode();
      }

      public override string ToString()
      {
        return FeatureSetId.ToString();
      }
    }

    public class AppFeatureKey
    {
      public int FeatureId;

      public override int GetHashCode()
      {
        return FeatureId.GetHashCode();
      }

      public override bool Equals(object obj)
      {
        AppFeatureKey featureKey = obj as AppFeatureKey;
        if (featureKey != null)
        {
          return FeatureId == featureKey.FeatureId;
        }
        return false;
      }

      public override string ToString()
      {
        return FeatureId.ToString();
      }

    }

  }
}
