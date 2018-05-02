using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace TestUtility
{
  public class Common
  {
    /// <summary>
    /// Convert file to byte array.
    /// </summary>
    /// <param name="input">Name of file (with full path) to convert.</param>
    public static byte[] FileToByteArray(string input)
    {
      byte[] output;

      FileStream sourceFile = new FileStream(input, FileMode.Open, FileAccess.Read); // Open streamer...

      BinaryReader binReader = new BinaryReader(sourceFile);
      try
      {
        output = binReader.ReadBytes((int)sourceFile.Length);
      }
      finally
      {
        sourceFile.Close(); // Dispose streamer...          
        binReader.Close(); // Dispose reader
      }

      return output;
    }

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
        return Decompress(compressedData);
      }
    }

    /// <summary>
    /// Decompress a zip archive file - assuming there is only one file in the archive.
    /// </summary>
    public static byte[] Decompress(MemoryStream compressedData)
    {
      ZipArchive archive = new ZipArchive(compressedData);

      using (var decompressedData = archive.Entries[0].Open())
      using (var ms = new MemoryStream())
      {
        decompressedData.CopyTo(ms);
        return ms.ToArray();
      }     
    }

    /// <summary>
    /// Test whether two arrays of doubles are equivalent.
    /// </summary>
    /// <returns>True or False.</returns>
    public static bool ArraysOfDoublesAreEqual(double[] arrayA, double[] arrayB, int precision = 2)
    {
      if (arrayA == null && arrayB == null)
        return true;
      if (arrayA == null || arrayB == null)
        return false;
      if (arrayA.Length != arrayB.Length)
        return false;

      for (int i = 0; i < arrayA.Length; ++i)
      {
        if (Math.Abs(Math.Round(arrayA[i]) - Math.Round(arrayB[i])) > precision)
          return false;
      }

      return true;
    }

    /// <summary>
    /// Compare two doubles and assert if different. Use to compare reports and exports in CSV
    /// </summary>
    /// <param name="expectedDouble">expected value</param>
    /// <param name="actualDouble">actual value</param>
    /// <param name="field">field name</param>
    /// <param name="rowCount">row index</param>
    /// <param name="precision">number of decimal places</param>
    /// <returns>false if they don't match, true if they match</returns>
    public static bool CompareDouble(double expectedDouble, double actualDouble, string field, int rowCount,
      int precision = 6)
    {
      if (Math.Abs(expectedDouble - actualDouble) < precision)
      {
        return true;
      }

      if (Math.Abs(Math.Round(expectedDouble) - Math.Round(actualDouble)) > precision)
      {
        Console.WriteLine("RowCount:" + rowCount + " " + field + " actual: " + actualDouble + " expected: " +
                          expectedDouble);
        Assert.Fail("Expected: " + expectedDouble + " Actual: " + actualDouble + " at row index " + rowCount +
                    " for field " + field);
      }
      return true;
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