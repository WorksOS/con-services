using System;
using FluentAssertions;
using VSS.TRex.Common.Types;
using VSS.TRex.Events;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
  public class ElevationMappingModeTests : IClassFixture<DITagFileFixture>
  {
    [Theory]
    [InlineData("ElevationMappingMode", "0247J009YU--TNZ 323F GS520--190115000234.tag")]
    [InlineData("ElevationMappingMode", "0247J009YU--TNZ 323F GS520--190115001235.tag")]
    [InlineData("ElevationMappingMode", "0247J009YU--TNZ 323F GS520--190115001735.tag")]
    [InlineData("ElevationMappingMode", "0247J009YU--TNZ 323F GS520--190115002235.tag")]
    [InlineData("ElevationMappingMode", "0247J009YU--TNZ 323F GS520--190115002735.tag")]
    public void ElevationMappingModeTests_Import_LatestElevationOnly(string folder, string fileName)
    {
      // Convert a TAG file using a TAGFileConverter into a mini-site model
      TAGFileConverter converter = DITagFileFixture.ReadTAGFile(folder, fileName);

      // Check the list is as expected, has one element and extract it
      converter.MachineTargetValueChangesAggregator.MinElevMappingStateEvents.EventListType.Should().Be(ProductionEventType.MinElevMappingStateChange);
      converter.MachineTargetValueChangesAggregator.MinElevMappingStateEvents.Count().Should().Be(1);
      converter.MachineTargetValueChangesAggregator.MinElevMappingStateEvents.GetStateAtIndex(0, out DateTime eventDate, out var eventValue);

      // Check date of event falls within the date range of the TAG file.
      eventDate.Should().BeOnOrAfter(converter.Processor.FirstDataTime);
      eventDate.Should().BeOnOrBefore(converter.Processor.DataTime);

      // These test files only contain latest elevation mapping modes.
      eventValue.Should().Be(MinElevMappingState.LatestElevation);
    }
  }
}
