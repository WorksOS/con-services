using System;
using System.Linq;
using Apache.Ignite.Core.Binary;
using k8s;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.TAGFiles.Models
{
    /// <summary>
    /// Represents the state of a TAG file stored in the TAG file buffer queue awaiting processing.
    /// </summary>
    public class TAGFileBufferQueueItem : IBinarizable, IFromToBinary, IEquatable<TAGFileBufferQueueItem>
    {
        private const byte kVersionNumber = 1;

        /// <summary>
        /// The date at which the TAG file was inserted into the buffer queue. This field is indexed to permit
        /// processing TAG files in the order they arrived
        /// </summary>
        public DateTime InsertUTC;

        /// <summary>
        /// The original filename for the TAG file
        /// </summary>
        public string FileName;

        /// <summary>
        /// The contents of the TAG file, as a byte array
        /// </summary>
        public byte[] Content;

        /// <summary>
        /// UID identifier of the project to process this TAG file into.
        /// This field is used as the affinity key map that determines which mutable server will
        /// store this TAG file.
        /// </summary>
        public Guid ProjectID;

        /// <summary>
        /// UID identifier of the asset to process this TAG file into
        /// </summary>
        public Guid AssetID;

        /// <summary>
        ///   Is machine a JohnDoe. No telematics device on board to identify machine or No AssetUID in system
        ///   JohnDoe machine are assigned a unique Guid
        /// </summary>
        public bool IsJohnDoe;

      public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());


      public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

      public void ToBinary(IBinaryRawWriter writer)
      {
        writer.WriteByte(kVersionNumber);

        writer.WriteLong(InsertUTC.Ticks);
        writer.WriteString(FileName);
        writer.WriteByteArray(Content);
        writer.WriteGuid(ProjectID);
        writer.WriteGuid(AssetID);
        writer.WriteBoolean(IsJohnDoe);
      }

      public void FromBinary(IBinaryRawReader reader)
      {
        var version = reader.ReadByte();

        if (version != kVersionNumber)
          throw new ArgumentException($"Invalid version number {version}, expected {kVersionNumber}");

        InsertUTC = new DateTime(reader.ReadLong());
        FileName = reader.ReadString();
        Content = reader.ReadByteArray();
        ProjectID = reader.ReadGuid() ?? Guid.Empty;
        AssetID = reader.ReadGuid() ?? Guid.Empty;
        IsJohnDoe = reader.ReadBoolean();
      }

      public bool Equals(TAGFileBufferQueueItem other)
      {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Content?.Length != other.Content?.Length) return false;

        return InsertUTC.Equals(other.InsertUTC) && 
               string.Equals(FileName, other.FileName) && 
               (Content == null || !Content.Where((b, i) => b != other.Content[i]).Any()) &&
               ProjectID.Equals(other.ProjectID) && 
               AssetID.Equals(other.AssetID) && 
               IsJohnDoe == other.IsJohnDoe;
      }

      public override bool Equals(object obj)
      {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TAGFileBufferQueueItem) obj);
      }

      public override int GetHashCode()
      {
        unchecked
        {
          var hashCode = InsertUTC.GetHashCode();
          hashCode = (hashCode * 397) ^ (FileName != null ? FileName.GetHashCode() : 0);
          hashCode = (hashCode * 397) ^ (Content != null ? Content.GetHashCode() : 0);
          hashCode = (hashCode * 397) ^ ProjectID.GetHashCode();
          hashCode = (hashCode * 397) ^ AssetID.GetHashCode();
          hashCode = (hashCode * 397) ^ IsJohnDoe.GetHashCode();
          return hashCode;
        }
      }
    }
}
