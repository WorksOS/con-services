using System;
using System.Collections;
using System.IO;
using VSS.TRex;
using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.netcore
{
    public class SubGridCellSegmentPassesDataWrapper_Static_Compressed_Tests
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
                Time = new DateTime(2000, 1, 2, 3, 4, 5)
            };
        }

        /// <summary>
        /// Tests that the machine ID set for an uncompressed non static cell pass wrapper is null (as expected)
        /// </summary>
        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_NoMachineIDSet_Test()
        {
            CellPass[,][] cellPasses = new CellPass[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension][];

            // Create each sub array and add a test cell pass to it
            SubGridUtilities.SubGridDimensionalIterator((x, y) => cellPasses[x, y] = new[] { TestCellPass() });


            ISubGridCellSegmentPassesDataWrapper item = new SubGridCellSegmentPassesDataWrapper_StaticCompressed();

            // Feed the cell passes to the segment and ask it to serialise itself which will create the amchien ID set
            item.SetState(cellPasses);
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                item.Write(writer);
            }

            BitArray MachineIDSet = item.GetMachineIDSet();

            // Check there is a machine ID set, and it contains only a single machine, beign the numebr of the internal machine ID used to construct TestPass
            Assert.True(MachineIDSet  != null, "Static compressed pass wrapper returned null machine ID set");
            Assert.Equal(_InternalMachineID + 1, MachineIDSet.Length);
            Assert.True(MachineIDSet[_InternalMachineID]);

            for (int i = 0; i < MachineIDSet.Length - 1; i++)
                Assert.False(MachineIDSet[i]);

        }
    }
}
