namespace VSS.Productivity3D.Push.Abstractions
{
  public static class HubRoutes
  {
    public const string ASSET_STATUS_CLIENT = "/public/v1/assetstatus";

    public const string ASSET_STATUS_SERVER = "/internal/v1/assetstatus";

    public const string NOTIFICATIONS = "/notifications";

    public const string PROJECT_EVENT_CLIENT = "/public/v1/projectevent";

    public const string PROJECT_EVENT_SERVER = "/internal/v1/projectevent";
  }
}
