
using System;
namespace VSS.Nighthawk.NHDataSvc.Interfaces.Events
{
  public interface IBatchedNHDataIdsEvent
  {
    string URL { get; set; }
    long[] Ids { get; set; }
    DateTime TimeStamp { get; set; } 
  }
}
