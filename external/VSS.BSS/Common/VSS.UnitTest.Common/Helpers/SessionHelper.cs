using VSS.UnitTest.Common.Contexts;


using System.Linq;
using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common 
{
  public class SessionHelper 
  {
    public SessionContext GetContextFor(ActiveUser activeUser, bool populateWorkingSet = true, bool selected = false) 
    {
      new SessionContext().NHOpContext = ContextContainer.Current.OpContext;

      var user = (from u in ContextContainer.Current.OpContext.UserReadOnly 
                  join lang in ContextContainer.Current.OpContext.LanguageReadOnly on u.fk_LanguageID equals lang.ID
                  where u.ID == activeUser.fk_UserID select new {
        customerID = u.fk_CustomerID,
        name = u.Name,
        labelType = u.AssetLabelPreferenceType,
        language = lang.ISOName
    }).FirstOrDefault();

      var sessionContext = new SessionContext();
      sessionContext.SessionID = activeUser.SessionID;
      sessionContext.ActiveUserID = activeUser.ID;
      sessionContext.CustomerID = user.customerID;
      sessionContext.UserID = activeUser.fk_UserID;
      sessionContext.UserName = user.name;
      sessionContext.UserAssetLabelTypeID = user.labelType;
      sessionContext.UserLanguage = user.language == null ? "en-US" : user.language;

      if(populateWorkingSet)
        Helpers.WorkingSet.Populate(activeUser, selected);

      return sessionContext;
    }

    public bool Logout(ActiveUser au)
    {
      au.Expired = true;
      return ContextContainer.Current.OpContext.SaveChanges() > 0;
    }
  }
}