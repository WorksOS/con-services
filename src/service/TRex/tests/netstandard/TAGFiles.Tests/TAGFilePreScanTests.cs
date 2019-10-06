using System.IO;
using FluentAssertions;
using VSS.TRex.Types.CellPasses;
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
          "Pre-scan execute returned false");

      preScan.ProcessedEpochCount.Should().Be(1478);
      preScan.ReadResult.Should().Be(TAGReadResult.NoError);
      preScan.SeedLatitude.Should().Be(0.8551829920414814);
      preScan.SeedLongitude.Should().Be(-2.1377653549870974);
      preScan.SeedHeight.Should().Be(25.045071376845993);
      preScan.SeedTimeUTC.Should().Be(System.DateTime.Parse("2014-08-26T17:40:39.3550000", System.Globalization.NumberFormatInfo.InvariantInfo));
      preScan.RadioType.Should().Be("torch");
      preScan.RadioSerial.Should().Be("5411502448");
      preScan.MachineType.Should().Be(39);
      preScan.MachineID.Should().Be("CB54XW  JLM00885");
      preScan.HardwareID.Should().Be("0523J019SW");
      preScan.DesignName.Should().Be("CAT DAY 22");
      preScan.ApplicationVersion.Should().Be("12.61-75222");
    }

    [Fact()]
    public void Test_TAGFilePreScan_Execute_JapaneseDesign()
    {
      TAGFilePreScan preScan = new TAGFilePreScan();

      Assert.True(preScan.Execute(new FileStream(Path.Combine("TestData", "TAGFiles", "JapaneseDesignTagfileTest.tag"), FileMode.Open, FileAccess.Read)),
        "Pre-scan execute returned false");

      preScan.ProcessedEpochCount.Should().Be(1222);
      preScan.ReadResult.Should().Be(TAGReadResult.NoError);
      preScan.SeedLatitude.Should().Be(0.65955923731934751);
      preScan.SeedLongitude.Should().Be(2.45317108556434);
      preScan.SeedHeight.Should().Be(159.53982475668218);
      preScan.SeedTimeUTC.Should().Be(System.DateTime.Parse("2019-06-17T01:43:14.8640000", System.Globalization.NumberFormatInfo.InvariantInfo));
      preScan.RadioType.Should().Be("torch");
      preScan.RadioSerial.Should().Be("5750F00368");
      preScan.MachineType.Should().Be(25);
      preScan.MachineID.Should().Be("320E03243");
      preScan.HardwareID.Should().Be("3337J201SW");
      preScan.DesignName.Should().Be("所沢地区　NO.210-NO.255");
      preScan.ApplicationVersion.Should().Be("13.11-RC1");
    }
  }
}
