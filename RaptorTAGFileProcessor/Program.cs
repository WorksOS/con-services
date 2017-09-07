using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.Machines;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.Servers.Client;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Storage;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.TAGFiles.Classes.Integrator;
using VSS.VisionLink.Raptor.TAGFiles.Tests;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Client
{
    class Program
    {
        private static ILog Log = null;
        private static int tAGFileCount = 0;

        public static void TestTileRendering(long projectID)
        {
            int gridCuts = 10; // eg: A 4x4 grid of tiles

            SiteModel siteModel = SiteModels.SiteModels.Instance().GetSiteModel(projectID, false);

            // Get the project extent so we know where to render
            BoundingWorldExtent3D extents = ProjectExtents.ProductionDataOnly(projectID);

            if (extents.IsValidPlanExtent)
            {
                // Modify extents to be a square area with the data to be rendered centered on it
                if (extents.SizeX > extents.SizeY)
                {
                    double Delta = (extents.SizeX - extents.SizeY) / 2;
                    extents.MinY -= Delta;
                    extents.MaxY += Delta;
                }
                else
                {
                    double Delta = (extents.SizeY - extents.SizeX) / 2;
                    extents.MinX -= Delta;
                    extents.MaxX += Delta;
                }

                double tileSize = extents.SizeX / gridCuts;

                for (int I = 0; I < gridCuts; I++)
                {
                    for (int J = 0; J < gridCuts; J++)
                    {
                        BoundingWorldExtent3D renderExtents = new BoundingWorldExtent3D
                            (extents.MinX + I * tileSize, extents.MinY + J * tileSize,
                             extents.MinX + (I + 1) * tileSize, extents.MinY + (J + 1) * tileSize);

                        Bitmap bmp = RaptorTileRenderingServer.NewInstance().RenderTile(new TileRenderRequestArgument
                        (projectID,
                         DisplayMode.Height,
                         renderExtents,
                         true, // CoordsAreGrid
                         500, // PixelsX
                         500, // PixelsY
                         new CombinedFilter(siteModel) // Filter1
                             {
                             SpatialFilter = new CellSpatialFilter()
                             {
                                 CoordsAreGrid = true,
                                 IsSpatial = true,
                                 Fence = new Fence(renderExtents)
                             }
                         },
                         null // filter 2
                        ));

                        if (bmp != null)
                        {
                            bmp.Save(String.Format("c:\\temp\\raptorignitedata\\bitmap{0}x{1}.bmp", I, J));
                            bmp.Save(String.Format("c:\\temp\\raptorignitedata\\bitmap{0}x{1}.png", I, J), ImageFormat.Png);
                        }
                    }
                }
            }
        }

        public static void ProcessSingleTAGFile(long projectID, string fileName)
        {
            // Create the site model and machine etc to aggregate the processed TAG file into
            SiteModel siteModel = SiteModels.SiteModels.Instance().GetSiteModel(projectID, true);
            Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, 0, false);

            // Convert a TAG file using a TAGFileConverter into a mini-site model
            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();
            TAGFileConverter converter = new TAGFileConverter();

            converter.Execute(new FileStream(fileName, FileMode.Open, FileAccess.Read));

            converter.SiteModel.ID = siteModel.ID;
            converter.Machine.ID = machine.ID;

            // Create the integrator and add the processed TAG file to its processing list
            integrator.AddTaskToProcessList(converter.SiteModel, converter.Machine, converter.SiteModelGridAggregator, converter.ProcessedCellPassCount, converter.MachineTargetValueChangesAggregator);

            // Construct an integration worker and ask it to perform the integration
            List<AggregatedDataIntegratorTask> ProcessedTasks = new List<AggregatedDataIntegratorTask>();
            AggregatedDataIntegratorWorker worker = new AggregatedDataIntegratorWorker(/*StorageProxy.Instance(), */integrator.TasksToProcess);

            worker.ProcessTask(ProcessedTasks);
        }

        public static void ProcessTAGFiles(long projectID, string[] files)
        {
            int batchSize = 20;
            int batchCount = 0;

            // Create the integration machinery responsibvle for tracking tasks and integrating them into the database
            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();
            AggregatedDataIntegratorWorker worker = new AggregatedDataIntegratorWorker(/*StorageProxy.Instance(), */integrator.TasksToProcess);
            List<AggregatedDataIntegratorTask> ProcessedTasks = new List<AggregatedDataIntegratorTask>();

            // Create the site model and machine etc to aggregate the processed TAG file into
            SiteModel siteModel = SiteModels.SiteModels.Instance().GetSiteModel(projectID, true);
            Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, 0, false);

            // Process each file into a task, and batch tasks into groups for integration to reduce the number of cache 
            // updates made for subgrid changes
            foreach (string fileName in files)
            {
                Log.Info(String.Format("Processing TAG file #{0}, {1}", ++tAGFileCount, fileName));

                TAGFileConverter converter = new TAGFileConverter();

                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    converter.Execute(fs);
                }

                converter.SiteModel.ID = siteModel.ID;
                converter.Machine.ID = machine.ID;

                integrator.AddTaskToProcessList(converter.SiteModel, converter.Machine, converter.SiteModelGridAggregator, converter.ProcessedCellPassCount, converter.MachineTargetValueChangesAggregator);
               
                if (++batchCount >= batchSize)
                {
                    worker.ProcessTask(ProcessedTasks);
                    batchCount = 0;
                }
            }

            if (batchCount > 0)
            {
                worker.ProcessTask(ProcessedTasks);
            }
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
                TAGFileProcessingServer TAGServer = new TAGFileProcessingServer();

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

                // Test out tile rendering against the processed TAG file data
               // TestTileRendering(projectID);
            }
            finally
            {
                Console.ReadKey();
            }
        }
    }
}
