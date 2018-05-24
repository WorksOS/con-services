using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.TAGFiles.Types;
using VSSTests.TRex.Tests.Common;
using Xunit;

namespace VSS.TRex.Executors.Tests
{
        public class TAGFilePreScanTests
    {
        [Fact()]
        public void Test_TAGFilePreScan_Creation()
        {
            TAGFilePreScan preScan = new TAGFilePreScan();

            Assert.True(preScan.ProcessedEpochCount == 0 &&
                preScan.ReadResult == TAGReadResult.NoError &&
                preScan.SeedLatitude == null &&
                preScan.SeedLongitude == null &&
                preScan.RadioType == string.Empty &&
                preScan.RadioSerial == string.Empty &&
                preScan.MachineType == CellPass.MachineTypeNull &&
                preScan.MachineID == string.Empty &&
                preScan.HardwareID == string.Empty,
                "TAGFilePreScan not constructed as expected");
        }

        [Fact()]
        public void Test_TAGFilePreScan_Execute()
        {
            TAGFilePreScan preScan = new TAGFilePreScan();

            Assert.True(preScan.Execute(new FileStream(Path.Combine("TestData", "TAGFiles", "TestTAGFile.tag"), FileMode.Open, FileAccess.Read)),
                "Prescan execute returned false");

            Assert.True(preScan.ProcessedEpochCount == 1478 &&
                preScan.ReadResult == TAGReadResult.NoError &&
                preScan.SeedLatitude == 0.8551829920414814 && // 0.8551829920414814
                preScan.SeedLongitude == -2.1377653549870974 && // -2.1377653549870974
                preScan.RadioType == "torch" &&
                preScan.RadioSerial == "5411502448" &&
                preScan.MachineType == 39 &&
                preScan.MachineID == "CB54XW  JLM00885" &&
                preScan.HardwareID == "0523J019SW",
                "TAGFilePreScan execution did not return expected results");
        }
    }
}