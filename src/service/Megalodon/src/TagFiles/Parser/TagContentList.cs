using System.Collections.Generic;
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
      // Write out each tagfile data entry to stream. First one must be a timestamp
      foreach (TagData td in Entries)
      {
        stream.WriteVarSizeUnsignedInt((uint)td.DictID); // write dictionary id first 
        td.Write(ref stream);
      }
    }

  }
}
