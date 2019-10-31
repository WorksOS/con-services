using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using TagFiles.Utils;

namespace MegalodonClient
{
  public partial class Form1 : Form
  {

 //   private string workFolder  = @"c:\data\megalodon";
    // Receiving byte array  
    byte[] bytes = new byte[1024];
    Socket senderSock;

    private string stx = Convert.ToChar(TagConstants.STX).ToString();
    private string rs = Convert.ToChar(TagConstants.RS).ToString();
    private string etx = Convert.ToChar(TagConstants.ETX).ToString();
    private string enq = Convert.ToChar(TagConstants.ENQ).ToString();
    private string ack = Convert.ToChar(TagConstants.ACK).ToString();
    private string nak = Convert.ToChar(TagConstants.NAK).ToString();
    private string soh = Convert.ToChar(TagConstants.SOH).ToString();
    private string eot = Convert.ToChar(TagConstants.EOT).ToString();

    private bool keepGoing = true;
    private bool wantHeader = true;

    public Form1()
    {
      InitializeComponent();
    }


    private void MakeTrack(string path)
    {
      // Create a file to write to.
      using (StreamWriter sw = File.CreateText(path))
      {
        sw.WriteLine("Hello");
        sw.WriteLine("And");
        sw.WriteLine("Welcome");
      }
    }

      private void BtnTrack_Click(object sender, EventArgs e)
    {

      SocketWriter.AsynchronousClient.StartClient();

      /*
      string pathString = System.IO.Path.Combine(workFolder, "track");
      System.IO.Directory.CreateDirectory(pathString);
      //string fileString = System.IO.Path.Combine(pathString, $"track{DateTime.Now.ToString("yyyyMMddHHmmss")}");
      //  string fileString = System.IO.Path.Combine(pathString, $"track{DateTime.Now.ToString("yyyyMMdd")}.txt");
      string fileString = System.IO.Path.Combine(pathString, "track.txt");
      MakeTrack(fileString);
      toolStripStatusLabel1.Text = $"LastAction: Output:{fileString}";
      */
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      
    }

    private void Button2_Click(object sender, EventArgs e)
    {




      try
      {
        // Create one SocketPermission for socket access restrictions 
        SocketPermission permission = new SocketPermission(
            NetworkAccess.Connect,    // Connection permission 
            TransportType.Tcp,        // Defines transport types 
            "",                       // Gets the IP addresses 
            SocketPermission.AllPorts // All ports 
            );

        // Ensures the code to have permission to access a Socket 
        permission.Demand();

        // Resolves a host name to an IPHostEntry instance            
        IPHostEntry ipHost = Dns.GetHostEntry("");

        // Gets first IP address associated with a localhost 
        //IPAddress ipAddr = ipHost.AddressList[0];
        IPAddress ipAddr = System.Net.IPAddress.Parse(txtTCIP.Text);

        // Creates a network endpoint 
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, Int32.Parse(txtPort.Text));

        // Create one Socket object to setup Tcp connection 
        senderSock = new Socket(
            ipAddr.AddressFamily,// Specifies the addressing scheme 
            SocketType.Stream,   // The type of socket  
            ProtocolType.Tcp     // Specifies the protocols  
            );

        senderSock.NoDelay = false;   // Using the Nagle algorithm 

        // Establishes a connection to a remote host 
        senderSock.Connect(ipEndPoint);
        label20.Text = "Socket connected to " + senderSock.RemoteEndPoint.ToString();

      }
      catch (Exception exc) { MessageBox.Show(exc.ToString()); }

    }

    private void ReceiveDataFromServer()
    {
      wantHeader = false;
      try
      {
        // Receives data from a bound Socket. 
        int bytesRec = senderSock.Receive(bytes);

        // Converts byte array to string 
        String theMessageToReceive = Encoding.Unicode.GetString(bytes, 0, bytesRec);

        // Continues to read the data till data isn't available 
        while (senderSock.Available > 0)
        {
          bytesRec = senderSock.Receive(bytes);
          theMessageToReceive += Encoding.Unicode.GetString(bytes, 0, bytesRec);
        }

        if (bytesRec == 1)
        {
          byte sb = bytes[0];
          if (sb == TagConstants.SOH)
          {
            wantHeader = true;
            label20.Text = $"The server reply SOH";
          }
          else if (sb == TagConstants.ACK)
            label20.Text = $"The server reply ACK";
          else if (sb == TagConstants.NAK)
            label20.Text = $"The server reply NAK";
          else
            label20.Text = $"The server reply value {sb}";

        }
        else
          label20.Text = $"The server reply: {theMessageToReceive}";
      }
      catch (Exception exc) { MessageBox.Show(exc.ToString()); }
    }

    private void Button3_Click(object sender, EventArgs e)
    {

      var unixTimestamp = TagUtils.GetCurrentUnixTimestampMillis();

      // from local time to unix time
      //     var dateTime = new DateTime(2015, 05, 24, 10, 2, 0, DateTimeKind.Local);
      //    var dateTimeOffset = new DateTimeOffset(dateTime);
      //   var unixDateTime = dateTimeOffset.ToUnixTimeSeconds();

      // going back again 
      //   var localDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixDateTime)
      //   .DateTime.ToLocalTime();

      var timeStamp = "TME" + unixTimestamp.ToString();


      // this would normally be read from socket
      var toSend = stx + rs + timeStamp +
        rs + "GPM3" +
        rs + "DESPlanB" +
        rs + "LAT-0.759971" +
        rs + "LON3.012268" +
        rs + "HGT-37.600" +
        rs + "MIDMyVessel1" +
        rs + "BOG0" +
        rs + "UTM0" +
        rs + "HDG92" +
        rs + "SERe6cd374b-22d5-4512-b60e-fd8152a0899b" +
        rs + "MTPHEX" + etx;



      try
      {
       // string theMessageToSend = toSend;
       // byte[] dataPacket = Encoding.Unicode.GetBytes(theMessageToSend);

        byte[] dataPacket = Encoding.UTF8.GetBytes(toSend);
        // Sends data to a connected Socket. 
        int bytesSend = senderSock.Send(dataPacket);

        ReceiveDataFromServer();

      }
      catch (Exception exc)
      {
        MessageBox.Show(exc.ToString());
      }

    }

    private void SendENQ()
    {
      byte[] byteData = new byte[1]; // enq buffer
      byteData[0] = TagConstants.ENQ;
      var bytesSend = senderSock.Send(byteData);
      ReceiveDataFromServer();
    }


    // Send ENG
    private void Button4_Click(object sender, EventArgs e)
    {
      SendENQ();
    }


    private void SendEOT()
    {
      byte[] byteData = new byte[1]; // enq buffer
      byteData[0] = TagConstants.EOT;
      var bytesSend = senderSock.Send(byteData);
    }

    private void Button5_Click(object sender, EventArgs e)
    {
      SendEOT();
    }

    // Send Epoch Update
    private void Button6_Click(object sender, EventArgs e)
    {
      var unixTimestamp = TagUtils.GetCurrentUnixTimestampMillis();
      var timeStamp = "TME" + unixTimestamp.ToString();

      // stx should be one byte not two
      var toSend = stx + rs + timeStamp +
        rs + "LEB504383.841" + rs + "LNB7043871.371" + rs + "LHB-20.882" +
        rs + "REB504384.745" + rs + "RNB7043869.853" + rs + "RHB-20.899" +
        rs + "BOG1" + rs + "MSD0.2" + rs + "HDG93" + etx;

      // var dataPacket = Encoding.Unicode.GetBytes(toSend);
      byte[] dataPacket = Encoding.UTF8.GetBytes(toSend);
      // Sends data to a connected Socket. 
      var bytesSend = senderSock.Send(dataPacket);

      ReceiveDataFromServer();
    }

    // Test SIM
    private void Button7_Click(object sender, EventArgs e)
    {
    //  var unixTimestamp = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
      var unixTimestamp = TagUtils.GetCurrentUnixTimestampMillis();

      int east = 0;
      int north = 0;
      double direction = 0.3; // 30cm

      var timeStamp = "TME" + unixTimestamp.ToString();
      var startHgt = Convert.ToDouble(txtStartHgt.Text);
      var startLat = Convert.ToDouble("36.206979");
      var startLon = Convert.ToDouble("-115.020131");
      var startLEB = Convert.ToDouble("2744.351658");
      var startLNB = Convert.ToDouble("1165.214447");
      var startLHB = Convert.ToDouble("595.656315");
      var startREB = Convert.ToDouble("2744.239249");
      var startRNB = Convert.ToDouble("1163.217788");
      var startRHB = Convert.ToDouble("595.630574");

      // this would normally be read from socket
      var thdr = stx + rs + timeStamp;

      var hdr =
        rs + "GPM3" +
        rs + "DES" +
        rs + "LAT" + TagUtils.ToRadians(startLat).ToString() + // 36.206979 Dimensions
        rs + "LON" + TagUtils.ToRadians(startLon).ToString() + //-115.020131
        rs + "HGT" + startHgt.ToString() +
        rs + "MIDVL HEX" +
        rs + "BOG0" +
        rs + "UTM0" +
        rs + "HDG360" +
        rs + "SERe6cd374b-22d5-4512-b60e-fd8152a0899b" +
        rs + "MTPHEX" + etx;

      var toSend = thdr + hdr;
      byte[] dataPacket = Encoding.UTF8.GetBytes(toSend);

   //   var dataPacket = Encoding.UTF8.GetString(bytes);

      try
      {
      //  string theMessageToSend = toSend;
      //  byte[] dataPacket = Encoding.Unicode.GetBytes(theMessageToSend);
        // Sends data to a connected Socket. 
        int bytesSend = senderSock.Send(dataPacket);

        ReceiveDataFromServer();


        while (keepGoing)
        {

          Application.DoEvents();

          east++;
          if (east == 100)
          {
            east = 1;
            direction = -direction;
            north++;
            startLNB = startLNB + 0.3;
            startRNB = startRNB + 0.3;
          }

          startLEB = startLEB + direction;
          startREB = startREB + direction;

          lblStatus.Text = $"East:{east} Direction:{ direction} North:{north}";

          System.Threading.Thread.Sleep(200);

          unixTimestamp = TagUtils.GetCurrentUnixTimestampMillis();
          timeStamp = "TME" + unixTimestamp.ToString();

          thdr = stx + rs + timeStamp;

          if (wantHeader)
            toSend = thdr + hdr;
          else
          {
            toSend = thdr +
            rs + "LEB"+ startLEB.ToString() + rs + "LNB" + startLNB.ToString() + rs + "LHB" + startLHB.ToString() +
            rs + "REB" + startREB.ToString() + rs + "RNB" + startRNB.ToString() + rs + "RHB" + startRHB.ToString() +
            rs + "BOG1" + rs + "MSD0.1" + rs + "HDG360" + etx;
          }
          // var dataPacket = Encoding.Unicode.GetBytes(toSend);
          byte[] dataPacket2 = Encoding.UTF8.GetBytes(toSend);
          // Sends data to a connected Socket. 
          var bytesSend2 = senderSock.Send(dataPacket2);

          Array.Resize(ref dataPacket2, 0);

          ReceiveDataFromServer();

        }

      }
      catch (Exception exc)
      {
        MessageBox.Show(exc.ToString());
      }
    }

    private void Button8_Click(object sender, EventArgs e)
    {
      keepGoing = false;
    }
  }
}
