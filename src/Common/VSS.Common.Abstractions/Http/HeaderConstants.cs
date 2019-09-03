using System.Collections.Generic;

namespace VSS.Common.Abstractions.Http
{
  public static class HeaderConstants
  {
    public const string ACCEPT = "Accept";

    public const string AUTHORIZATION = "Authorization";

    public const string CACHE_CONTROL = "Cache-Control";

    public const string CONTENT_TYPE = "Content-Type";

    public const string ORIGIN = "Origin";

    public const string REQUEST_ID = "Request-ID";

    public const string X_JWT_ASSERTION = "X-JWT-Assertion";

    public const string X_REQUEST_ID = "X-Request-ID";

    public const string X_REQUESTED_WITH = "X-Requested-With";

    public const string X_VISION_LINK_CLEAR_CACHE = "X-VisionLink-ClearCache";

    public const string X_VISION_LINK_CUSTOMER_UID = "X-VisionLink-CustomerUID";

    public const string X_VISION_LINK_USER_UID = "X-VisionLink-UserUid";

    public const string X_VSS_REQUEST_ID = "X-VSS-Request-ID";

    /// <summary>
    /// Used to override the Service Discovery name from the UI or external applications
    /// To override the project-service value, you would need a header X-VSS-Override-project-service specified.
    /// </summary>
    public const string X_VSS_SERVICE_OVERRIDE_PREFIX = "X-VSS-Override-";

    /// <summary>
    /// If a header is prefixed with this, it will be considered a valid internal header
    /// </summary>
    public static List<string> InternalHeaderPrefix => new List<string> {X_VSS_SERVICE_OVERRIDE_PREFIX};

    /// <summary>
    ///   Headers to be kept when we make a request to any services developed by our team (e.g Filter / Project)
    /// </summary>
    public static List<string> InternalHeaders => new List<string>
    {
      X_VISION_LINK_CUSTOMER_UID,
      X_VISION_LINK_USER_UID,
      X_VISION_LINK_CLEAR_CACHE,
      X_REQUEST_ID,
      REQUEST_ID,
      X_VSS_REQUEST_ID,
      X_JWT_ASSERTION,
      AUTHORIZATION // Can be removed once we are using service discovery everywhere
    };

    /// <summary>
    ///   Headers to be kept when a request is made to a service outside of our Service Infrastructure (e.g Customer API / ALK
    ///   Maps)
    /// </summary>
    public static List<string> ExternalHeaders => new List<string>
    {
      X_VISION_LINK_CUSTOMER_UID,
      X_VISION_LINK_USER_UID,
      X_VISION_LINK_CLEAR_CACHE,
      AUTHORIZATION,
      X_REQUEST_ID,
      REQUEST_ID,
      X_VSS_REQUEST_ID
    };
  }
}
