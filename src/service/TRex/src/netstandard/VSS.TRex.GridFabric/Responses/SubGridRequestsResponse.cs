using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Models;

namespace VSS.TRex.GridFabric.Responses
{
  /// <summary>
  /// SubGridRequestsResponse represents the formal completion response sent back to a requestor from a 
  /// SubGridRequests request. Content includes the cluster node identity returning the response, a general response
  /// code covering the request plus additional statistical data such as the number of subgrids processed by 
  /// that cluster node from the overall pool of subgrid requested
  /// </summary>
  public class SubGridRequestsResponse : BaseRequestResponse, IAggregateWith<SubGridRequestsResponse>
  {
    private const byte VERSION_NUMBER = 1;

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
    writer.WriteByte(VERSION_NUMBER);
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
      var version = reader.ReadByte();

      if (version != VERSION_NUMBER)
        throw new TRexSerializationVersionException(VERSION_NUMBER, version);

      ResponseCode = (SubGridRequestsResponseResult)reader.ReadInt();
      ClusterNode = reader.ReadString();
      NumSubgridsProcessed = reader.ReadLong();
      NumSubgridsExamined = reader.ReadLong();
      NumProdDataSubGridsProcessed = reader.ReadLong();
      NumProdDataSubGridsExamined = reader.ReadLong();
      NumSurveyedSurfaceSubGridsProcessed = reader.ReadLong();
      NumSurveyedSurfaceSubGridsExamined = reader.ReadLong();
    }

    public SubGridRequestsResponse AggregateWith(SubGridRequestsResponse other)
    {
      // No explicit 'accumulation' logic for response codes apart from prioritizing failure over success results
      ResponseCode = ResponseCode == SubGridRequestsResponseResult.Unknown ? other.ResponseCode :
        ResponseCode == SubGridRequestsResponseResult.OK & other.ResponseCode != SubGridRequestsResponseResult.OK ? other.ResponseCode : ResponseCode;

      ClusterNode = other.ClusterNode; // No explicit 'aggregation' logic for response codes

      NumSubgridsProcessed += other.NumSubgridsProcessed;
      NumSubgridsExamined += other.NumSubgridsExamined;
      NumProdDataSubGridsProcessed += other.NumProdDataSubGridsProcessed;
      NumProdDataSubGridsExamined += other.NumProdDataSubGridsExamined;
      NumSurveyedSurfaceSubGridsProcessed += other.NumSurveyedSurfaceSubGridsProcessed;
      NumSurveyedSurfaceSubGridsExamined += other.NumSurveyedSurfaceSubGridsExamined;

      return this;
    }
  }
}
