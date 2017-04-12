using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
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
        public static void TestTileRendering()
        {
            int ID = 1;
            int gridCuts = 10; // eg: A 4x4 grid of tiles

            SiteModel siteModel = SiteModels.SiteModels.Instance().GetSiteModel(ID, false);

            // Get the project extent so we know where to render
            BoundingWorldExtent3D extents = ProjectExtents.ProductionDataOnly(ID);

            if (extents.IsValidPlanExtent)
            {
                try
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
                            (ID,
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
                catch (Exception E)
                {
                    throw;
                }
            }
        }

        public static void ProcessSingleTAGFile(string fileName)
        {
            // Create the site model and machine etc to aggregate the processed TAG file into
            SiteModel siteModel = SiteModels.SiteModels.Instance().GetSiteModel(2, true);
            Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, 0, false);

            // Convert a TAG file usign a TAGFileConverter into a mini-site model
            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();
            TAGFileConverter converter = new TAGFileConverter();

            // converter.Execute(new FileStream(TAGTestConsts.TestDataFilePath() + "TAGFiles\\TestTAGFile.tag", FileMode.Open, FileAccess.Read));
            converter.Execute(new FileStream(fileName, FileMode.Open, FileAccess.Read));

            converter.SiteModel.ID = siteModel.ID;
            converter.Machine.ID = machine.ID;

            // ISubGridFactory factory = new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>();
            // ServerSubGridTree tree = new ServerSubGridTree(siteModel);
            // ProductionEventChanges events = new ProductionEventChanges(siteModel, machine.ID);

            // Create the integrator and add the processed TAG file to its processing list
            integrator.AddTaskToProcessList(converter.SiteModel, converter.Machine, converter.SiteModelGridAggregator, converter.ProcessedCellPassCount, converter.MachineTargetValueChangesAggregator);

            // Construct an integration worker and ask it to perform the integration
            List<AggregatedDataIntegratorTask> ProcessedTasks = new List<AggregatedDataIntegratorTask>();
            AggregatedDataIntegratorWorker worker = new AggregatedDataIntegratorWorker(StorageProxy.Instance(), integrator.TasksToProcess);

            try
            {
                worker.ProcessTask(ProcessedTasks);
            }
            catch (Exception E)
            {
                throw;
            }
        }

        public static void ProcessTAGFilesInFolder(string folder)
        {
            int startcount = 0;
            int stopcount = 2000;
            int count = 0;

            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();
            AggregatedDataIntegratorWorker worker = new AggregatedDataIntegratorWorker(StorageProxy.Instance(), integrator.TasksToProcess);

            // Create the site model and machine etc to aggregate the processed TAG file into
            SiteModel siteModel = SiteModels.SiteModels.Instance().GetSiteModel(2, true);
            Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, 0, false);

            string[] files = Directory.GetFiles(folder);
            foreach (string fileName in files)
            {
                if (startcount-- <= 0)
                {
                    Console.WriteLine("Processing TAG file #{0}, {1}", ++count, fileName);
                    TAGFileConverter converter = new TAGFileConverter();

                    // converter.Execute(new FileStream(TAGTestConsts.TestDataFilePath() + "TAGFiles\\TestTAGFile.tag", FileMode.Open, FileAccess.Read));
                    using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    {
                        converter.Execute(fs);
                    }

                    converter.SiteModel.ID = siteModel.ID;
                    converter.Machine.ID = machine.ID;

                    try
                    {
                        List<AggregatedDataIntegratorTask> ProcessedTasks = new List<AggregatedDataIntegratorTask>();
                        integrator.AddTaskToProcessList(converter.SiteModel, converter.Machine, converter.SiteModelGridAggregator, converter.ProcessedCellPassCount, converter.MachineTargetValueChangesAggregator);
                        worker.ProcessTask(ProcessedTasks);
                    }
                    catch (Exception E)
                    {
                        throw;
                    }

                    stopcount--;
                    if (stopcount == 0)
                    {
                        break;
                    }
                }
            }
        }

        public static void ProcessMachine333TAGFiles()
        {
            ProcessTAGFilesInFolder(TAGTestConsts.TestDataFilePath() + "TAGFiles\\Machine333");
        }

        public static void ProcessMachine10101TAGFiles()
        {
            ProcessTAGFilesInFolder(TAGTestConsts.TestDataFilePath() + "TAGFiles\\Machine10101");
        }

        static void Main(string[] args)
        {
            // Obtain a TAGFileProcessign client server
            TAGFileProcessingServer TAGServer = new TAGFileProcessingServer();
                
            // ProcessMachine10101TAGFiles();
            // ProcessMachine333TAGFiles();
            ProcessSingleTAGFile(TAGTestConsts.TestDataFilePath() + "TAGFiles\\Machine10101\\2085J063SV--C01 XG 01 YANG--160804061209.tag");
            //ProcessSingleTAGFile();
            // Process all TAG files for project 4733:
            //ProcessTAGFilesInFolder(TAGTestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 1");
            //ProcessTAGFilesInFolder(TAGTestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 2");
            //ProcessTAGFilesInFolder(TAGTestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 3");
            //ProcessTAGFilesInFolder(TAGTestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 4");

            // Test out tile rendering against the processed TAG file data
            TestTileRendering();
        }
    }
}
