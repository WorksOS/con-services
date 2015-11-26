using System;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes;
using Newtonsoft.Json;
using VSS.KafkaWrapper.Interfaces;
using VSS.KafkaWrapper.Models;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Helpers
{

  public abstract class CheckForGeofenceHandler : ISubscriber<KafkaMessage>
  {
    protected Guid _useruid;
    protected Guid _geofenceuid;
    protected DateTime _actionUtc;
    protected bool _found;

    public CheckForGeofenceHandler(Guid useruid, Guid geofenceuid, DateTime actionUtc)
    {
      Init(useruid, geofenceuid, actionUtc);
    }

    public void Init(Guid useruid, Guid geofenceuid, DateTime actionUtc)
    {
      _useruid = useruid;
      _geofenceuid = geofenceuid;
      _actionUtc = actionUtc;
      _found = false;
    }

    public abstract void Handle(KafkaMessage message);

    public bool HasFound()
    {
      return _found;
    }
  }

  public class CheckForGeofenceCreateHandler : CheckForGeofenceHandler
  {

    public CheckForGeofenceCreateHandler(Guid useruid, Guid geofenceuid, DateTime actionUtc)
      : base(useruid, geofenceuid, actionUtc)
    {
    }
    public CreateGeofenceModel geofenceEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("CreateGeofenceEvent"))
        return;

      geofenceEvent = JsonConvert.DeserializeObject<CreateGeofenceModel>(message.Value);
      if (geofenceEvent == null)
        return;

      if (geofenceEvent.CreateGeofenceEvent.ActionUTC.Equals(_actionUtc) && geofenceEvent.CreateGeofenceEvent.UserUID.Equals(_useruid) && 
        geofenceEvent.CreateGeofenceEvent.GeofenceUID.Equals(
        _geofenceuid))
        _found = true;
    }

    public CreateGeofenceModel Response()
    {
      geofenceEvent = JsonConvert.DeserializeObject<CreateGeofenceModel>(message.Value);
      return geofenceEvent;
    }
  }

  public class CheckForGeofenceUpdateHandler : CheckForGeofenceHandler
  {

    public CheckForGeofenceUpdateHandler(Guid useruid, Guid geofenceuid, DateTime actionUtc)
      : base(useruid, geofenceuid, actionUtc)
    {
    }
    public UpdateGeofenceModel geofenceEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("UpdateGeofenceEvent"))
        return;

      geofenceEvent = JsonConvert.DeserializeObject<UpdateGeofenceModel>(message.Value);
      if (geofenceEvent == null)
        return;

      if (geofenceEvent.UpdateGeofenceEvent.ActionUTC.Equals(_actionUtc) && geofenceEvent.UpdateGeofenceEvent.UserUID.Equals(_useruid) &&
        geofenceEvent.UpdateGeofenceEvent.GeofenceUID.Equals(
        _geofenceuid))
        _found = true;
    
      geofenceEvent = JsonConvert.DeserializeObject<UpdateGeofenceModel>(message.Value);

    }
  }

  public class CheckForGeofenceDeletetHandler : CheckForGeofenceHandler
  {

    public CheckForGeofenceDeletetHandler(Guid useruid, Guid geofenceuid, DateTime actionUtc)
      : base(useruid, geofenceuid, actionUtc)
    {
    }
    public DeleteGeofenceModel geofenceEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("DeleteGeofenceEvent"))
        return;

      geofenceEvent = JsonConvert.DeserializeObject<DeleteGeofenceModel>(message.Value);
      if (geofenceEvent == null)
        return;

      if (geofenceEvent.DeleteGeofenceEvent.ActionUTC.Equals(_actionUtc) && geofenceEvent.DeleteGeofenceEvent.UserUID.Equals(_useruid) &&
        geofenceEvent.DeleteGeofenceEvent.GeofenceUID.Equals(
        _geofenceuid))
        _found = true;
    
      geofenceEvent = JsonConvert.DeserializeObject<DeleteGeofenceModel>(message.Value);

    }
  }
}