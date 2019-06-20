using System;
using System.Collections;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.IO;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
    public class SubGridCellSegmentPassesDataWrapper_Static_Compressed_Tests : IClassFixture<DILoggingFixture>
    {
        private const int _InternalMachineID = 105;

        /// <summary>
        /// A handy test cell pass for the unit tests below to use
        /// </summary>
        /// <returns></returns>
        private CellPass TestCellPass()
        {
            return new CellPass()
            {
                Amplitude = 100,
                CCA = 101,
                CCV = 102,
                Frequency = 103,
                gpsMode = GPSMode.Fixed,
                HalfPass = false,
                Height = 104,
                InternalSiteModelMachineIndex = _InternalMachineID,
                GPSModeStore = 106,
                MachineSpeed = 106,
                MaterialTemperature = 107,
                MDP = 108,
                PassType = PassType.Track,
                RadioLatency = 109,
                RMV = 110,
                Time = DateTime.SpecifyKind(new DateTime(2000, 1, 2, 3, 4, 5), DateTimeKind.Utc)
            };
        }

        /// <summary>
        /// Tests that the machine ID set for an uncompressed non static cell pass wrapper is null (as expected)
        /// </summary>
        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_NoMachineIDSet_Test()
        {
            var cellPasses = new Cell_NonStatic[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

            // Create each sub array and add a test cell pass to it
            SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
              cellPasses[x, y].Passes = new TRexSpan<CellPass>(new CellPass[1], 0, 1, false, false);
              cellPasses[x, y].Passes.Add(TestCellPass());
            });

            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_StaticCompressed();

            // Feed the cell passes to the segment and ask it to serialise itself which will create the machine ID set
            item.SetState(cellPasses);
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream(Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION)))
            {
                item.Write(writer);
            }

            BitArray MachineIDSet = item.GetMachineIDSet();

            // Check there is a machine ID set, and it contains only a single machine, being the number of the internal machine ID used to construct TestPass
            Assert.True(MachineIDSet  != null, "Static compressed pass wrapper returned null machine ID set");
            Assert.Equal(_InternalMachineID + 1, MachineIDSet.Length);
            Assert.True(MachineIDSet[_InternalMachineID]);

            for (int i = 0; i < MachineIDSet.Length - 1; i++)
                Assert.False(MachineIDSet[i]);

        }
    }
}
