using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TagFiles.Common;
using TagFiles.Utils;
using VSS.Common.Abstractions.Configuration;
using Microsoft.Extensions.Logging;

namespace TagFiles
{
  public class SocketManager : ISocketManager
  {

   // public string StatusMessage;

    private int port = 1500;
    private string tcip = "127.0.0.1";

    //public string TxtRecv;

    private bool _HeaderRequired = true;
    public bool HeaderRequired  // read-write instance property
    {
      get => _HeaderRequired;
      set => _HeaderRequired = value;
    }

    public bool _PortRestartNeeded = false;
    public bool PortRestartNeeded 
    {
      get => _PortRestartNeeded;
      set => _PortRestartNeeded = value;
    }

    public bool DumpContent = false;

    SocketPermission permission;

    private Socket _SocketListener;
    public Socket SocketListener
    {
      get => _SocketListener;
      set => _SocketListener = value;

    }


    IPEndPoint ipEndPoint;
    Socket handler;

    private readonly ILogger _log;
    private readonly IConfigurationStore _config;


    public delegate void CallbackEventHandler(string something, int mode);
    public event CallbackEventHandler Callback;
    public static ManualResetEvent allDone = new ManualResetEvent(false);


    public SocketManager(ILoggerFactory log, IConfigurationStore configStore)
    {
      _log = log.CreateLogger<SocketManager>();
      _config = configStore;

      var _TCIP = configStore.GetValueString("TCIP");
      if (string.IsNullOrEmpty(_TCIP))
      {
        throw new ArgumentException($"Missing variable TCIP in appsettings.json");
      }
      else
        tcip = _TCIP;

      var _Port = configStore.GetValueString("Port");
      if (string.IsNullOrEmpty(_Port))
      {
        throw new ArgumentException($"Missing variable Port in appsettings.json");
      }
      else
        port = Int32.Parse(_Port);

    }

    public void CreateSocket()
    {
      try
      {
        _PortRestartNeeded = false;
        // Creates one SocketPermission object for access restrictions
        permission = new SocketPermission(
        NetworkAccess.Accept,     // Allowed to accept connections 
        TransportType.Tcp,        // Defines transport types 
        "",                       // The IP addresses of local host 
        SocketPermission.AllPorts // Specifies all ports 
        );

        // Listening Socket object 
        _SocketListener = null;

        // Ensures the code to have permission to access a Socket 
        permission.Demand();

        // Resolves a host name to an IPHostEntry instance 
        IPHostEntry ipHost = Dns.GetHostEntry("");

        // Gets first IP address associated with a localhost 
        //  IPAddress ipAddr = ipHost.AddressList[0];

        IPAddress ipAddr = System.Net.IPAddress.Parse(tcip);

        // Creates a network endpoint 
        ipEndPoint = new IPEndPoint(ipAddr, port);

        _log.LogInformation($"Opening socket at {ipAddr}:{port}");

        // Create one Socket object to listen the incoming connection 
        _SocketListener = new Socket(
            ipAddr.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp
            );

        // Associates a Socket with a local endpoint 
        _SocketListener.Bind(ipEndPoint);

        //StatusMessage = "Server started.";
      }
      catch (Exception exc)
      {
        MegalodonLogger.LogError($"CreateSocket Exception. {exc.ToString()}");
      }
    }

    public void ListenOnPort()
    {
      try
      {
        // Places a Socket in a listening state and specifies the maximum 
        // Length of the pending connections queue 
        _SocketListener.Listen(10);

        // Set the event to nonsignaled state.
        allDone.Reset();

        var msg =$"Server is now listening on {ipEndPoint.Address} port: {ipEndPoint.Port}";
        Console.WriteLine(msg);
        _log.LogInformation(msg);

        _SocketListener.BeginAccept(
          new AsyncCallback(AcceptCallback),
          _SocketListener);

        allDone.WaitOne();

      }
      catch (Exception exc)
      {
        _log.LogError($"ListenOnPort Exception. {exc.ToString()}");
      }
    }


    public void AcceptCallback(IAsyncResult ar)
    {

      // Signal the main thread to continue.  
      allDone.Set();

      if (Callback != null)
        Callback("Connection made", TagConstants.CALLBACK_CONNECTION_MADE);

      Socket listener = null;

      // A new Socket to handle remote host communication 
      Socket handler = null;
      try
      {
        // Receiving byte array 
        byte[] buffer = new byte[1024];
        // Get Listening Socket object 
        listener = (Socket)ar.AsyncState;
        // Create a new socket 
        handler = listener.EndAccept(ar);

        // Using the Nagle algorithm 
        handler.NoDelay = false;

        // Creates one object array for passing data 
        object[] obj = new object[2];
        obj[0] = buffer;
        obj[1] = handler;

        // Begins to asynchronously receive data 
        handler.BeginReceive(
            buffer,        // An array of type Byt for received data 
            0,             // The zero-based position in the buffer  
            buffer.Length, // The number of bytes to receive 
            SocketFlags.None,// Specifies send and receive behaviors 
            new AsyncCallback(ReceiveCallback),//An AsyncCallback delegate 
            obj            // Specifies infomation for receive operation 
            );

        // Begins an asynchronous operation to accept an attempt 
        AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
        listener.BeginAccept(aCallback, listener);
      }
      catch (Exception exc)
      {
        _log.LogError($"AcceptCallback Exception. {exc.ToString()}");
      }
    }



    public void SendCallback(IAsyncResult ar)
    {
      try
      {
        // A Socket which has sent the data to remote host 
        Socket handler = (Socket)ar.AsyncState;

        // The number of bytes sent to the Socket 
        int bytesSend = handler.EndSend(ar);
        Console.WriteLine(
            "Sent {0} bytes to Client", bytesSend);
      }
      catch (Exception exc)
      {
        //  MessageBox.Show(exc.ToString());
      }
    }

    /// <summary>
    /// Handles incoming data packets
    /// </summary>
    /// <param name="ar"></param>
    public void ReceiveCallback(IAsyncResult ar)
    {
      var _ETX = (char)TagConstants.ETX;
      var _STX = (char)TagConstants.STX;
      var _ACK = (char)TagConstants.ACK;
      var _ENQ = (char)TagConstants.ENQ;
      var _EOT = (char)TagConstants.EOT;

      bool keepListening = false;

      try
      {
        // Fetch a user-defined object that contains information 
        object[] obj = new object[2];
        obj = (object[])ar.AsyncState;

        // Received byte array 
        byte[] buffer = (byte[])obj[0];

        // A Socket to handle remote host communication. 
        handler = (Socket)obj[1];

        // Received message 
        string content = string.Empty;
        string content2 = string.Empty;

        // The number of bytes received. 
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
          // content += Encoding.Unicode.GetString(buffer, 0, bytesRead);
          content += Encoding.UTF8.GetString(buffer, 0, bytesRead);

          //     content2 += Encoding.UTF8.GetString(buffer, 0, bytesRead);

          if (Callback != null)
            Callback($"Input Lenght:{content.Length}", TagConstants.CALLBACK_LOG_INFO_MSG);


          if (DumpContent)
          {
            //            DumpPacketHelper.SaveData(ref buffer); //.DumpPacket(content);
            //    DumpPacketHelper.SaveData2(content); //.DumpPacket(content);


            byte[] cbytes = Encoding.ASCII.GetBytes(content);
            DebugPacketHelper.SaveData2(ref cbytes);
            DebugPacketHelper.DumpPacket(content);
            //    DumpPacketHelper.DumpPacket2(content2);

          }

          byte[] byteData = new byte[1]; // response buffer

          if (bytesRead == 1 | content.IndexOf(_ETX) > -1)
          {

            if (bytesRead == 1)
            {
              byte cb = buffer[0]; // control byte coming from client 


              if (DumpContent)
              {
                byte[] byteData2 = new byte[1];
                byteData2[0] = cb;
                DebugPacketHelper.SaveData(ref byteData2); //.DumpPacket(content);
              }


              switch (cb)
              {
                case TagConstants.ENQ:
                  keepListening = true;
                  if (_HeaderRequired)
                    byteData[0] = TagConstants.SOH;
                  else
                    byteData[0] = TagConstants.ACK;
                  handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
                  if (Callback != null)
                    if (_HeaderRequired)
                      Callback("ENQ recieved. Returning SOH", TagConstants.CALLBACK_LOG_INFO_MSG);
                    else
                      Callback("ENQ recieved. Returning ACK", TagConstants.CALLBACK_LOG_INFO_MSG);

                  if (DumpContent)
                    DebugPacketHelper.DumpPacket("ENQ");

                  break;
                case TagConstants.EOT:
                  if (Callback != null)
                    Callback("EOT recieved. No responce sent", TagConstants.CALLBACK_LOG_INFO_MSG);
                  if (DumpContent)
                    DebugPacketHelper.DumpPacket("EOT");

                  break;
                default:
                  // log warning
                  keepListening = true;
                  byteData[0] = TagConstants.NAK; // todo
                  handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
                  if (Callback != null)
                    Callback($"Unexpected control char recieved. Returning NAK. Value:{cb}", TagConstants.CALLBACK_LOG_INFO_MSG);
                  if (DumpContent)
                    DebugPacketHelper.DumpPacket("??");

                  break;
              }
            }
            else
            {
              keepListening = true;
              // Convert byte array to string
              string str = content.Substring(0, content.LastIndexOf(_ETX));
              str = str.Trim(_STX);
              if (Callback != null)
              {
                Callback(str, TagConstants.CALLBACK_PARSE_PACKET); // process datapacket
                if (_HeaderRequired) // Prepare the reply message 
                {
                  byteData[0] = TagConstants.SOH;
                  Callback("SOH returned", TagConstants.CALLBACK_LOG_INFO_MSG);
                }
                else
                {
                  byteData[0] = TagConstants.ACK;
                  Callback("ACK returned", TagConstants.CALLBACK_LOG_INFO_MSG);
                }
                // Sends data asynchronously to a connected Socket 
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
              }
            }

            if (keepListening)
            {
              if (Callback != null)
                Callback("Listening...", 0);
              byte[] buffernew = new byte[1024];
              obj[0] = buffernew;
              obj[1] = handler;
              handler.BeginReceive(buffernew, 0, buffernew.Length,
                  SocketFlags.None,
                  new AsyncCallback(ReceiveCallback), obj);
            }

            //   TxtRecv = "Read " + str.Length * 2 + " bytes from client.\n Data: " + str;

            //this is used because the UI couldn't be accessed from an external Thread
            /*
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
              tbAux.Text = "Read " + str.Length * 2 + " bytes from client.\n Data: " + str;
            }

            );*/

            // Continues to asynchronously receive data


          }
          else
          {
            if (Callback != null)
              Callback("No ETX continue. Returning NAK Listening...", TagConstants.CALLBACK_LOG_INFO_MSG);

            byteData[0] = TagConstants.NAK;
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);

            // Continues to asynchronously receive data
            byte[] buffernew = new byte[1024];
            obj[0] = buffernew;
            obj[1] = handler;
            handler.BeginReceive(buffernew, 0, buffernew.Length,
                SocketFlags.None,
                new AsyncCallback(ReceiveCallback), obj);
          }


      //    TxtRecv = content;

          /*
          this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
          {
            tbAux.Text = content;
          }
          

          );*/
        }
      }
      catch (Exception exc)
      {
        // This is where we can handle a forced break by client

        _log.LogError($"ReceiveCallback Exception. {exc.ToString()}");
        _PortRestartNeeded = true; // tell manger we lost connection to client
      }
    }




  }
}
