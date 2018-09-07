using System;
using VSS.TRex.Events.Interfaces;

namespace VSS.TRex.Events
{
  /// <summary>
  /// Tracks events and dates through the scan of a series of cell passes to minimise the number of lookups that are mede into the event list
  /// </summary>
  public struct SiteModelMachineTargetValueTrackingState<T>
  {
    public DateTime StartDate;
    public DateTime EndDate;
    public int Index;
    public int Stamp;
    public T ThisEvent;
    public T NextEvent; 

    public void Initialise()
    {
      StartDate = DateTime.MinValue;
      EndDate = DateTime.MinValue;
      Index = -1;
      Stamp = -1;
      ThisEvent = default;
      NextEvent = default;
    }

    /// <summary>
    /// Determines if the current event period held in the tracking state brackets a cell pass time supplid in _Time
    /// </summary>
    /// <param name="_Time"></param>
    /// <returns></returns>
    public bool IsCurrentEventSuitable(DateTime _Time) => Index >= 0 && _Time >= StartDate && _Time < EndDate;

    public bool IsNextEventSuitable(int stamp, DateTime _time, IProductionEvents<T> eventList)
    {
      if (Stamp != stamp || Index < 0)
        return false;

      ProductionEvents<T> _prodEvents = (ProductionEvents<T>) eventList;

      int EventListCount = _prodEvents.Events.Count;

      int NewIndex = Index + 1;
      if (NewIndex < EventListCount)
      {
        DateTime NewEndDate;
        T NewNextEvent;

        int NewNextEventIndex = NewIndex + 1;
        if (NewNextEventIndex < EventListCount)
        {
          NewNextEvent = _prodEvents.Events[NewNextEventIndex].State;
          NewEndDate = _prodEvents.Events[NewNextEventIndex].Date;
        }
        else
        {
          NewNextEvent = default;
          NewEndDate = DateTime.MaxValue;
        }

        if (_time < NewEndDate)
        {
          Index = NewIndex;

          StartDate = EndDate;
          EndDate = NewEndDate;

          ThisEvent = NextEvent;
          NextEvent = NewNextEvent;

          return true;
        }
      }
      return false;
    }

    public void RecordEventState(int stamp, IProductionEvents<T> eventList)
    {
      Stamp = stamp;

      ProductionEvents<T> _prodEvents = (ProductionEvents<T>)eventList;
      int EventListCount = _prodEvents.Events.Count;

      if (Index < 0)
      {
        // There was no event logged at the time which is possible for example with map reset events
        StartDate = DateTime.MinValue;

        if (EventListCount > 0)
        {
          ThisEvent = _prodEvents.Events[0].State;
          EndDate = _prodEvents.Events[0].Date;
        }
        else
        {
          ThisEvent = default;
          EndDate = DateTime.MaxValue;
        }

        NextEvent = default;
        return;
      }

      if (Index < EventListCount)
      {
        ThisEvent = _prodEvents.Events[Index].State;
        StartDate = _prodEvents.Events[Index].Date;

        if (Index < EventListCount - 1)
        {
          NextEvent = _prodEvents.Events[Index + 1].State;
          EndDate = _prodEvents.Events[Index + 1].Date;
        }
        else
        {
          NextEvent = default;
          EndDate = DateTime.MaxValue;
        }
      }
      else if (EventListCount == 0)
      {
        // if there are no events in the list then just use the null/default value for all lookups
        StartDate = DateTime.MinValue;
        EndDate = DateTime.MaxValue;
        ThisEvent = default;
        NextEvent = default;
      }
    }


    /// <summary>
    /// Determines if the target or event value maintained in the tracking state is still valid for the given stamp and time
    /// parameters supplied by the caller. If not, the tracking state retrieves the appropriate value from the event list
    /// and caches it for use. Once the internal tracking state is validated and updated as necessary, the
    /// coorect target or evnet value is returned for use.
    /// </summary>
    /// <param name="stamp"></param>
    /// <param name="_Time"></param>
    /// <param name="events"></param>
    /// <returns></returns>
    public T DetermineTrackingStateValue(int stamp, DateTime _Time, IProductionEvents<T> events)
    {
      T result = default;

      if (Stamp == stamp)
      {
        if (_Time >= EndDate)
        {
          if (IsNextEventSuitable(Stamp, _Time, events))
            result = ThisEvent;
          else
          {
            result = events.GetValueAtDate(_Time, out int _);
            RecordEventState(Stamp, events);
          }
        }
      }
      else if (IsCurrentEventSuitable(_Time))
        Stamp = stamp;
      else
      {
        result = events.GetValueAtDate(_Time, out int _);
        RecordEventState(Stamp, events);
      }

      return result;
    }
  }
}
