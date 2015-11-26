using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.KafkaWrapper.Interfaces;
using VSS.KafkaWrapper.Models;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.CustomerAssetService;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Helpers
{
  public abstract class CheckForCustomerAssetHandler : ISubscriber<KafkaMessage>
  {
    protected Guid _Customeruid;
    protected Guid _Assetuid;
    protected DateTime _actionUtc;
    protected bool _found;

    public CheckForCustomerAssetHandler(Guid Customeruid, Guid Assetuid, DateTime actionUtc)
    {
      Init(Customeruid,Assetuid,actionUtc);
    }

    public void Init(Guid Customeruid, Guid Assetuid, DateTime actionUtc)
    {

      _Customeruid = Customeruid;
      _Assetuid = Assetuid;
      _actionUtc = actionUtc;
      _found = false;
    }

    public abstract void Handle(KafkaMessage message);

    public bool HasFound()
    {
      return _found;
    }
  }


  public class CheckForCustomerAssetAssociateHandler : CheckForCustomerAssetHandler
  {

    public CheckForCustomerAssetAssociateHandler(Guid Customeruid, Guid Assetuid, DateTime actionUtc)
      : base(Customeruid, Assetuid, actionUtc)
    {
    }
    public AssociateCustomerAssetModel CustomerEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("AssociateCustomerAssetEvent"))
        return;

      CustomerEvent = JsonConvert.DeserializeObject<AssociateCustomerAssetModel>(message.Value);
      if (CustomerEvent == null)
        return;

      if (CustomerEvent.AssociateCustomerAssetEvent.ActionUTC.Equals(_actionUtc) &&
        CustomerEvent.AssociateCustomerAssetEvent.CustomerUID.Equals(_Customeruid) &&
        CustomerEvent.AssociateCustomerAssetEvent.AssetUID.Equals(_Assetuid)) 
        _found = true;
    
      CustomerEvent = JsonConvert.DeserializeObject<AssociateCustomerAssetModel>(message.Value);

    }
  }

  public class CheckForCustomerAssetDissociateHandler : CheckForCustomerAssetHandler
  {

    public CheckForCustomerAssetDissociateHandler(Guid Customeruid, Guid Assetuid, DateTime actionUtc)
      : base(Customeruid, Assetuid, actionUtc)
    {
    }
    public DissociateCustomerAssetModel CustomerEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("DissociateCustomerAssetEvent"))
        return;

      CustomerEvent = JsonConvert.DeserializeObject<DissociateCustomerAssetModel>(message.Value);
      if (CustomerEvent == null)
        return;

      if (CustomerEvent.DissociateCustomerAssetEvent.ActionUTC.Equals(_actionUtc) &&
        CustomerEvent.DissociateCustomerAssetEvent.CustomerUID.Equals(_Customeruid)&&
        CustomerEvent.DissociateCustomerAssetEvent.AssetUID.Equals(_Assetuid)) 
        _found = true;
    
      CustomerEvent = JsonConvert.DeserializeObject<DissociateCustomerAssetModel>(message.Value);

    }
  }

}
