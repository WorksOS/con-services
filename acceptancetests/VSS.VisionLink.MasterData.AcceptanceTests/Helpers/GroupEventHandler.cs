using System;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes;
using Newtonsoft.Json;
using VSS.KafkaWrapper.Interfaces;
using VSS.KafkaWrapper.Models;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Helpers
{

  public abstract class CheckForGroupHandler : ISubscriber<KafkaMessage>
  {
    protected Guid _groupUid;
    protected Guid _userUid;
    protected DateTime _actionUtc;
    protected bool _found;

    public CheckForGroupHandler(Guid GroupUid, Guid UserUid, DateTime actionUtc)
    {
      Init(GroupUid, UserUid, actionUtc);
    }

    public void Init(Guid groupUid, Guid userUid, DateTime actionUtc)
    {
      _groupUid = groupUid;
        _userUid = userUid;
      _actionUtc = actionUtc;
      _found = false;
    }

    public abstract void Handle(KafkaMessage message);

    public bool HasFound()
    {
      return _found;
    }
  }

  public class CheckForGroupCreateHandler : CheckForGroupHandler
  {

    public CheckForGroupCreateHandler(Guid groupUid, Guid userUid, DateTime actionUtc)
      : base(groupUid, userUid, actionUtc)
    {
    }
    public CreateGroupServiceModel groupEvent;
    public KafkaMessage message;
    public override void Handle(KafkaMessage message)
    {
      if (!message.Value.Contains("CreateGroupEvent"))
        return;

      groupEvent = JsonConvert.DeserializeObject<CreateGroupServiceModel>(message.Value);
      if (groupEvent == null)
        return;

      if (groupEvent.CreateGroupEvent.ActionUTC.Equals(_actionUtc) && groupEvent.CreateGroupEvent.GroupUID.Equals(_groupUid)
          && groupEvent.CreateGroupEvent.UserUID.Equals(_userUid))
        _found = true;
    
      groupEvent = JsonConvert.DeserializeObject<CreateGroupServiceModel>(message.Value);

    }
  }
  public class CheckForGroupUpdateHandler : CheckForGroupHandler
  {

      public CheckForGroupUpdateHandler(Guid groupUid, Guid userUid, DateTime actionUtc)
          : base(groupUid, userUid, actionUtc)
      {
      }
      public UpdateGroupServiceModel groupEvent;
      public KafkaMessage message;
      public override void Handle(KafkaMessage message)
      {
          if (!message.Value.Contains("UpdateGroupEvent"))
              return;

          groupEvent = JsonConvert.DeserializeObject<UpdateGroupServiceModel>(message.Value);
          if (groupEvent == null)
              return;

          if (groupEvent.UpdateGroupEvent.ActionUTC.Equals(_actionUtc) && groupEvent.UpdateGroupEvent.GroupUID.Equals(_groupUid)
              && groupEvent.UpdateGroupEvent.UserUID.Equals(_userUid))
              _found = true;

          groupEvent = JsonConvert.DeserializeObject<UpdateGroupServiceModel>(message.Value);

      }
  }


  public class CheckForGroupDeleteHandler : CheckForGroupHandler
  {

      public CheckForGroupDeleteHandler(Guid groupUid, Guid userUid, DateTime actionUtc)
          : base(groupUid, userUid, actionUtc)
      {
      }
      public DeleteGroupServiceModel groupEvent;
      public KafkaMessage message;
      public override void Handle(KafkaMessage message)
      {
          if (!message.Value.Contains("DeleteGroupEvent"))
              return;

          groupEvent = JsonConvert.DeserializeObject<DeleteGroupServiceModel>(message.Value);
          if (groupEvent == null)
              return;

          if (groupEvent.DeleteGroupEvent.ActionUTC.ToString("yyyyMMddhhmmss").Equals(_actionUtc.ToString("yyyyMMddhhmmss")) 
              && groupEvent.DeleteGroupEvent.GroupUID.Equals(_groupUid)
              && groupEvent.DeleteGroupEvent.UserUID.Equals(_userUid))
              _found = true;
         
          groupEvent = JsonConvert.DeserializeObject<DeleteGroupServiceModel>(message.Value);

      }
  }
}