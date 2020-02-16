using System.Security.Cryptography.X509Certificates;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Constants
{
  public class ControllerConstants
  {
    public const string AssetIdentifierControllerName = "AssetIdentifier";
    public const string DeviceIdentifierControllerName = "DeviceIdentifier";
    public const string CustomerIdentifierControllerName = "CustomerIdentifier";
    public const string ServiceIdentifierControllerName = "ServiceIdentifier";
    public const string CredentialControllerName = "Credential";

    public const string CustomerLookupControllerName = "CustomerLookup";
    public const string ServiceLookupControllerName = "ServiceLookup";
    public const string StoreLookupControllerName = "StoreLookup";
    public const string OemLookupControllerName = "OEMLookup";

    public const string QueryServiceGetActionName = "Retrieve";
    public const string QueryServicePostActionName = "Create";
    public const string QueryServicePutActionName = "Update";
  }
}
