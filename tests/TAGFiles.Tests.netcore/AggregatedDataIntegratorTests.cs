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
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Machines;
using Xunit;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.Integrator.Tests
{
        public class AggregatedDataIntegratorTests
    {
        [Fact()]
        public void Test_AggregatedDataIntegrator_AddTaskToProcessList()
        {
            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();

            SiteModel siteModel = new SiteModel("TestName", "TestDesc", 1, 1.0, null);
            Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, 0, false);
            ISubGridFactory factory = new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>();
            ServerSubGridTree tree = new ServerSubGridTree(siteModel);
            ProductionEventLists events = new ProductionEventLists(siteModel, machine.ID);

            integrator.AddTaskToProcessList(siteModel, machine, tree, 0, events);

            Assert.Equal(1, integrator.CountOfTasksToProcess);
            Assert.True(integrator.CanAcceptMoreAggregatedCellPasses, "CanAcceptMoreAggregatedCellPasses is false");
        }

        [Fact()]
        public void Test_AggregatedDataIntegrator_Creation()
        {
            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();

            Assert.True(integrator.CanAcceptMoreAggregatedCellPasses, "CanAcceptMoreAggregatedCellPasses is false");
            Assert.Equal(0, integrator.CountOfTasksToProcess);
        }

        [Fact()]
        public void Test_AggregatedDataIntegrator_GetStatistics()
        {
            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();

            integrator.GetStatistics(out int outstandingCellPasses, out long totalCellPassesProcessed, out int pendingFilesToBeProcessed);

            Assert.Equal(0, outstandingCellPasses);
            Assert.Equal(0, pendingFilesToBeProcessed);
            Assert.Equal(0, totalCellPassesProcessed);
        }

        [Fact()]
        public void Test_AggregatedDataIntegrator_IncrementOutstandingCellPasses()
        {
            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();

            integrator.IncrementOutstandingCellPasses(1000);

            integrator.GetStatistics(out int outstandingCellPasses, out long totalCellPassesProcessed, out int pendingFilesToBeProcessed);

            Assert.Equal(1000, outstandingCellPasses);
        }
    }
}