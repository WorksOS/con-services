using System.Collections.Generic;
using System.Threading.Tasks;

namespace VSS.Productivity3D.Push.Abstractions
{
  public interface IHubClient
  {
    void SetupHeaders(IDictionary<string, string> headers);

    /// <summary>
    /// Is the Client connected to the SignalR Hub
    /// </summary>
    bool Connected { get; }

    /// <summary>
    /// Is the hub attempting to connect already?
    /// </summary>
    bool IsConnecting { get; }

    /// <summary>
    /// Disconnect from the Hub.
    /// </summary>
    Task Disconnect();

    /// <summary>
    /// Start the connection to the Hub, will trigger a background task to retry the connection until it connects
    /// Will also reconnect on a non-planned disconnect.
    /// </summary>
    Task Connect();

    /// <summary>
    /// Start the connection to the Hub, will work on same task and retry the connection until it connects
    /// Will also reconnect on a non-planned disconnect.
    /// </summary>
    Task ConnectAndWait(); // todoJeannie used for testing only. fix this? a retry on connection will start a new Task.Factory task
  }
}
