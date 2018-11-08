using System;
using System.IO;
using VSS.TRex;
using System.Text;
using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Cells
{
        public class CellPassTests
    {
        public static CellPass ATestCellPass()
        {
            return new CellPass()
            {
                Amplitude = 1,
                CCA = 2,
                CCV = 3,
                Frequency = 4,
                gpsMode = GPSMode.AutonomousPosition,
                HalfPass = false,
                Height = 5,
                //MachineID = 6,
                InternalSiteModelMachineIndex = 6,
                MachineSpeed = 7,
                MaterialTemperature = 8,
                MDP = 9,
                PassType = PassType.Front,
                RadioLatency = 10,
                RMV = 11,
                Time = new DateTime(2017, 1, 1, 12, 30, 0)
            };
        }

        public static CellPass ATestCellPass2()
        {
            return new CellPass()
            {
                Amplitude = 10,
                CCA = 20,
                CCV = 30,
                Frequency = 40,
                gpsMode = GPSMode.DGPS,
                HalfPass = true,
                Height = 50,
                //MachineID = 60,
                InternalSiteModelMachineIndex = 60,
                MachineSpeed = 70,
                MaterialTemperature = 80,
                MDP = 90,
                PassType = PassType.Rear,
                RadioLatency = 100,
                RMV = 110,
                Time = new DateTime(2017, 1, 1, 12, 45, 0)
            };
        }

        /// <summary>
        /// Test creation of a new cell pass with no non-null values specified
        /// </summary>
        [Fact]
        public void Test_CellPass_CreateNullPass()
        {
            CellPass cp = new CellPass();
            cp.Clear();

            Assert.True(
                cp.Amplitude == CellPassConsts.NullAmplitude &&
                cp.CCA == CellPassConsts.NullCCA &&
                cp.CCV == CellPassConsts.NullCCV &&
                cp.Frequency == CellPassConsts.NullFrequency &&
                cp.gpsMode == CellPassConsts.NullGPSMode &&
                cp.HalfPass == false &&
                cp.Height == CellPassConsts.NullHeight &&
                //cp.MachineID == CellPass.NullMachineID &&
                cp.InternalSiteModelMachineIndex == CellPassConsts.NullInternalSiteModelMachineIndex &&
                cp.MachineSpeed == CellPassConsts.NullMachineSpeed &&
                cp.MaterialTemperature == CellPassConsts.NullMaterialTemperatureValue &&
                cp.MDP == CellPassConsts.NullMDP &&
                cp.PassType == PassType.Front &&
                cp.RadioLatency == CellPassConsts.NullRadioLatency &&
                cp.RMV == CellPassConsts.NullRMV &&
                cp.Time == CellPassConsts.NullTime,
                "Newly created/cleared CellPass does not contain all null values");
        }

        /// <summary>
        /// Test extraction of a machine ID and time as a pair returns the expected values
        /// </summary>
        [Fact]
        public void Test_CellPass_MachineIDAndTime()
        {
            CellPass cp = ATestCellPass();

            DateTime testTime = DateTime.Now;
            //cp.MachineID = 100;
            cp.InternalSiteModelMachineIndex = 100;
            cp.Time = testTime;

            //cp.MachineIDAndTime(out long MachineID, out DateTime Time);
            // Assert.True(MachineID == 100 && Time == testTime, "Machine ID and time are not the expected values");

            cp.MachineIDAndTime(out short internalSiteModelMachineIndex, out DateTime Time);
            Assert.True(internalSiteModelMachineIndex == 100 && Time == testTime, "Machine ID and time are not the expected values");
        }

        /// <summary>
        /// Test setting fields for vide stae off crrect resets the appropriate fields
        /// </summary>
        [Fact]
        public void Test_CellPass_SetFieldsFroVibeStateOff()
        {
            CellPass cp = ATestCellPass();

            Assert.False(cp.CCV == CellPassConsts.NullCCV ||
                           cp.RMV == CellPassConsts.NullRMV ||
                           cp.Frequency == CellPassConsts.NullFrequency ||
                           cp.Amplitude == CellPassConsts.NullAmplitude,
                           "One or more fields for vibe state off are already null, compromising the test");

            cp.SetFieldsForVibeStateOff();

            Assert.True(cp.CCV == CellPassConsts.NullCCV &&
                          cp.RMV == CellPassConsts.NullRMV &&
                          cp.Frequency == CellPassConsts.NullFrequency &&
                          cp.Amplitude == CellPassConsts.NullAmplitude, 
                          "Appropriate fields for vibe state off are not null");
        }

        /// <summary>
        /// Test the Equality comparer funcions as expected
        /// </summary>
        [Fact]
        public void Test_CellPass_EqualityCheck_Self()
        {
            CellPass cp1;

            cp1 = ATestCellPass();
            Assert.True(cp1.Equals(cp1), "Equality check on self failed (returned true)");
        }

        /// <summary>
        /// Test the Equality comparer funcions as expected
        /// </summary>
        [Fact]
        public void Test_CellPass_EqualityCheck()
        {
            CellPass cp1;
            CellPass cp2;

            cp1 = ATestCellPass();
            cp2 = ATestCellPass();
            Assert.True(cp1.Equals(cp2), "Equality check on identical cell passes failed (returned false)");

            cp2 = ATestCellPass2();
            Assert.False(cp1.Equals(cp2), "Equality check on different cell passes failed (returned true)");
        }

        /// <summary>
        /// Test the Equality comparer funcions as expected
        /// </summary>
        [Fact]
        public void Test_CellPass_AssignCellPasses()
        {
            CellPass cp1 = ATestCellPass();
            CellPass cp2 = ATestCellPass2();

            Assert.False(cp1.Equals(cp2), "Equality check on different cell passes failed (returned true)");

            cp1.Assign(cp2);
            Assert.True(cp1.Equals(cp2), "Equality check on assigned cell passes failed (returned false)");
        }

        /// <summary>
        /// Test reading and writing binary format
        /// </summary>
        [Fact]
        public void Test_CellPass_BinaryReadWrite()
        {
            CellPass cp1 = ATestCellPass();
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8, true);

            cp1.Write(bw);

            ms.Position = 0;
            BinaryReader br = new BinaryReader(ms, Encoding.UTF8, true);
            CellPass cp2 = new CellPass();

            cp2.Read(br);

            Assert.True(cp1.Equals(cp2), "Equality check on same cell passes failed after write then read (returned true)");

            // Check negative condition by writing and reading a second different cell pass then comparing the results of reading the two cell passes
            // to ensure they are different

            cp2 = ATestCellPass2();
            MemoryStream ms2 = new MemoryStream();
            BinaryWriter bw2 = new BinaryWriter(ms2, Encoding.UTF8, true);

            cp2.Write(bw2);

            ms2.Position = 0;
            BinaryReader br2 = new BinaryReader(ms2, Encoding.UTF8, true);

            cp2.Read(br2);

            Assert.False(cp1.Equals(cp2), "Equality check on different cell passes failed after write then read (returned true)");
        }

        /// <summary>
        /// Ensure the ToString() method returns non-null, and does not error out
        /// </summary>
        [Fact]
        public void Test_CellPass_ToString()
        {
            CellPass cp = ATestCellPass();

            Assert.False(string.IsNullOrEmpty(cp.ToString()), "ToString() result is null or empty");
        }
    }
}
