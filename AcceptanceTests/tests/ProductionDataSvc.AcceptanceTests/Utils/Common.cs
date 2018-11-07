using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace ProductionDataSvc.AcceptanceTests.Utils
{
  public class Common
  {
    /// <summary>
    /// Test whether two lists are equivalent.
    /// </summary>
    public static bool ListsAreEqual<T>(List<T> listA, List<T> listB)
    {
      if (listA == null && listB == null)
        return true;
      if (listA == null || listB == null)
        return false;
      if (listA.Count != listB.Count)
        return false;

      for (int i = 0; i < listA.Count; ++i)
      {
        if (!listB.Exists(item => item.Equals(listA[i])))
          return false;
      }

      return true;
    }

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

    /// <summary>
    /// Sort a csv file which is in one large string
    /// </summary>
    /// <param name="csvData">csv file loaded into a string</param>
    /// <returns>string which is sorted by datetime and next key</returns>
    public static string SortCsvFileIntoString(string csvData)
    {
      var idx = csvData.IndexOf(Environment.NewLine, StringComparison.Ordinal);
      var header = csvData.Substring(0, idx);
      var datalines = csvData.Substring(idx + 2)
        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
      var sorted = datalines.Select(line => new
      {
        SortKey = DateTime.Parse(line.Split(',')[0]), // Sort by data time
        SortKeyThenBy = line.Split(',')[1], // Sort by string for 2nd key
        Line = line
      }
      ).OrderBy(x => x.SortKey).ThenBy(x => x.SortKeyThenBy).Select(x => x.Line);

      var sb = new StringBuilder();
      sb.Append(header);
      sb.Append(Environment.NewLine);
      foreach (var row in sorted)
      {
        sb.Append(row);
        sb.Append(Environment.NewLine);
      }
      return sb.ToString();
    }
  }
}
