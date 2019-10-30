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

  public class MegalodonService : IHostedService
  {

    private TagFile tagFile = new TagFile();
    private ISocketManager _socketManager;
    private Timer _timer;
    private int executionCount = 0;
    private readonly IConfigurationStore _config;
    private readonly ILogger _log;

    public MegalodonService(ILoggerFactory log, IConfigurationStore configStore, ISocketManager socketManager)
    {
      _log = log.CreateLogger<MegalodonService>();
      _config = configStore;
      _socketManager = socketManager;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      tagFile.CreateTagfileDictionary();

   //   _socketManager.TCIP = "127.0.0.1"; //todo config
  //    _socketManager.Port = 1500; // todo config
      _socketManager.Callback += new SocketManager.CallbackEventHandler(ListenerCallback);
      _socketManager.CreateSocket();
      _log.LogInformation($"Waiting connection on");
      _socketManager.ListenOnPort();
      _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(TagConstants.TAG_FILE_INTERVAL_SECS));

      return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      throw new NotImplementedException();
    }

    private void DoWork(object state)
    {
      // todo close tagfile
      executionCount++;
      if (tagFile.Parser.ReadyToWriteEpoch())
        ;// todo
      MegalodonLogger.LogInfo($"Timed Hosted Service is working. Count: {executionCount}");
    }

    public void EndProcess()
    {
      try
      {
        MegalodonLogger.LogInfo("Shutting down Megaldon");
        if (_socketManager.SocketListener.Connected)
        {
          _socketManager.SocketListener.Shutdown(SocketShutdown.Receive);
          _socketManager.SocketListener.Close();
          if (!tagFile.Parser.HeaderRequired)
            tagFile.WriteTagFileToDisk(); // close last tagfile
        }

      }
      catch (Exception exc)
      {
        MegalodonLogger.LogInfo($"Exception shutting down Megalodon. {exc.ToString()}");
      }
    }


    // Callback for socket listener
    void ListenerCallback(string packet, int mode)
    {
      if (mode == TagConstants.CALLBACK_MODE_PARSE)
      {
        tagFile.ParseText(packet);
        // DebugPacketHelper.(ref packet);
        _socketManager.HeaderRequired = tagFile.Parser.HeaderRequired;
      }
    }

  }

}
