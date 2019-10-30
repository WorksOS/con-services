﻿using System.Net.Sockets;
using static TagFiles.SocketManager;

namespace TagFiles
{
  public interface ISocketManager
  {
    bool HeaderRequired
    {
      get;
      set;
    }

    Socket SocketListener
    {
      get;
      set;
    }

    void CreateSocket();
    void ListenOnPort();
    event CallbackEventHandler Callback;
    
  }
}
