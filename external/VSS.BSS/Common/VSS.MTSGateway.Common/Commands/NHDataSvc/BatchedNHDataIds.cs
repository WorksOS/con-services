using System;
using VSS.Nighthawk.NHDataSvc.Interfaces.Events;

namespace VSS.Nighthawk.MTSGateway.Common.Commands.NHDataSvc
{
  public class BatchedNHDataIds : IBatchedNHDataIdsEvent
  {
    public string URL { get; set; }
    public long[] Ids { get; set; }
    public DateTime TimeStamp { get; set; }
  }
}
