using System;
using System.Collections.Generic;
using System.IO;
using VSS.VisionLink.Raptor.Executors;
using VSS.VisionLink.Raptor.Machines;
using VSS.VisionLink.Raptor.SiteModels;
using VSSTests.TRex.Tests.Common;
using Xunit;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.Integrator.Tests
{
    public class AggregatedDataIntegratorWorkerTests
    {
        [Fact(Skip = "Not Implemented")]
        public void Test_AggregatedDataIntegratorWorker_AggregatedDataIntegratorWorkerTest()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_AggregatedDataIntegratorWorker_AggregatedDataIntegratorWorkerTest1()
        {
            Assert.True(false);
        }

        [Fact()]
        public void Test_AggregatedDataIntegratorWorker_ProcessTask()
        {
            // Convert a TAG file using a TAGFileConverter into a mini-site model
            TAGFileConverter converter = new TAGFileConverter();

            Assert.True(converter.Execute(new FileStream(TAGTestConsts.TestTAGFileName(), FileMode.Open, FileAccess.Read)),
                "Converter execute returned false");

            // Create the site model and machine etc to aggregate the processed TAG file into
            SiteModel siteModel = new SiteModel("TestName", "TestDesc", 1, 1.0);
            Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);
            // ISubGridFactory factory = new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>();
            // ServerSubGridTree tree = new ServerSubGridTree(siteModel);
            // ProductionEventChanges events = new ProductionEventChanges(siteModel, machine.ID);

            // Create the integrator and add the processed TAG file to its processing list
            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();

            integrator.AddTaskToProcessList(siteModel, machine, converter.SiteModelGridAggregator, converter.ProcessedCellPassCount, converter.MachineTargetValueChangesAggregator);

            // Construct an integration worker and ask it to perform the integration
            List<AggregatedDataIntegratorTask> ProcessedTasks = new List<AggregatedDataIntegratorTask>();

            AggregatedDataIntegratorWorker worker = new AggregatedDataIntegratorWorker(integrator.TasksToProcess);
            worker.ProcessTask(ProcessedTasks);

            Assert.True(1 == ProcessedTasks.Count);
        }
    }
}