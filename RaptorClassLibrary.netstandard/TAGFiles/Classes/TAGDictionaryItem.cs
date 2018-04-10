using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes
{
    /// <summary>
    /// Describes an entry in the TAG file schema dictionary read in from the TAG file
    /// </summary>
    public class TAGDictionaryItem
    {
        /// <summary>
        /// Name of the TAG, eg: Easting
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Typpe of value this name represents, // eg: Double precision IEEE real
        /// </summary>
        public TAGDataType Type { get; set; }

        /// <summary>
        /// The internal value type id referred to by the value descriptor in the dictionary
        /// </summary>
        public short ID;             // eg: 12. Value will refer to type by using this value

        /// <summary>
        /// Constructor for TAG dictionary item accepting name, type and id
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
        public TAGDictionaryItem(string name, TAGDataType type, short id)
        {
            Name = name;
            Type = type;
            ID = id;
        }
    }
}
