using System.Security.Claims;

namespace VSS.WebApi.Common
{
  /// <summary>
  /// Principal for all services that use TID authentication. Extend this class to add functionality for a specific service.
  /// </summary>
  public class TIDCustomPrincipal : ClaimsPrincipal
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TIDCustomPrincipal"/> class.
    /// </summary>
    /// <param name="identity">The identity.</param>
    /// <param name="customerUid">The customer uid.</param>
    /// <param name="customerName">The customer name</param>
    /// <param name="userName">The user email address or application name</param>
    /// <param name="isApplication">if set to <c>true</c> [is application].</param>
    public TIDCustomPrincipal(ClaimsIdentity identity, string customerUid, string customerName, string userName, bool isApplication = false, string tpaasApplicationName = "") : base(identity)
    {
      CustomerUid = customerUid;
      CustomerName = customerName;
      UserEmail = userName;
      IsApplication = isApplication;
      TpaasApplicationName = tpaasApplicationName;
    }

    /// <summary>
    /// Get TPaas Application name
    /// </summary>
    public string TpaasApplicationName { get; private set; }

    /// <summary>
    /// Gets the customer uid.
    /// </summary>
    /// <value>
    /// The customer uid.
    /// </value>
    public string CustomerUid { get; private set; }

    /// <summary>
    /// Gets the customer name.
    /// </summary>
    /// <value>
    /// The customer name.
    /// </value>
    public string CustomerName { get; private set; }

    /// <summary>
    /// Gets the user email address or application name.
    /// </summary>
    /// <value>
    /// The user email address or application name.
    /// </value>
    public string UserEmail { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance is application.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is application; otherwise, <c>false</c>.
    /// </value>
    public bool IsApplication { get; private set; } = false;

  }
}