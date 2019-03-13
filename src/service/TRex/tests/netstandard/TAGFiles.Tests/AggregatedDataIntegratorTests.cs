using VSS.TRex.TAGFiles.Classes.Integrator;
using System;
using VSS.TRex.Events;
using VSS.TRex.SiteModels;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
        public class AggregatedDataIntegratorTests : IClassFixture<DILoggingFixture>
  {
        [Fact]
        public void Test_AggregatedDataIntegrator_AddTaskToProcessList()
        {
            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();

            SiteModel siteModel = new SiteModel(/*"TestName", "TestDesc", */Guid.NewGuid(), 1.0);
            VSS.TRex.Machines.Machine machine = new VSS.TRex.Machines.Machine("TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);
            ServerSubGridTree tree = new ServerSubGridTree(siteModel.ID);
            ProductionEventLists events = new ProductionEventLists(siteModel, machine.InternalSiteModelMachineIndex);

            integrator.AddTaskToProcessList(siteModel, siteModel.ID, machine, machine.ID, tree, 0, events);

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
