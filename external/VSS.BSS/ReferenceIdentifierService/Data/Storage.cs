using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.Bss;
using VSS.Nighthawk.ReferenceIdentifierService.Encryption;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Implementations.Helpers;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Data
{
  public class Storage : IStorage
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly int _assetCacheLifetimeInMinutes;
    private readonly ICacheManager _cacheManager;
    private readonly int _customerCacheLifetimeInMinutes;
    private readonly int _deviceCacheLifetimeInMinutes;
    private readonly INHOpContextFactory _nhOpContxtFactory;
    private readonly int _serviceCacheLifetimeInMinutes;
    private readonly IStringEncryptor _stringEncryptor;

    public Storage(INHOpContextFactory nhOpContxtFactory, ICacheManager cacheManager, IStringEncryptor stringEncryptor,
      int customerCacheLifetimeInMinutes,
      int assetCacheLifetimeInMinutes,
      int deviceCacheLifetimeInMinutes,
      int serviceCacheLifetimeInMinutes)
    {
      _nhOpContxtFactory = nhOpContxtFactory;
      _cacheManager = cacheManager;
      _stringEncryptor = stringEncryptor;
      _customerCacheLifetimeInMinutes = customerCacheLifetimeInMinutes;
      _assetCacheLifetimeInMinutes = assetCacheLifetimeInMinutes;
      _deviceCacheLifetimeInMinutes = deviceCacheLifetimeInMinutes;
      _serviceCacheLifetimeInMinutes = serviceCacheLifetimeInMinutes;
    }

    private static Exception RedefineDuplicatesException<TException>(InvalidOperationException e) 
      where TException: DuplicateReferenceFoundException, new()
    {
      if (e.Message.ToLower().Contains("sequence contains more than one element") && e.StackTrace.Contains("System.Linq.Enumerable.SingleOrDefault"))
      {
        Log.IfError("Reference duplicate discovered in database", e);
        return new TException();
      }
      Log.IfError("Invalid database operation", e);
      return e;
    }

    private static Exception RedefineDuplicatesException<TException>(UpdateException e)
      where TException : CreatingDuplicateException, new()
    {
      if ((null != e.InnerException) && (e.InnerException is SqlException) && (e.InnerException.Message.Contains("Cannot insert duplicate key")))
      {
        Log.IfError("Attempt to create duplicate reference in database", e);
        return new TException();
      }
      Log.IfError("Invalid database operation", e);
      return e;
    }

    #region IStorage Members

    public Guid? FindCustomerReference(IdentifierDefinition identifierDefinition)
    {
      if (identifierDefinition == null)
        return null;

      Log.IfInfoFormat(Resources.CacheFetchAttempt, "Customer Reference", identifierDefinition.StoreId, identifierDefinition.Alias, identifierDefinition.Value);
      Guid? uid = GetFromCache(KeyStore.CustomerReference, identifierDefinition);

      if (uid == null)
      {
        Log.IfInfoFormat(Resources.DatabaseFetchAttempt, "Customer Reference", identifierDefinition.StoreId, identifierDefinition.Alias, identifierDefinition.Value);
        try
        {
          bool retry = false;
          using (INH_OP _opCtx = _nhOpContxtFactory.CreateContext())
          {
            try
            {
              uid = GetCustomerReferenceUid(identifierDefinition, _opCtx);
            }
            catch(EntityCommandExecutionException ecee)
            {
              //bug 34350 if Physical connection is not usable exception occurs try clearing the pool and trying again
              if (Config.Default.UnusableConnectionRetry && ((SqlException)(ecee.InnerException)).Message.Contains("error: 19 - Physical connection is not usable"))
              {
                EntityConnection ec = (EntityConnection)_opCtx.Connection;
                SqlConnection.ClearPool((SqlConnection)ec.StoreConnection);
                retry = true;
              }
              else throw;
            }
          }

          if(retry)
          {
            using(INH_OP ctx = _nhOpContxtFactory.CreateContext())
            {
              uid = GetCustomerReferenceUid(identifierDefinition, ctx);
            }
          }
        }
        catch (InvalidOperationException e)
        {
          throw RedefineDuplicatesException<DuplicateCustomerReferenceFoundException>(e);
        }
        catch (Exception e)
        {
          Log.IfError("Error accessing the database", e);
          throw;
        }
        Log.IfInfoFormat(Resources.CacheAdd, "Customer Reference", identifierDefinition.StoreId, identifierDefinition.Alias, identifierDefinition.Value, uid);
        AddToCache(KeyStore.CustomerReference, identifierDefinition, uid, _customerCacheLifetimeInMinutes);
      }
      else
      {
        Log.IfInfoFormat(Resources.CacheFetchSuccess, "Customer Reference", identifierDefinition.StoreId, identifierDefinition.Alias, identifierDefinition.Value, uid);
      }

      return uid;
    }

    private static Guid? GetCustomerReferenceUid(IdentifierDefinition identifierDefinition, INH_OP _opCtx)
    {
      Guid aUid = (from devref in _opCtx.CustomerReferenceReadOnly
                   where devref.fk_StoreID == identifierDefinition.StoreId
                         && devref.Alias == identifierDefinition.Alias
                         && devref.Value == identifierDefinition.Value
                   select devref.UID).SingleOrDefault();

      return (aUid == Guid.Empty)
        ? (Guid?)null
        : aUid;
    }

    public void UpdateCustomerReference(IdentifierDefinition identifierDefinition)
    {
      if (identifierDefinition == null)
        return;

      try
      {
        using (INH_OP _opCtx = _nhOpContxtFactory.CreateContext())
        {
          var customersToUpdate = (from cr in _opCtx.CustomerReference
                          where cr.Alias == identifierDefinition.Alias &&
                          cr.UID == identifierDefinition.UID
                          select cr).ToList();

          foreach(var c in customersToUpdate)
          {
            Log.IfInfoFormat("Updating StoreID:{0}, UID:{1} to Value:{2}", c.fk_StoreID, c.UID, c.Value);
            c.Value = identifierDefinition.Value;
            AddToCache(KeyStore.CustomerReference, identifierDefinition, identifierDefinition.UID, _customerCacheLifetimeInMinutes);
          }

          _opCtx.SaveChanges();
        }

      }
      catch (Exception e)
      {
        Log.IfError("Error accessing the database", e);
        throw;
      }
    }

    public IList<AccountInfo> FindAccountsForDealer(Guid dealerUid)
    {
      try
      {
        using (INH_OP _opCtx = _nhOpContxtFactory.CreateContext())
        {
          var accounts = (from dealer in _opCtx.CustomerReadOnly
                          join
                            customerRelationship in _opCtx.CustomerRelationshipReadOnly on dealer.ID equals customerRelationship.fk_ParentCustomerID
                          join account in _opCtx.CustomerReadOnly on customerRelationship.fk_ClientCustomerID equals account.ID
                          where dealer.CustomerUID == dealerUid
                          && account.IsActivated == true
                          && account.fk_CustomerTypeID == (int)CustomerTypeEnum.Account
                          select new AccountInfo
                          {
                            CustomerUid = account.CustomerUID.Value,
                            DealerAccountCode = account.DealerAccountCode
                          }).ToList();

          return accounts;
        }
      }
      catch (Exception e)
      {
        Log.IfError("Error accessing the database", e);
        throw;
      }
    }

    public void AddCustomerReference(IdentifierDefinition identifierDefinition)
    {
      if (identifierDefinition == null)
        return;

      var customerRef = new CustomerReference
      {
        fk_StoreID = identifierDefinition.StoreId,
        Alias = identifierDefinition.Alias,
        Value = identifierDefinition.Value,
        UID = identifierDefinition.UID
      };
      try
      {
        using (INH_OP _opCtx = _nhOpContxtFactory.CreateContext())
        {
          _opCtx.CustomerReference.AddObject(customerRef);
          _opCtx.SaveChanges();
        }
      }
      catch (UpdateException updateException)
      {
        throw RedefineDuplicatesException<CreatingDuplicateCustomerReferenceException>(updateException);
      }
      catch (Exception e)
      {
        Log.IfError("Error accessing the database", e);
        throw;
      }

      AddToCache(KeyStore.CustomerReference, identifierDefinition, customerRef.UID, _customerCacheLifetimeInMinutes);
    }

    public List<Guid?> FindAllCustomersForService(Guid serviceUid)
    {
      using (var opCtx = _nhOpContxtFactory.CreateContext())
      {
        var serviceId = (from service in opCtx.ServiceReadOnly
                         where service.ServiceUID == serviceUid
                         select service.ID)
                         .SingleOrDefault();
        var keyDate = DateTime.UtcNow.KeyDate();

        var customers = (from s in opCtx.ServiceViewReadOnly
                        where (s.fk_ServiceID == serviceId && s.EndKeyDate > keyDate)
                        select s.fk_CustomerID).ToList();

        return (from cust in customers
                join a in opCtx.CustomerReadOnly
                on new { Id = cust } equals new { Id = a.ID }
                where cust == a.ID
                select a.CustomerUID).ToList();
      }
    }

    /// <summary>
    /// This method will find the Customer or Dealer parent for an Account or the Dealer parent of a Dealer.
    /// Any other combination will result in a null.
    /// </summary>
    /// <param name="childUid"></param>
    /// <param name="parentCustomerType"></param>
    /// <returns></returns>
    public Guid? FindCustomerParent(Guid childUid, CustomerTypeEnum parentCustomerType)
    {
      using (var opCtx = _nhOpContxtFactory.CreateContext())
      {
        var childId = (from c in opCtx.CustomerReadOnly
                       where c.CustomerUID == childUid
                       select c.ID).SingleOrDefault();

        if (childId <= 0)
          return null;

        CustomerRelationshipTypeEnum relationshipType;

        switch (parentCustomerType)
        {
          case CustomerTypeEnum.Dealer:
            relationshipType = CustomerRelationshipTypeEnum.TCSDealer;
            break;
          case CustomerTypeEnum.Customer:
            relationshipType = CustomerRelationshipTypeEnum.TCSCustomer;
            break;
          default:
            return null;
        }

        return (from cr in opCtx.CustomerRelationshipReadOnly
                join c in opCtx.CustomerReadOnly on cr.fk_ParentCustomerID equals c.ID
                where cr.fk_ClientCustomerID == childId
                   && cr.fk_CustomerRelationshipTypeID == (int)relationshipType
                select c.CustomerUID).SingleOrDefault();
      }
    }

    public Guid? FindCustomerGuidByCustomerId(long customerId)
    {
      using (var opCtx = _nhOpContxtFactory.CreateContext())
      {
        return (
          from c in opCtx.CustomerReadOnly
          where c.ID == customerId
          select c.CustomerUID)
        .SingleOrDefault();
      }
    }

    public long FindStoreByCustomerId(long customerId)
    {
      using (var opCtx = _nhOpContxtFactory.CreateContext())
      {
        return (
          from c in opCtx.CustomerStoreReadOnly
          where c.fk_CustomerID == customerId
          select c.fk_StoreID)
        .SingleOrDefault();
      }
    }

    public int FindOemIdentifierByCustomerId(long customerId)
    {
      using (var opCtx = _nhOpContxtFactory.CreateContext())
      {
        return (
          from c in opCtx.CustomerReadOnly
          where c.ID == customerId
          select c.fk_DealerNetworkID)
        .SingleOrDefault();
      }
    }

    public IList<IdentifierDefinition> FindDealers(IList<IdentifierDefinition> orgIdentifierDefinitions, long storeId)
    {
      using (var opCtx = _nhOpContxtFactory.CreateContext())
      {
        IList<IdentifierDefinition> dealers = new List<IdentifierDefinition>();
        foreach (IdentifierDefinition identifier in orgIdentifierDefinitions)
        {
          var dealer = (from c in opCtx.CustomerReadOnly
                        join q in opCtx.CustomerReferenceReadOnly on c.CustomerUID equals q.UID
                        join cs in opCtx.CustomerStoreReadOnly on c.ID equals cs.fk_CustomerID
                        where c.fk_CustomerTypeID == 0 && //Selecting only Dealer customer type
                        q.fk_StoreID == cs.fk_StoreID && q.fk_StoreID == storeId &&
                        q.Alias == identifier.Alias &&
                        q.Value == identifier.Value
                        select new { q.Alias, q.Value }).SingleOrDefault();

          if (dealer != null)
          {
            dealers.Add(new IdentifierDefinition { Alias = dealer.Alias, Value = dealer.Value });
          }
        }
        return dealers;
      }
    }

    public Guid? FindAssetReference(IdentifierDefinition identifierDefinition)
    {
      if (identifierDefinition == null)
        return null;

      Log.IfInfoFormat(Resources.CacheFetchAttempt, "Asset Reference", identifierDefinition.StoreId, identifierDefinition.Alias, identifierDefinition.Value);
      Guid? uid = GetFromCache(KeyStore.AssetReference, identifierDefinition);

      if (uid != null)
      {
        Log.IfInfoFormat(Resources.CacheFetchSuccess, "Asset Reference", identifierDefinition.StoreId, identifierDefinition.Alias, identifierDefinition.Value, uid);
        return uid;
      }

      Log.IfInfoFormat(Resources.DatabaseFetchAttempt, "Asset Reference", identifierDefinition.StoreId, identifierDefinition.Alias, identifierDefinition.Value);
      try
      {
        using (INH_OP _opCtx = _nhOpContxtFactory.CreateContext())
        {
          Guid assetReference = (from assetRef in _opCtx.AssetReferenceReadOnly
            where assetRef.fk_StoreID == identifierDefinition.StoreId &&
                  assetRef.Alias == identifierDefinition.Alias &&
                  assetRef.Value == identifierDefinition.Value
            select assetRef.UID).SingleOrDefault();

          uid = (assetReference == Guid.Empty)
            ? (Guid?) null
            : assetReference;
        }

        Log.IfInfoFormat(Resources.CacheAdd, "Asset Reference", identifierDefinition.StoreId, identifierDefinition.Alias, identifierDefinition.Value, uid);
        AddToCache(KeyStore.AssetReference, identifierDefinition, uid, _assetCacheLifetimeInMinutes);

        return uid;
      }
      catch (InvalidOperationException e)
      {
        throw RedefineDuplicatesException<DuplicateAssetReferenceFoundException>(e);
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error retrieving Asset Reference information", ex);
        throw;
      }
    }

    public void AddAssetReference(IdentifierDefinition identifierDefinition)
    {
      if (identifierDefinition == null)
        return;

      var assetRef = new AssetReference
      {
        fk_StoreID = identifierDefinition.StoreId,
        Alias = identifierDefinition.Alias,
        Value = identifierDefinition.Value,
        UID = identifierDefinition.UID
      };
      try
      {
        using (INH_OP _opCtx = _nhOpContxtFactory.CreateContext())
        {
          _opCtx.AssetReference.AddObject(assetRef);
          _opCtx.SaveChanges();
        }
      }
      catch (UpdateException updateException)
      {
        throw RedefineDuplicatesException<CreatingDuplicateAssetReferenceException>(updateException);
      }
      catch (Exception ex)
      {
        Log.IfError("Error accessing the database", ex);
        throw;
      }

      AddToCache(KeyStore.AssetReference, identifierDefinition, assetRef.UID, _assetCacheLifetimeInMinutes);
    }

    /// <summary>
    ///   gets the deviceUids for devices associated to the supplied asset
    ///   for what this is going to be used for to begin with this will not be placed into the cache because it will not be
    ///   used enough
    /// </summary>
    /// <param name="assetUid"></param>
    /// <returns></returns>
    public IList<Guid> GetAssociatedDevices(Guid assetUid)
    {
      if (assetUid == Guid.Empty)
        return null;

      try
      {
        using (INH_OP _opCtx = _nhOpContxtFactory.CreateContext())
        {
          return (from a in _opCtx.AssetReadOnly
            join d in _opCtx.DeviceReadOnly on a.fk_DeviceID equals d.ID
            where a.AssetUID == assetUid
                  && a.fk_DeviceID != 0
                  && d.DeviceUID.HasValue
            select d.DeviceUID.Value).ToList();
        }
      }
      catch (Exception ex)
      {
        Log.IfError("Error accessing the database", ex);
        throw;
      }
    }

    /// <summary>
    ///   Gets the CustomerUID related to the supplied AssetUID or returns null if none is available
    /// </summary>
    /// <param name="assetUid"></param>
    /// <returns></returns>
    public Guid? FindOwner(Guid assetUid)
    {
      if (assetUid == Guid.Empty)
        return null;

      try
      {
        using (INH_OP _opCtx = _nhOpContxtFactory.CreateContext())
        {
          return (from a in _opCtx.AssetReadOnly
            join d in _opCtx.DeviceReadOnly on a.fk_DeviceID equals d.ID
            join c in _opCtx.CustomerReadOnly on d.OwnerBSSID equals c.BSSID
            where a.AssetUID == assetUid
                  && a.fk_DeviceID != 0
            select c.CustomerUID).FirstOrDefault();
        }
      }
      catch (Exception ex)
      {
        Log.IfError("Error accessing the database", ex);
        throw;
      }
    }

    public Guid? FindDeviceReference(IdentifierDefinition identifierDefinition)
    {
      if (identifierDefinition == null)
        return null;

      Log.IfInfoFormat(Resources.CacheFetchAttempt, "Device Reference", identifierDefinition.StoreId, identifierDefinition.Alias, identifierDefinition.Value);
      Guid? uid = GetFromCache(KeyStore.DeviceReference, identifierDefinition);

      if (uid == null)
      {
        Log.IfInfoFormat(Resources.DatabaseFetchAttempt, "Device Reference", identifierDefinition.StoreId, identifierDefinition.Alias, identifierDefinition.Value);
        try
        {
          using (INH_OP _opCtx = _nhOpContxtFactory.CreateContext())
          {
            Guid aUid = (from devref in _opCtx.DeviceReferenceReadOnly
              where devref.fk_StoreID == identifierDefinition.StoreId
                    && devref.Alias == identifierDefinition.Alias
                    && devref.Value == identifierDefinition.Value
              select devref.UID).SingleOrDefault();

            uid = (aUid == Guid.Empty)
              ? (Guid?) null
              : aUid;
          }
        }
        catch (InvalidOperationException e)
        {
          throw RedefineDuplicatesException<DuplicateDeviceReferenceFoundException>(e);
        }
        catch (Exception e)
        {
          Log.IfError("Error accessing the database", e);
          throw;
        }

        Log.IfInfoFormat(Resources.CacheAdd, "Device Reference", identifierDefinition.StoreId, identifierDefinition.Alias, identifierDefinition.Value, uid);
        AddToCache(KeyStore.DeviceReference, identifierDefinition, uid, _deviceCacheLifetimeInMinutes);
      }
      else
      {
        Log.IfInfoFormat(Resources.CacheFetchSuccess, "Device Reference", identifierDefinition.StoreId, identifierDefinition.Alias, identifierDefinition.Value, uid);
      }

      return uid;
    }

    public void AddDeviceReference(IdentifierDefinition identifierDefinition)
    {
      if (identifierDefinition == null)
        return;

      var deviceRef = new DeviceReference
      {
        fk_StoreID = identifierDefinition.StoreId,
        Alias = identifierDefinition.Alias,
        Value = identifierDefinition.Value,
        UID = identifierDefinition.UID
      };
      try
      {
        using (INH_OP _opCtx = _nhOpContxtFactory.CreateContext())
        {
          _opCtx.DeviceReference.AddObject(deviceRef);
          _opCtx.SaveChanges();
        }
      }
      catch (UpdateException updateException)
      {
        throw RedefineDuplicatesException<CreatingDuplicateDeviceReferenceException>(updateException);
      }
      catch (Exception e)
      {
        Log.IfError("Error accessing the database", e);
        throw;
      }

      AddToCache(KeyStore.DeviceReference, identifierDefinition, deviceRef.UID, _deviceCacheLifetimeInMinutes);
    }

    /// <summary>
    ///   gets the assetUid for the asset associated to the supplied device
    ///   for what this is going to be used for to begin with this will not be placed into the cache because it will not be
    ///   used enough
    /// </summary>
    /// <param name="deviceUid"></param>
    /// <returns></returns>
    public Guid? GetAssociatedAsset(Guid deviceUid)
    {
      if (deviceUid == Guid.Empty)
        return null;
      try
      {
        using (INH_OP _opCtx = _nhOpContxtFactory.CreateContext())
        {
          return (from a in _opCtx.AssetReadOnly
            join d in _opCtx.DeviceReadOnly on a.fk_DeviceID equals d.ID
            where d.DeviceUID == deviceUid
                  && d.ID != 0
                  && d.DeviceUID.HasValue
                  && a.AssetUID.HasValue
            select a.AssetUID).FirstOrDefault();
        }
      }
      catch (Exception e)
      {
        Log.IfError("Error accessing the database", e);
        throw;
      }
    }

    public Guid? FindServiceReference(IdentifierDefinition identifierDefinition)
    {
      if (identifierDefinition == null)
        return null;

      Log.IfInfoFormat(Resources.CacheFetchAttempt, "Service Reference", identifierDefinition.StoreId, identifierDefinition.Alias, identifierDefinition.Value);

      Guid? uid = GetFromCache(KeyStore.ServiceReference, identifierDefinition);

      if (uid != null)
      {
        Log.IfInfoFormat(Resources.CacheFetchSuccess, "Service Reference", identifierDefinition.StoreId, identifierDefinition.Alias, identifierDefinition.Value, uid);
        return uid;
      }

      Log.IfInfoFormat(Resources.DatabaseFetchAttempt, "Service Reference", identifierDefinition.StoreId, identifierDefinition.Alias, identifierDefinition.Value);
      try
      {
        using (INH_OP _opCtx = _nhOpContxtFactory.CreateContext())
        {
          Guid serviceReference = (from serviceRef in _opCtx.ServiceReferenceReadOnly
            where serviceRef.fk_StoreID == identifierDefinition.StoreId &&
                  serviceRef.Alias == identifierDefinition.Alias &&
                  serviceRef.Value == identifierDefinition.Value
            select serviceRef.UID).SingleOrDefault();

          uid = (serviceReference == Guid.Empty)
            ? (Guid?) null
            : serviceReference;
        }

        AddToCache(KeyStore.ServiceReference, identifierDefinition, uid, _serviceCacheLifetimeInMinutes);
        Log.IfInfoFormat(Resources.CacheAdd, "Service Reference", identifierDefinition.StoreId, identifierDefinition.Alias, identifierDefinition.Value, uid);

        return uid;
      }
      catch (InvalidOperationException e)
      {
        throw RedefineDuplicatesException<DuplicateServiceReferenceFoundException>(e);
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error retrieving Service Reference information", ex);
        throw;
      }
    }

    public void AddServiceReference(IdentifierDefinition identifierDefinition)
    {
      if (identifierDefinition == null)
        return;

      var serviceRef = new ServiceReference
      {
        fk_StoreID = identifierDefinition.StoreId,
        Alias = identifierDefinition.Alias,
        Value = identifierDefinition.Value,
        UID = identifierDefinition.UID
      };
      try
      {
        using (INH_OP _opCtx = _nhOpContxtFactory.CreateContext())
        {
          _opCtx.ServiceReference.AddObject(serviceRef);
          _opCtx.SaveChanges();
        }
      }
      catch (UpdateException updateException)
      {
        throw RedefineDuplicatesException<CreatingDuplicateServiceReferenceException>(updateException);
      }
      catch (Exception ex)
      {
        Log.IfError("Error accessing the database", ex);
        throw;
      }

      AddToCache(KeyStore.ServiceReference, identifierDefinition, serviceRef.UID, _serviceCacheLifetimeInMinutes);
    }

    public List<Guid?> GetAssetActiveServices(Guid assetUid)
    {
      using (var opCtx = _nhOpContxtFactory.CreateContext())
      {
        var deviceId = (from asset in opCtx.AssetReadOnly
                        where asset.AssetUID == assetUid
                        select asset.fk_DeviceID).SingleOrDefault();

        var keyDate = DateTime.UtcNow.KeyDate();
        return (from s in opCtx.ServiceReadOnly
                where (s.fk_DeviceID == deviceId && s.CancellationKeyDate > keyDate)
                select s.ServiceUID).ToList();
      }
    }

    public IList<ServiceLookupItem> GetAssetActiveServices(string serialNumber, string makeCode)
    {
      if (string.IsNullOrEmpty(serialNumber))
        return null;

      int now = DateTime.UtcNow.KeyDate();

      using (var opCtx = _nhOpContxtFactory.CreateContext())
      {
        return (from a in opCtx.AssetReadOnly
                join d in opCtx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                join s in opCtx.ServiceReadOnly on d.ID equals s.fk_DeviceID
                join t in opCtx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals t.ID
                where a.SerialNumberVIN == serialNumber
                   && a.fk_MakeCode == makeCode
                   && s.CancellationKeyDate > now
                select new ServiceLookupItem
                {
                  Type = t.Name,
                  UID = s.ServiceUID.HasValue ? s.ServiceUID.Value : Guid.Empty
                }).ToList();
      }
    }

    public IList<ServiceLookupItem> GetDeviceActiveServices(string serialNumber, DeviceTypeEnum deviceType)
    {
      if (string.IsNullOrEmpty(serialNumber))
        return null;

      int now = DateTime.UtcNow.KeyDate();

      using (var opCtx = _nhOpContxtFactory.CreateContext())
      {
        return (from s in opCtx.ServiceReadOnly
                join d in opCtx.DeviceReadOnly on s.fk_DeviceID equals d.ID
                join t in opCtx.ServiceTypeReadOnly on s.fk_ServiceTypeID equals t.ID
                where d.GpsDeviceID == serialNumber
                   && d.fk_DeviceTypeID == (int)deviceType
                   && s.CancellationKeyDate > now
                select new ServiceLookupItem
                {
                  Type = t.Name,
                  UID = s.ServiceUID.HasValue ? s.ServiceUID.Value : Guid.Empty
                }).ToList();
      }
    }

    public Credentials FindCredentialsForUrl(string url)
    {
      if (string.IsNullOrEmpty(url))
        return null;

      Log.IfInfoFormat("Trying to retrieve information for Service Provider from cache");
      Credentials credentials = GetFromClosetMatchCache(KeyStore.Credentials, url);

      if (credentials != null)
        return credentials;

      try
      {
        using (INH_OP _opCtx = _nhOpContxtFactory.CreateContext())
        {
          var creds = (from sp in _opCtx.ServiceProviderReadOnly
            where sp.ProviderName.ToLower().StartsWith("storeapi") && url.StartsWith(sp.ServerIPAddress)
            orderby sp.ServerIPAddress.Length descending
            select new
            {
              sp.UserName,
              sp.Password,
              URL = sp.ServerIPAddress
            }).FirstOrDefault();

          if (creds != null)
          {
            credentials = new Credentials
            {
              EncryptedPassword = !string.IsNullOrEmpty(creds.Password) ? EncryptString(creds.Password) : null,
              UserName = creds.UserName
            };

            AddToCache(KeyStore.Credentials, creds.URL, credentials, _serviceCacheLifetimeInMinutes);
          }

          return credentials;
        }
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error retrieving Credential information for url", ex);
        throw;
      }
    }

    #endregion

    private Guid? GetFromCache(string keyName, IdentifierDefinition identifierDefinition)
    {
      return (Guid?) _cacheManager.GetData(KeyStore.GetKey(keyName, identifierDefinition));
    }

    private Credentials GetFromClosetMatchCache(string keyName, string data)
    {
      return (Credentials) _cacheManager.GetClosestData(String.Format("{0}.{1}", keyName, data));
    }

    private void AddToCache(string keyName, IdentifierDefinition identifierDefinition, Guid? data, int lifetimeInMinutes)
    {
      _cacheManager.Add(KeyStore.GetKey(keyName, identifierDefinition), data, lifetimeInMinutes);
    }

    private void AddToCache(string keyName, string url, Credentials data, int lifetimeInMinutes)
    {
      _cacheManager.Add(String.Format("{0}.{1}", keyName, url), data, lifetimeInMinutes);
    }

    private string EncryptString(string clearText)
    {
      return Convert.ToBase64String(_stringEncryptor.EncryptStringToBytes(clearText,
        Encoding.UTF8.GetBytes(Config.Default.AESKeyByte),
        Encoding.UTF8.GetBytes(Config.Default.AESIVByte)));
    }

    private static class KeyStore
    {
      public const string AssetReference = "AssetReference";
      public const string CustomerReference = "CustomerReference";
      public const string DeviceReference = "DeviceReference";
      public const string ServiceReference = "ServiceReference";
      public const string Credentials = "Credentials";

      public static string GetKey(string keyName, IdentifierDefinition keyValue)
      {
        return String.Format("{0}.{1}.{2}.{3}", keyName, keyValue.StoreId, keyValue.Alias, keyValue.Value);
      }
    }
  }
}