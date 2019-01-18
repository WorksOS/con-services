using System.Collections.Generic;
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
    [Fact]
    public void Test_TAGFileCellPassGeneration_CapturesCellPassCreation()
    {
      var Lines = new List<string>();

      // Setup the mutation hook to capture cell pass generation
      ICell_NonStatic_MutationHook Hook = DIContext.Obtain<ICell_NonStatic_MutationHook>();

      var passWriter = new CellPassWriter(x => Lines.Add(x));
      Hook.SetActions(passWriter);
      try
      {
        var converter = DITagFileFixture.ReadTAGFile("TestTAGFile.tag");
        converter.ProcessedCellPassCount.Should().Be(16525);
      }
      finally
      {
        Hook.ClearActions();
      }

      Lines.Count.Should().Be(16525);
    }
  }
}

