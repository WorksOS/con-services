using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using FluentAssertions;
using TAGFiles.Tests.Utilities;
using VSS.TRex.DI;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
  public class TAGFileCellPassTests : IClassFixture<DITagFileFixture>
  {
    private void CompareMutationLogs(List<string> Lines, string mutationLogFileName, string[] mutationLog)
    {
      for (int i = 0; i < Lines.Count; i++)
      {
        string because = $"In file {mutationLogFileName} at line {i}";

        var logSplit = mutationLog[i].Split(new[] { ' ', ':' });
        var lineSplit = Lines[i].Split(new[] {' ', ':' });

        // Check the mutation log 'command'
        lineSplit[0].Should().Be(logSplit[0], because);

        if (String.CompareOrdinal(lineSplit[0], "AddPass") == 0 || string.CompareOrdinal(lineSplit[0], "ReplacePass") == 0)
        {
          // Compare the components up to the date
          for (int j = 1; j < logSplit.Length - 1; j++)
            lineSplit[j].Should().Be(logSplit[j], because);

          // Compare the date with a 1 millisecond tolerance
          DateTime logDate = DateTime.ParseExact(logSplit.Last(), "yyyy-MM-dd-HH-mm-ss.fff", CultureInfo.InvariantCulture);
          DateTime lineDate = DateTime.ParseExact(lineSplit.Last(), "yyyy-MM-dd-HH-mm-ss.fff", CultureInfo.InvariantCulture);

          var span = logDate - lineDate;
          Math.Abs(span.TotalMilliseconds).Should().BeLessThan(2.0, because + $" Line={Lines[i]}, mutationLog={mutationLog[i]}");
        }
        else
        {
          // Generic assertion of line equality
          Lines[i].Should().Be(mutationLog[i], because);
        }
      }
    }

    [Theory(Skip = "Cell pass count and line count dependent on varying mutation log schema for now")]
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

    [Theory(Skip="Run locally - mutation logs not currently source controlled due to size")]
    [InlineData("2652J085SW--CASE CX160C--121031183620.tag", "CellMutationLog-2652J085SW--CASE CX160C--121031183620.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101151938.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101151938.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101152438.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101152438.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101160718.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101160718.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101161218.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101161218.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101161758.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101161758.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101162745.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101162745.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101163256.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101163256.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101164059.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101164059.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101164411.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101164411.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101170609.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101170609.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101172035.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101172035.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101180448.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101180448.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101180636.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101180636.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101182050.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101182050.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101184728.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101184728.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101185528.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101185528.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101203422.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101203422.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101205030.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101205030.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101205530.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101205530.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101215100.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101215100.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121101223541.tag", "CellMutationLog-2652J085SW--CASE CX160C--121101223541.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102151722.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102151722.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102162347.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102162347.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102162847.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102162847.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102163347.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102163347.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102165622.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102165622.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102170448.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102170448.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102170948.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102170948.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102174333.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102174333.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102175141.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102175141.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102184258.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102184258.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102184758.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102184758.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102185258.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102185258.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102192656.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102192656.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102193202.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102193202.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102193704.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102193704.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102194205.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102194205.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102195231.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102195231.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102202326.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102202326.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102202826.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102202826.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102203418.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102203418.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102203918.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102203918.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102210135.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102210135.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102210635.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102210635.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102211320.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102211320.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102211822.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102211822.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102212527.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102212527.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102213155.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102213155.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102213929.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102213929.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102224546.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102224546.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121102225319.tag", "CellMutationLog-2652J085SW--CASE CX160C--121102225319.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103160919.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103160919.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103161419.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103161419.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103162209.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103162209.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103163447.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103163447.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103164055.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103164055.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103164614.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103164614.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103165114.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103165114.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103165930.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103165930.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103170906.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103170906.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103171407.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103171407.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103174946.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103174946.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103175446.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103175446.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103180016.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103180016.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103181721.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103181721.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103183211.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103183211.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103183712.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103183712.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103195234.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103195234.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103195739.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103195739.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103200239.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103200239.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103200934.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103200934.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103202155.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103202155.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103203430.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103203430.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103211431.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103211431.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103212016.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103212016.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103212947.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103212947.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103213834.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103213834.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103214718.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103214718.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103220638.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103220638.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103224053.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103224053.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103224730.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103224730.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103225241.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103225241.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103230216.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103230216.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121103230716.tag", "CellMutationLog-2652J085SW--CASE CX160C--121103230716.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105161838.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105161838.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105162716.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105162716.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105162951.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105162951.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105163906.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105163906.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105164856.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105164856.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105165935.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105165935.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105171758.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105171758.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105172537.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105172537.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105190901.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105190901.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105192147.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105192147.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105192647.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105192647.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105195130.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105195130.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105195630.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105195630.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105200132.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105200132.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105200632.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105200632.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105201133.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105201133.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105201827.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105201827.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105203253.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105203253.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105203926.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105203926.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105204426.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105204426.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105205158.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105205158.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105212013.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105212013.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105213738.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105213738.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105214048.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105214048.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105222816.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105222816.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105223609.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105223609.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105224314.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105224314.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105230425.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105230425.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105230934.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105230934.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105231807.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105231807.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105232947.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105232947.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121105233447.tag", "CellMutationLog-2652J085SW--CASE CX160C--121105233447.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106000756.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106000756.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106004416.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106004416.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106162447.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106162447.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106180110.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106180110.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106180434.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106180434.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106184536.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106184536.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106185038.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106185038.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106185712.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106185712.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106192602.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106192602.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106193102.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106193102.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106193624.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106193624.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106194210.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106194210.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106194554.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106194554.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106202439.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106202439.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106202939.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106202939.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106205519.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106205519.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106210019.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106210019.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106210519.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106210519.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106211019.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106211019.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106211555.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106211555.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106222138.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106222138.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106224801.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106224801.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106225406.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106225406.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121106225946.tag", "CellMutationLog-2652J085SW--CASE CX160C--121106225946.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107001429.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107001429.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107002047.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107002047.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107002547.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107002547.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107003057.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107003057.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107003159.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107003159.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107170305.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107170305.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107170958.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107170958.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107171742.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107171742.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107172243.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107172243.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107172743.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107172743.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107173651.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107173651.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107174308.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107174308.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107175941.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107175941.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107183817.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107183817.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107184318.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107184318.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107184818.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107184818.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107192042.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107192042.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107192915.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107192915.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107210452.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107210452.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107193415.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107193415.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107195157.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107195157.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107205806.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107205806.tag.txt")]
    [InlineData("2652J085SW--CASE CX160C--121107210953.tag", "CellMutationLog-2652J085SW--CASE CX160C--121107210953.tag.txt")]
    public void Test_TAGFileCellPassGeneration_CompareKnownCellPassConstruction_Dimensions2018CaseMachine(string tagFileName, string mutationLogFileName)
    {
      var Lines = new List<string>();
      ICell_NonStatic_MutationHook Hook = DIContext.Obtain<ICell_NonStatic_MutationHook>();

      Hook.SetActions(new CellPassWriter(x => Lines.Add(x)));
      try
      {
        DITagFileFixture.ReadTAGFile("Dimensions2018-CaseMachine", tagFileName);
      }
      finally
      {
        Hook.ClearActions();
      }

      //File.WriteAllLines(Path.Combine(@"C:\temp\SavedMutationLogsFromTests\" + mutationLogFileName), Lines);

      // Load the 'truth' mutation log
      var mutationLog = File.ReadAllLines(Path.Combine("TestData", "TagFiles", "Dimensions2018-CaseMachine", mutationLogFileName));

      CompareMutationLogs(Lines, mutationLogFileName, mutationLog);
    }

    [Theory]
    [InlineData(@"C:\Dev\VSS.Productivity3D.MonoRepo\src\service\TRex\tests\netstandard\TAGFiles.Tests\TestData\TAGFiles\Dimensions2018-CaseMachine")]
    public void Test_TAGFileCellPassGeneration_CompareKnownCellPassConstruction_Folders(string folderName)
    {
      // Get list of TAG files
      var fileNames = Directory.GetFiles(folderName, "*.tag");

      foreach (var tagFileName in fileNames)
      {
        var linesLogFileName = Path.Combine(Path.GetDirectoryName(tagFileName), $"CellMutationLog-{Path.GetFileName(tagFileName)}.txt.output");
        var mutationLogFileName = Path.Combine(Path.GetDirectoryName(tagFileName), $"CellMutationLog-{Path.GetFileName(tagFileName)}.txt");

        var Lines = new List<string>();
        ICell_NonStatic_MutationHook Hook = DIContext.Obtain<ICell_NonStatic_MutationHook>();

        Hook.SetActions(new CellPassWriter(x => Lines.Add(x)));
        try
        {
          DITagFileFixture.ReadTAGFileFullPath(tagFileName);
        }
        finally
        {
          Hook.ClearActions();
        }

        File.WriteAllLines(linesLogFileName, Lines);
        CompareMutationLogs(Lines, mutationLogFileName, File.ReadAllLines(mutationLogFileName));
      }
    }
  }
}

