using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.KafkaWrapper.Interfaces;
using VSS.KafkaWrapper.Models;
using VSS.MasterData.Subscription.AcceptanceTests.Utils.Features.Classes.SubscriptionService;

namespace VSS.MasterData.Subscription.AcceptanceTests.Helpers
{
  public abstract class CheckForSubscriptionHandler : ISubscriber<KafkaMessage>
  {
    protected Guid _subscriptionUid;
    protected Guid _customerUid;
    protected DateTime _actionUtc;
    protected bool _found;

    public CheckForSubscriptionHandler(Guid subscriptionUid, DateTime actionUtc)
    {
      Init(subscriptionUid, actionUtc);
    }

    public void Init(Guid subscriptionUid, DateTime actionUtc)
    {
      _subscriptionUid = subscriptionUid;
      _actionUtc = actionUtc;
      _found = false;
    }

    public abstract void Handle(KafkaMessage message);

    public bool HasFound()
    {
      return _found;
    }
  }

  public class CheckForAssetSubscriptionCreateHandler : CheckForSubscriptionHandler
  {

    public CheckForAssetSubscriptionCreateHandler(Guid subscriptionUid, DateTime actionUtc)
      : base(subscriptionUid, actionUtc)
    {
    }
    public CreateAssetSubscriptionModel subscriptionEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("CreateAssetSubscriptionEvent"))
        return;

      subscriptionEvent = JsonConvert.DeserializeObject<CreateAssetSubscriptionModel>(message.Value);
      if (subscriptionEvent == null)
        return;

      if (subscriptionEvent.CreateAssetSubscriptionEvent.ActionUTC.Equals(_actionUtc) && subscriptionEvent.CreateAssetSubscriptionEvent.SubscriptionUID.Equals(_subscriptionUid))
        _found = true;
      if (_found == true)
        subscriptionEvent = JsonConvert.DeserializeObject<CreateAssetSubscriptionModel>(message.Value);
    }
  }

  public class CheckForAssetSubscriptionUpdateHandler : CheckForSubscriptionHandler
  {

    public CheckForAssetSubscriptionUpdateHandler(Guid subscriptionUid, DateTime actionUtc)
      : base(subscriptionUid, actionUtc)
    {
    }
    public UpdateAssetSubscriptionModel subscriptionEvent;
    public KafkaMessage message;

    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("UpdateAssetSubscriptionEvent"))
        return;

      subscriptionEvent = JsonConvert.DeserializeObject<UpdateAssetSubscriptionModel>(message.Value);
      if (subscriptionEvent == null)
        return;

      if (subscriptionEvent.UpdateAssetSubscriptionEvent.ActionUTC.Equals(_actionUtc)
                && subscriptionEvent.UpdateAssetSubscriptionEvent.SubscriptionUID.Equals(_subscriptionUid))
        _found = true;
      if (_found == true)
        subscriptionEvent = JsonConvert.DeserializeObject<UpdateAssetSubscriptionModel>(message.Value);

    }
  }

  public class CheckForProjectSubscriptionCreateHandler : CheckForSubscriptionHandler
  {

    public CheckForProjectSubscriptionCreateHandler(Guid subscriptionUid, DateTime actionUtc)
      : base(subscriptionUid, actionUtc)
    {
    }
    public CreateProjectSubscriptionModel subscriptionEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("CreateProjectSubscriptionEvent"))
        return;

      subscriptionEvent = JsonConvert.DeserializeObject<CreateProjectSubscriptionModel>(message.Value);
      if (subscriptionEvent == null)
        return;

      if (subscriptionEvent.CreateProjectSubscriptionEvent.ActionUTC.Equals(_actionUtc) && subscriptionEvent.CreateProjectSubscriptionEvent.SubscriptionUID.Equals(_subscriptionUid))
        _found = true;
      if (_found == true)
        subscriptionEvent = JsonConvert.DeserializeObject<CreateProjectSubscriptionModel>(message.Value);
    }
  }

  public class CheckForProjectSubscriptionUpdateHandler : CheckForSubscriptionHandler
  {

    public CheckForProjectSubscriptionUpdateHandler(Guid subscriptionUid, DateTime actionUtc)
      : base(subscriptionUid, actionUtc)
    {
    }
    public UpdateProjectSubscriptionModel subscriptionEvent;
    public KafkaMessage message;

    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("UpdateProjectSubscriptionEvent"))
        return;

      subscriptionEvent = JsonConvert.DeserializeObject<UpdateProjectSubscriptionModel>(message.Value);
      if (subscriptionEvent == null)
        return;

      if (subscriptionEvent.UpdateProjectSubscriptionEvent.ActionUTC.Equals(_actionUtc)
                && subscriptionEvent.UpdateProjectSubscriptionEvent.SubscriptionUID.Equals(_subscriptionUid))
        _found = true;
      if (_found == true)
        subscriptionEvent = JsonConvert.DeserializeObject<UpdateProjectSubscriptionModel>(message.Value);

    }
  }

  public class CheckForAssociateProjectSubscriptionHandler : CheckForSubscriptionHandler
  {

    public CheckForAssociateProjectSubscriptionHandler(Guid subscriptionUid, DateTime actionUtc)
      : base(subscriptionUid, actionUtc)
    {
    }
    public AssociateProjectSubscriptionModel subscriptionEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("AssociateProjectSubscriptionEvent"))
        return;

      subscriptionEvent = JsonConvert.DeserializeObject<AssociateProjectSubscriptionModel>(message.Value);
      if (subscriptionEvent == null)
        return;

      if (subscriptionEvent.AssociateProjectSubscriptionEvent.ActionUTC.Equals(_actionUtc) && subscriptionEvent.AssociateProjectSubscriptionEvent.SubscriptionUID.Equals(_subscriptionUid))
        _found = true;
      if (_found == true)
        subscriptionEvent = JsonConvert.DeserializeObject<AssociateProjectSubscriptionModel>(message.Value);
    }
  }

  public class CheckForDissociateProjectSubscriptionHandler : CheckForSubscriptionHandler
  {

    public CheckForDissociateProjectSubscriptionHandler(Guid subscriptionUid, DateTime actionUtc)
      : base(subscriptionUid, actionUtc)
    {
    }
    public DissociateProjectSubscriptionModel subscriptionEvent;
    public KafkaMessage message;

    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("DissociateProjectSubscriptionEvent"))
        return;

      subscriptionEvent = JsonConvert.DeserializeObject<DissociateProjectSubscriptionModel>(message.Value);
      if (subscriptionEvent == null)
        return;

      if (subscriptionEvent.DissociateProjectSubscriptionEvent.ActionUTC.Equals(_actionUtc)
                && subscriptionEvent.DissociateProjectSubscriptionEvent.SubscriptionUID.Equals(_subscriptionUid))
        _found = true;
      if (_found == true)
        subscriptionEvent = JsonConvert.DeserializeObject<DissociateProjectSubscriptionModel>(message.Value);

    }
  }

  public class CheckForCustomerSubscriptionCreateHandler : CheckForSubscriptionHandler
  {

    public CheckForCustomerSubscriptionCreateHandler(Guid subscriptionUid, DateTime actionUtc)
      : base(subscriptionUid, actionUtc)
    {
    }
    public CreateCustomerSubscriptionModel subscriptionEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("CreateCustomerSubscriptionEvent"))
        return;

      subscriptionEvent = JsonConvert.DeserializeObject<CreateCustomerSubscriptionModel>(message.Value);
      if (subscriptionEvent == null)
        return;

      if (subscriptionEvent.CreateCustomerSubscriptionEvent.ActionUTC.Equals(_actionUtc) && subscriptionEvent.CreateCustomerSubscriptionEvent.SubscriptionUID.Equals(_subscriptionUid))
        _found = true;
      if (_found == true)
        subscriptionEvent = JsonConvert.DeserializeObject<CreateCustomerSubscriptionModel>(message.Value);
    }
  }

  public class CheckForCustomerSubscriptionUpdateHandler : CheckForSubscriptionHandler
  {

    public CheckForCustomerSubscriptionUpdateHandler(Guid subscriptionUid, DateTime actionUtc)
      : base(subscriptionUid, actionUtc)
    {
    }
    public UpdateCustomerSubscriptionModel subscriptionEvent;
    public KafkaMessage message;

    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("UpdateCustomerSubscriptionEvent"))
        return;

      subscriptionEvent = JsonConvert.DeserializeObject<UpdateCustomerSubscriptionModel>(message.Value);
      if (subscriptionEvent == null)
        return;

      if (subscriptionEvent.UpdateCustomerSubscriptionEvent.ActionUTC.Equals(_actionUtc)
                && subscriptionEvent.UpdateCustomerSubscriptionEvent.SubscriptionUID.Equals(_subscriptionUid))
        _found = true;
      if (_found == true)
        subscriptionEvent = JsonConvert.DeserializeObject<UpdateCustomerSubscriptionModel>(message.Value);

    }
  }
}