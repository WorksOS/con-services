using VSS.VisionLink.Raptor.Executors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using System.IO;
using VSS.VisionLink.Raptor.TAGFiles.Tests;
using VSS.VisionLink.Raptor.Cells;
using VSSTests.TRex.Tests.Common;
using Xunit;

namespace VSS.VisionLink.Raptor.Executors.Tests
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
                preScan.RadioType == String.Empty &&
                preScan.RadioSerial == String.Empty &&
                preScan.MachineType == CellPass.MachineTypeNull &&
                preScan.MachineID == String.Empty &&
                preScan.HardwareID == String.Empty,
                "TAGFilePreScan not constructed as expected");
        }

        [Fact()]
        public void Test_TAGFilePreScan_Execute()
        {
            TAGFilePreScan preScan = new TAGFilePreScan();

            Assert.True(preScan.Execute(new FileStream(TAGTestConsts.TestDataFilePath() + "TAGFiles\\TestTAGFile.tag", FileMode.Open, FileAccess.Read)),
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