using System.IO;
using FluentAssertions;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.Types;
using Xunit;

namespace TAGFiles.Tests
{
  public class TAGFilePreScanTests
  {
    [Fact()]
    public void Test_TAGFilePreScan_Creation()
    {
      TAGFilePreScan preScan = new TAGFilePreScan();

      preScan.ReadResult.Should().Be(TAGReadResult.NoError);
      preScan.SeedLatitude.Should().BeNull();
      preScan.SeedLongitude.Should().BeNull();
      preScan.RadioType.Should().Be(string.Empty);
      preScan.RadioSerial.Should().Be(string.Empty);
      preScan.MachineType.Should().Be(CellPassConsts.MachineTypeNull);
      preScan.MachineID.Should().Be(string.Empty);
      preScan.HardwareID.Should().Be(string.Empty);
      preScan.SeedHeight.Should().BeNull();
      preScan.SeedTimeUTC.Should().BeNull();
      preScan.DesignName.Should().Be(string.Empty);
      preScan.ApplicationVersion.Should().Be(string.Empty);

    }

    [Fact()]
    public void Test_TAGFilePreScan_Execute()
    {
      TAGFilePreScan preScan = new TAGFilePreScan();

      Assert.True(preScan.Execute(new FileStream(Path.Combine("TestData", "TAGFiles", "TestTAGFile.tag"), FileMode.Open, FileAccess.Read)),
          "Prescan execute returned false");

      preScan.ProcessedEpochCount.Should().Be(1478);
      preScan.ReadResult.Should().Be(TAGReadResult.NoError);
      preScan.SeedLatitude.Should().Be(0.8551829920414814);
      preScan.SeedLongitude.Should().Be(-2.1377653549870974);
      preScan.SeedHeight.Should().Be(25.045071376845993);
      preScan.SeedTimeUTC.Should().Be(System.DateTime.Parse("26/08/2014 5:40:39.355 PM"));
      preScan.RadioType.Should().Be("torch");
      preScan.RadioSerial.Should().Be("5411502448");
      preScan.MachineType.Should().Be(39);
      preScan.MachineID.Should().Be("CB54XW  JLM00885");
      preScan.HardwareID.Should().Be("0523J019SW");
      preScan.DesignName.Should().Be("䌀䄀吀 䐀䄀夀 ㈀㈀");
      preScan.ApplicationVersion.Should().Be("12.61-75222");
    }
  }
}
