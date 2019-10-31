using System.Collections.Generic;
using System.Text;
using TagFiles.Parser;
using TagFiles.Types;

namespace TagFiles
{
  /// <summary>
  /// Contains the data entries for a tagfile
  /// </summary>
  public class TagContentList
  {
    
    public readonly List<TagData> Entries = new List<TagData>();

    public void ClearList()
    {
      Entries.Clear();
    }

    /// <summary>
    /// Genric way to add entry
    /// </summary>
    /// <param name="td"></param>
    public void AddEntry(TagData td)
    {
      Entries.Add(td);
    }

    /// <summary>
    /// Time entry
    /// </summary>
    /// <param name="td"></param>
    public void AddTimeEntry(TagData td)
    {
      td.DataType = TAGDataType.t32bitUInt;
      td.DictID = (short)DictionaryItem.Time;
      Entries.Add(td);
    }

    /// <summary>
    /// Time Delta
    /// </summary>
    /// <param name="td"></param>
    public void AddTimeDeltaEntry(TagData td)
    {
      td.DataType = TAGDataType.t4bitUInt;
      td.DictID = (short)DictionaryItem.TimeDelta;
      Entries.Add(td);
    }

    /// <summary>
    /// Week entry
    /// </summary>
    /// <param name="td"></param>
    public void AddWeekEntry(TagData td)
    {
      td.DataType = TAGDataType.t16bitUInt;
      td.DictID = (short)DictionaryItem.Week;
      Entries.Add(td);
    }

    public void AddDoubleEntry(TagData td)
    {
      td.DataType = TAGDataType.t32bitUInt;
      Entries.Add(td);
    }


    public void Write(NybbleStream stream)
    {
      // testing
  //    WriteTestContent(stream);
   //   return;

      // Write out each tagfile data entry to stream. First one must be a timestamp
      foreach (TagData td in Entries)
      {
        stream.WriteVarSizeUnsignedInt((uint)td.DictID); // write dictionary id first 
        td.Write(ref stream);
      }

    }


    public void WriteTestContent(NybbleStream stream)
    {
      // test timestamp 
      stream.WriteVarSizeUnsignedInt(1);
      var td = new TagData_UnsignedInt() { DataType = TAGDataType.t32bitUInt, Data = 2 };
      td.Write(ref stream);

      // week
      stream.WriteVarSizeUnsignedInt(3);
      var td2 = new TagData_UnsignedInt() { DataType = TAGDataType.t16bitUInt, Data = 1 };
      td2.Write(ref stream);

      // coord system
      stream.WriteVarSizeUnsignedInt(4);
      var td3 = new TagData_UnsignedInt() { DataType = TAGDataType.t4bitUInt, Data = 3 };
      td3.Write(ref stream);

      // UTM system
      stream.WriteVarSizeUnsignedInt(5);
      var td4 = new TagData_UnsignedInt() { DataType = TAGDataType.t4bitUInt, Data = 1 };
      td4.Write(ref stream);

      // Left
      stream.WriteVarSizeUnsignedInt(7);
      var td5 = new TagData_Empty() { DataType = TAGDataType.tEmptyType};
      td5.Write(ref stream);

      // easting
      stream.WriteVarSizeUnsignedInt(9);
      var td6 = new TagData_Double() { DataType = TAGDataType.tIEEEDouble, Data = 12345.123};
      td6.Write(ref stream);


      // machineid
      stream.WriteVarSizeUnsignedInt(21);
      var td7 = new TagData_String() { DataType = TAGDataType.tANSIString, Data = "HEX"};
      td7.Write(ref stream);


      // radio type
      stream.WriteVarSizeUnsignedInt(20);
      var td8 = new TagData_String() { DataType = TAGDataType.tANSIString, Data = "torch" };
      td8.Write(ref stream);

      string unicodeString =
               "Unicode string with codes outside the traditional ASCII code range, " +
               "Pi (\u03a0) and Sigma (\u03a3).";

      // Design
      
      stream.WriteVarSizeUnsignedInt(14);
      var td9 = new TagData_Unicode() { DataType = TAGDataType.tUnicodeString, Data = unicodeString };
      td9.Write(ref stream);
      

      // The encoding.
      UnicodeEncoding unicode = new UnicodeEncoding();
      MegalodonLogger.LogInfo("Original string:");
      MegalodonLogger.LogInfo(unicodeString);

      // Encode the string.
      byte[] encodedBytes = unicode.GetBytes(unicodeString);
      MegalodonLogger.LogInfo("");
      MegalodonLogger.LogInfo("Encoded bytes:");

      MegalodonLogger.LogInfo($"byte array lenght {encodedBytes.Length}");
      foreach (byte b in encodedBytes)
      {
        MegalodonLogger.LogInfo($"[{b}]");
      }
      MegalodonLogger.LogInfo("");

      // Decode bytes back to string.
      // Notice Pi and Sigma characters are still present.
      string decodedString = unicode.GetString(encodedBytes);
      MegalodonLogger.LogInfo("");
      MegalodonLogger.LogInfo("Decoded bytes:");
      MegalodonLogger.LogInfo(decodedString);








    }



    /*
    public readonly Dictionary<DictionaryItem, EpochItem> Entries;

    public void AddEntry(EpochItem epItem, DictionaryItem id)
    {
      Entries.Add(id, epItem);
    }

    public void AddDoubleEntry(double value, DictionaryItem dataType)
    {
      Entries.Add(dataType, new DoubleItem() { Data = value });
    }
    */

  }
}
