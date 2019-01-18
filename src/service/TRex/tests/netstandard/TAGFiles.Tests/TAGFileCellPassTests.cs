using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TAGFiles.Tests.Utilities;
using VSS.TRex.DI;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
  public class TAGFileCellPassTests : IClassFixture<DITagFileFixture>
  {
    [Theory]
    [InlineData("TestTAGFile.tag", 16525, 16525)]
    [InlineData("TestTAGFile3.tag", 16525, 16525)]
    public void Test_TAGFileCellPassGeneration_CapturesCellPassCreation(string fileName, int cellPassCount, int lineCount)
    {
      var Lines = new List<string>();

      // Setup the mutation hook to capture cell pass generation
      ICell_NonStatic_MutationHook Hook = DIContext.Obtain<ICell_NonStatic_MutationHook>();

      Hook.SetActions(new CellPassWriter(x => Lines.Add(x)));
      try
      {
        var converter = DITagFileFixture.ReadTAGFile(fileName);
        converter.ProcessedCellPassCount.Should().Be(cellPassCount);
      }
      finally
      {
        Hook.ClearActions();
      }

      Lines.Count.Should().Be(lineCount);

      // Temporarily save the file to capture it
      var fn = Path.Combine(Path.GetTempPath(), fileName + ".MutationLog.txt");
      File.WriteAllLines(fn, Lines);
    }
  }
}

