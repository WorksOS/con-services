using System.Collections.Generic;
using System.Text;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes
{
    /// <summary>
    /// The core dictionary that maps the TAG file items by the ID used in the TAG file to the 
    /// description of that value type as used in the TAG file
    /// </summary>
    public class TAGDictionary
    {
        /// <summary>
        /// The core dictionary that maps the TAG file items by the ID used in the TAG file to the 
        /// description of that value type as used in the TAG file
        /// </summary>
        public Dictionary<short, TAGDictionaryItem> Entries { get; }

        public TAGDictionary()
        {
            Entries = new Dictionary<short, TAGDictionaryItem>();
        }

        /// <summary>
        /// Reads the TAG file schema dictionary from the TAG file data using the TAG file reader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public bool Read(TAGReader reader)
        {
            string fieldName;
            while ((fieldName = Encoding.ASCII.GetString(reader.ReadANSIString())) != string.Empty)
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

                TAGDataType fieldType = (TAGDataType)tempFieldType;

                short id;
                if  (!reader.ReadVarInt(out id))
                {
                    return false;
                }

                Entries.Add(id, new TAGDictionaryItem(fieldName, fieldType, id));
            }

            return true;
        }
    }
}
