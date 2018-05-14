using System;
using VSS.VisionLink.Raptor.TAGFiles.Classes;
using System.IO;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using VSS.VisionLink.Raptor.TAGFiles.Classes.Sinks;
using VSSTests.TRex.Tests.Common;
using Xunit;

namespace VSS.VisionLink.Raptor.TAGFiles.Tests
{
        public class TAGFileTests
    {
        [Fact]
        public void Test_TAGFile_Creation()
        {
            TAGFile file = new TAGFile();

            Assert.NotNull(file);
        }

        [Fact]
        public void Test_TAGFile_Read_Stream()
        {
            // Create the TAG file and reader classes
            TAGFile file = new TAGFile();
            TAGReader reader = new TAGReader(new FileStream(TAGTestConsts.TestDataFilePath() + "TAGFiles\\TestTAGFile-TAGFile-Read-Stream.tag", FileMode.Open, FileAccess.Read));

            // Create the state and sink
            TAGProcessorStateBase stateBase = new TAGProcessorStateBase(); // Derivatives to construct later
            TAGValueSink sink = new TAGValueSink(stateBase);

            //Read the TAG file
            TAGReadResult result = file.Read(reader, sink);

            Assert.Equal(TAGReadResult.NoError, result);
        }

        [Fact]
        public void Test_TAGFile_Read_File()
        {
            // Create the TAG file and reader classes
            TAGFile file = new TAGFile();

            // Create the state and sink
            TAGProcessorStateBase stateBase = new TAGProcessorStateBase(); // Derivatives to construct later
            TAGValueSink sink = new TAGValueSink(stateBase);

            //Read the TAG file
            TAGReadResult result = file.Read(TAGTestConsts.TestDataFilePath() + "TAGFiles\\TestTAGFile-TAGFile-Read-File.tag", sink);

            Assert.Equal(TAGReadResult.NoError, result);
        }
    }
}
