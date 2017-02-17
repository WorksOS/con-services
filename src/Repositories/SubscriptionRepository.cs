using System.Collections.Generic;
using VSS.TagFileAuth.Service.Utils;
using Microsoft.Extensions.Logging;
using VSS.TagFileAuth.Service.Repositories.DBModels;

namespace VSS.TagFileAuth.Service.Repositories
{

  public class SubscriptionRepository : RepositoryBase
  {
    private readonly ILogger log;
    public Dictionary<string, ServiceType> _serviceTypes = null;

    public SubscriptionRepository(IConfigurationStore connectionString, ILoggerFactory logger)  : base(connectionString)
    {
      log = logger.CreateLogger<SubscriptionRepository>();      
    }


    //public async Task<Subscription> GetSubscription(string subscriptionUid)
    //{
    //  if (_serviceTypes == null)
    //    _serviceTypes = (await GetServiceTypes()).ToDictionary(k => k.Name, v => v);

    //  await PerhapsOpenConnection();

    //  var subscription = (await Connection.QueryAsync<Subscription>
    //    (@"SELECT 
    //            SubscriptionUID, fk_CustomerUID AS CustomerUID, fk_ServiceTypeID AS ServiceTypeID, StartDate, EndDate, LastActionedUTC
    //          FROM Subscription
    //          WHERE SubscriptionUID = @subscriptionUid"
    //      , new { subscriptionUid }
    //    )).FirstOrDefault();

    //  PerhapsCloseConnection();
    //  return subscription;
    //}

    //public async Task<IEnumerable<Subscription>> GetSubscriptions_UnitTest(string subscriptionUid)
    //{
    //  if(_serviceTypes == null)
    //    _serviceTypes = (await GetServiceTypes()).ToDictionary(k => k.Name, v => v);

    //  await PerhapsOpenConnection();

    //  var subscriptions = (await Connection.QueryAsync<Subscription>
    //    (@"SELECT 
    //            SubscriptionUID, fk_CustomerUID AS CustomerUID, fk_ServiceTypeID AS ServiceTypeID, StartDate, EndDate, LastActionedUTC
    //          FROM Subscription
    //          WHERE SubscriptionUID = @subscriptionUid"
    //      , new { subscriptionUid }
    //     ));

    //  PerhapsCloseConnection();
    //  return subscriptions;
    //}
    

    //private async Task<IEnumerable<ServiceType>> GetServiceTypes()
    //{
    //  await PerhapsOpenConnection();

    //  var serviceTypes = (await Connection.QueryAsync<ServiceType>
    //      (@"SELECT 
    //            s.ID, s.Description AS Name, sf.ID AS ServiceTypeFamilyID, sf.Description AS ServiceTypeFamilyName
    //          FROM ServiceTypeEnum s JOIN ServiceTypeFamilyEnum sf on s.fk_ServiceTypeFamilyID = sf.ID"
    //      ));
    //  PerhapsCloseConnection();
    //  return serviceTypes;
    //}
  }
}