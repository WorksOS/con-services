using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Subscription.Data.Models
{
  public class Subscription
  {
    public string SubscriptionUID { get; set; }
    public string CustomerUID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime EffectiveUTC { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }

    public class CustomerAssetSubscriptionData : ICustomerAssetSubscriptionData
    {
        public string fk_AssetUID { get; set; }
        public string SubscriptionType { get; set; }
        public long SubscriptionTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public interface ICustomerAssetSubscriptionData : ICustomerSubscriptionData
    {
        string fk_AssetUID { get; set; }
    }

    public interface ICustomerProjectSubscriptionData : ICustomerSubscriptionData
    {
        string fk_ProjectUID { get; set; }
    }

    public interface ICustomerSubscriptionData
    {
        string SubscriptionType { get; set; }
        long SubscriptionTypeId { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
    }

    public class CustomerProjectSubscriptionData : ICustomerProjectSubscriptionData
    {
        public string fk_ProjectUID { get; set; }
        public string SubscriptionType { get; set; }
        public long SubscriptionTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ActiveProjectCustomerSubscriptionModel : CustomerSubscriptionModel
    {
        public string SubscriptionGuid { get; set; }
    }

    public class CustomerSubscriptionModel
    {
        public string SubscriptionType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ActiveProjectCustomerSubscriptionList
    {
        public List<ActiveProjectCustomerSubscriptionModel> Subscriptions { get; set; }
    }

    public class CustomerSubscriptionList
    {
        public List<CustomerSubscriptionModel> Subscriptions { get; set; } 
    }
}
