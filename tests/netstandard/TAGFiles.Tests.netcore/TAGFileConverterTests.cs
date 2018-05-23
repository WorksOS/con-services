using System.IO;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Tests.netcore.TestFixtures;
using VSSTests.TRex.Tests.Common;
using Xunit;

namespace VSS.TRex.Executors.Tests
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

            Assert.True(converter.Machine != null &&
                converter.SiteModelGridAggregator != null &&
                converter.MachineTargetValueChangesAggregator != null &&
                converter.ReadResult == TAGReadResult.NoError &&
                converter.ProcessedCellPassCount == 16525 &&
                converter.ProcessedEpochCount == 1478,
                "TAGFileConverter did not execute as expected");
        }
    }
}