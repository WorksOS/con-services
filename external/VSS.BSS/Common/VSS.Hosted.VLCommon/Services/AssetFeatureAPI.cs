using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VSS.Hosted.VLCommon.Services.Types;
using log4net;



namespace VSS.Hosted.VLCommon
{
  internal class AssetFeatureAPI : IAssetFeatureAPI
  {
    private static readonly ILog log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

    public Dictionary<long, List<AppFeatureEnum>> GetAssetsThatSupportAppFeatures(IEnumerable<long> assetIDs, IEnumerable<AppFeatureEnum> features, long customerID)
    {
      log.IfDebugFormat("AssetFeatureAPI.GetAssetsThatSupportFeatures: called for {0} assets, {1} features, customerID={2}",
        assetIDs == null ? "all" : assetIDs.Count().ToString(), features.Count(), customerID);

      List<int> featureList = new List<int>();
      foreach (AppFeatureEnum feature in features)
      {
        featureList.Add((int)feature);
      }

      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        int keyDate = DateTime.UtcNow.KeyDate();
        Dictionary<long, List<AppFeatureEnum>> retValue = new Dictionary<long, List<AppFeatureEnum>>();

        // JBP 8/25/2011 - DB Optimization.  Testing has revealed that with less than 100 assets being requested, it is faster to get all of the 
        // results from the DB and discard the one that are unneeded.  For larger datasets, the serialization and deserialization of the 
        // assetIDs to and from SQL strings becomes a severe performance hit (measured times up to 2 sec per query) so we get all of the 
        // results and filter in memory
        if (assetIDs != null && assetIDs.Count() < 100)
        {
          var assetFeatures = (from ca in ctx.CustomerAssetReadOnly.Where(f => f.fk_CustomerID == customerID && assetIDs.Contains(f.fk_AssetID))
                               join a in ctx.AssetReadOnly on ca.fk_AssetID equals a.AssetID
                               join d in ctx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                               join dt in ctx.DeviceTypeReadOnly on d.fk_DeviceTypeID equals dt.ID
                               join af in ctx.AppFeatureSetAppFeatureReadOnly on dt.fk_AppFeatureSetID equals af.fk_AppFeatureSetID
                               join sv in ctx.ServiceViewReadOnly.Where(sv => sv.fk_CustomerID == customerID
                                 && sv.StartKeyDate <= keyDate && sv.EndKeyDate >= keyDate)
                                 on a.AssetID equals sv.fk_AssetID
                               join s in ctx.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                               join sf in ctx.ServiceTypeAppFeatureReadOnly on s.fk_ServiceTypeID equals sf.fk_ServiceTypeID
                               from f in ctx.AppFeatureReadOnly
                               where af.fk_AppFeatureID == sf.fk_AppFeatureID
                               && f.ID == sf.fk_AppFeatureID
                                 && ((af.IsOwnershipRequired == false && f.IsOwnershipRequired == false) || ca.IsOwned)
                                 && featureList.Contains(f.ID)
                               select new { a.AssetID, FeatureID = f.ID }).ToList();



          foreach (var item in assetFeatures)
          {
            if (!retValue.ContainsKey(item.AssetID))
            {
              retValue.Add(item.AssetID, new List<AppFeatureEnum>());
            }
            // Removing duplicate entries here means that we don't have to use the Distinct on the DB call which speeds it up substantially.
            if (!retValue[item.AssetID].Contains((AppFeatureEnum)item.FeatureID))
            {
              retValue[item.AssetID].Add((AppFeatureEnum)item.FeatureID);
            }
          }
        }
        else
        {
          // Get information for all assets from DB and then post-filter.
          var assetFeatures = (from ca in ctx.CustomerAssetReadOnly.Where(f => f.fk_CustomerID == customerID)
                               join a in ctx.AssetReadOnly on ca.fk_AssetID equals a.AssetID
                               join d in ctx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                               join dt in ctx.DeviceTypeReadOnly on d.fk_DeviceTypeID equals dt.ID
                               join af in ctx.AppFeatureSetAppFeatureReadOnly on dt.fk_AppFeatureSetID equals af.fk_AppFeatureSetID
                               join sv in ctx.ServiceViewReadOnly.Where(sv => sv.fk_CustomerID == customerID
                                 && sv.StartKeyDate <= keyDate && sv.EndKeyDate >= keyDate)
                                 on a.AssetID equals sv.fk_AssetID
                               join s in ctx.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                               join sf in ctx.ServiceTypeAppFeatureReadOnly on s.fk_ServiceTypeID equals sf.fk_ServiceTypeID
                               from f in ctx.AppFeatureReadOnly
                               where af.fk_AppFeatureID == sf.fk_AppFeatureID
                                  && f.ID == sf.fk_AppFeatureID
                                 && ((af.IsOwnershipRequired == false && f.IsOwnershipRequired == false) || ca.IsOwned)
                                 && featureList.Contains(f.ID)
                               select new { a.AssetID, FeatureID = f.ID }).ToList();


          Dictionary<long, bool> assetIDDict = new Dictionary<long, bool>();
          if (assetIDs != null && assetFeatures.Count > 0)
          {
            assetIDDict = assetIDs.Distinct().ToDictionary(key => key, value => true);
            assetFeatures.RemoveAll(x => !assetIDDict.ContainsKey(x.AssetID));
          }

          foreach (var item in assetFeatures)
          {
            if (!retValue.ContainsKey(item.AssetID))
            {
              retValue.Add(item.AssetID, new List<AppFeatureEnum>());
            }
            // Removing duplicate entries here means that we don't have to use the Distinct on the DB call which speeds it up substantially.
            if (!retValue[item.AssetID].Contains((AppFeatureEnum)item.FeatureID))
            {
              retValue[item.AssetID].Add((AppFeatureEnum)item.FeatureID);
            }
          }
        }


        log.IfDebugFormat("AssetFeatureAPI.GetAssetsThatSupportFeatures: returning {0} assets.", retValue.Keys.Count);
        return retValue;
      }
    }

    #region VLSupport

    public List<AssetInfo> GetAssetSearchResults(string searchTerm)
    {
      searchTerm = searchTerm.ToUpper();
      List<AssetInfo> searchResult = new List<AssetInfo>();
      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        searchResult = (from a in ctx.AssetReadOnly
                        join d in ctx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                        where ((a.SerialNumberVIN == searchTerm)
                                   || (a.Name == searchTerm)
                                   || (d.GpsDeviceID == searchTerm)
                                   || (a.SerialNumberVIN.Contains(searchTerm)
                                   || a.Name.Contains(searchTerm)
                                   || d.GpsDeviceID.Contains(searchTerm)))
                        select new AssetInfo
                        {
                          assetID = a.AssetID,
                          assetName = a.Name,
                          assetSerialNumber = a.SerialNumberVIN,
                          gpsDeviceID = d.GpsDeviceID
                        }).ToList();
      }
      return searchResult;
    }

    public List<int> GetActiveServicePlans(long CustomerID, long assetID, int startDate, int endDate)
    {
      List<int> activeServicePlans;

      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        activeServicePlans = (from sv in ctx.ServiceViewReadOnly
                              join s in ctx.ServiceReadOnly on sv.fk_ServiceID equals s.ID
                              where sv.fk_CustomerID == CustomerID
                                    && sv.fk_AssetID == assetID
                                    && sv.StartKeyDate <= endDate
                                    && sv.EndKeyDate >= startDate
                              select s.fk_ServiceTypeID).Distinct().ToList();

      }
      return activeServicePlans;
    }

    public AssetAlias GetAssetIDChanges(long assetID)
    {
      AssetAlias assetIDchanges;

      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        assetIDchanges = ctx.AssetAliasReadOnly.Where(t => t.fk_AssetID == assetID).OrderByDescending(t => t.InsertUTC).FirstOrDefault();

      }

      return assetIDchanges;
    }

    public bool DoesAssetSupportFeature(long assetId, AppFeatureEnum feature)
    {
      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        int deviceTypeId = (from a in ctx.AssetReadOnly
                            join d in ctx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                            where a.AssetID == assetId
                            select d.fk_DeviceTypeID).FirstOrDefault();

        return AppFeatureMap.DoesFeatureSetSupportsFeature(DeviceTypeList.GetAppFeatureSetId(deviceTypeId), feature);
      }
    }

    public List<DevicePersonality> GetDevicePersonality(string gpsDeviceID)
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {

        return (from d in opCtx.DeviceReadOnly
                join dp in opCtx.DevicePersonalityReadOnly on d.ID equals dp.fk_DeviceID
                where d.GpsDeviceID == gpsDeviceID
                select dp).ToList();
      }
    }

    private string EcmNameStr(List<string> names)
    {
      int nameCount = names.Count;

      if (nameCount == 0)
        return null;

      string retval = names[0];

      if (nameCount > 1)
      {
        for (int i = 1; i < nameCount; i++)
          retval += string.Format("\n{0}", names[i]);
      }
      return retval;
    }

    #endregion
  }
}
