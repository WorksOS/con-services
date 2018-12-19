using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using VSS.Productivity3D.Push.Abstractions;

namespace VSS.Productivity3D.Push.Clients
{
  /// <summary>
  /// Wrap a IHubClient in a IHostedSerivce allowing it to be injected via DI (IServiceCollection).
  /// As well as started and stopped with the ASP Core Service lifecycle
  /// </summary>
  /// <typeparam name="T">The Type to be used, should be added to the IServiceCollection as well (to allow for DI)</typeparam>
  public class HostedClientService<T> : IHostedService where T : class, IHubClient
  {
    private T t;

    public T Client => t;

    public HostedClientService(T t)
    {
      this.t = t;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      return t.Connect();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      return t.Disconnect();
    }
  }
}