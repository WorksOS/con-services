using System.Threading.Tasks;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Landfill.Common.Models;

namespace VSS.VisionLink.Landfill.Common.Interfaces
{
  public interface ISubscriptionRepository
  {
    Subscription GetSubscription(string subscriptionUid);
    Task<int> StoreSubscription(ISubscriptionEvent evt);
  }
}