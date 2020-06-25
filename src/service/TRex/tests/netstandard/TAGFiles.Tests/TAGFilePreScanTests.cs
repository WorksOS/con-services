using System.IO;
using FluentAssertions;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace TAGFiles.Tests
{
  public class TAGFilePreScanTests : IClassFixture<DITagFileFixture>
  {
    [Fact()]
    public void Test_TAGFilePreScan_Creation()
    {
      TAGFilePreScan preScan = new TAGFilePreScan();

      preScan.ReadResult.Should().Be(TAGReadResult.NoError);
      preScan.SeedLatitude.Should().BeNull();
      preScan.SeedLongitude.Should().BeNull();
      preScan.SeedNorthing.Should().BeNull();
      preScan.SeedEasting.Should().BeNull();
      preScan.SeedElevation.Should().BeNull();
      preScan.SeedTimeUTC.Should().BeNull();
      preScan.RadioType.Should().Be(string.Empty);
      preScan.RadioSerial.Should().Be(string.Empty);
      preScan.MachineType.Should().Be(CellPassConsts.MachineTypeNull);
      preScan.MachineID.Should().Be(string.Empty);
      preScan.HardwareID.Should().Be(string.Empty);
      preScan.SeedHeight.Should().BeNull();
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
      TAGFilePreScan preScan = new TAGFilePreScan();

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
    public void Test_TAGFilePreScan_NEEposition()
    {
      //  Lat/Long refers to the GPS_BASE_Position
      //    therefore SeedLatitude and SeedLongitude not available
      //    NE can be used along with the projects CSIB to resolve to a LL

      var preScan = new TAGFilePreScan();

      Assert.True(preScan.Execute(new FileStream(Path.Combine("TestData", "TAGFiles", "SeedPosition-usingNEE.tag"), FileMode.Open, FileAccess.Read)),
        "Pre-scan execute returned false");

      preScan.ProcessedEpochCount.Should().Be(1);
      preScan.ReadResult.Should().Be(TAGReadResult.NoError);
      preScan.SeedLatitude.Should().Be(null);
      preScan.SeedLongitude.Should().Be(null);
      preScan.SeedHeight.Should().Be(null);
      preScan.SeedNorthing.Should().Be(5876814.5384829007);
      preScan.SeedEasting.Should().Be(7562822.7801738745);
      preScan.SeedElevation.Should().Be(127.31059507932183);
      preScan.SeedTimeUTC.Should().Be(System.DateTime.Parse("2020-06-05T23:29:53.761", System.Globalization.NumberFormatInfo.InvariantInfo));
      preScan.RadioType.Should().Be("torch");
      preScan.RadioSerial.Should().Be("5850F00892");
      preScan.MachineType.Should().Be(MachineType.Excavator);
      preScan.MachineID.Should().Be("M316F PAK115");
      preScan.HardwareID.Should().Be("1639J101YU");
      preScan.DesignName.Should().Be("L03P");
      preScan.ApplicationVersion.Should().Be("EW-1.11.0-2019_3 672");
    }

    [Fact()]
    public void Test_TAGFilePreScan_Execute_NoSeedLLH()
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
