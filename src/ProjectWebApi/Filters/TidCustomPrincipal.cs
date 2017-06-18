using System.Security.Claims;

namespace ProjectWebApi.Filters
{
  /// <summary>
  /// 
  /// </summary>
  public class TIDCustomPrincipal : ClaimsPrincipal
  {
 
    /// <summary>
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="customerUid"></param>
    /// <param name="emailAddress"></param>
    public TIDCustomPrincipal(ClaimsIdentity identity, string customerUid, string emailAddress) : base(identity)
    {
      CustomerUid = customerUid;
      EmailAddress = emailAddress;
    }

    /// <summary>
    /// 
    /// </summary>
    public string CustomerUid { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public string EmailAddress { get; private set; }

  }
}