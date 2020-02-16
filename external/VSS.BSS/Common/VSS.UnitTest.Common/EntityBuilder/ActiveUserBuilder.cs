using System;
using VSS.UnitTest.Common.Contexts;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common
{
  public class ActiveUserBuilder 
  {
    private string _sessionId = Guid.NewGuid().ToString();
    private User _user;
    private User _impersonatedUser = null;
    private DateTime _lastActivityUTC;

    public ActiveUserBuilder SessionId(string sessionId)
    {
      _sessionId = sessionId;
      return this;
    }

    public ActiveUserBuilder ForUser(User user)
    {
      _user = user;
      return this;
    }

    public ActiveUserBuilder WithLastActivity(DateTime lastActivity)
    {
      _lastActivityUTC = lastActivity;
      return this;
    }

    public ActiveUserBuilder ForImpersonatedUser(User impersonatedUser)
    {
      _impersonatedUser = impersonatedUser;
      return this;
    }

    public ActiveUser Build()
    {
      var activeUser = new ActiveUser();

      activeUser.ID = IdGen.GetId();
      activeUser.SessionID = _sessionId;
      activeUser.Expired = false;
      activeUser.fk_UserID = _user.ID;
      activeUser.LastActivityUTC = _lastActivityUTC;
      if (_impersonatedUser != null)
        activeUser.fk_ImpersonatedUserID = _impersonatedUser.ID;

      return activeUser;
    }

    public ActiveUser Save()
    {
      var activeUser = Build();

      //ContextContainer.Current.OpContext.User.AddObject(_user);
      //ContextContainer.Current.OpContext.User.AddObject(_impersonatedUser);
      ContextContainer.Current.OpContext.ActiveUser.AddObject(activeUser);
      ContextContainer.Current.OpContext.SaveChanges();

      return activeUser;
    }
  }
}
