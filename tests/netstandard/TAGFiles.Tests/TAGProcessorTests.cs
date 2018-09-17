using System;
using System.Linq;
using VSS.TRex.Events;
using VSS.TRex.Geometry;
using VSS.TRex.Machines;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace TAGFiles.Tests
{
        public class TAGProcessorTests : IClassFixture<DITagFileTestsDIFixture>
    {
        [Fact()]
        public void Test_TAGProcessor_Creation()
        {
            var SiteModel = new SiteModel();
            var Machine = new VSS.TRex.Machines.Machine();
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel.ID);
            var MachineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);
        }

        [Fact()]
        public void Test_TAGProcessor_ProcessEpochContext()
        {
            var SiteModel = new SiteModel();
            var Machine = new VSS.TRex.Machines.Machine();
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel.ID);
            var MachineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

            // Set the blade left and right tip locations to a trivial epoch, the epoch and do it again to trigger a swathing scan, then 
            // check to see if it generated anything!

            Fence interpolationFence = new Fence();
            interpolationFence.SetRectangleFence(0, 0, 1, 1);

            DateTime StartTime = new DateTime(2000, 1, 1, 1, 1, 1);
            processor.DataLeft = new XYZ(0, 0, 5);
            processor.DataRight = new XYZ(1, 0, 5);
            processor.DataTime = StartTime;

            Assert.True(processor.ProcessEpochContext(), "ProcessEpochContext returned false in default TAGProcessor state (1)");

            DateTime EndTime = new DateTime(2000, 1, 1, 1, 1, 3);
            processor.DataLeft = new XYZ(0, 1, 5);
            processor.DataRight = new XYZ(1, 1, 5);
            processor.DataTime = EndTime;

            Assert.True(processor.ProcessEpochContext(), "ProcessEpochContext returned false in default TAGProcessor state (2)");

            Assert.Equal(9, processor.ProcessedCellPassesCount);

            Assert.Equal(2, processor.ProcessedEpochCount);
        }

        [Fact()]
        public void Test_TAGProcessor_DoPostProcessFileAction()
        {
            var SiteModel = new SiteModel();
            var Machine = new VSS.TRex.Machines.Machine();
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel.ID);
            var MachineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

            // Set the state of the processor to emulate the end of processing this TAG file at which point the processor should emit
            // a "Stop recording event". In this instance, the NoGPSModeSet flag will also be true which should trigger emission of 
            // a 'NoGPS' GPS mode state event and a 'UTS' positioning technology state event

            DateTime eventDate = new DateTime(2000, 1, 1, 1, 1, 1);

            // Setting the first data time will create the start event
            processor.DataTime = eventDate;

            DateTime eventDate2 = eventDate.AddMinutes(1);
            processor.DataTime = eventDate2;
            processor.DoPostProcessFileAction(true);

            Assert.True(MachineTargetValueChangesAggregator.GPSModeStateEvents.LastStateValue() == GPSMode.NoGPS &&
                          MachineTargetValueChangesAggregator.GPSModeStateEvents.LastStateDate() == eventDate,
                          "DoPostProcessFileAction did not set GPSMode event");

            Assert.True(MachineTargetValueChangesAggregator.PositioningTechStateEvents.LastStateValue() == PositioningTech.UTS &&
                          MachineTargetValueChangesAggregator.PositioningTechStateEvents.LastStateDate().Date == eventDate,
                          "DoPostProcessFileAction did not set positioning tech event");

            Assert.True(MachineTargetValueChangesAggregator.StartEndRecordedDataEvents.LastStateValue() == ProductionEventType.EndEvent /*EndRecordedData*/ &&
                          MachineTargetValueChangesAggregator.StartEndRecordedDataEvents.LastStateDate().Date == eventDate2,
                          "DoPostProcessFileAction did not set end recorded data event");
        }

        [Fact()]
        public void Test_TAGProcessor_DoEpochPreProcessAction()
        {
            var SiteModel = new SiteModel();
            var Machine = new Machine();
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel.ID);
            var MachineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

            Assert.True(processor.DoEpochPreProcessAction(), "EpochPreProcessAction returned false in default TAGProcessor state");

            // Current PreProcessAction activity is limited to handling proofing runs. This will be handled by proofing run tests elsewhere
        }
    }
}
