using System.IO;
using FluentAssertions;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.TTM;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.Types;
using Xunit;
using Consts = VSS.TRex.Common.Consts;

namespace TAGFiles.Tests
{
  public class TAGFilePreScanTests
  {
    [Fact()]
    public void Test_TAGFilePreScan_Creation()
    {
      var preScan = new TAGFilePreScan();

      preScan.ReadResult.Should().Be(TAGReadResult.NoError);
      preScan.SeedLatitude.Should().BeNull();
      preScan.SeedLongitude.Should().BeNull();
      preScan.SeedHeight.Should().BeNull();
      preScan.SeedNorthing.Should().BeNull();
      preScan.SeedEasting.Should().BeNull();
      preScan.SeedElevation.Should().BeNull();
      preScan.SeedTimeUTC.Should().BeNull();
      preScan.RadioType.Should().Be(string.Empty);
      preScan.RadioSerial.Should().Be(string.Empty);
      preScan.MachineType.Should().Be(CellPassConsts.MachineTypeNull);
      preScan.MachineID.Should().Be(string.Empty);
      preScan.HardwareID.Should().Be(string.Empty);
      preScan.DesignName.Should().Be(string.Empty);
      preScan.ApplicationVersion.Should().Be(string.Empty);

    }

    [Fact()]
    public void Test_TAGFilePreScan_Execute()
    {
      var preScan = new TAGFilePreScan();

      Assert.True(preScan.Execute(new FileStream(Path.Combine("TestData", "TAGFiles", "TestTAGFile.tag"), FileMode.Open, FileAccess.Read)),
          "Pre-scan execute returned false");

      preScan.ProcessedEpochCount.Should().Be(1478);
      preScan.ReadResult.Should().Be(TAGReadResult.NoError);
      preScan.SeedLatitude.Should().Be(0.8551829920414814);
      preScan.SeedLongitude.Should().Be(-2.1377653549870974);
      preScan.SeedHeight.Should().Be(25.045071376845993);
      preScan.SeedNorthing.Should().Be(5427420.4410656113);
      preScan.SeedEasting.Should().Be(537671.61978842877);
      preScan.SeedElevation.Should().Be(41.549531624124079);
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
      var preScan = new TAGFilePreScan();

      Assert.True(preScan.Execute(new FileStream(Path.Combine("TestData", "TAGFiles", "JapaneseDesignTagfileTest.tag"), FileMode.Open, FileAccess.Read)),
        "Pre-scan execute returned false");

      preScan.ProcessedEpochCount.Should().Be(1222);
      preScan.ReadResult.Should().Be(TAGReadResult.NoError);
      preScan.SeedLatitude.Should().Be(0.65955923731934751);
      preScan.SeedLongitude.Should().Be(2.45317108556434);
      preScan.SeedHeight.Should().Be(159.53982475668218);
      preScan.SeedNorthing.Should().Be(198863.25259713328);
      preScan.SeedEasting.Should().Be(63668.71188769384);
      preScan.SeedElevation.Should().Be(114.32995182440192);
      preScan.SeedTimeUTC.Should().Be(System.DateTime.Parse("2019-06-17T01:43:14.8640000", System.Globalization.NumberFormatInfo.InvariantInfo));
      preScan.RadioType.Should().Be("torch");
      preScan.RadioSerial.Should().Be("5750F00368");
      preScan.MachineType.Should().Be(25);
      preScan.MachineID.Should().Be("320E03243");
      preScan.HardwareID.Should().Be("3337J201SW");
      preScan.DesignName.Should().Be("所沢地区　NO.210-NO.255");
      preScan.ApplicationVersion.Should().Be("13.11-RC1");
    }

    [Fact()]
    public void Test_TAGFilePreScan_Execute_NoSeedLLH_NoUTM()
    {
      var preScan = new TAGFilePreScan();

      Assert.True(preScan.Execute(new FileStream(Path.Combine("TestData", "TAGFiles", "TestTagFile_NoSeedLLHandNoUTM.tag"), FileMode.Open, FileAccess.Read)),
        "Pre-scan execute returned false");

      preScan.ProcessedEpochCount.Should().Be(272);
      preScan.ReadResult.Should().Be(TAGReadResult.NoError);
      preScan.SeedLatitude.Should().BeNull();
      preScan.SeedLongitude.Should().BeNull();
      preScan.SeedHeight.Should().BeNull();
      preScan.SeedNorthing.Should().Be(121931.74737617122);
      preScan.SeedEasting.Should().Be(564022.58097323333);
      preScan.SeedElevation.Should().Be(398.75510795260766);
      preScan.SeedTimeUTC.Should().Be(System.DateTime.Parse("2012-07-26T08:52:46.0350000", System.Globalization.NumberFormatInfo.InvariantInfo));
      preScan.RadioType.Should().Be("torch");
      preScan.RadioSerial.Should().Be("5123545219");
      preScan.MachineType.Should().Be(25);
      preScan.MachineID.Should().Be("B LHR934 S33251");
      preScan.HardwareID.Should().Be("1112J010SW");
      preScan.DesignName.Should().Be("Monthey Kontaktboden C2c6");
      preScan.ApplicationVersion.Should().Be("12.20-53094");
    }
  }
}
