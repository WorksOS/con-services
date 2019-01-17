using System;
using System.IO;
using FluentAssertions;
using TAGFiles.Tests.Utilities;
using VSS.TRex.DI;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
  public class TAGFileCellPassTests
  {
    [Fact]
    public void Test_TAGFileCellPassGeneration_Default()
    {
      // Setup the mutation hook to capture cell pass generation
      string cellPassFileName = $@"c:\temp\CellPasses-{DateTime.Now.Ticks}.txt";
      var writer = new CellPassWriter(new StreamWriter(new FileStream(cellPassFileName, FileMode.CreateNew, FileAccess.ReadWrite)));
      DIContext.Obtain<ICell_NonStatic_MutationHook>().SetActions(writer); 

      // Read in the TAG file
      DITagFileFixture.ReadTAGFile("TestTAGFile.tag");

      // Close the writer and clean up the DI context
      writer.Close();
      DIBuilder.Continue().Remove<ICell_NonStatic_MutationHook>();

      //  examine the result
      var lines = File.ReadAllLines(cellPassFileName);
      lines.Length.Should().Be(1);
    }
  }
}
