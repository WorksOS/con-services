using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.TAGFiles.GridFabric.Services;
using VSS.VisionLink.Raptor.Machines;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Arguments;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Requests;
using VSS.VisionLink.Raptor.TAGFiles.Servers.Client;
using VSSTests.TRex.Tests.Common;

/*
Arguments for building project #5, Dimensions:
5 "J:\PP\Construction\Office software\SiteVision Office\Test Files\VisionLink Data\Dimensions 2012\Dimensions2012-Model 381\Model 381"

Arguments for building project #6, Christchurch Southern Motorway:
6 "J:\PP\Construction\Office software\SiteVision Office\Test Files\VisionLink Data\Southern Motorway\TAYLORS COMP"
*/

namespace VSS.VisionLink.Raptor.Client
{
    class Program
    {
        private static ILog Log;

        // Singleton request object for submitting TAG files. Creating these is relatively slow and support concurrent operations.
        private static SubmitTAGFileRequest submitTAGFileRequest;
        private static ProcessTAGFileRequest processTAGFileRequest;

        private static int tAGFileCount = 0;

        public static void SubmitSingleTAGFile(Guid projectID, Guid assetID, string fileName)
        {
            submitTAGFileRequest = submitTAGFileRequest ?? new SubmitTAGFileRequest();
            SubmitTAGFileRequestArgument arg;

            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);

                arg = new SubmitTAGFileRequestArgument()
                {
                    ProjectID = projectID,
                    AssetID = assetID,
                    TagFileContent = bytes,
                    TAGFileName = Path.GetFileName(fileName)
                };
            }

            Log.Info($"Submitting TAG file #{++tAGFileCount}: {fileName}");

            submitTAGFileRequest.Execute(arg);

            // Submit the same TAG fils to different projects to simulate more load (a total of four copies of the tag file)
//            arg.ProjectID += 1000000;
//            submitTAGFileRequest.Execute(arg);
//            arg.ProjectID += 1000000;
//            submitTAGFileRequest.Execute(arg);
//            arg.ProjectID += 1000000;
//            submitTAGFileRequest.Execute(arg);
        }

        public static void ProcessSingleTAGFile(Guid projectID, string fileName)
        {
            Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);

            processTAGFileRequest = processTAGFileRequest ?? new ProcessTAGFileRequest();
            ProcessTAGFileRequestArgument arg;

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
                            FileName = Path.GetFileName(fileName),
                            TagFileContent = bytes
                        }
                    }
                };
            }

            processTAGFileRequest.Execute(arg);
        }

        public static void ProcessTAGFiles(Guid projectID, string[] files)
        {
            Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);

            processTAGFileRequest = processTAGFileRequest ?? new ProcessTAGFileRequest();
            ProcessTAGFileRequestArgument arg = new ProcessTAGFileRequestArgument
            {
                ProjectID = projectID,
                AssetID = machine.ID,
                TAGFiles = new List<ProcessTAGFileRequestFileItem>()
            };

            foreach (string file in files)
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, bytes.Length);

                    arg.TAGFiles.Add(new ProcessTAGFileRequestFileItem { FileName = Path.GetFileName(file), TagFileContent = bytes });     
                }
            }

            processTAGFileRequest.Execute(arg);
        }

        public static void SubmitTAGFiles(Guid projectID, string[] files)
        {
            Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);

            foreach (string file in files)
                SubmitSingleTAGFile(projectID, machine.ID, file);
        }

        public static void ProcessTAGFilesInFolder(Guid projectID, string folder)
        {
            // If it is a single file, just process it
            if (File.Exists(folder))
            {
                // ProcessTAGFiles(projectID, new string[] { folder });
                SubmitTAGFiles(projectID, new [] { folder });
            }
            else
            {
                string[] folders = Directory.GetDirectories(folder);
                foreach (string f in folders)
                {
                    ProcessTAGFilesInFolder(projectID, f);
                }

                // ProcessTAGFiles(projectID, Directory.GetFiles(folder));
                SubmitTAGFiles(projectID, Directory.GetFiles(folder));
            }
        }

        public static void ProcessMachine333TAGFiles(Guid projectID)
        {
            ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Machine333");
        }

        public static void ProcessMachine10101TAGFiles(Guid projectID)
        {
            ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Machine10101");
        }

        static void Main(string[] args)
        {
            string logFileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".log";
            GlobalContext.Properties["LogName"] = logFileName;
            log4net.Config.XmlConfigurator.Configure();

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

                Guid projectID = Guid.Empty;
                string folderPath;
                try
                {
                    projectID = Guid.Parse(args[0]);
                    folderPath = args[1];
                }
                catch
                {
                    Console.WriteLine($"Invalid project ID {args[0]} or folder path {args[1]}");
                    return;
                }

                if (projectID == Guid.Empty)
                {
                    return;
               }

                // Obtain a TAGFileProcessing client server
                TAGFileProcessingClientServer TAGServer = new TAGFileProcessingClientServer();

                // Ensure the continuous query service is installed
                TAGFileBufferQueueServiceProxy proxy = new TAGFileBufferQueueServiceProxy();
                try
                {
                    proxy.Deploy();
                }
                catch (Exception e)
                {
                    Log.Error($"Exception occurred deploying service: {e}");
                }

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
