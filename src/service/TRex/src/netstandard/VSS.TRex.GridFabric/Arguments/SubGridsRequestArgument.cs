using System;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Types;

namespace VSS.TRex.GridFabric.Arguments
{
  /// <summary>
  /// Contains all the parameters necessary to be sent for a generic subgrids request made to the compute cluster
  /// </summary>
  public class SubGridsRequestArgument : BaseApplicationServiceRequestArgument, IEquatable<SubGridsRequestArgument>
  {
    /// <summary>
    /// The request ID for the subgrid request
    /// </summary>
    public Guid RequestID = Guid.Empty;

    /// <summary>
    /// The grid data type to extract from the processed subgrids
    /// </summary>
    public GridDataType GridDataType { get; set; } = GridDataType.All;

    /// <summary>
    /// The serialized contents of the SubGridTreeSubGridExistenceBitMask that notes the address of all subgrids that need to be requested for production data
    /// </summary>
    public byte[] ProdDataMaskBytes { get; set; }

    /// <summary>
    /// The serialized contents of the SubGridTreeSubGridExistenceBitMask that notes the address of all subgrids that need to be requested for surveyed surface data ONLY
    /// </summary>
    public byte[] SurveyedSurfaceOnlyMaskBytes { get; set; }

    /// <summary>
    /// The name of the message topic that subgrid responses should be sent to
    /// </summary>
    public string MessageTopic { get; set; } = string.Empty;

    /// <summary>
    /// Denotes whether results of these requests should include any surveyed surfaces in the site model
    /// </summary>
    public bool IncludeSurveyedSurfaceInformation { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public SubGridsRequestArgument()
    {
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteGuid(RequestID);
      writer.WriteInt((int)GridDataType);

      writer.WriteByteArray(ProdDataMaskBytes);
      writer.WriteByteArray(SurveyedSurfaceOnlyMaskBytes);

      writer.WriteString(MessageTopic);
      writer.WriteBoolean(IncludeSurveyedSurfaceInformation);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      RequestID = reader.ReadGuid() ?? Guid.Empty;
      GridDataType = (GridDataType)reader.ReadInt();

      ProdDataMaskBytes = reader.ReadByteArray();
      SurveyedSurfaceOnlyMaskBytes = reader.ReadByteArray();

      MessageTopic = reader.ReadString();
      IncludeSurveyedSurfaceInformation = reader.ReadBoolean();
    }

    public bool Equals(SubGridsRequestArgument other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;

      return base.Equals(other) && 
             RequestID.Equals(other.RequestID) && 
             GridDataType == other.GridDataType &&
             (Equals(ProdDataMaskBytes, other.ProdDataMaskBytes) ||
              ProdDataMaskBytes != null && other.ProdDataMaskBytes != null &&
              ProdDataMaskBytes.Length == other.ProdDataMaskBytes.Length && 
              ProdDataMaskBytes.SequenceEqual(other.ProdDataMaskBytes)) &&
             (Equals(SurveyedSurfaceOnlyMaskBytes, other.SurveyedSurfaceOnlyMaskBytes) ||
              SurveyedSurfaceOnlyMaskBytes != null && other.SurveyedSurfaceOnlyMaskBytes != null &&
              SurveyedSurfaceOnlyMaskBytes.Length == other.SurveyedSurfaceOnlyMaskBytes.Length &&
              SurveyedSurfaceOnlyMaskBytes.SequenceEqual(other.SurveyedSurfaceOnlyMaskBytes)) &&
             string.Equals(MessageTopic, other.MessageTopic) && 
             IncludeSurveyedSurfaceInformation == other.IncludeSurveyedSurfaceInformation;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((SubGridsRequestArgument) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ RequestID.GetHashCode();
        hashCode = (hashCode * 397) ^ (int) GridDataType;
        hashCode = (hashCode * 397) ^ (ProdDataMaskBytes != null ? ProdDataMaskBytes.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (SurveyedSurfaceOnlyMaskBytes != null ? SurveyedSurfaceOnlyMaskBytes.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (MessageTopic != null ? MessageTopic.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ IncludeSurveyedSurfaceInformation.GetHashCode();
        return hashCode;
      }
    }
  }
}
