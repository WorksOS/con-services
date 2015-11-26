using System;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes;
using Newtonsoft.Json;
using VSS.KafkaWrapper.Interfaces;
using VSS.KafkaWrapper.Models;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.CustomerService;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Helpers
{

  public abstract class CheckForCustomerHandler : ISubscriber<KafkaMessage>
  {
    protected Guid _Customeruid;
    protected DateTime _actionUtc;
    protected bool _found;

    public CheckForCustomerHandler(Guid Customeruid, DateTime actionUtc)
    {
      Init(Customeruid, actionUtc);
    }

    public void Init(Guid Customeruid, DateTime actionUtc)
    {

      _Customeruid = Customeruid;
      _actionUtc = actionUtc;
      _found = false;
    }

    public abstract void Handle(KafkaMessage message);

    public bool HasFound()
    {
      return _found;
    }
  }

  public class CheckForCustomerCreateHandler : CheckForCustomerHandler
  {

    public CheckForCustomerCreateHandler(Guid Customeruid, DateTime actionUtc)
      : base(Customeruid, actionUtc)
    {
    }
    public CreateCustomerModel CustomerEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("CreateCustomerEvent"))
        return;

      CustomerEvent = JsonConvert.DeserializeObject<CreateCustomerModel>(message.Value);
      if (CustomerEvent == null)
        return;

      if (CustomerEvent.CreateCustomerEvent.ActionUTC.Equals(_actionUtc) &&
        CustomerEvent.CreateCustomerEvent.CustomerUID.Equals(
        _Customeruid))
        _found = true;
    
      CustomerEvent = JsonConvert.DeserializeObject<CreateCustomerModel>(message.Value);

    }
  }

  public class CheckForCustomerUpdateHandler : CheckForCustomerHandler
  {

    public CheckForCustomerUpdateHandler( Guid Customeruid, DateTime actionUtc)
      : base( Customeruid, actionUtc)
    {
    }
    public UpdateCustomerModel CustomerEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("UpdateCustomerEvent"))
        return;

      CustomerEvent = JsonConvert.DeserializeObject<UpdateCustomerModel>(message.Value);
      if (CustomerEvent == null)
        return;

      if (CustomerEvent.UpdateCustomerEvent.ActionUTC.Equals(_actionUtc) &&
        CustomerEvent.UpdateCustomerEvent.CustomerUID.Equals(
        _Customeruid))
        _found = true;
    

   
      CustomerEvent = JsonConvert.DeserializeObject<UpdateCustomerModel>(message.Value);

    }
  }

  public class CheckForCustomerDeleteHandler : CheckForCustomerHandler
  {

    public CheckForCustomerDeleteHandler(Guid Customeruid, DateTime actionUtc)
      : base(Customeruid, actionUtc)
    {
    }
    public DeleteCustomerModel CustomerEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("DeleteCustomerEvent"))
        return;

      CustomerEvent = JsonConvert.DeserializeObject<DeleteCustomerModel>(message.Value);
      if (CustomerEvent == null)
        return;

      if (CustomerEvent.DeleteCustomerEvent.ActionUTC.Equals(_actionUtc) &&
        CustomerEvent.DeleteCustomerEvent.CustomerUID.Equals(
        _Customeruid))
        _found = true;
    
      CustomerEvent = JsonConvert.DeserializeObject<DeleteCustomerModel>(message.Value);

    }
  }
}