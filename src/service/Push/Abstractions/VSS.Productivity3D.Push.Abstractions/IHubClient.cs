using System.Threading.Tasks;

namespace VSS.Productivity3D.Push.Abstractions
{
  public interface IHubClient
  {
    /// <summary>
    /// Is the Client connected to the SignalR Hub
    /// </summary>
    bool Connected { get; }

    /// <summary>
    /// Disconnect from the Hub.
    /// </summary>
    Task Disconnect();

    /// <summary>
    /// Start the connection to the Hub, will trigger a background task to retry the connection until it connects
    /// Will also reconnect on a non-planned disconnect.
    /// </summary>
    Task Connect();
  }
}