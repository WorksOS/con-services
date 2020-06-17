using System.Collections.Generic;
using System.Text;
using TagFiles.Parser;
using TagFiles.Types;

namespace TagFiles
{
  /// <summary>
  /// The core dictionary that maps the TAG file items by the ID used in the TAG file to the 
  /// description of that value type as used in the TAG file
  /// </summary>
  public class TAGDictionary
  {

    private const int DEFAULT_TAG_FILE_SCHEMA_DICTIONARY_CAPACITY = 100;

    /// <summary>
    /// The core dictionary that maps the TAG file items by the ID used in the TAG file to the 
    /// description of that value type as used in the TAG file
    /// </summary>
    public readonly Dictionary<short, TAGDictionaryItem> Entries;


    public void AddEntry(string fieldName, TAGDataType fieldType, short id)
    {
      Entries.Add(id, new TAGDictionaryItem(fieldName, fieldType, id));
    }

    public TAGDictionary()
    {
      //   Entries = new Dictionary<short, TAGDictionaryItem>(DEFAULT_TAG_FILE_SCHEMA_DICTIONARY_CAPACITY);
      Entries = new Dictionary<short, TAGDictionaryItem>();
    }

    /// <summary>
    /// Reads the TAG file schema dictionary from the TAG file data using the TAG file reader
    /// </summary>
    /// <returns></returns>
    public bool Read(NybbleStream reader)
    {
      string fieldName;
      while ((fieldName = reader.ReadANSIString()) != string.Empty)
      {
        uint tempFieldType = reader.ReadUnSignedIntegerValue(1);

        // If field type is 15, then read an extra var int to determine extended data type
        if (tempFieldType == 15)
        {
          if (!reader.ReadVarInt(out short tempFieldTypeExtended))
          {
            return false;
          }
          tempFieldType += (uint)tempFieldTypeExtended;
        }

        var fieldType = (TAGDataType)tempFieldType;

        if (!reader.ReadVarInt(out var id))
        {
          return false;
        }

        Entries.Add(id, new TAGDictionaryItem(fieldName, fieldType, id));
      }

      return true;
    }

    /// <summary>
    /// Write stream of nybbles
    /// </summary>
    /// <param name="stream"></param>
    public void Write(NybbleStream stream)
    {

      foreach (KeyValuePair<short, TAGDictionaryItem> kvp in Entries)
      {
        TAGDictionaryItem theElement = kvp.Value;

        byte[] bytes = Encoding.ASCII.GetBytes(theElement.Name);

        for (int i = 0; i < bytes.Length; i++)
        {
          // bytes[i];
          stream.WriteFixedSizeUnsignedInt((uint)bytes[i], 2);
        }
        stream.WriteFixedSizeUnsignedInt(0, 2); // end of name marker

        stream.WriteNybble((byte)(theElement.Type));
        stream.WriteVarSizeUnsignedInt((uint)theElement.ID);
      }

      stream.WriteFixedSizeUnsignedInt(0, 2); // end of dictionary marker
    }
  }

}
