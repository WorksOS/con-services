using VSS.TRex.TAGFiles.Classes.Integrator;
using System;
using VSS.TRex.Events;
using VSS.TRex.Machines;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
        public class AggregatedDataIntegratorTests : IClassFixture<DITagFileFixture>
  {
        [Fact]
        public void Test_AggregatedDataIntegrator_AddTaskToProcessList()
        {
            var integrator = new AggregatedDataIntegrator();

            SiteModel siteModel = new SiteModel(/*"TestName", "TestDesc", */Guid.NewGuid(), 1.0);
            IMachinesList machines = new MachinesList();
            machines.Add(new Machine("TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false));
            ServerSubGridTree tree = new ServerSubGridTree(siteModel.ID, StorageMutability.Mutable);
            MachinesProductionEventLists events = new MachinesProductionEventLists(siteModel, 1);

            integrator.AddTaskToProcessList(siteModel, siteModel.ID, machines, tree, 0, events);

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
