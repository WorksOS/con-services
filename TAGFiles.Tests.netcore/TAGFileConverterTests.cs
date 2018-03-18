using VSS.VisionLink.Raptor.Executors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using VSS.VisionLink.Raptor.TAGFiles.Tests;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using VSSTests.TRex.Tests.Common;
using Xunit;

namespace VSS.VisionLink.Raptor.Executors.Tests
{
        public class TAGFileConverterTests
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

            Assert.True(converter.Execute(new FileStream(TAGTestConsts.TestDataFilePath() + "TAGFiles\\TestTAGFile.tag", FileMode.Open, FileAccess.Read)),
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