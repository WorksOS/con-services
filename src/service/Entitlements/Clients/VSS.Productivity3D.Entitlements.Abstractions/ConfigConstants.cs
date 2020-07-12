namespace VSS.Productivity3D.Entitlements.Abstractions
{
  public class ConfigConstants
  {
    /// <summary>
    /// Do services check entitlements if it's enabled?
    /// This allows the services to ignore entitlements, and allow request in - this allows us to track down calls that need to happen without entitlements being enforced by the back end
    /// E.g Creating a project in WM needs to call project service validation endpoints
    /// We want to be able to turn off the services checking while leave the UI to validate the entitlement
    /// </summary>
    public const string ENABLE_ENTITLEMENTS_SERVICES_CONFIG_KEY = "ENABLE_ENTITLEMENTS_SERVICE_CHECKING";

    /// <summary>
    /// Is entitlement checking enabled anywhere? if this is false, then the entitlement service will allow all queries
    /// </summary>
    public const string ENABLE_ENTITLEMENTS_CONFIG_KEY = "ENABLE_ENTITLEMENTS_CHECKING";
    public const string ENTITLEMENTS_ACCEPT_EMAIL_KEY = "ENTITLEMENTS_ALLOWED_EMAILS";
  }
}
