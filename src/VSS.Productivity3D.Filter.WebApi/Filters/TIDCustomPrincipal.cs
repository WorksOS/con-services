using System.Security.Claims;

namespace VSS.Productivity3D.Filter.WebApi.Filters
{
  /// <summary>
  /// 
  /// </summary>
  public class TIDCustomPrincipal : ClaimsPrincipal
  {
    /// <inheritdoc />
    /// <summary>
    /// Initializes a new instance of the <see cref="T:VSS.Productivity3D.Filter.WebApi.Filters.TIDCustomPrincipal" /> class.
    /// </summary>
    /// <param name="identity">The identity.</param>
    /// <param name="customerUid">The customer uid.</param>
    /// <param name="emailAddress">The email address.</param>
    /// <param name="customerName"></param>
    /// <param name="isApplication">if set to <c>true</c> [is application].</param>
    public TIDCustomPrincipal(ClaimsIdentity identity, string customerUid, string emailAddress, string customerName,
      bool isApplication = false) : base(identity)
    {
      CustomerUid = customerUid;
      EmailAddress = emailAddress;
      this.isApplication = isApplication;
      CustomerName = customerName;
    }

    /// <summary>
    /// Gets the name of the customer.
    /// </summary>
    /// <value>
    /// The name of the customer.
    /// </value>
    public string CustomerName { get; private set; }

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
    public bool isApplication { get; private set; } = false;

  }
}