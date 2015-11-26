using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.KafkaWrapper.Interfaces;
using VSS.KafkaWrapper.Models;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.SubscriptionService;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Helpers
{
  public abstract class CheckForSubscriptionHandler : ISubscriber<KafkaMessage>
  {
    protected Guid _subscriptionUid;
    protected Guid _customerUid;
    protected DateTime _actionUtc;
    protected bool _found;

    public CheckForSubscriptionHandler(Guid subscriptionUid, Guid customerUid, DateTime actionUtc)
    {
      Init(subscriptionUid, customerUid, actionUtc);
    }

    public void Init(Guid subscriptionUid, Guid customerUid, DateTime actionUtc)
    {
      _subscriptionUid = subscriptionUid;
      _customerUid = customerUid;
      _actionUtc = actionUtc;
      _found = false;
    }

    public abstract void Handle(KafkaMessage message);

    public bool HasFound()
    {
      return _found;
    }
  }

  public class CheckForSubscriptionCreateHandler : CheckForSubscriptionHandler
  {

    public CheckForSubscriptionCreateHandler(Guid subscriptionUid, Guid customerUid, DateTime actionUtc)
      : base(subscriptionUid, customerUid,actionUtc)
    {
    }
    public CreateSubscriptionModel subscriptionEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("CreateSubscriptionEvent"))
        return;

      subscriptionEvent = JsonConvert.DeserializeObject<CreateSubscriptionModel>(message.Value);
      if (subscriptionEvent == null)
        return;

      if (subscriptionEvent.CreateSubscriptionEvent.ActionUTC.Equals(_actionUtc) && subscriptionEvent.CreateSubscriptionEvent.CustomerUID.Equals(_customerUid)
        && subscriptionEvent.CreateSubscriptionEvent.SubscriptionUID.Equals(_subscriptionUid))
        _found = true;
      subscriptionEvent = JsonConvert.DeserializeObject<CreateSubscriptionModel>(message.Value);
    }  
  }

  public class CheckForSubscriptionUpdateHandler : CheckForSubscriptionHandler
  {

    public CheckForSubscriptionUpdateHandler(Guid subscriptionUid, Guid customerUid, DateTime actionUtc)
      : base(subscriptionUid, customerUid, actionUtc)
    {
    }
    public UpdateSubscriptionModel subscriptionEvent;
    public KafkaMessage message;

    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("UpdateSubscriptionEvent"))
        return;

      subscriptionEvent = JsonConvert.DeserializeObject<UpdateSubscriptionModel>(message.Value);
      if (subscriptionEvent == null)
        return;

      if (subscriptionEvent.UpdateSubscriptionEvent.ActionUTC.Equals(_actionUtc) && subscriptionEvent.UpdateSubscriptionEvent.CustomerUID.Equals(_customerUid)
                && subscriptionEvent.UpdateSubscriptionEvent.SubscriptionUID.Equals(_subscriptionUid))
        _found = true;
      subscriptionEvent = JsonConvert.DeserializeObject<UpdateSubscriptionModel>(message.Value);

    }
  }
}
