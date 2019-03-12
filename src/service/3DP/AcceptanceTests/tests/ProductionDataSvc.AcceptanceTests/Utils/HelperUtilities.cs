using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;

namespace ProductionDataSvc.AcceptanceTests.Utils
{
  public class HelperUtilities
  {
    /// <summary>
    /// Decompress a zip archive file - assuming there is only one file in the archive.
    /// </summary>
    public static byte[] Decompress(byte[] data)
    {
      using (var compressedData = new MemoryStream(data))
      {
        var archive = new ZipArchive(compressedData);

        using (var decompressedData = archive.Entries[0].Open())
        using (var ms = new MemoryStream())
        {
          decompressedData.CopyTo(ms);
          return ms.ToArray();
        }
      }
    }

    public static List<string> SortCsvFile(string csvData)
    {
      var idx = csvData.IndexOf(Environment.NewLine, StringComparison.Ordinal);
      var header = csvData.Substring(0, idx);
      var datalines = csvData.Substring(idx + 2)
                             .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

      var sorted = datalines.Select(line => new
      {
        SortKey = DateTime.Parse(line.Split(',')[0]), // Sort by first column (Time)
        SortKeyThenBy = line.Split(',')[1],           // Then sort by second column
        Line = line
      }).OrderBy(x => x.SortKey)
        .ThenBy(x => x.SortKeyThenBy)
        .Select(x => x.Line)
        .ToList();

      sorted.Insert(0, header);

      return sorted;
    }

    public static void CompareExportCsvFiles(List<string> actualCsvExport, List<string> expectedCsvExport)
    {
      // Assert the correct number of rows.
      actualCsvExport.Count.Should().Be(expectedCsvExport.Count);

      // Assert the header lines match.
      actualCsvExport[0].Should().Be(expectedCsvExport[0]);

      for (var index = 1; index < expectedCsvExport.Count; index++)
      {
        var expectedCells = expectedCsvExport[index].Split(',');
        var actualCells = actualCsvExport.ElementAt(index).Split(',');

        // Assert the correct number of columns.
        expectedCells.Length.Should().Be(actualCells.Length);

        // First pass, to allow for DST shifts in North America against our saved test data. Allow the report time to be out by only 1 hour.
        DateTime.Parse(actualCells[0]).Should().BeCloseTo(DateTime.Parse(expectedCells[0]), 1.Hours());

        for (var i = 1; i < expectedCells.Length; i++)
        {
          actualCells[i].Should().Be(expectedCells[i]);
        }
      }
    }
  }
}
