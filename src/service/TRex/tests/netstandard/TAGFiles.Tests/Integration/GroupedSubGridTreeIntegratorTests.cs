using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.TAGFiles.Classes.Integrator;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests.Integration
{
  public class GroupedSubGridTreeIntegratorTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var integrator = new GroupedSubGridTreeIntegrator();

      integrator.Should().NotBeNull();
    }

    [Theory]
    [InlineData("Dimensions2018-CaseMachine", 164, 0, 10, 4)] // Take the first 10
    [InlineData("Dimensions2018-CaseMachine", 164, 10, 10, 2)] // Take the next 10
    [InlineData("Dimensions2018-CaseMachine", 164, 20, 10, 3)] // Take the next 10
    [InlineData("Dimensions2018-CaseMachine", 164, 30, 10, 2)] // Take the next 10
    [InlineData("Dimensions2018-CaseMachine", 164, 0, 164, 9)] // Take the lot
    public void Integrator_TAGFileSet(string tagFileCollectionFolder, int expectedFileCount, int skipTo, int numToTake, int expectedSubGridCount)
    {
      Directory.GetFiles(Path.Combine("TestData", "TAGFiles", tagFileCollectionFolder), "*.tag").Length.Should().Be(expectedFileCount);

      // Convert TAG files using TAGFileConverters into mini-site models
      var converters = Directory.GetFiles(Path.Combine("TestData", "TAGFiles", tagFileCollectionFolder), "*.tag")
        .ToList().OrderBy(x => x).Skip(skipTo).Take(numToTake).Select(DITagFileFixture.ReadTAGFileFullPath).ToArray();

      converters.Length.Should().Be(numToTake);

      // Create the integrator and add the processed TAG file to its processing list
      var integrator = new GroupedSubGridTreeIntegrator
      {
        Trees = converters
          .Select(c => (c.SiteModelGridAggregator, 
                        c.MachineTargetValueChangesAggregator.StartEndRecordedDataEvents.FirstStateDate(),
                        c.MachineTargetValueChangesAggregator.StartEndRecordedDataEvents.LastStateDate()))
          .ToList()
      };

      var result = integrator.IntegrateSubGridTreeGroup();

      result.Should().NotBeNull();

      // Check the set of TAG files created the expected number of sub grids
      result.CountLeafSubGridsInMemory().Should().Be(expectedSubGridCount);

      int nonNullCellCount = 0;
      result.ScanAllSubGrids(leaf => (nonNullCellCount += (leaf as IServerLeafSubGrid).CountNonNullCells()) > -1);
      nonNullCellCount.Should().BeGreaterThan(0);
    }
  }
}
