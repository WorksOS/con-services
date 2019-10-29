using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;
using TagFiles;
using TagFiles.Common;
using TagFiles.Interface;
using TagFiles.Types;

namespace MegalodonServer
{
  public partial class Form1 : Form
  {

    private string workFolder = @"c:\megalodon";
    private string archiveFolder = @"c:\megalodon\archive";
    //   private string trackFolder = @"c:\data\megalodon\track";
    //   private string tagfileFolder = @"c:\data\megalodon\tagfile";
    private string testTagfile = @"c:\megalodon\tagfile\test--180828014716.tag";
    private TagFile tagFile = new TagFile();
    private SocketListener sl;
    private string recmsg;
    private IMegalodon2 megaldon2;

    public Form1()
    {
      InitializeComponent();
    }

    private void log(string msg)
    {
      listBox1.Items.Add(msg);
    }

    private void Archive(string path)
    {
      var path2 = System.IO.Path.Combine(archiveFolder, Path.GetFileName(path));
      if (File.Exists(path2))
        File.Delete(path2);
      File.Move(path, path2);
      listBox1.Items.Add($"{path} was moved to {path2}");

    }

    private void ProcessTrack(string path)
    {
      listBox1.Items.Add($"File:{path}");
      string[] readText = File.ReadAllLines(path);
      foreach (string s in readText)
      {
        listBox1.Items.Add(s);
      }
      Archive(path);
    }

    private void ProcessTracks()
    {
      string pathString = System.IO.Path.Combine(workFolder, "track");
      System.IO.Directory.CreateDirectory(pathString);
      System.IO.Directory.CreateDirectory(archiveFolder);

      string[] fileArray = Directory.GetFiles(pathString, "*.txt");
      if (fileArray.Length == 0)
        listBox1.Items.Add("No files to process");
      else
        foreach (string s in fileArray)
        {
          ProcessTrack(s);
        }
    }

    private void BtnProcessTrack_Click(object sender, EventArgs e)
    {
      //  ProcessTracks();
    }

    private void BtnClear_Click(object sender, EventArgs e)
    {
      listBox1.Items.Clear();
    }

    private void BtnMove_Click(object sender, EventArgs e)
    {
      /*
      string[] fileArray = Directory.GetFiles(archiveFolder, "*.txt");
      if (fileArray.Length == 0)
        listBox1.Items.Add("No files to move");
      else
        foreach (string s in fileArray)
        {
          var path2 = System.IO.Path.Combine(trackFolder, Path.GetFileName(s));
          if (File.Exists(path2))
            File.Delete(path2);
          File.Move(s, path2);
        }
        */

    }

    private void BtnTagfile_Click(object sender, EventArgs e)
    {

    }

    private void BtnWrite_Click(object sender, EventArgs e)
    {
      tagFile.CreateTestData();
    }

    private void DumpPacket(ref string packet)
    {

      string pathString = System.IO.Path.Combine(workFolder, "log");
      System.IO.Directory.CreateDirectory(pathString);
      pathString = System.IO.Path.Combine(pathString, "megalodon.dat");
      using (StreamWriter w = File.AppendText(pathString))
      {
        w.Write($"\r\n{packet}");
      }
    }


    // Callback for socket listener
    void sr_Callback(string packet, int mode)
    {
      if (mode == TagConstants.CALLBACK_MODE_PARSE)
      {
        tagFile.ParseText(packet);
        if (chkDump.Checked)
          DumpPacket(ref packet);
        sl.HeaderRequired = tagFile.Parser.HeaderRequired;
      }

      sl.DumpContent = chkDumpContent.Checked;
      // Display datapacket
      this.label1.Invoke((MethodInvoker)delegate
      {
        // Running on the UI thread
        this.label1.Text = packet; // maybe extra info and not a packet
        this.listBox1.Items.Add(packet);
      }
  );


      //   packet = packet.Replace((char)TagConstants.RS, '=');
      // recmsg =  packet;
    }

    private void BtnOpenPort_Click(object sender, EventArgs e)
    {

      tagFile.CreateTagfileDictionary();

      // method 2
      if (sl == null)
        sl = new SocketListener();

      sl.TCIP = txtTCIP.Text;
      sl.Port = Int32.Parse(txtPort.Text);
      sl.Callback += new SocketListener.CallbackEventHandler(sr_Callback);
      sl.StartListener();
      //     listBox1.Items.Add(sl.StatusMessage);
      listBox1.Items.Add("Waiting connection...");
      sl.ListenOnPort();
      //  listBox1.Items.Add(sl.StatusMessage);


      //   SocketListener.AsynchronousSocketListener.StartListening(Convert.ToInt32(txtPort.Text));
    }

    private void BtnCloseTagFile_Click(object sender, EventArgs e)
    {
      tagFile.WriteTagFileToDisk();
    }

    private void BtnCloseListener_Click(object sender, EventArgs e)
    {
      try
      {
        if (sl.sListener.Connected)
        {
          sl.sListener.Shutdown(SocketShutdown.Receive);
          sl.sListener.Close();
        }

        if (megaldon2 != null)
          megaldon2.EndProcess();

      }
      catch (Exception exc)
      {
        MessageBox.Show(exc.ToString());
      }
    }

    private void Button1_Click(object sender, EventArgs e)
    {
      label1.Text = recmsg;
    }

    private void BtnNewTagfile_Click(object sender, EventArgs e)
    {
      tagFile.WriteTagFileToDisk();
    }

    private void BtnDump_Click(object sender, EventArgs e)
    {

      string pathString = @"c:\megalodon\log";
      System.IO.Directory.CreateDirectory(pathString);
      pathString = System.IO.Path.Combine(pathString, "serverlog.txt");

      using (StreamWriter w = File.AppendText(pathString))
      {
        w.WriteLine("****LOG**** " + DateTime.Now.ToString());
        for (var i = 0; i < listBox1.Items.Count; i++)
        {
          w.WriteLine(listBox1.Items[i]);
        }
      }
    }

    private void BtnThread_Click(object sender, EventArgs e)
    {
      backgroundWorker1.WorkerReportsProgress = true;
      backgroundWorker1.WorkerSupportsCancellation = true;

      if (backgroundWorker1.IsBusy != true)
      {
        // Start the asynchronous operation.
        backgroundWorker1.RunWorkerAsync();
      }

    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
      if (backgroundWorker1.WorkerSupportsCancellation == true)
      {
        // Cancel the asynchronous operation.
        backgroundWorker1.CancelAsync();
      }
    }

    private void BackgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
    {
      BackgroundWorker worker = sender as BackgroundWorker;

      for (int i = 1; i <= 10; i++)
      {
        if (worker.CancellationPending == true)
        {
          e.Cancel = true;
          break;
        }
        else
        {
          // Perform a time consuming operation and report progress.
          System.Threading.Thread.Sleep(500);
          worker.ReportProgress(i * 10);
        }
      }
    }

    private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      label1.Text = (e.ProgressPercentage.ToString() + "%");
    }

    private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      if (e.Cancelled == true)
      {
        label1.Text = "Canceled!";
      }
      else if (e.Error != null)
      {
        label1.Text = "Error: " + e.Error.Message;
      }
      else
      {
        label1.Text = "Done!";
      }
    }

    private void BtnInterface_Click(object sender, EventArgs e)
    {
      megaldon2 = new Megalodon2();
      megaldon2.StartProcess(txtTCIP.Text, Int32.Parse(txtPort.Text));
    }
  }
}
