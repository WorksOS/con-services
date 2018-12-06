namespace VSS.Productivity3D.Push.Abstractions
{
  /// <summary>
  /// This is used for Clients to the INotificationHub, by implemented both the ability to send, and the ability to connect to a hub server
  /// </summary>
  public interface INotificationHubClient : INotificationHub, IHubClient
  {

  }
}