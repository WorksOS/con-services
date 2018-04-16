using System;
using System.Linq;
using VSS.VisionLink.Raptor.Events;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Machines;
using Xunit;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.Tests
{
        public class TAGProcessorTests
    {
        [Fact()]
        public void Test_TAGProcessor_Creation()
        {
            var SiteModel = new SiteModel();
            var Machine = new Machine();
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel);
            var MachineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, long.MaxValue);

            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);
        }

        [Fact()]
        public void Test_TAGProcessor_ProcessEpochContext()
        {
            var SiteModel = new SiteModel();
            var Machine = new Machine();
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel);
            var MachineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, long.MaxValue);

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
            var Machine = new Machine();
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel);
            var MachineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, long.MaxValue);

            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

            // Set the state of the processor to emulate the end of processing this TAG file at which point the processor should emit
            // a "Stop recording event". In this instance, the NoGPSModeSet flag will also be true which should trigger emission of 
            // a 'NoGPS' GPS mode state event and a 'UTS' positioning technology state event

            DateTime eventDate = new DateTime(2000, 1, 1, 1, 1, 1);
            processor.DataTime = eventDate;
            processor.DoPostProcessFileAction(true);

            Assert.True(MachineTargetValueChangesAggregator.GPSModeStateEvents.Last().State == GPSMode.NoGPS &&
                          MachineTargetValueChangesAggregator.GPSModeStateEvents.Last().Date == eventDate,
                          "DoPostProcessFileAction did not set GPSMode event");

            Assert.True(MachineTargetValueChangesAggregator.PositioningTechStateEvents.Last().State == PositioningTech.UTS &&
                          MachineTargetValueChangesAggregator.PositioningTechStateEvents.Last().Date == eventDate,
                          "DoPostProcessFileAction did not set positioning tech event");

            Assert.True(MachineTargetValueChangesAggregator.StartEndRecordedDataEvents.Last().State == ProductionEventType.EndEvent /*EndRecordedData*/ &&
                          MachineTargetValueChangesAggregator.StartEndRecordedDataEvents.Last().Date == eventDate,
                          "DoPostProcessFileAction did not set end recorded data event");
        }

        [Fact()]
        public void Test_TAGProcessor_DoEpochPreProcessAction()
        {
            var SiteModel = new SiteModel();
            var Machine = new Machine();
            var SiteModelGridAggregator = new ServerSubGridTree(SiteModel);
            var MachineTargetValueChangesAggregator = new ProductionEventLists(SiteModel, long.MaxValue);

            TAGProcessor processor = new TAGProcessor(SiteModel, Machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

            Assert.True(processor.DoEpochPreProcessAction(), "EpochPreProcessAction returned false in default TAGProcessor state");

            // Current PreProcessAction activity is limited to handling proofing runs. This will be handled by proofing run tests elsewhere
        }
    }
}