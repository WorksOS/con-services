using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;
using VSS.VisionLink.Raptor.Machines;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Arguments;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Requests;
using VSS.VisionLink.Raptor.TAGFiles.Servers.Client;
using VSSTests.TRex.Tests.Common;

namespace VSS.TRex.Tools.TagfileSubmitter
{
  class Program
  {
    private static ILog Log = null;
    //        private static int tAGFileCount = 0;

    public static void ProcessSingleTAGFile(long projectID, string fileName)
    {
      Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, 0, false);

      ProcessTAGFileRequest request = new ProcessTAGFileRequest();
      ProcessTAGFileRequestArgument arg = null;

      using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      {
        byte[] bytes = new byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);

        arg = new ProcessTAGFileRequestArgument()
        {
          ProjectID = projectID,
          AssetID = machine.ID,
          TAGFiles = new List<ProcessTAGFileRequestFileItem>()
                    {
                        new ProcessTAGFileRequestFileItem()
                        {
                            FileName = fileName,
                            TagFileContent = bytes
                        }
                    }
        };
      }

      request.Execute(arg);
    }

    public static void ProcessTAGFiles(long projectID, string[] files)
    {
      Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, 0, false);

      ProcessTAGFileRequest request = new ProcessTAGFileRequest();
      ProcessTAGFileRequestArgument arg = new ProcessTAGFileRequestArgument()
      {
        ProjectID = projectID,
        AssetID = machine.ID
      };

      arg.TAGFiles = new List<ProcessTAGFileRequestFileItem>();

      foreach (string file in files)
      {
        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
        {
          byte[] bytes = new byte[fs.Length];
          fs.Read(bytes, 0, bytes.Length);

          arg.TAGFiles.Add(new ProcessTAGFileRequestFileItem() { FileName = file, TagFileContent = bytes });
        }
      }

      request.Execute(arg);
    }

    public static void ProcessTAGFilesInFolder(long projectID, string folder)
    {
      // If it is a single file, just process it
      if (File.Exists(folder))
      {
        ProcessTAGFiles(projectID, new string[] { folder });
      }
      else
      {
        string[] folders = Directory.GetDirectories(folder);
        foreach (string f in folders)
        {
          ProcessTAGFilesInFolder(projectID, f);
        }

        ProcessTAGFiles(projectID, Directory.GetFiles(folder));
      }
    }

    public static void ProcessMachine333TAGFiles(long projectID)
    {
      ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Machine333");
    }

    public static void ProcessMachine10101TAGFiles(long projectID)
    {
      ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Machine10101");
    }

    static void Main(string[] args)
    {
      string logFileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".log";
      log4net.GlobalContext.Properties["LogName"] = logFileName;
      //log4net.Config.XmlConfigurator.Configure();

      Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

      Log.Info("Initialising TAG file processor");

      try
      {
        // Pull relevant arguments off the command line
        if (args.Length < 2)
        {
          Console.WriteLine("Usage: ProcessTAGFiles <ProjectID> <FolderPath>");
          return;
        }

        long projectID = -1;
        string folderPath = "";
        try
        {
          projectID = Convert.ToInt64(args[0]);
          folderPath = args[1];
        }
        catch
        {
          Console.WriteLine(String.Format("Invalid project ID {0} or folder path {1}", args[0], args[1]));
          return;
        }

        if (projectID == -1)
        {
          return;
        }

        // Obtain a TAGFileProcessing client server
        TAGFileProcessingClientServer TAGServer = new TAGFileProcessingClientServer();

        ProcessTAGFilesInFolder(projectID, folderPath);

        // ProcessMachine10101TAGFiles(projectID);
        // ProcessMachine333TAGFiles(projectID);

        //ProcessSingleTAGFile(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Machine10101\\2085J063SV--C01 XG 01 YANG--160804061209.tag");
        //ProcessSingleTAGFile(projectID);

        // Process all TAG files for project 4733:
        //ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 1");
        //ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 2");
        //ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 3");
        //ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 4");
      }
      finally
      {
        Console.ReadKey();
      }
    }
  }
}
