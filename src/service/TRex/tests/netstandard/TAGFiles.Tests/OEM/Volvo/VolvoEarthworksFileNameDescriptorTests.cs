using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using VSS.TRex.TAGFiles.Classes.OEM.Volvo;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests.OEM.Volvo
{
  public class VolvoEarthworksFileNameDescriptorTests : IClassFixture<DILoggingFixture>
  {
    private const string TEST_FILE_NAME = "lift1_Lag 1_1_utm27W_2020-03-11 14-19-07_S135B556186.csv";
    [Fact]
    public void Creation()
    {
      var descriptor = new VolvoEarthworksFileNameDescriptor(TEST_FILE_NAME);
      descriptor.Should().NotBeNull();
    }

    [Fact]
    public void Creation2()
    {
      var descriptor = new VolvoEarthworksFileNameDescriptor(TEST_FILE_NAME);

      descriptor.Lift.Should().Be("lift1");
      descriptor.DesignName.Should().Be("Lag 1");
      descriptor.Counter.Should().Be(1);
      descriptor.CSName.Should().Be("utm27W");
      descriptor.Date.Should().Be(DateTime.SpecifyKind(new DateTime(2020, 3, 11, 14, 19, 7), DateTimeKind.Utc));
      descriptor.MachineID.Should().Be("S135B556186");
    }
  }
}
