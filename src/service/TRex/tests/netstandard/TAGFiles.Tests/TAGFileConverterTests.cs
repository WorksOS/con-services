using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Nito.AsyncEx;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace TAGFiles.Tests
{
  public class TAGFileConverterTests : IClassFixture<DITagFileFixture>
  {
    [Fact()]
    public void Test_TAGFileConverter_Creation()
    {
      TAGFileConverter converter = new TAGFileConverter();

      Assert.True(converter.Machine == null &&
                  converter.SiteModel == null &&
                  converter.SiteModelGridAggregator == null &&
                  converter.MachineTargetValueChangesAggregator == null &&
                  converter.ReadResult == TAGReadResult.NoError &&
                  converter.ProcessedCellPassCount == 0 &&
                  converter.ProcessedEpochCount == 0,
        "TAGFileConverter not created as expected");
    }

    [Fact()]
    public void Test_TAGFileConverter_Execute_SingleFileOnce()
    {
      TAGFileConverter converter = DITagFileFixture.ReadTAGFile("TestTAGFile.tag");

      Assert.True(converter.Machine != null, "converter.Machine == null");
      Assert.True(converter.MachineTargetValueChangesAggregator != null,
        "converter.MachineTargetValueChangesAggregator");
      Assert.True(converter.ReadResult == TAGReadResult.NoError,
        $"converter.ReadResult == TAGReadResult.NoError [= {converter.ReadResult}");
      Assert.True(converter.ProcessedCellPassCount == 16525,
        $"converter.ProcessedCellPassCount != 16525 [={converter.ProcessedCellPassCount}]");
      Assert.True(converter.ProcessedEpochCount == 1478, $"converter.ProcessedEpochCount != 1478, [= {converter.ProcessedEpochCount}]");
      Assert.True(converter.SiteModelGridAggregator != null, "converter.SiteModelGridAggregator == null");
    }

    [Fact()]
    public void Test_TAGFileConverter_Execute_SingleFileTwice()
    {
      TAGFileConverter converter1 = DITagFileFixture.ReadTAGFile("TestTAGFile.tag");
      TAGFileConverter converter2 = DITagFileFixture.ReadTAGFile("TestTAGFile.tag");

      converter1.ReadResult.Should().Be(TAGReadResult.NoError);
      converter2.ReadResult.Should().Be(TAGReadResult.NoError);

      converter1.ProcessedCellPassCount.Should().Be(converter2.ProcessedCellPassCount);

      converter1.ProcessedCellPassCount.Should().Be(converter2.ProcessedCellPassCount);
      converter1.ProcessedEpochCount.Should().Be(converter2.ProcessedEpochCount);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public void Test_TAGFileConverter_Execute_SingleFileMultipleTimesConcurrently(int instanceCount)
    {
      var result = Enumerable.Range(1, instanceCount).Select(x => Task.Factory.StartNew(() => DITagFileFixture.ReadTAGFile("TestTAGFile.tag"))).WhenAll().Result;

      result.Length.Should().Be(instanceCount);

      result.All(x => x.ProcessedCellPassCount == 16525).Should().Be(true);
      result.All(x => x.ProcessedEpochCount == 1478).Should().Be(true);
      result.All(x => x.SiteModelGridAggregator.CountLeafSubgridsInMemory() == 12).Should().Be(true);
    }

    [Fact]
    public void Test_TAGFileConverter_OnGroundState()
    {
      TAGFileConverter converter = DITagFileFixture.ReadTAGFile("Dimensions2018-CaseMachine", "2652J085SW--CASE CX160C--121101215100.tag");
      converter.Processor.OnGroundFlagSet.Should().Be(true);

      DateTime theTime = new DateTime(2012, 11, 1, 20, 53, 23, 841, DateTimeKind.Unspecified);
      converter.Processor.OnGrounds.GetOnGroundAtDateTime(theTime).Should().NotBe(OnGroundState.YesMachineSoftware);
    }


  }
}
