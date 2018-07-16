using System.IO;
using VSS.TRex.Executors.Executors;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
        public class TAGFileConverterTests : IClassFixture<DILoggingFixture>
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
        public void Test_TAGFileConverter_Execute()
        {
            TAGFileConverter converter = new TAGFileConverter();

            Assert.True(converter.Execute(new FileStream(Path.Combine("TestData", "TAGFiles", "TestTAGFile.tag"), FileMode.Open, FileAccess.Read)),
                "Converter execute returned false");

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
    }
}
