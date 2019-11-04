using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TagFiles.Common;
using VSS.Common.Abstractions.Configuration;

namespace TagFiles.Interface
{
  /// <summary>
  /// Megalodon controller started via as a windows service
  /// </summary>
  public class MegalodonService : IHostedService
  {

    private TagFile tagFile = new TagFile();
    private ISocketManager _socketManager;
    private Timer _timer;
    private int executionCount = 0;
    private readonly IConfigurationStore _config;
    private readonly ILogger _log;

    /// <summary>
    /// Configure Megalodon
    /// </summary>
    /// <param name="log"></param>
    /// <param name="configStore"></param>
    /// <param name="socketManager"></param>
    public MegalodonService(ILoggerFactory log, IConfigurationStore configStore, ISocketManager socketManager )
    {
      _log = log.CreateLogger<MegalodonService>();
      _config = configStore;
      _socketManager = socketManager;
      // Check we have important settings
      var _TCIP = configStore.GetValueString("TCIP");
      if (string.IsNullOrEmpty(_TCIP))
      {
        throw new ArgumentException($"Missing variable TCIP in appsettings.json");
      }
      var _Port = configStore.GetValueString("Port");
      if (string.IsNullOrEmpty(_Port))
      {
        throw new ArgumentException($"Missing variable Port in appsettings.json");
      }

      var _Folder = configStore.GetValueString("TagfileFolder");
      if (string.IsNullOrEmpty(_Folder))
      {
        throw new ArgumentException($"Missing variable TagfileFolder in appsettings.json");
      }
      else
        tagFile.TagfileFolder = _Folder;

      tagFile.MachineSerial = configStore.GetValueString("Serial");
      tagFile.MachineID = configStore.GetValueString("MachineName");
      tagFile.SendTagFilesToProduction = configStore.GetValueBool("SendTagFilesToProduction") ?? false;
      tagFile.Log = _log;
      DualLog($"Socket Settings: {_TCIP}:{_Port}");
    }

    /// <summary>
    /// Open Port
    /// </summary>
    private void SetupPort()
    {
      DualLog("Opening port");
      _socketManager.Callback += new SocketManager.CallbackEventHandler(SocketManagerCallback);
      _socketManager.CreateSocket();
      _log.LogInformation($"Waiting connection on");
      _socketManager.ListenOnPort();
    }

    /// <summary>
    /// Close Port
    /// </summary>
    private void StopPort()
    {
      DualLog("Closing Port.");

      _timer?.Change(Timeout.Infinite, 0);

      try
      {
        if (_socketManager.SocketListener.Connected)
        {
          _socketManager.SocketListener.Shutdown(SocketShutdown.Receive);
          _socketManager.SocketListener.Close();
        }

      }
      catch (Exception exc)
      {
        _log.LogError($"StopAsync. Exception shutting down Megalodon. {exc.ToString()}");
      }
    }

    /// <summary>
    /// Started via a Windows service
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
      DualLog("Starting Megalodon Service");
      SetupPort();
      _timer = new Timer(TimerDoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(TagConstants.TAG_FILE_MONITOR_SECS));
      return Task.CompletedTask;
    }

    /// <summary>
    /// Windows service shutting down
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
      DualLog("Stopping Megalodon Service");
      StopPort();
      return Task.CompletedTask;
    }

    /// <summary>
    /// If we lose client connection go back into waiting for connection state
    /// </summary>
    private void RestartPort()
    {
      DualLog("Port Restart Required");
      _socketManager.ListenOnPort();
      _timer?.Change(TimeSpan.Zero,TimeSpan.FromSeconds(TagConstants.TAG_FILE_MONITOR_SECS));
    }

    /// <summary>
    /// Monitors port for lost connections
    /// </summary>
    /// <param name="state"></param>
    private void TimerDoWork(object state)
    {
      
      if (_socketManager.PortRestartNeeded)
      {
        _socketManager.PortRestartNeeded = false;
        RestartPort();
      }

      /* Debug Only
      executionCount++;
      var msg = $"Megalodon service Timed Hosted Service is working. Count: {executionCount}";
      Console.WriteLine(msg);
      _log.LogInformation(msg);
      */

    }

    /// <summary>
    ///  Manage SocketManager messages here
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="mode"></param>
    void SocketManagerCallback(string packet, int mode)
    {
      if (mode == TagConstants.CALLBACK_PARSE_PACKET)
      {
        tagFile.ParseText(packet);
        // DebugPacketHelper.(ref packet);
        _socketManager.HeaderRequired = tagFile.Parser.HeaderRequired;
      }
      else if (mode == TagConstants.CALLBACK_CONNECTION_MADE)
      {
        tagFile.EnableTagFileCreationTimer = true;
      }
    }

    /// <summary>
    /// Outputs to console and serilog
    /// </summary>
    /// <param name="msg"></param>
    private void DualLog(string msg, bool error = false)
    {
      // Console.Write(msg); turns out serilog goes to console anyway
      // leaving as might be handy for alternative debug logging
      if (error)
        _log.LogError(msg);
      else
        _log.LogInformation(msg);
    }

  }

}
