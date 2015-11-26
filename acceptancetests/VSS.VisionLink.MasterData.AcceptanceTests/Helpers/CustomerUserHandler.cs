using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.KafkaWrapper.Interfaces;
using VSS.KafkaWrapper.Models;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.CustomerUserService;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Helpers
{
  public abstract class CheckForCustomerUserHandler : ISubscriber<KafkaMessage>
  {
    protected Guid _Customeruid;
    protected Guid _Useruid;
    protected DateTime _actionUtc;
    protected bool _found;

    public CheckForCustomerUserHandler(Guid Customeruid, Guid Useruid, DateTime actionUtc)
    {
      Init(Customeruid, Useruid, actionUtc);
    }

    public void Init(Guid Customeruid, Guid Useruid, DateTime actionUtc)
    {

      _Customeruid = Customeruid;
      _Useruid = Useruid;
      _actionUtc = actionUtc;
      _found = false;
    }

    public abstract void Handle(KafkaMessage message);

    public bool HasFound()
    {
      return _found;
    }
  }


  public class CheckForCustomerUserAssociateHandler : CheckForCustomerUserHandler
  {

    public CheckForCustomerUserAssociateHandler(Guid Customeruid, Guid Useruid, DateTime actionUtc)
      : base(Customeruid, Useruid, actionUtc)
    {
    }
    public AssociateCustomerUserModel CustomerEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("AssociateCustomerUserEvent"))
        return;

      CustomerEvent = JsonConvert.DeserializeObject<AssociateCustomerUserModel>(message.Value);
      if (CustomerEvent == null)
        return;

      if (CustomerEvent.AssociateCustomerUserEvent.ActionUTC.Equals(_actionUtc) &&
        CustomerEvent.AssociateCustomerUserEvent.CustomerUID.Equals(_Customeruid) &&
        CustomerEvent.AssociateCustomerUserEvent.UserUID.Equals(_Useruid))
        _found = true;
    
      CustomerEvent = JsonConvert.DeserializeObject<AssociateCustomerUserModel>(message.Value);

    }
  }

  public class CheckForCustomerUserDissociateHandler : CheckForCustomerUserHandler
  {

    public CheckForCustomerUserDissociateHandler(Guid Customeruid, Guid Assetuid, DateTime actionUtc)
      : base(Customeruid, Assetuid, actionUtc)
    {
    }
    public DissociateCustomerUserModel CustomerEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("DissociateCustomerUserEvent"))
        return;

      CustomerEvent = JsonConvert.DeserializeObject<DissociateCustomerUserModel>(message.Value);
      if (CustomerEvent == null)
        return;

      if (CustomerEvent.DissociateCustomerUserEvent.ActionUTC.Equals(_actionUtc) &&
        CustomerEvent.DissociateCustomerUserEvent.CustomerUID.Equals(_Customeruid) &&
        CustomerEvent.DissociateCustomerUserEvent.UserUID.Equals(_Useruid))
        _found = true;
    
      CustomerEvent = JsonConvert.DeserializeObject<DissociateCustomerUserModel>(message.Value);

    }
  }
}
