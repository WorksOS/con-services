using System;

namespace VSS.TRex.Events.Interfaces
{
  public interface IStartEndProductionEvents : IProductionEvents, IProductionEvents<ProductionEventType>, IProductionEventPairs
  {
      int IndexOfClosestEventPriorToDate(DateTime eventDate, ProductionEventType eventType);
      int IndexOfClosestEventSubsequentToDate(DateTime eventDate, ProductionEventType eventType);

      /// <summary>
      /// Implements search semantics for paired events where it is important to locate bracketing pairs of
      /// start and stop evnets given a date/time
      /// </summary>
      /// <param name="eventDate"></param>
      /// <param name="StartEventDate"></param>
      /// <param name="EndEventDate"></param>
      /// <returns></returns>
      new bool FindStartEventPairAtTime(DateTime eventDate, out DateTime StartEventDate, out DateTime EndEventDate);

      /// <summary>
      /// Implements collation semantics for event lists that do not contain homogenous lists of events. Machine start/stop and 
      /// data recording start/end are examples
      /// </summary>
      new void Collate();

      /// <summary>
      /// Provides business logic for adding start/end production event types where events define contiguous ranges rather
      /// then singular state changes at points in time
      /// </summary>
      /// <param name="Event"></param>
      /// <returns></returns>
      //void PutValueAtDate(ProductionEvents<ProductionEventType>.Event Event);
  }
}
