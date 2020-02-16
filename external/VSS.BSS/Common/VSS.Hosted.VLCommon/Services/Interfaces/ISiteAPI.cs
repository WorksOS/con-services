using System.Collections.Generic;
using ED=VSS.Nighthawk.ExternalDataTypes.Enumerations;
namespace VSS.Hosted.VLCommon
{
    public interface ISiteAPI
    {
      event SiteDispatchedEvent SiteDispatched;
      event SiteRemovedEvent SiteRemoved;

      bool Delete(INH_OP ctx, long siteID);
      bool DeleteWithTimeStamp(INH_OP ctx, long siteID);
      bool Undelete(INH_OP ctx, long customerID, long siteID);
      long Update(INH_OP ctx, long customerID, long siteID, List<Param> modifiedProperties,long userId);
      bool Assign(INH_OP opCtx, bool assign, Site site, long assetID, DeviceTypeEnum deviceType);
      bool Purge(INH_OP opCtx, string gpsDeviceID, DeviceTypeEnum deviceType);
    
      string GetCustomerSiteNameForLocation(long customerID, double latitude, double longitude);
     
      bool IsPointInAmericas(double lat, double lon);
      List<Site> GetAllSitesForCustomer(long customerID, bool includeDeleted = false);
      Dictionary<long, string> GetSitesNamesForCustomer(long customerID, List<long> siteIDs);
      
      List<ItemNamePair> GetSiteNamesForCustomer(long customerID, int pageNumber, int numberToTake);
      int GetSiteNamesForCustomerCount(long customerID, int numberToTake);
      
      string FindSiteForLatLon(List<Site> sitesForUser, double latitude, double longitude);
      string GetFavoriteSiteNameForLocation(List<Site> userFavSites, double latitude, double longitude);


      void GetSitePolygon(Site site, List<Point> points);
      void CheckValidSite(INH_OP ctx, long customerID, string name);
    }
}

