using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VSS.WebApi.Common
{
  public abstract class BaseHostedService : IHostedService
  {
    private Task _executingTask;
    private CancellationTokenSource _cts;

    protected BaseHostedService(ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory)
    {
      Logger = loggerFactory.CreateLogger(GetType());
      ScopeFactory = scopeFactory;
    }

    protected ILogger Logger { get; }

    protected IServiceScopeFactory ScopeFactory { get; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      // Create a linked token so we can trigger cancellation outside of this token's cancellation
      _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

      // Store the task we're executing
      _executingTask = ExecuteAsync(_cts.Token);

      // If the task is completed then return it, otherwise it's running
      return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      // Stop called without start
      if (_executingTask == null)
      {
        return;
      }

      // Signal cancellation to the executing method
      _cts.Cancel();

      // Wait until the task completes or the stop token triggers
      await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));

      // Throw if cancellation triggered
      cancellationToken.ThrowIfCancellationRequested();
    }

    // Derived classes should override this and execute a long running method until 
    // cancellation is requested
    protected abstract Task ExecuteAsync(CancellationToken cancellationToken);
  }
}
