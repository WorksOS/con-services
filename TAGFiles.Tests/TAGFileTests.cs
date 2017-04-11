using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.TAGFiles.Classes;
using System.IO;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using VSS.VisionLink.Raptor.TAGFiles.Classes.Sinks;

namespace VSS.VisionLink.Raptor.TAGFiles.Tests
{
    [TestClass]
    public class TAGFileTests
    {
        [TestMethod]
        public void Test_TAGFile_Creation()
        {
            TAGFile file = new TAGFile();

            Assert.IsTrue(file != null, "Failed to create TAGfile");
        }

        [TestMethod]
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

            Assert.IsTrue(result == TAGReadResult.NoError, "TAG file reading resulted in error code {0}", result);
        }

        [TestMethod]
        public void Test_TAGFile_Read_File()
        {
            // Create the TAG file and reader classes
            TAGFile file = new TAGFile();

            // Create the state and sink
            TAGProcessorStateBase stateBase = new TAGProcessorStateBase(); // Derivatives to construct later
            TAGValueSink sink = new TAGValueSink(stateBase);

            //Read the TAG file
            TAGReadResult result = file.Read(TAGTestConsts.TestDataFilePath() + "TAGFiles\\TestTAGFile-TAGFile-Read-File.tag", sink);

            Assert.IsTrue(result == TAGReadResult.NoError, "TAG file reading resulted in error code {0}", result);
        }
    }
}
