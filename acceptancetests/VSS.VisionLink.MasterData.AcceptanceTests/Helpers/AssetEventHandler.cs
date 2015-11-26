using System;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes;
using Newtonsoft.Json;
using VSS.KafkaWrapper.Interfaces;
using VSS.KafkaWrapper.Models;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Helpers
{

  public abstract class CheckForAssetHandler : ISubscriber<KafkaMessage>
  {
    protected Guid _assetUid;
    protected DateTime _actionUtc;
    protected bool _found;

    public CheckForAssetHandler(Guid assetUid, DateTime actionUtc)
    {
      Init(assetUid, actionUtc);
    }

    public void Init(Guid assetUid, DateTime actionUtc)
    {
      _assetUid = assetUid;
      _actionUtc = actionUtc;
      _found = false;
    }

    public abstract void Handle(KafkaMessage message);

    public bool HasFound()
    {
      return _found;
    }
  }

  public class CheckForAssetCreateHandler : CheckForAssetHandler
  {

    public CheckForAssetCreateHandler(Guid assetUid, DateTime actionUtc)
      : base(assetUid, actionUtc)
    {
    }
    public CreateAssetModel assetEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("CreateAssetEvent"))
        return;

      assetEvent = JsonConvert.DeserializeObject<CreateAssetModel>(message.Value);
      if (assetEvent == null)
        return;

      if (assetEvent.CreateAssetEvent.ActionUTC.Equals(_actionUtc) && assetEvent.CreateAssetEvent.AssetUID.Equals(_assetUid))
        _found = true;
      assetEvent = JsonConvert.DeserializeObject<CreateAssetModel>(message.Value);
    }  
  }

  public class CheckForAssetUpdateHandler : CheckForAssetHandler
  {

    public CheckForAssetUpdateHandler(Guid assetUid, DateTime actionUtc)
      : base(assetUid, actionUtc)
    {
    }
    public UpdateAssetModel assetEvent;
    public KafkaMessage message;

    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("UpdateAssetEvent"))
        return;

       assetEvent = JsonConvert.DeserializeObject<UpdateAssetModel>(message.Value);
      if (assetEvent == null)
        return;

      if (assetEvent.UpdateAssetEvent.ActionUTC.Equals(_actionUtc) && assetEvent.UpdateAssetEvent.AssetUID.Equals(_assetUid))
        _found = true;   
      assetEvent = JsonConvert.DeserializeObject<UpdateAssetModel>(message.Value);

    }

  }

  public class CheckForAssetDeleteHandler : CheckForAssetHandler
  {
    public DeleteAssetModel assetEvent;
    public KafkaMessage message;
    public CheckForAssetDeleteHandler(Guid assetUid, DateTime actionUtc)
      : base(assetUid, actionUtc)
    {
    }

    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("DeleteAssetEvent"))
        return;

      assetEvent = JsonConvert.DeserializeObject<DeleteAssetModel>(message.Value);
      if (assetEvent == null)
        return;

      if (assetEvent.DeleteAssetEvent.ActionUTC.ToString("yyyyMMddhhmmss").Equals(_actionUtc.ToString("yyyyMMddhhmmss")) && assetEvent.DeleteAssetEvent.AssetUID.Equals(_assetUid))
        _found = true;
    
      assetEvent = JsonConvert.DeserializeObject<DeleteAssetModel>(message.Value);
    }
  }
}