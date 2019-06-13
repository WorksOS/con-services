using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes
{
    /// <summary>
    /// The core dictionary that maps the TAG file items by the ID used in the TAG file to the 
    /// description of that value type as used in the TAG file
    /// </summary>
    public class TAGDictionary
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<TAGDictionary>();

        private const int DEFAULT_TAG_FILE_SCHEMA_DICTIONARY_CAPACITY = 100;

        /// <summary>
        /// The core dictionary that maps the TAG file items by the ID used in the TAG file to the 
        /// description of that value type as used in the TAG file
        /// </summary>
        public Dictionary<short, TAGDictionaryItem> Entries { get; }

        public TAGDictionary()
        {
            Entries = new Dictionary<short, TAGDictionaryItem>(DEFAULT_TAG_FILE_SCHEMA_DICTIONARY_CAPACITY);
        }

        /// <summary>
        /// Reads the TAG file schema dictionary from the TAG file data using the TAG file reader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public bool Read(TAGReader reader)
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

                if  (!reader.ReadVarInt(out var id))
                {
                    return false;
                }

                Entries.Add(id, new TAGDictionaryItem(fieldName, fieldType, id));
            }

            if (Entries.Count > DEFAULT_TAG_FILE_SCHEMA_DICTIONARY_CAPACITY)
              Log.LogInformation($"TAG file schema dictionary final size of {Entries.Count} exceeds default capacity for dictionary of {DEFAULT_TAG_FILE_SCHEMA_DICTIONARY_CAPACITY}. Consider increasing it.");

            return true;
        }
    }
}
