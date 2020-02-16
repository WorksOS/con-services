using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.Hosted.VLCommon;
using System.Threading;
using System.Data.Entity.Core.EntityClient;
using System.Configuration;
using System.Data.Entity.Core.Objects;

namespace VSS.Hosted.VLCommon
{
  public class SessionContext
  {
    public static SessionContext SystemContext
    {
      get{ return new SessionContext { SessionID=Guid.NewGuid().ToString("N")}; }
    }

    public string SessionID { get; set; }
    public long ActiveUserID { get; set; }
    public long? CustomerID { get; set; }
    public string CustomerName { get; set; }
    public string MapAPIProvider { get; set; }
    public int CustomerTypeID { get; set; }
    public long? UserID { get; set; }
    public int? UserAssetLabelTypeID { get; set; }
    public string UserName { get; set; }
    public string PasswordHash { get; set; } // pwdHash + Salt are useful for validating a pwd without incuring a DB read.
    public string UserSalt { get; set; }
    public string UserFirstName { get; set; }
    public string UserLastName { get; set; }
    public string UserPhone { get; set; }
    public string UserEmail { get; set; }
    public string UserTimeZone { get; set; }
    public string UserLanguage { get; set; }
    public int UserLanguageID { get; set; }
    public int? UserUnits { get; set; }
    public byte? UserLocationDisplayType { get; set; }
    public int? meterLabelPreferenceType { get; set; }
    public TemperatureUnitEnum TemperatureUnit { get; set; }
    public PressureUnitEnum PressureUnit { get; set; }
    public DateTime? PasswordExpiery { get; set; }
    public DateTime lastLogin { get; set; }
    public bool IsVerificationReminder { get; set; }
    public int VerificationRemainingDays { get; set; }
    public bool IsVerificationPending { get; set; }

    public override string ToString()
    {
      StringBuilder builder = new StringBuilder();
      builder.AppendFormat("ActiveUserID({0}),", this.ActiveUserID);
      builder.AppendFormat("CustomerID({0}),", this.CustomerID ?? 0);
      builder.AppendFormat("UserName('{0}'),", this.UserName);
      builder.AppendFormat("SessionID({0}),", this.SessionID);

      return builder.ToString();
    }

    #region Entity Model
    public INH_OP NHOpContext
    {
      get
      {
        if (nhOpContext == null)
        {
          nhOpContext = ObjectContextFactory.NewNHContext<INH_OP>();
          nhOpThreadId = Thread.CurrentThread.ManagedThreadId;
        }
        if (nhOpThreadId != Thread.CurrentThread.ManagedThreadId)
          throw new ThreadStateException("Attempt to use NHOpContext across threads");
        return nhOpContext;
      }
      set
      {
        nhOpContext = value;
        nhOpThreadId = Thread.CurrentThread.ManagedThreadId;
      }
    }  

    public void DisposeNHOpContext()
    {
      if (nhOpContext != null)
      {
        nhOpContext.Dispose();
        nhOpContext = null;
        nhOpThreadId = 0;
      }
    }

    [ThreadStatic]
    private static INH_OP nhOpContext = null;
    [ThreadStatic]
    private static int nhOpThreadId = 0;
    [ThreadStatic]
    private static int nhRawThreadId = 0;
    [ThreadStatic]
    private static int nhRptThreadId = 0;

    #endregion
  }
}
