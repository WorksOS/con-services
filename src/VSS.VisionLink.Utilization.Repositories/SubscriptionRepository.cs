using System.Linq;
using System.Threading.Tasks;
using Dapper;
using VSS.Interfaces.Events.MasterData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.VisionLink.Landfill.Common.Interfaces;
using VSS.VisionLink.Landfill.Common.Models;

namespace VSS.VisionLink.Landfill.Repositories
{
  /*public class SubscriptionRepository : RepositoryBase, ISubscriptionRepository
  {
    private static readonly AsyncLock Locker = new AsyncLock();

    public SubscriptionRepository(string connectionString)
      : base(connectionString)
    {
    }


    public async Task<int> StoreSubscription(ISubscriptionEvent evt)
    {
      var upsertedCount = 0;
      var subscription = new Subscription();
      string eventType = "Unknown"; 
      if (evt is CreateSubscriptionEvent)
      {
        var subscriptionEvent = (CreateSubscriptionEvent)evt;
        subscription.subscriptionUid = subscriptionEvent.SubscriptionUID.ToString();
        subscription.customerUid = subscriptionEvent.CustomerUID.ToString();
        subscription.startDate = subscriptionEvent.StartDate;
        subscription.lastActionedUtc = subscriptionEvent.ActionUTC;
        eventType = "CreateSubscriptiontEvent";
      }
      else if (evt is UpdateSubscriptionEvent)
      {
        var subscriptionEvent = (UpdateSubscriptionEvent)evt;
        //The only field that can be updated is subscirption enddate. If it is empty - do nothing
        if (!subscriptionEvent.EndDate.HasValue)
          return 0;
        subscription.subscriptionUid = subscriptionEvent.SubscriptionUID.ToString();
        subscription.customerUid = subscriptionEvent.CustomerUID.ToString();
        subscription.endDate = subscriptionEvent.EndDate.Value;
        subscription.lastActionedUtc = subscriptionEvent.ActionUTC;
        eventType = "UpdateSubscriptiontEvent";
      }
      
      upsertedCount = await UpsertSubscriptionDetail(subscription, eventType);
      PerhapsCloseConnection();
      return upsertedCount;
    }

    /// <summary>
    /// All detail-related columns can be inserted, 
    ///    but only certain columns can be updated.
    ///    on deletion, a flag will be set.
    /// </summary>
    /// <param name="subscription"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private async Task<int> UpsertSubscriptionDetail(Subscription subscription, string eventType)
    {
      using (await Locker.LockAsync())
      {
        PerhapsOpenConnection();
        log.DebugFormat("SubscriptionRepository: Upserting eventType{0} subscriptionUid={1}", eventType, subscription.subscriptionUid);
        var upsertedCount = 0;

        var existing = (await Connection.QueryAsync<Subscription>
          (@"SELECT 
                  subscriptionUid, customerUid, startDate, lastActionedUTC, endDate
                FROM subscriptions
                WHERE subscriptionUid = @subscriptionUid", new { subscription.subscriptionUid })).FirstOrDefault();

        if (existing == null && eventType == "CreateSubscriptiontEvent")
        {
          upsertedCount = await CreateSubscription(subscription);
        }

        if (eventType == "UpdateProjectEvent")
        {
          upsertedCount = await UpdateSubscription(subscription, existing);
        }

       
        log.DebugFormat("SubscriptionRepository: upserted {0} rows", upsertedCount);
        PerhapsCloseConnection();
        return upsertedCount;
      }
    }

    private async Task<int> CreateSubscription(Subscription subscription)
    {
      const string insert =
        @"INSERT subscriptions
                (subscriptionUid, customerUid, startDate, lastActionedUTC, endDate)
                VALUES
                (@subscriptionUid, @customerUid, @startDate, @lastActionedUTC, @endDate)";
      return await Connection.ExecuteAsync(insert, subscription);
    }

    private async Task<int> UpdateSubscription(Subscription subscription, Subscription existing)
    {
      if (existing != null)
      {
        if (subscription.lastActionedUtc >= existing.lastActionedUtc)
        {
          const string update =
            @"UPDATE subscriptions                
                SET endDate = @endDate,
                  lastActionedUTC = @lastActionedUtc
              WHERE subscriptionUid = @subscriptionUid";
          return await Connection.ExecuteAsync(update, subscription);
        }
        else
        {
          log.DebugFormat("SubscriptionRepository: old update event ignored currentActionedUTC{0} newActionedUTC{1}",
            existing.lastActionedUtc, subscription.lastActionedUtc);
        }
      }
      else
      {
        log.DebugFormat("SubscriptionRepository: can't update as none existing newActionedUTC {0}",
          subscription.lastActionedUtc);
      }
      return await Task.FromResult(0);
    }

    public Subscription GetSubscription(string subscriptionUid)
    {
      PerhapsOpenConnection();
      var project = Connection.Query<Subscription>
        (@"SELECT 
                  subscriptionUid, customerUid, startDate, lastActionedUTC, endDate
              FROM subscriptions
              WHERE subscriptionUid = @subscriptionUid"
          , new { subscriptionUid }
        ).FirstOrDefault();
      PerhapsCloseConnection();
      return project;
    }
  }*/
}