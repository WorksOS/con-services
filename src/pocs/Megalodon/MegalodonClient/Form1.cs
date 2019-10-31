using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            label20.Text = $"The server reply SOH";
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

      Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

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
      Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
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
      Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
      var timeStamp = "TME" + unixTimestamp.ToString();

      // this would normally be read from socket
      var toSend1 = stx + rs + timeStamp +
        rs + "GPM3" +
        rs + "DES" +
        rs + "LAT-0.759971" +
        rs + "LON3.012268" +
        rs + "HGT-37.600" +
        rs + "MIDVL HEX" +
        rs + "BOG0" +
        rs + "UTM0" +
        rs + "HDG360" +
        rs + "SERe6cd374b-22d5-4512-b60e-fd8152a0899b" +
        rs + "MTPHEX" + etx;

      byte[] dataPacket = Encoding.UTF8.GetBytes(toSend1);

   //   var dataPacket = Encoding.UTF8.GetString(bytes);

      try
      {
      //  string theMessageToSend = toSend;
      //  byte[] dataPacket = Encoding.Unicode.GetBytes(theMessageToSend);
        // Sends data to a connected Socket. 
        int bytesSend = senderSock.Send(dataPacket);

        ReceiveDataFromServer();


        SendENQ();


        Int32 unixTimestamp2 = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        var timeStamp2 = "TME" + unixTimestamp.ToString();

        // stx should be one byte not two
        var toSend = stx + rs + timeStamp2 +
          rs + "LEB504383.841" + rs + "LNB7043871.371" + rs + "LHB-20.882" +
          rs + "REB504384.745" + rs + "RNB7043869.853" + rs + "RHB-20.899" +
          rs + "BOG1" + rs + "MSD0.1" + rs + "HDG360" + etx;

        // var dataPacket = Encoding.Unicode.GetBytes(toSend);
        byte[] dataPacket2 = Encoding.UTF8.GetBytes(toSend);
        // Sends data to a connected Socket. 
        var bytesSend2 = senderSock.Send(dataPacket2);

        ReceiveDataFromServer();

        SendENQ();


      }
      catch (Exception exc)
      {
        MessageBox.Show(exc.ToString());
      }
    }
  }
}
