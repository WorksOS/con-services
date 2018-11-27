using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;

namespace VSS.TRex.TAGFiles.GridFabric.Arguments
{
    /// <summary>
    /// Represents an internal TAG file item to be processed into a site model. It defines the underlying filename for 
    /// the TAG file, and the content of the file as a byte array
    /// </summary>
    public class ProcessTAGFileRequestFileItem
    {
      private const byte VERSION_NUMBER = 1;

        public string FileName { get; set; }

        public byte[] TagFileContent { get; set; }

        public bool IsJohnDoe { get; set; }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public ProcessTAGFileRequestFileItem()
        {
        }

      /// <summary>
      /// Creates a new item and serialises its content from the supplied IBinaryRawReader
      /// </summary>
      public ProcessTAGFileRequestFileItem(IBinaryRawReader reader)
      {
        FromBinary(reader);
      }

      public void ToBinary(IBinaryRawWriter writer)
      {
        writer.WriteByte(VERSION_NUMBER);
        writer.WriteString(FileName);
        writer.WriteBoolean(IsJohnDoe);
        writer.WriteByteArray(TagFileContent);
      }

      public void FromBinary(IBinaryRawReader reader)
      {
        byte readVersionNumber = reader.ReadByte();

        if (readVersionNumber != VERSION_NUMBER)
          throw new TRexSerializationVersionException(VERSION_NUMBER, readVersionNumber);

        FileName = reader.ReadString();
        IsJohnDoe = reader.ReadBoolean();
        TagFileContent = reader.ReadByteArray();
      }
  }
}
