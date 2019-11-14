using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TagFiles.Common;
using TagFiles.Utils;
using VSS.Common.Abstractions.Configuration;
using System.Diagnostics;

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

      _log.LogInformation($"Configuring {TagConstants.APP_NAME}.");

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

      var _Folder = configStore.GetValueString("InstallFolder");
      if (string.IsNullOrEmpty(_Folder))
      {
        throw new ArgumentException($"Missing variable InstallFolder in appsettings.json");
      }
      else
      {
        // Make sure folder exists early for monitor
        // tagfile path
        var tmpPath = Path.Combine(_Folder, TagConstants.TAGFILE_FOLDER);
        Directory.CreateDirectory(tmpPath);
        tagFile.TagFileFolder = tmpPath;
        tmpPath = Path.Combine(tmpPath, TagConstants.TAGFILE_FOLDER_TOSEND);
        Directory.CreateDirectory(tmpPath);
        // logging path
        tmpPath = Path.Combine(_Folder, TagConstants.LOG_FOLDER);
        Directory.CreateDirectory(tmpPath);
      }

      var _seedLat = configStore.GetValueString("SeedLat");
      if (!string.IsNullOrEmpty(_seedLat))
      {
        tagFile.Parser.SeedLat =  TagUtils.ToRadians(Convert.ToDouble(_seedLat));
      }

      var _seedLon = configStore.GetValueString("SeedLon");
      if (!string.IsNullOrEmpty(_seedLon))
      {
        tagFile.Parser.SeedLon = TagUtils.ToRadians(Convert.ToDouble(_seedLon));
      }

      var _SerialOverride = configStore.GetValueString("SerialOverride");
      if (!string.IsNullOrEmpty(_SerialOverride))
      {
        tagFile.MachineSerial = _SerialOverride;
        tagFile.Parser.ForceSerial = _SerialOverride;
      }

      tagFile.MachineID = configStore.GetValueString("MachineName");

      tagFile.SendTagFilesDirect = configStore.GetValueBool("SendTagFilesDirect") ?? false;

      var fBOG = configStore.GetValueBool("ForceBOG") ?? false;
      tagFile.Parser.ForceBOG = fBOG;

      tagFile.Log = _log;
      tagFile.Parser.Log = _log;
      _log.LogInformation($"Socket Settings: {_TCIP}:{_Port}");

      var _TagFileIntervalSecs = configStore.GetValueString("TagFileIntervalSecs");
      if (!string.IsNullOrEmpty(_TagFileIntervalSecs))
      {
        tagFile.TagFileIntervalMilliSecs = Convert.ToInt32(_TagFileIntervalSecs) * 1000;
      }


    }

    /// <summary>
    /// Open Port
    /// </summary>
    private void SetupPort()
    {
      _log.LogInformation("Opening port");
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
      // This is highly unsafe
      _log.LogInformation("Closing Port.");
      _timer?.Change(Timeout.Infinite, 0);
      tagFile.ShutDown(); // close existing tagfile

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
        _log.LogError($"StopAsync. Exception shutting down {TagConstants.APP_NAME}. {exc.ToString()}");
      }
    }

    /// <summary>
    /// Started via a Windows service
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
      new Thread(() =>
      {
        _log.LogInformation($"Starting {TagConstants.APP_NAME}");
        SetupPort();
        _timer = new Timer(TimerDoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(TagConstants.TAG_FILE_MONITOR_SECS));
      }).Start();
      return Task.CompletedTask;
    }

    /// <summary>
    /// Windows service shutting down
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
      _log.LogInformation($"Stopping {TagConstants.APP_NAME}");
      StopPort();
      return Task.CompletedTask;
    }

    /// <summary>
    /// If we lose client connection go back into waiting for connection state
    /// </summary>
    private void RestartPort()
    {
      _log.LogInformation("Port Restart Required");
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
        _log.LogInformation("Restarting port due to lost connection");
        RestartPort();
        _socketManager.SendAck(); // in case they are waiting on response
      }

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
        _socketManager.HeaderRequired = tagFile.Parser.HeaderRequired;
      }
      else if (mode == TagConstants.CALLBACK_CONNECTION_MADE)
      {
        _log.LogInformation("Connection made by client");
        tagFile.EnableTagFileCreationTimer = true;
      }
    }


  }

}
