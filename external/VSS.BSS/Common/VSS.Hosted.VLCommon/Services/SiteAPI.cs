using System;
using System.Data;
using System.Linq;
using System.Data.Entity.Core.Objects;
using System.Collections.Generic;
using ThreeDAPIs.ProjectMasterData;
using VSS.Hosted.VLCommon.DataAccess.Views;
using VSS.Hosted.VLCommon.Events;
using VSS.Hosted.VLCommon.Helpers;
using VSS.Hosted.VLCommon.Services.MDM.Models;
using VSS.Hosted.VLCommon.Utilities;
using Newtonsoft.Json;
using System.Configuration;
using log4net;
using System.Text;
using ED = VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Hosted.VLCommon.Services.MDM;



namespace VSS.Hosted.VLCommon
{
  public delegate void SiteDispatchedEvent(object sender, SiteDispatchedEventArgs e);
  public delegate void SiteRemovedEvent(object sender, SiteRemovedEventArgs e);

  internal class SiteAPI : ISiteAPI
  {
    private static readonly Polygon americasPolygon = new Polygon("<Polygon><Point><x>-101.601562</x><y>74.402163</y></Point><Point><x>-170.15625</x><y>74.116047</y></Point><Point><x>-173.671875</x><y>46.316584</y></Point><Point><x>-87.890625</x><y>-56.752723</y></Point><Point><x>-21.445312</x><y>-53.956086</y></Point><Point><x>-64.335937</x><y>73.627789</y></Point><Point><x>-101.601562</x><y>74.402163</y></Point></Polygon>");
    private static readonly IUUIDSequentialGuid UuidBuilder = new UUIDSequentialGuid();

    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    private static readonly bool EnableNextGenSync = Convert.ToBoolean(ConfigurationManager.AppSettings["VSP.GeofenceAPI.EnableSync"]);

    public event SiteDispatchedEvent SiteDispatched;
    public event SiteRemovedEvent SiteRemoved;

    #region ISiteAPI Members

    public bool Delete(INH_OP ctx, long siteID)
    {
      return Delete(ctx, siteID, false);
    }

    public bool DeleteWithTimeStamp(INH_OP ctx, long siteID)
    {
      return Delete(ctx, siteID, true);
    }

    public bool Undelete(INH_OP ctx, long customerID, long siteID)
    {
      Site s = (from sites in ctx.Site where sites.ID == siteID select sites).Single();

      CheckValidSite(ctx, customerID, s.Name);

      s.Visible = true;
      s.UpdateUTC = DateTime.UtcNow;

      bool unDeleted = ctx.SaveChanges() > 0;


      var customerGuid = (from customer in ctx.CustomerReadOnly
                          where customer.ID == s.fk_CustomerID
                          select customer.CustomerUID).FirstOrDefault();

      var geometryWKT = GetWicketFromPoints(s.PolygonPoints.ToList());

      var userGuid = (from user in ctx.UserReadOnly
                      where user.ID == s.fk_UserID
                      select user.UserUID).FirstOrDefault();

      Guid userUid;

      //Next Gen
      if (unDeleted && EnableNextGenSync)
      {
        if (geometryWKT != null && !String.IsNullOrEmpty(userGuid) && (Guid.TryParse(userGuid, out userUid)))
        {
          if (!customerGuid.HasValue)
          {
            log.IfWarnFormat("Not syncing site id {0} with next gen as customerGuid is empty", s.ID);
            return unDeleted;
          }

          var geofenceDetails = new
          {
            GeofenceName = s.Name,
            Description = String.IsNullOrWhiteSpace(s.Description) ? null : s.Description,
            GeofenceType = (from st in ctx.SiteTypeReadOnly where st.ID == s.fk_SiteTypeID select st.Description).FirstOrDefault(),
            GeometryWKT = geometryWKT,
            FillColor = s.Colour,
            IsTransparent = s.Transparent,
            CustomerUID = customerGuid.Value,
            UserUID = userUid,
            GeofenceUID = (s.SiteUID != null) ? s.SiteUID.Value : Guid.Empty,
            ActionUTC = DateTime.UtcNow,
          };

          var success = API.GeofenceService.CreateGeofence(geofenceDetails);
          if (!success)
          {
            log.IfInfoFormat("Error occurred while creating Site in VSP stack. Site Id :{0}", s.ID);
          }
        }
      }

      return unDeleted;
    }

    public long Update(INH_OP ctx, long customerID, long siteID, List<Param> modifiedProperties, long userId)
    {
      // this is the site name that exists in the model for the site id
      string previousSiteName = (from sites in ctx.Site where sites.ID == siteID select sites.Name).FirstOrDefault();

      // this is the site name submitted from the client
      Param newSiteName = modifiedProperties.Find(e => e.Name == "Name");

      // check if site names still match or have changed
      StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;

      if ((newSiteName != null) && !stringComparer.Equals(previousSiteName, newSiteName.Value.ToString())) //previousSiteName != newSiteName.Value.ToString())
      {
        // site name changed, check for duplicates
        CheckValidSite(ctx, customerID, newSiteName.Value.ToString());
      }

      // site name has not changed, update data
      Site s = (from sites in ctx.Site where sites.ID == siteID select sites).Single();

      var userGuid = (from user in ctx.UserReadOnly
                      where user.ID == userId
                      select user.UserUID).FirstOrDefault();

      var updatedEntries = modifiedProperties.ToDictionary(field => field.Name, field => field.Value);
      Guid userUid;
      if (Guid.TryParse(userGuid, out userUid))
      {
        updatedEntries.Add("UserUID", userUid);
      }
      if (s.SiteUID != null)
      {
        updatedEntries.Add("GeofenceUID", s.SiteUID.Value);
      }
      updatedEntries.Add("ActionUTC", DateTime.UtcNow);

      if (updatedEntries.ContainsKey("Description") && (string)updatedEntries["Description"] == s.Description)
      {
        updatedEntries.Remove("Description");
      }

      if (updatedEntries.ContainsKey("Name"))
      {
        var val = (string)updatedEntries["Name"];
        updatedEntries.Remove("Name");
        if (val != s.Name)
        {
          updatedEntries.Add("GeofenceName", val);
        }
      }

      if (updatedEntries.ContainsKey("fk_SiteTypeID"))
      {
        var siteTypeId = (int)updatedEntries["fk_SiteTypeID"];
        var siteType = (from st in ctx.SiteTypeReadOnly where st.ID == siteTypeId select st.Description).FirstOrDefault();
        updatedEntries.Remove("fk_SiteTypeID");
        if (siteTypeId != s.fk_SiteTypeID)
        {
          updatedEntries.Add("GeofenceType", siteType);
        }
      }
      if (updatedEntries.ContainsKey("Colour"))
      {
        var val = (int?)updatedEntries["Colour"];
        updatedEntries.Remove("Colour");
        if (val != s.Colour)
        {
          updatedEntries.Add("FillColor", val);
        }
      }
      if (updatedEntries.ContainsKey("Transparent"))
      {
        var val = (bool?)updatedEntries["Transparent"];
        updatedEntries.Remove("Transparent");
        if (val != s.Transparent)
        {
          updatedEntries.Add("IsTransparent", val);
        }
      }
      if (updatedEntries.ContainsKey("UpdateUTC"))
      {
        updatedEntries.Remove("UpdateUTC");
      }

      Site updatedSite = API.Update<Site>(ctx, s, modifiedProperties);

      //Next Gen 
      if (updatedSite != null && EnableNextGenSync)
      {
        var success = API.GeofenceService.UpdateGeofence(updatedEntries);
        if (!success)
        {
          log.IfInfoFormat("Error occurred while updating Site in VSP stack. Site Id :{0}", s.ID);
        }
      }
      if (updatedSite != null && s.fk_SiteTypeID != (int)SiteTypeEnum.Landfill && updatedSite.fk_SiteTypeID == (int)SiteTypeEnum.Landfill)
      {
        SyncAssignSiteToProject(ctx, customerID, updatedSite);
      }

      return (updatedSite != null) ? updatedSite.ID : -1;
    }

    private void SyncAssignSiteToProject(INH_OP ctx, long customerID, Site updatedSite)
    {
      //Only landfill sites
      log.IfDebugFormat("SyncSite: SiteType is {0}, siteUID {1}", updatedSite.fk_SiteTypeID, updatedSite.SiteUID);
      if (updatedSite.fk_SiteTypeID == (int)SiteTypeEnum.Landfill)
      {
        var projectUid = SitesOverlapHelper.ProjectAssociatedWithLandfillSite(ctx, customerID, updatedSite);
        log.IfDebugFormat("SyncSite: ProjectUid is {0}", projectUid);
        if (projectUid.HasValue)
        {
          string errorMessage = "Missing site UID";
          log.IfDebugFormat("SyncSite: Trying to sync site with projectUID {0}", projectUid);
          var synchronzier = new ProjectSynchronizer();
          bool syncAssignSite = updatedSite.SiteUID.HasValue ?
            synchronzier.SyncAssignSiteToProject(projectUid.Value, updatedSite.SiteUID.Value,
            updatedSite.UpdateUTC ?? DateTime.UtcNow, out errorMessage) : false;
          log.IfDebugFormat("SyncSite: result is {0} message {1}", syncAssignSite, errorMessage);
          if (!syncAssignSite)
          {
            //For now we just log it.
            log.IfInfoFormat(
                "Failed to synch assign site {0} to project {1} for customer {2}",
                updatedSite.SiteUID, projectUid.Value, customerID);
            log.Info(errorMessage);
          }
        }
      }
    }

    private string GetWicketFromPoints(List<Point> points)
    {
      if (points.Count == 0)
        return "";

      var polygonWkt = new StringBuilder("POLYGON((");
      foreach (var point in points)
      {
        polygonWkt.Append(String.Format("{0} {1},", point.x, point.y));
      }
      polygonWkt.Append(String.Format("{0} {1}))", points[0].x, points[0].y));
      return polygonWkt.ToString();
    }

    public bool Assign(INH_OP opCtx, bool assign, Site site, long assetID, DeviceTypeEnum deviceType)
    {
      // 1) look up a SiteDispatched. If found, update its Removed field equal to 'assign'
      // 2) else, create a SiteDispatched record
      // 3) send the device the assignment/unassignment

      //These site types should never get here as a location should never be inside them, but for safety make sure they are never sent to the device.
      if (site.fk_SiteTypeID == (int)SiteTypeEnum.Import || site.fk_SiteTypeID == (int)SiteTypeEnum.Export)
        return false;

      bool assigned = true;

      if (AppFeatureMap.DoesFeatureSetSupportsFeature(DeviceTypeList.GetAppFeatureSetId((int)deviceType), AppFeatureEnum.SitePurge))
      {
        string gpsDeviceID = (from a in opCtx.AssetReadOnly
                              join d in opCtx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                              where a.AssetID == assetID
                              select d.GpsDeviceID).FirstOrDefault();

        if (!string.IsNullOrEmpty(gpsDeviceID))
        {
          if (!IsA5N2(deviceType) && deviceType != DeviceTypeEnum.DCM300)
          {
            // Note, the following NH_RAW dispatch is not transactional with the subsequent NH_OP insert/update
            // This means, NH_RAW dispatch can succeed, but the NH_OP operation can fail independently of each other
            assigned = API.MTSOutbound.AssignSite(opCtx, new string[] { gpsDeviceID }, site.ID, assign, deviceType);
          }

          if (assigned)
          {
            SiteDispatched siteDispatched = (from sd in opCtx.SiteDispatched
                                             where sd.fk_AssetID == assetID && sd.fk_SiteID == site.ID
                                             select sd).FirstOrDefault();

            if (siteDispatched != null)
            {
              if (siteDispatched.Removed == assign)
              {
                var removeAction = new Action(() =>
                {
                  siteDispatched.Removed = !assign;
                  siteDispatched.UpdateUTC = DateTime.UtcNow;
                  int rowsAffected = opCtx.SaveChanges();
                  OnSiteRemoved(site, deviceType, gpsDeviceID);
                });

                var objectContext = opCtx as ObjectContext;
                if (null != objectContext)
                {
                  if (objectContext.Connection.State == ConnectionState.Closed) objectContext.Connection.Open();
                  using (var transaction = objectContext.Connection.BeginTransaction())
                  {
                    removeAction();
                    transaction.Commit();
                  }
                  if (objectContext.Connection.State != ConnectionState.Closed) objectContext.Connection.Close();
                }
                else
                {
                  removeAction();
                }
              }
            }
            else
            {
              var dispatchAction = new Action(() =>
              {
                siteDispatched = new SiteDispatched();
                siteDispatched.UpdateUTC = DateTime.UtcNow;
                siteDispatched.Removed = !assign;
                siteDispatched.fk_AssetID = assetID;
                siteDispatched.fk_SiteID = site.ID;
                opCtx.SiteDispatched.AddObject(siteDispatched);
                int rowsAffected = opCtx.SaveChanges();
                OnSiteDispatched(site, deviceType, gpsDeviceID);
              });

              var objectContext = opCtx as ObjectContext;
              if (null != objectContext)
              {
                if (objectContext.Connection.State == ConnectionState.Closed) objectContext.Connection.Open();
                using (var transaction = objectContext.Connection.BeginTransaction())
                {
                  dispatchAction();
                  transaction.Commit();
                }
                if (objectContext.Connection.State != ConnectionState.Closed) objectContext.Connection.Close();
              }
              else
              {
                dispatchAction();
              }
            }
          }
        }
      }

      return assigned;
    }

    public bool Purge(INH_OP opCtx, string gpsDeviceID, DeviceTypeEnum deviceType)
    {
      bool purged = false;
      //Crosscheck not removed from here as existing sites in the crosscheck migrated from TCM have to be purged
      if (AppFeatureMap.DoesFeatureSetSupportsFeature(DeviceTypeList.GetAppFeatureSetId((int)deviceType), AppFeatureEnum.SitePurge))
      {
        long assetID = (from a in opCtx.AssetReadOnly
                        where a.Device.GpsDeviceID == gpsDeviceID && a.Device.fk_DeviceTypeID == (int)deviceType
                        select a.AssetID).FirstOrDefault();

        if (assetID != 0)
        {
          purged = API.MTSOutbound.SendPurgeSites(opCtx, new string[] { gpsDeviceID }, deviceType);

          if (purged)
          {
            List<SiteDispatched> sitesDispatched = (from sd in opCtx.SiteDispatched
                                                    where sd.fk_AssetID == assetID
                                                    select sd).ToList<SiteDispatched>();

            foreach (SiteDispatched sd in sitesDispatched)
            {
              sd.Removed = true;
              sd.UpdateUTC = DateTime.UtcNow;
            }
            int rowsAffected = opCtx.SaveChanges();
          }
        }
      }

      return purged;
    }

    #endregion

    #region Privates

    protected virtual void OnSiteDispatched(Site site, DeviceTypeEnum deviceType, string deviceId)
    {
      if (SiteDispatched != null)
      {
        SiteDispatched(this, new SiteDispatchedEventArgs { Site = site, DeviceType = deviceType, DeviceId = deviceId });
      }
    }

    protected virtual void OnSiteRemoved(Site site, DeviceTypeEnum deviceType, string deviceId)
    {
      if (SiteRemoved != null)
      {
        SiteRemoved(this, new SiteRemovedEventArgs { Site = site, DeviceType = deviceType, DeviceId = deviceId });
      }
    }

    private bool Delete(INH_OP ctx, long siteID, bool timeStampName)
    {
      bool deleted = false;

      Site site = (from sites in ctx.Site
                   where sites.ID == siteID
                   select sites)
                .Single();

      site.Visible = false;
      site.UpdateUTC = DateTime.UtcNow;
      if (timeStampName)
      {
        site.Name = SiteUtilities.DecoratedName(site.Name, site.UpdateUTC.Value);
      }

      //Purge from Device
      var siteAssetIDs = (from sd in ctx.SiteDispatchedReadOnly
                          where sd.fk_SiteID == siteID
                          select sd.fk_AssetID)
                                 .ToList<long>();


      var objectContext = ctx as ObjectContext;
      if (null != objectContext)
      {
        var nhOpConnection = objectContext.Connection;
        if (nhOpConnection.State == ConnectionState.Closed) nhOpConnection.Open();

        using (var transaction = nhOpConnection.BeginTransaction())
        {
          deleted = DeleteSiteForAssets(ctx, siteAssetIDs, site);
          transaction.Commit();
        }
        if (nhOpConnection.State != ConnectionState.Closed) nhOpConnection.Close();
      }
      else
      {
        deleted = DeleteSiteForAssets(ctx, siteAssetIDs, site);
      }

      return deleted;
    }

    private bool DeleteSiteForAssets(INH_OP ctx, IEnumerable<long> siteAssetIDs, Site site)
    {
      bool deleted;
      var devices = new List<KeyValuePair<string, DeviceTypeEnum>>();
      foreach (long siteAssetId in siteAssetIDs)
      {
        int? deviceTypeId = (from a in ctx.AssetReadOnly
                             join d in ctx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                             where a.AssetID == siteAssetId
                             select d.fk_DeviceTypeID)
          .FirstOrDefault();

        if (!deviceTypeId.HasValue)
        {
          continue;
        }
        var deviceType = (DeviceTypeEnum)deviceTypeId.Value;

        if (!(AppFeatureMap.DoesFeatureSetSupportsFeature(DeviceTypeList.GetAppFeatureSetId((int)deviceType), AppFeatureEnum.SitePurge)))
        {
          continue;
        }

        string gpsDeviceId = (from a in ctx.AssetReadOnly
                              join d in ctx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                              where a.AssetID == siteAssetId
                              select d.GpsDeviceID)
          .FirstOrDefault();

        if (NegateSiteDispatch(ctx, site, siteAssetId, deviceType, gpsDeviceId))
        {
          devices.Add(new KeyValuePair<string, DeviceTypeEnum>(gpsDeviceId, deviceType));
        }
      }
      deleted = ctx.SaveChanges() > 0;

      var userGuid = (from user in ctx.UserReadOnly
                      where user.ID == site.fk_UserID
                      select user.UserUID).FirstOrDefault();

      Guid customerUID = (from u in ctx.User
                          join c in ctx.CustomerReadOnly
                          on u.fk_CustomerID equals c.ID
                          where u.ID == site.fk_UserID
                          select c.CustomerUID ?? new Guid()).FirstOrDefault();

      var customerId = (from c in ctx.CustomerReadOnly
                        where c.CustomerUID == customerUID
                        select c.ID).FirstOrDefault();

      if (deleted && EnableNextGenSync)
      {
        if (site.SiteUID.HasValue)
        {
          var success = API.GeofenceService.DeleteGeofence(site.SiteUID.ToString(), userGuid);
          if (!success)
          {
            log.IfInfoFormat("Error occurred while deleting Site in VSP stack. Site Id :{0}", site.ID);
          }
          log.IfDebugFormat("Delete Geofence Event Payload Data : Geofence UID {0} UserUID : {1} , Posted Status : {2}", site.SiteUID.ToString(), userGuid, success);
        }
      }

      foreach (var device in devices)
      {
        if (!IsA5N2(device.Value) && device.Value != DeviceTypeEnum.DCM300)
        {
          continue;
        }
        OnSiteRemoved(site, device.Value, device.Key);
      }

      // Note, the MTSOutbound dispatch is the last step, assuming previous steps have all been successful
      foreach (var device in devices)
      {
        if (IsA5N2(device.Value) || device.Value == DeviceTypeEnum.DCM300)
        {
          continue;
        }
        API.MTSOutbound.AssignSite(ctx, new[] { device.Key }, site.ID, false, device.Value);
      }
      return deleted;
    }

    public bool IsA5N2(DeviceTypeEnum deviceType)
    {
      return (deviceType == DeviceTypeEnum.PL231 || deviceType == DeviceTypeEnum.PL241 ||
              deviceType == DeviceTypeEnum.PL631 || deviceType == DeviceTypeEnum.PL641 ||
              deviceType == DeviceTypeEnum.PLE631 || deviceType == DeviceTypeEnum.PLE641 ||
              deviceType == DeviceTypeEnum.PLE641PLUSPL631 || deviceType == DeviceTypeEnum.PL131 || deviceType == DeviceTypeEnum.PL141 ||
              deviceType == DeviceTypeEnum.PL440 || deviceType == DeviceTypeEnum.PL240 || deviceType == DeviceTypeEnum.PL161 || deviceType == DeviceTypeEnum.PLE642 || deviceType == DeviceTypeEnum.PL542
              || deviceType == DeviceTypeEnum.PLE742 || deviceType == DeviceTypeEnum.PL240B);

    }

    public bool NegateSiteDispatch(INH_OP opCtx, Site site, long assetId, DeviceTypeEnum deviceType, string gpsDeviceId)
    {
      bool removed = false;

      if (!string.IsNullOrEmpty(gpsDeviceId))
      {
        SiteDispatched siteDispatched = (from sd in opCtx.SiteDispatched
                                         where sd.fk_AssetID == assetId && sd.fk_SiteID == site.ID
                                         select sd).FirstOrDefault();
        if (siteDispatched != null && !siteDispatched.Removed)
        {
          siteDispatched.Removed = true;
          removed = true;
          siteDispatched.UpdateUTC = DateTime.UtcNow;
        }
      }
      return removed;
    }

    public void GetSitePolygon(Site site, List<Point> points)
    {

      double backBearing = 0;
      double forwardBearing = 0;
      double distance = 0;
      double endLat = 0;
      double endLon = 0;

      foreach (Point p in points)
      {
        site.MaxLon = Math.Max(p.Longitude, site.MaxLon.HasValue ? site.MaxLon.Value : Int32.MinValue);
        site.MaxLat = Math.Max(p.Latitude, site.MaxLat.HasValue ? site.MaxLat.Value : Int32.MinValue);
        site.MinLon = Math.Min(p.Longitude, site.MinLon.HasValue ? site.MinLon.Value : Int32.MaxValue);
        site.MinLat = Math.Min(p.Latitude, site.MinLat.HasValue ? site.MinLat.Value : Int32.MaxValue);
        site.AddPoint(p.Latitude, p.Longitude);
      }

      RobbinsFormulae.EllipsoidRobbinsReverse(site.MinLat.Value, site.MinLon.Value, site.MaxLat.Value, site.MaxLon.Value,
        out forwardBearing, out backBearing, out distance);

      RobbinsFormulae.EllipsoidRobbinsForward(site.MinLat.Value, site.MinLon.Value, forwardBearing, (distance / 2.0), out endLat, out endLon);

      foreach (Point p in points)
      {
        RobbinsFormulae.EllipsoidRobbinsReverse(endLon, endLat, p.Latitude, p.Longitude,
        out forwardBearing, out backBearing, out distance);
      }
    }

    public void CheckValidSite(INH_OP ctx, long customerID, string name)
    {
      string containsSite = (from s in ctx.Site
                             where s.fk_CustomerID == customerID
                                 && s.Visible == true && s.Name == name
                             select s.Name).FirstOrDefault<string>();
      if (!string.IsNullOrEmpty(containsSite))
      {
        throw new InvalidOperationException("Can not have multiple sites with the same name", new IntentionallyThrownException());
      }
    }
    #endregion

    #region ISiteAPI for Site / Asset containment


    /// <summary>
    /// Returns true if input point is in "Americas" polygon, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool IsPointInAmericas(double lat, double lon)
    {
      return americasPolygon.Inside(lat, lon);
    }

    /// <summary>
    /// Returns a list of all sites for a customer, sorted in increasing order of area.  Sorting is important here, as it supports
    /// the business rule of using the smallest site that contains the asset's location
    /// </summary>
    /// <returns></returns>
    public List<Site> GetAllSitesForCustomer(long customerID, bool includeDeleted = false)
    {
      List<Site> sitesByArea;
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
        sitesByArea = (from site in opCtx.SiteReadOnly
                       where site.fk_CustomerID == customerID
                          && (site.Visible || includeDeleted)
                       orderby site.AreaST ascending
                       select site).ToList();
      }
      return sitesByArea;
    }

    /// <summary>
    /// Returns a dictionary indexed by site ID of site names for the given list of sites.
    /// </summary>
    /// <returns></returns>
    public Dictionary<long, string> GetSitesNamesForCustomer(long customerID, List<long> siteIDs)
    {
      //Note: query does not check for deleted sites as this is used by the Load Count report
      //where even deleted site names are to be shown.
      Dictionary<long, string> sites = null;
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
        sites = (from site in opCtx.SiteReadOnly
                 where site.fk_CustomerID == customerID
                       && siteIDs.Contains(site.ID)
                 select new { site.ID, site.Name }).ToDictionary(s => s.ID, s => s.Name);
      }
      return sites;
    }

    /// <summary>
    /// Returns a list of all site IDNamePairs for a customer, sorted by name. 
    /// </summary>
    /// <returns></returns>
    private IQueryable<ItemNamePair> GetSiteNamesForCustomer(INH_OP opCtx, long customerID)
    {
      int projectSiteType = (int)SiteTypeEnum.Project;

      IQueryable<ItemNamePair> siteNames = (from s in opCtx.SiteReadOnly
                                            where s.fk_CustomerID == customerID &&
                                            s.fk_SiteTypeID != projectSiteType &&
                                            s.Visible
                                            orderby s.Name
                                            select new ItemNamePair { ID = s.ID, name = s.Name });

      return siteNames;
    }

    public List<ItemNamePair> GetSiteNamesForCustomer(long customerID, int pageNumber, int numberToTake)
    {
      List<ItemNamePair> siteNames = null;
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
        IQueryable<ItemNamePair> sites = GetSiteNamesForCustomer(opCtx, customerID).OrderBy(c => c.name);
        if (pageNumber > 0)
          sites = sites.Skip(pageNumber * numberToTake);

        siteNames = sites.Take(numberToTake).ToList();
      }
      return siteNames;
    }

    /// <summary>
    /// Returns a list of all site IDNamePairs for a customer, sorted by name. 
    /// </summary>
    /// <returns></returns>
    public int GetSiteNamesForCustomerCount(long customerID, int numberToTake)
    {
      int sites;
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
        sites = GetSiteNamesForCustomer(opCtx, customerID).Count();
      }

      int numPages = (int)Math.Ceiling(sites / (double)numberToTake);
      return numPages;
    }

    /// <summary>
    /// Returns the name of the innermost site containing the input lat/lon point.
    /// Sites considered are all sites associated with the input customer ID 
    /// </summary>
    /// <returns>Name of the first site in the list containing the input latitude/longitude.  
    ///          If the point is not within any site, an empty string is returned. </returns>
    public string GetCustomerSiteNameForLocation(long customerID, double latitude, double longitude)
    {
      List<Site> customerSites = GetSitesForCustomer(customerID);
      return FindSiteForLatLon(customerSites, latitude, longitude);
    }

    public string GetFavoriteSiteNameForLocation(List<Site> userFavSites, double latitude, double longitude)
    {
      return FindSiteForLatLon(userFavSites, latitude, longitude);
    }
    /// <summary>
    /// Finds the first site in the provided site list containing the latitude/longitude point input by the user
    /// </summary>
    /// <returns>Name of the first site in the list containing the input latitude/longitude.  
    ///          If the point is not within any site, an empty string is returned. </returns>
    public string FindSiteForLatLon(List<Site> sitesList, double latitude, double longitude)
    {
      foreach (Site site in sitesList)
      {
        if (site.Inside(latitude, longitude))
        {
          return site.Name;
        }
      }

      return string.Empty;
    }

    #endregion ISiteAPI for Site / Asset containment

    #region Privates ISiteAPI for Site / Asset containment

    private class AssetWithPosition
    {
      public long AssetID = 0;
      public double Latitude = 0;
      public double Longitude = 0;
    }

    /// <summary>
    /// Returns a list of all sites for a customer, sorted in increasing order of area.  Sorting is important here, as it supports
    /// the business rule of using the smallest site that contains the asset's location
    /// </summary>
    /// <param name="customerID"></param>
    /// <returns></returns>
    private List<Site> GetSitesForCustomer(long customerID)
    {
      List<Site> sitesByArea;
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
        sitesByArea = (from site in opCtx.SiteReadOnly
                       where site.fk_CustomerID == customerID
                          && site.Visible
                       orderby site.AreaST ascending
                       select site).ToList();
      }

      return sitesByArea;
    }

    private List<Site> GetSitesForIDs(List<long> siteIDs)
    {
      List<Site> sitesByArea;
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
        sitesByArea = (from site in opCtx.SiteReadOnly
                       where siteIDs.Contains(site.ID)
                       orderby site.AreaST ascending
                       select site).ToList();
      }

      return sitesByArea;
    }

    private List<long> PopulateAssetsInSitesList(List<AssetWithPosition> assetsWithPosition, List<Site> sites)
    {
      List<long> assetsInSites = new List<long>();
      foreach (AssetWithPosition assetWithPosition in assetsWithPosition)
      {
        foreach (Site site in sites)
        {
          if (site.Inside(assetWithPosition.Latitude, assetWithPosition.Longitude))
          {
            assetsInSites.Add(assetWithPosition.AssetID);
            break;  // only the first site encountered is added to the dictionary, assuming they are ordered smallest to biggest.
          }
        }
      }
      return assetsInSites;
    }

    #endregion ISiteAPI for Site / Asset containment


  }
}
