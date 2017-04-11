using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.TAGFiles.Classes.Integrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.Events;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.Integrator.Tests
{
    [TestClass()]
    public class AggregatedDataIntegratorTests
    {
        [TestMethod()]
        public void Test_AggregatedDataIntegrator_AddTaskToProcessList()
        {
            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();

            SiteModel siteModel = new SiteModel("TestName", "TestDesc", 1, 1.0);
            Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, 0, false);
            ISubGridFactory factory = new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>();
            ServerSubGridTree tree = new ServerSubGridTree(siteModel);
            ProductionEventChanges events = new ProductionEventChanges(siteModel, machine.ID);

            integrator.AddTaskToProcessList(siteModel, machine, tree, 0, events);

            Assert.IsTrue(integrator.CountOfTasksToProcess == 1, "Tasks to process count is not 1");
            Assert.IsTrue(integrator.CanAcceptMoreAggregatedCellPasses, "CanAcceptMoreAggregatedCellPasses is false");
        }

        [TestMethod()]
        public void Test_AggregatedDataIntegrator_Creation()
        {
            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();

            Assert.IsTrue(integrator.CanAcceptMoreAggregatedCellPasses, "CanAcceptMoreAggregatedCellPasses is false");
            Assert.IsTrue(integrator.CountOfTasksToProcess == 0, "CountOfTasksToProcess is not zero");
        }

        [TestMethod()]
        public void Test_AggregatedDataIntegrator_GetStatistics()
        {
            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();

            int outstandingCellPasses = 0;
            long totalCellPassesProcessed = 0;
            int pendingFilesToBeProcessed = 0;

            integrator.GetStatistics(ref outstandingCellPasses, ref totalCellPassesProcessed, ref pendingFilesToBeProcessed);

            Assert.IsTrue(outstandingCellPasses == 0, "outstandingCellPasses is not zero");
            Assert.IsTrue(pendingFilesToBeProcessed == 0, "pendingFilesToBeProcessed is not zero");
            Assert.IsTrue(totalCellPassesProcessed == 0, "totalCellPassesProcessed is not zero");
        }

        [TestMethod()]
        public void Test_AggregatedDataIntegrator_IncrementOutstandingCellPasses()
        {
            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();

            integrator.IncrementOutstandingCellPasses(1000);

            int outstandingCellPasses = 0;
            long totalCellPassesProcessed = 0;
            int pendingFilesToBeProcessed = 0;

            integrator.GetStatistics(ref outstandingCellPasses, ref totalCellPassesProcessed, ref pendingFilesToBeProcessed);

            Assert.IsTrue(outstandingCellPasses == 1000, "outstandingCellPasses is not 1000");
        }
    }
}