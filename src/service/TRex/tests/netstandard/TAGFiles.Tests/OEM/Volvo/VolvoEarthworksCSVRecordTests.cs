using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using VSS.TRex.TAGFiles.Classes.OEM.Volvo;
using Xunit;

namespace TAGFiles.Tests.OEM.Volvo
{
  public class VolvoEarthworksCSVRecordTests
  {
    private const string HEADER = "Time,CellN_m,CellE_m,PassNumber,DesignName,Machine,Speed_mph,TargPassCount,ValidPos," +
      "Lift,LastEDV,LastFreq_Hz,LastAmp_mm,TargThickness_FT,MachineGear,VibeState,LastTemp_f,LastCMV,ICMVType";
    private const string LINE = "2020-Mar-11 14:19:07.150,7103556.75,452796.15,1,Undirfylling,SD115B,1.1408375079,6,Yes," +
      "1,0.0,0,-1000.0,0.0,Forward,On,32.0,0,2";

    [Fact]
    public void Creation()
    {
      var record = new VolvoEarthworksCSVRecord();
      record.Should().NotBeNull();
    }

    [Fact]
    public void Creation2()
    {
      var headerIndex = 0;
      var headerLocations = HEADER.Split(',').Select(x => new KeyValuePair<string, int>(x, headerIndex++)).ToDictionary(k => k.Key, v => v.Value);

      var record = new VolvoEarthworksCSVRecord(LINE, headerLocations);

      record.Should().BeEquivalentTo(new VolvoEarthworksCSVRecord
      {
        Time = DateTime.SpecifyKind(new DateTime(2020, 3, 11, 14, 19, 7, 150), DateTimeKind.Utc),
        CellN_m = double.Parse("7103556.75"),
        CellE_m = double.Parse("452796.15"),
        PassNumber = 1,
        DesignName = "Undirfylling",
        Machine = "SD115B",
        Speed_mph = double.Parse("1.1408375079"),
        TargetPassCount = 6,
        ValidPos = true,
        Lift = 1,
        LastEDV = 0.0d,
        LastFreq_Hz = 0,
        LastAmp_mm = double.Parse("-1000.0"),
        TargThickness_FT = 0.0,
        MachineGear = "Forward",
        VibeState = "On",
        LastTemp_f = double.Parse("32.0"),
        LastCMV = 0,
        ICMVType = 2
      });
    }
  }
}
