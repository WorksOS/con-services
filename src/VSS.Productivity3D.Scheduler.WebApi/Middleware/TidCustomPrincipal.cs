using System.Security.Claims;

namespace VSS.Peoductivity3D.Scheduler.WebApi.Middleware
{
  /// <summary>
  /// 
  /// </summary>
  public class TIDCustomPrincipal : ClaimsPrincipal
  {

    /// <summary>
    /// Initializes a new instance of the <see cref="TIDCustomPrincipal"/> class.
    /// </summary>
    /// <param name="identity">The identity.</param>
    /// <param name="customerUid">The customer uid.</param>
    /// <param name="emailAddress">The email address.</param>
    /// <param name="isApplication">if set to <c>true</c> [is application].</param>
    public TIDCustomPrincipal(ClaimsIdentity identity, string customerUid, string emailAddress, bool isApplication = false) : base(identity)
    {
      CustomerUid = customerUid;
      EmailAddress = emailAddress;
      IsApplication = isApplication;
    }

    /// <summary>
    /// Gets the customer uid.
    /// </summary>
    /// <value>
    /// The customer uid.
    /// </value>
    public string CustomerUid { get; private set; }

    /// <summary>
    /// Gets the email address.
    /// </summary>
    /// <value>
    /// The email address.
    /// </value>
    public string EmailAddress { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance is application.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is application; otherwise, <c>false</c>.
    /// </value>
    public bool IsApplication { get; private set; } = false;

  }
}