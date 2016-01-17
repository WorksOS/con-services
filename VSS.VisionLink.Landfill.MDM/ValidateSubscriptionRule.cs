using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.VisionLink.Landfill.MDM.Interfaces;

namespace VSS.VisionLink.Landfill.MDM
{
  public class ValidateSubscriptionRule : IMDMRule<ISubscriptionEvent>
  {
    public ISubscriptionEvent ExecuteRule(ISubscriptionEvent incoming)
    {
      var subscription = incoming as CreateSubscriptionEvent;
      if (subscription !=null)
        if (subscription.SubscriptionType != "Landfill")
          return null;
      return incoming;
    }
  }
}