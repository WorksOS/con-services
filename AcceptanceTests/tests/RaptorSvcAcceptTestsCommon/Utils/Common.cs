﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace RaptorSvcAcceptTestsCommon.Utils
{
    public class Common
    {
        /// <summary>
        /// Convert file to byte array.
        /// </summary>
        /// <param name="input">Name of file (with full path) to convert.</param>
        /// <returns></returns>
        public static byte[] FileToByteArray(string input)
        {
            byte[] output = null;

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
        /// <typeparam name="T"></typeparam>
        /// <param name="listA"></param>
        /// <param name="listB"></param>
        /// <returns></returns>
        public static bool ListsAreEqual<T>(List<T> listA, List<T> listB)
        {
            if (listA == null && listB == null)
                return true;
            else if (listA == null || listB == null)
                return false;
            else
            {
                if (listA.Count != listB.Count)
                    return false;

                for (int i = 0; i < listA.Count; ++i)
                {
                    if (!listB.Exists(item => item.Equals(listA[i])))
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Decompress a zip archive file - assuming there is only one file in the archive.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] data)
        {
            using (var compressedData = new MemoryStream(data))
            {
                ZipArchive archive = new ZipArchive(compressedData);

                using (var decompressedData = archive.Entries[0].Open())
                {
                    using(var ms = new MemoryStream())
                    {
                        decompressedData.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
        }

    /// <summary>
    /// Test whether two arrays of doubles are equivalent.
    /// </summary>
    /// <param name="arrayA"></param>
    /// <param name="arrayB"></param>
    /// /// <param name="precision"></param>
    /// <returns>True or False.</returns>
    public static bool ArraysOfDoublesAreEqual(double[] arrayA, double[] arrayB, int precision = 2)
    {
      if (arrayA == null && arrayB == null)
        return true;
      else if (arrayA == null || arrayB == null)
        return false;
      else
      {
        if (arrayA.Length != arrayB.Length)
          return false;

        for (int i = 0; i < arrayA.Length; ++i)
        {
          if (Math.Round(arrayA[i], precision) != Math.Round(arrayB[i], precision))
            return false;
        }

        return true;
      }
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
    public static bool CompareDouble(double expectedDouble, double actualDouble, string field, int rowCount, int precision = 6)
    {
      if (expectedDouble == actualDouble)
      {
        return true;
      }
      if (Math.Round(expectedDouble, precision) != Math.Round(actualDouble, precision))
      {
        Console.WriteLine("RowCount:" + rowCount + " " + field + " actual: " + actualDouble + " expected: " + expectedDouble);
        Assert.Fail("Expected: " + expectedDouble + " Actual: " + actualDouble + " at row index " + rowCount + " for field " + field);
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
      var datalines = csvData.Substring(idx+2).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
      var sorted = datalines.Select(line => new
        {
          SortKey = DateTime.Parse(line.Split(',')[0]),  // Sort by data time
          SortKeyThenBy = line.Split(',')[1],            // Sort by string for 2nd key
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

    /// <summary>
    /// Convert an byte stream to image
    /// </summary>
    /// <param name="imageStream">byte array</param>
    /// <returns>image</returns>
    public static Image ConvertToImage(byte[] imageStream)
    {
      using (var ms = new MemoryStream(imageStream))
      {
        return Image.FromStream(ms);
      }
    }

    /// <summary>
    /// Compare the images and return the differnce
    /// </summary>
    /// <param name="expectedTileData">byte array of expected tile data</param>
    /// <param name="actualTileData">byte array of actual tile data</param>
    /// <param name="expFileName">expected file name that is saved</param>
    /// <param name="actFileName">actual file name that is saved</param>
    /// <param name="threshold">threshold for comparison. Used to ignore minor differences</param>
    /// <returns>the differnce of the images as a percentage</returns>
    public static float CompareImagesAndGetDifferencePercent(byte[] expectedTileData, byte[] actualTileData, string expFileName, string actFileName, byte threshold = 0)
    {
      var expectedImage = ConvertToImage(expectedTileData);
      var actualImage = ConvertToImage(actualTileData);
      expectedImage.Save(expFileName);
      actualImage.Save(actFileName);
      var diff = XnaFan.ImageComparison.ExtensionMethods.PercentageDifference(expectedImage, actualImage, threshold);
      return diff;
    }
  }
}
