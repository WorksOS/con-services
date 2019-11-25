using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using TagFiles.Utils;

/// <summary>
/// For developer use to test Megalodon server
/// </summary>
namespace MegalodonClient
{
  public partial class Form1 : Form
  {

    
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

    private bool portOpen = false;

    private bool keepGoing = true;
    private bool wantHeader = true;

    public Form1()
    {
      InitializeComponent();
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
            if (chkSOH.Checked)
              listBox1.Items.Add($"{DateTime.Now.ToString()} : SOH");
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



    private void SendENQ()
    {
      byte[] byteData = new byte[1]; // enq buffer
      byteData[0] = TagConstants.ENQ;
      var bytesSend = senderSock.Send(byteData);
      ReceiveDataFromServer();
    }


    private void SendEOT()
    {
      byte[] byteData = new byte[1]; // enq buffer
      byteData[0] = TagConstants.EOT;
      var bytesSend = senderSock.Send(byteData);
    }


    private void BtnRunSim_Click(object sender, EventArgs e)
    {
   //   uint totalCycles = 0;

      bool IsBOG = true;
      int epochCount = 0;
      int passCount = 1;

      keepGoing = true;

      if (!portOpen)
        BtnOpenPort_Click(sender, e);

      // start with an enquiry
      SendENQ();

      //  var unixTimestamp = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
      long unixTimestamp;

      if (chkUseCustomDate.Checked)
        unixTimestamp = TagUtils.GetCustomTimestampMillis(dtpStart.Value);
      else
        unixTimestamp = TagUtils.GetCurrentUnixTimestampMillis();

      listBox1.Items.Clear();
      int east = 0;
      int north = 0;
      double eDirection = Convert.ToDouble(txtEast.Text);
      double nDirection = Convert.ToDouble(txtNorth.Text);
      double currrentHgt = Convert.ToDouble(txtStartHgt.Text);
      double maxHgt = currrentHgt + 2.0;
      double minHgt = currrentHgt - 2.0;
      double hgtInterval = 0.01;
      int nSteps = Convert.ToInt32(txtNSteps.Text);
      int eSteps = Convert.ToInt32(txtESteps.Text);

      var timeStamp = "TME" + unixTimestamp.ToString();
      // var startHgt = currrentHgt;

      var startLat = Convert.ToDouble(txtLat.Text);
      var startLon = Convert.ToDouble(txtLong.Text);

      var startLEB = Convert.ToDouble(txtLE.Text);
      var startLNB = Convert.ToDouble(txtLN.Text);
      //    var startLHB = currrentHgt.ToString();

      var startREB = Convert.ToDouble(txtRE.Text);
      var startRNB = Convert.ToDouble(txtRN.Text);
      //  var startRHB = currrentHgt.ToString();

      // this would normally be read from socket
      var thdr = stx + rs + timeStamp;
      var machineType = "MTP" + cboType.Text;
      var heading = "HDG90";
      var valBOG = "0";

      var hdr =
        rs + "GPM3" +
        rs + "DES" + txtDesign.Text +
        rs + "LAT" + TagUtils.ToRadians(startLat).ToString() + // 36.206979 Dimensions
        rs + "LON" + TagUtils.ToRadians(startLon).ToString() + //-115.020131
        rs + "HGT" + currrentHgt.ToString() +
        rs + "MID" + txtVName.Text +
        rs + "UTM0" +
        rs + heading +
        rs + "SER" + txtSerial.Text +
        rs + machineType + etx;

      var toSend = thdr + rs + "BOG0" +  hdr;
      var dataPacket = Encoding.UTF8.GetBytes(toSend);

      try
      {
        // Sends data to a connected Socket. 
        int bytesSend = senderSock.Send(dataPacket);

        ReceiveDataFromServer();
        int epochLimit = Convert.ToInt32(txtEpochLimit.Text);

        // Loop epoch updates
        while (keepGoing & epochCount < epochLimit)
        {

          if (chkTwoEpochsOnly.Checked)
            epochCount++;

          Application.DoEvents();
          System.Threading.Thread.Sleep(Convert.ToInt32(txtSleep.Text));

          east++;

          if (east == eSteps) // # epochs b4 turn around
          {
            east = 1; // step counter for easting
            eDirection = -eDirection; // direction easting goes
            north++;
            IsBOG = false;
            if (north > nSteps) // is it time to start whole cycle again
            {
              listBox1.Items.Add($"Sim. Total passcount cycles: {passCount}");
              passCount++;
              north = 1;
              startLNB = Convert.ToDouble(txtLN.Text);
              startRNB = Convert.ToDouble(txtRN.Text);
            }
            else
            {
              startLNB = startLNB + nDirection;
              startRNB = startRNB + nDirection;
            }
          }

          startLEB = startLEB + eDirection;
          startREB = startREB + eDirection;

          if (chkVaryHgt.Checked)
          {
            if (currrentHgt < minHgt | currrentHgt > maxHgt)
              hgtInterval = hgtInterval * -1;
            currrentHgt = currrentHgt + hgtInterval;
            if (hgtInterval < 0)
              heading = "HDG270";
            else
              heading = "HDG90";
          }

          var dir = eDirection < 0 ? "Left" : "Right";
          lblStatus.Text = $"Passcount:{passCount}, NorthStep:{north}, EastStep:{east}, Heading:{ dir}, Hgt:{currrentHgt}";

          TimeSpan duration = DateTime.Now - dtpStart.Value;
          if (chkUseCustomDate.Checked)
            unixTimestamp = TagUtils.GetCustomTimestampMillis(dtpStart.Value + duration);
          else
            unixTimestamp = TagUtils.GetCurrentUnixTimestampMillis();

          timeStamp = "TME" + unixTimestamp.ToString();

          thdr = stx + rs + timeStamp;


          if (IsBOG)
            valBOG = "1";
          else
          {
            valBOG = "0";
            IsBOG = true; // false only for the turn around
          }

          if (wantHeader)
          {

            toSend = thdr + rs + valBOG + hdr;

            var dataPacketHdr = Encoding.UTF8.GetBytes(toSend);
            // Sends data to a connected Socket. 
            senderSock.Send(dataPacketHdr);
            ReceiveDataFromServer();
            if (IsBOG)
              valBOG = "1";
            else
            {
              valBOG = "0";
              IsBOG = true; // false only for the turn around
            }
          }

          if (chkVaryBOG.Checked) 
            if (eDirection < 0)
              valBOG = "0";

          toSend = thdr +
          rs + "LEB" + startLEB.ToString() + rs + "LNB" + startLNB.ToString() + rs + "LHB" + currrentHgt.ToString() +
          rs + "REB" + startREB.ToString() + rs + "RNB" + startRNB.ToString() + rs + "RHB" + currrentHgt.ToString() +
          rs + "BOG" + valBOG + rs + heading + etx;
          var dataPacketEpoch = Encoding.UTF8.GetBytes(toSend);
          senderSock.Send(dataPacketEpoch);
          Array.Resize(ref dataPacketEpoch, 0);
          ReceiveDataFromServer();
        }

      }
      catch (Exception exc)
      {
        MessageBox.Show(exc.ToString());
      }
    }

    private void BtnStop_Click(object sender, EventArgs e)
    {
      keepGoing = false;
    }

    private void BtnSendHeader_Click(object sender, EventArgs e)
    {
      var unixTimestamp = TagUtils.GetCurrentUnixTimestampMillis();
      listBox1.Items.Clear();
      double currrentHgt = Convert.ToDouble(txtStartHgt.Text);
      var timeStamp = "TME" + unixTimestamp.ToString();
      // var startHgt = currrentHgt;

      var startLat = Convert.ToDouble("36.206979");
      var startLon = Convert.ToDouble("-115.020131");

      // this would normally be read from socket
      var thdr = stx + rs + timeStamp;
      var machineType = "MTP" + cboType.Text;
      var heading = "HDG90";

      var hdr =
        rs + "GPM3" +
        rs + "DES" +
        rs + "LAT" + TagUtils.ToRadians(startLat).ToString() + // 36.206979 Dimensions
        rs + "LON" + TagUtils.ToRadians(startLon).ToString() + //-115.020131
        rs + "HGT" + currrentHgt.ToString() +
        rs + "MID" + txtVName.Text +
        rs + "BOG1" +
        rs + "UTM0" +
        rs + heading +
        rs + "SERe6cd374b-22d5-4512-b60e-fd8152a0899b" +
        rs + machineType + etx;

      var toSend = thdr + hdr;
      var dataPacket = Encoding.UTF8.GetBytes(toSend);

      try
      {

        // Sends data to a connected Socket. 
        int bytesSend = senderSock.Send(dataPacket);

        ReceiveDataFromServer();

      }
      catch (Exception exc)
      {
        MessageBox.Show(exc.ToString());
      }

    }

    private void BtnSendEpoch_Click(object sender, EventArgs e)
    {
      var unixTimestamp = TagUtils.GetCurrentUnixTimestampMillis();
      int east = 0;
      int north = 0;
      double eDirection = Convert.ToDouble(txtEast.Text);
      double nDirection = Convert.ToDouble(txtNorth.Text);
      double currrentHgt = Convert.ToDouble(txtStartHgt.Text);
      double maxHgt = currrentHgt + 2.0;
      double minHgt = currrentHgt - 2.0;
      double hgtInterval = 0.01;

      var timeStamp = "TME" + unixTimestamp.ToString();

      var startLEB = Convert.ToDouble(txtLE.Text);
      var startLNB = Convert.ToDouble(txtLN.Text);

      var startREB = Convert.ToDouble(txtRE.Text);
      var startRNB = Convert.ToDouble(txtRN.Text);

      // this would normally be read from socket
      var thdr = stx + rs + timeStamp;
      var machineType = "MTP" + cboType.Text;
      var heading = "HDG90";

      east++;
      if (east == 100)
      {
        east = 1;
        eDirection = -eDirection;
        north++;
        startLNB = startLNB + nDirection;
        startRNB = startRNB + nDirection;
      }

      startLEB = startLEB + eDirection;
      startREB = startREB + eDirection;

      if (chkVaryHgt.Checked)
      {
        if (currrentHgt < minHgt | currrentHgt > maxHgt)
          hgtInterval = hgtInterval * -1;
        currrentHgt = currrentHgt + hgtInterval;
        if (hgtInterval < 0)
          heading = "HDG270";
        else
          heading = "HDG90";
      }

      lblStatus.Text = $"East:{east} E-Direction:{ eDirection} North:{north} Hgt:{currrentHgt}";

      unixTimestamp = TagUtils.GetCurrentUnixTimestampMillis();
      timeStamp = "TME" + unixTimestamp.ToString();

      thdr = stx + rs + timeStamp;

      var toSend = thdr +
      rs + "LEB" + startLEB.ToString() + rs + "LNB" + startLNB.ToString() + rs + "LHB" + currrentHgt.ToString() +
      rs + "REB" + startREB.ToString() + rs + "RNB" + startRNB.ToString() + rs + "RHB" + currrentHgt.ToString() +
      rs + "BOG1" + rs + "MSD0.1" + rs + heading + etx;
      var dataPacketEpoch = Encoding.UTF8.GetBytes(toSend);
      senderSock.Send(dataPacketEpoch);
      ReceiveDataFromServer();
    }

    private void BtnSendENQ_Click(object sender, EventArgs e)
    {
      SendENQ();

    }

    private void BtnSendEOT_Click(object sender, EventArgs e)
    {
      SendEOT();
    }

    private void BtnOpenPort_Click(object sender, EventArgs e)
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
        portOpen = true;
      }
      catch (Exception exc) { MessageBox.Show(exc.ToString()); }
    }

    private void BtnDefChch_Click(object sender, EventArgs e)
    {
      txtStartHgt.Text = "-3.0";
      txtLE.Text = "1576594.0";
      txtLN.Text = "5177503.0";
      txtRE.Text = "1576597.0";
      txtRN.Text = "5177503.0";
      txtLat.Text = "-43.555278";
      txtLong.Text = "172.709705";
    }

    private void BtnDefDimensions_Click(object sender, EventArgs e)
    {
      txtStartHgt.Text = "542.0";
      txtLE.Text = "2744.0";
      txtLN.Text = "1163.0";
      txtRE.Text = "2748.0";
      txtRN.Text = "1163.0";
      txtLat.Text = "36.206979";
      txtLong.Text = "-115.020131";

    }
  }
}
