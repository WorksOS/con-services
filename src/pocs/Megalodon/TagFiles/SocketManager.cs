using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TagFiles.Common;
using VSS.Common.Abstractions.Configuration;
using Microsoft.Extensions.Logging;

namespace TagFiles
{
  /// <summary>
  /// Handles all aspects of incoming data through the socket
  /// </summary>
  public class SocketManager : ISocketManager
  {

    private int port = 1500;
    private string tcip = "127.0.0.1";
    private int logEntries = 100;

    public bool _PortRestartNeeded = false;
    public bool PortRestartNeeded 
    {
      get => _PortRestartNeeded;
      set => _PortRestartNeeded = value;
    }

    private bool _HeaderRequired = true;
    public bool HeaderRequired  // read-write instance property
    {
      get => _HeaderRequired;
      set => _HeaderRequired = value;
    }

    private bool _DebugTraceToLog = false;

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
    private bool logStartup = true;
    private uint epochsSeen = 0;
    public delegate void CallbackEventHandler(string something, int mode);
    public event CallbackEventHandler Callback;
    public static ManualResetEvent allDone = new ManualResetEvent(false);


    /// <summary>
    /// Intialize SocketManager
    /// </summary>
    /// <param name="log"></param>
    /// <param name="configStore"></param>
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

      _DebugTraceToLog = configStore.GetValueBool("DebugTraceToLog") ?? false;

    }

    /// <summary>
    /// Create the socket 
    /// </summary>
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
        //IPHostEntry ipHost = Dns.GetHostEntry("");
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

      }
      catch (Exception exc)
      {
        _log.LogError($"CreateSocket Exception. {exc.ToString()}");
      }
    }

    /// <summary>
    /// Go into a wait state for client connection
    /// </summary>
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

    /// <summary>
    /// Trigger when a client connects
    /// </summary>
    /// <param name="ar"></param>    
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

   
    /// <summary>
    /// Callback on send
    /// </summary>
    /// <param name="ar"></param>
    public void SendCallback(IAsyncResult ar)
    {
      try
      {
        // A Socket which has sent the data to remote host 
        Socket handler = (Socket)ar.AsyncState;

        // The number of bytes sent to the Socket 
        int bytesSend = handler.EndSend(ar);
      //  Console.WriteLine( "Sent {0} bytes to Client", bytesSend);
      }
      catch (Exception exc)
      {
        _log.LogError($"SendCallback Exception. {exc.ToString()}");
      }
    }

    private string FormatTrace(string msg)
    {
      return msg.Replace(TagConstants.CHAR_STX.ToString(), "<STX>").Replace(TagConstants.CHAR_ETX.ToString(), "<ETX>").
                 Replace(TagConstants.CHAR_RS.ToString(), "<RS>").Replace(TagConstants.CHAR_ENQ.ToString(), "<ENQ>");
    }

    /// <summary>
    /// Handles incoming data packets
    /// </summary>
    /// <param name="ar"></param>
    public void ReceiveCallback(IAsyncResult ar)
    {

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

        // The number of bytes received. 
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
          content += Encoding.UTF8.GetString(buffer, 0, bytesRead);

          if (logStartup)
          { // record first 5 entries for possible trouble shooting
            epochsSeen++;
            if (epochsSeen == 1)
              _log.LogInformation($"** Logging first {logEntries} datapackets **");
            _log.LogInformation(FormatTrace(content));
            if (epochsSeen > logEntries)
              logStartup = false; 
          }
          else if (_DebugTraceToLog)
          { // write content to log for debugging
            _log.LogDebug(FormatTrace(content));
          }

          byte[] byteData = new byte[1]; // response buffer

          if (bytesRead == 1 | content.IndexOf(TagConstants.CHAR_ETX) > -1)
          {

            if (bytesRead == 1)
            {
              byte cb = buffer[0]; // control byte coming from client 

              switch (cb)
              {
                case TagConstants.ENQ:
                  keepListening = true;
                  if (_HeaderRequired)
                    byteData[0] = TagConstants.SOH;
                  else
                    byteData[0] = TagConstants.ACK;
                  handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
                  if (_DebugTraceToLog)
                  {
                    if (_HeaderRequired)
                      _log.LogDebug("ENQ recieved. Returning SOH");
                    else
                      _log.LogDebug("ENQ recieved. Returning ACK");
                  }
                  break;

                case TagConstants.EOT: // not used by TMC client
                  _log.LogDebug("EOT recieved. No responce sent");
                  break;

                default:
                  // log warning
                  keepListening = true;
                  byteData[0] = TagConstants.NAK; // todo
                  handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
                  _log.LogError($"Unexpected control char recieved. Returning NAK. Value:{cb}");
                  break;
              }
            }
            else
            {
              keepListening = true;
              // Convert byte array to string
              string str = content.Substring(0, content.LastIndexOf(TagConstants.CHAR_ETX));
              str = str.Trim(TagConstants.CHAR_STX);
              if (Callback != null)
              {
                Callback(str, TagConstants.CALLBACK_PARSE_PACKET); // process datapacket
                if (_HeaderRequired) // Prepare the reply message 
                {
                  byteData[0] = TagConstants.SOH;
                  if (_DebugTraceToLog)
                    _log.LogDebug("SOH returned");
                }
                else
                {
                  byteData[0] = TagConstants.ACK;
                  if (_DebugTraceToLog)
                    _log.LogDebug("ACK returned");
                }
                // Sends data asynchronously to a connected Socket 
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
              }
            }

            if (keepListening)
            {
              if (_DebugTraceToLog)
               _log.LogDebug("Listening...");
              byte[] buffernew = new byte[1024];
              obj[0] = buffernew;
              obj[1] = handler;
              handler.BeginReceive(buffernew, 0, buffernew.Length,
                  SocketFlags.None,
                  new AsyncCallback(ReceiveCallback), obj);
            }
            // Continues to asynchronously receive data
          }
          else
          {
            
            _log.LogWarning("No ETX continue. Returning NAK Listening...");
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
        }
      }
      catch (Exception exc)
      {
        // This is where we can handle a forced socket break by client
        _log.LogError($"ReceiveCallback Exception. {exc.ToString()}");
        _PortRestartNeeded = true; // tell the controller we lost connection to client
      }
    }

    public void SendAck()
    {
      _log.LogInformation("Sending an ACK to Client");
      byte[] byteData = new byte[1]; // response buffer
      byteData[0] = TagConstants.ACK;
      handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
    }


  }
}
