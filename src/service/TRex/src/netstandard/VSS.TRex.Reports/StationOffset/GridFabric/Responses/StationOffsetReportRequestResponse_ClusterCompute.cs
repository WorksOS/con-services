using System.Collections.Generic;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Responses
{
  public class StationOffsetReportRequestResponse_ClusterCompute : SubGridsPipelinedResponseBase, IAggregateWith<StationOffsetReportRequestResponse_ClusterCompute>
  {
    public List<StationOffsetRow> StationOffsetRows;

    public StationOffsetReportRequestResponse_ClusterCompute()
    {
      StationOffsetRows = new List<StationOffsetRow>();
    }

    /// <summary>
    /// Aggregate new stationOffsets into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    public StationOffsetReportRequestResponse_ClusterCompute AggregateWith(StationOffsetReportRequestResponse_ClusterCompute other)
    {
      this.StationOffsetRows.AddRange(other.StationOffsetRows);
      return this;
    }
  }
}
