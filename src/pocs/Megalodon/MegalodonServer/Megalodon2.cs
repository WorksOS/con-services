using System;
using System.Net.Sockets;
using System.Threading;
using TagFiles;
using TagFiles.Common;

namespace MegalodonServer
{
  public class Megalodon2 : IMegalodon2
  {

    private TagFile tagFile = new TagFile();
    private SocketManager socketManager;
    private Timer _timer;
    private int executionCount = 0;

    public void StartProcess(string ip, int port)
    {
      tagFile.CreateTagfileDictionary();
      if (socketManager == null)
        socketManager = new SocketManager();
      socketManager.TCIP = ip;
      socketManager.Port = port;
      socketManager.Callback += new SocketManager.CallbackEventHandler(ListenerCallback);
      socketManager.CreateSocket();
      MegalodonLogger.LogInfo($"Waiting connection on {socketManager.TCIP}:{socketManager.Port} ...");
      socketManager.ListenOnPort();
      _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(TagConstants.TAG_FILE_INTERVAL_SECS));

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
        if (socketManager.SocketListener.Connected)
        {
          socketManager.SocketListener.Shutdown(SocketShutdown.Receive);
          socketManager.SocketListener.Close();
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
        socketManager.HeaderRequired = tagFile.Parser.HeaderRequired;
      }
    }

  }
}
