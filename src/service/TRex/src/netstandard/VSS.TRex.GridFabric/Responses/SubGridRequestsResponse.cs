using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Models;

namespace VSS.TRex.GridFabric.Responses
{
  /// <summary>
  /// SubGridRequestsResponse represents the formal completion response sent back to a requestor from a 
  /// SubGridRequests request. Content includes the cluster node identity returning the response, a general response
  /// code covering the request plus additional statistical data such as the number of subgrids processed by 
  /// that cluster node from the overall pool of subgrid requested
  /// </summary>
  public class SubGridRequestsResponse : BaseRequestResponse, IEquatable<BaseRequestResponse>
  {
    /// <summary>
    /// The general subgrids request response code returned for the request
    /// </summary>
    public SubGridRequestsResponseResult ResponseCode { get; set; } = SubGridRequestsResponseResult.Unknown;

    /// <summary>
    /// The moniker of the cluster node making the response
    /// </summary>
    public string ClusterNode { get; set; } = string.Empty;

    /// <summary>
    /// The number of subgrids in the total subgrids request processed by the responding cluster node
    /// </summary>
    public long NumSubgridsProcessed { get; set; } = -1;

    /// <summary>
    /// The total number of subgrids scanned by the processing cluster node. This should match the overall number
    /// of subgrids in the request unless ResponseCode indicates a failure.
    /// </summary>
    public long NumSubgridsExamined { get; set; } = -1;

    /// <summary>
    /// The number of subgrids containing production data in the total subgrids request processed by the responding cluster node
    /// </summary>
    public long NumProdDataSubGridsProcessed { get; set; } = -1;

    /// <summary>
    /// The total number of subgrids containing production data scanned by the processing cluster node. This should match the overall number
    /// of production data subgrids in the request unless ResponseCode indicates a failure.
    /// </summary>
    public long NumProdDataSubGridsExamined { get; set; } = -1;

    /// <summary>
    /// The number of subgrids containing surveyed surfaces data in the total subgrids request processed by the responding cluster node
    /// </summary>
    public long NumSurveyedSurfaceSubGridsProcessed { get; set; } = -1;

    /// <summary>
    /// The total number of subgrids containing surveyed surface data scanned by the processing cluster node. This should match the overall number
    /// of surveyed surface subgrids in the request unless ResponseCode indicates a failure.
    /// </summary>
    public long NumSurveyedSurfaceSubGridsExamined { get; set; } = -1;

  public override void ToBinary(IBinaryRawWriter writer)
  {
    writer.WriteInt((int)ResponseCode);
    writer.WriteString(ClusterNode);
    writer.WriteLong(NumSubgridsProcessed);
    writer.WriteLong(NumSubgridsExamined);
    writer.WriteLong(NumProdDataSubGridsProcessed);
    writer.WriteLong(NumProdDataSubGridsExamined);
    writer.WriteLong(NumSurveyedSurfaceSubGridsProcessed);
    writer.WriteLong(NumSurveyedSurfaceSubGridsExamined);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      ResponseCode = (SubGridRequestsResponseResult)reader.ReadInt();
      ClusterNode = reader.ReadString();
      NumSubgridsProcessed = reader.ReadLong();
      NumSubgridsExamined = reader.ReadLong();
      NumProdDataSubGridsProcessed = reader.ReadLong();
      NumProdDataSubGridsExamined = reader.ReadLong();
      NumSurveyedSurfaceSubGridsProcessed = reader.ReadLong();
      NumSurveyedSurfaceSubGridsExamined = reader.ReadLong();
    }

    protected bool Equals(SubGridRequestsResponse other)
    {
      return ResponseCode == other.ResponseCode && 
             string.Equals(ClusterNode, other.ClusterNode) && 
             NumSubgridsProcessed == other.NumSubgridsProcessed && 
             NumSubgridsExamined == other.NumSubgridsExamined && 
             NumProdDataSubGridsProcessed == other.NumProdDataSubGridsProcessed && 
             NumProdDataSubGridsExamined == other.NumProdDataSubGridsExamined && 
             NumSurveyedSurfaceSubGridsProcessed == other.NumSurveyedSurfaceSubGridsProcessed && 
             NumSurveyedSurfaceSubGridsExamined == other.NumSurveyedSurfaceSubGridsExamined;
    }

    public bool Equals(BaseRequestResponse other)
    {
      return Equals(other as SubGridRequestsResponse);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((SubGridRequestsResponse) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (int) ResponseCode;
        hashCode = (hashCode * 397) ^ (ClusterNode != null ? ClusterNode.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ NumSubgridsProcessed.GetHashCode();
        hashCode = (hashCode * 397) ^ NumSubgridsExamined.GetHashCode();
        hashCode = (hashCode * 397) ^ NumProdDataSubGridsProcessed.GetHashCode();
        hashCode = (hashCode * 397) ^ NumProdDataSubGridsExamined.GetHashCode();
        hashCode = (hashCode * 397) ^ NumSurveyedSurfaceSubGridsProcessed.GetHashCode();
        hashCode = (hashCode * 397) ^ NumSurveyedSurfaceSubGridsExamined.GetHashCode();
        return hashCode;
      }
    }
  }
}
