using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Events.Interfaces
{
  public interface IProductionEvents
  {
    int Count();

    void Sort();

    void Collate(IProductionEventLists container);

    MemoryStream GetMutableStream();
    MemoryStream GetImmutableStream();

    void SaveToStore(IStorageProxy storageProxy);

    ProductionEventType EventListType { get; }

    void CopyEventsFrom(IProductionEvents eventsList);

    bool EventsChanged { get; set; }

    void LoadFromStore(IStorageProxy storageProxy);

    void ReadEvents(BinaryReader reader);

    /// <summary>
    /// Returns a generic object reference to the internal list of events in this list
    /// The purpose of this is to facilitate CopyEventsFrom
    /// </summary>
    /// <returns></returns>
    object RawEventsObjects();

    List<string> ToStrings(DateTime startDate, DateTime endDate, int maxEventsToReturn);

    DateTime FirstStateDate();
    DateTime LastStateDate();

    string EventChangeListPersistantFileName();
  }

  public interface IProductionEvents<T> : IProductionEvents
  {
    void CopyEventsFrom(IProductionEvents<T> eventsList);

    T GetValueAtDate(DateTime eventDate, out int stateChangeIndex, T defaultValue);

    T LastStateValue(T defaultValue = default(T));

    void GetStateAtIndex(int index, out DateTime dateTime, out T state);

    void SetStateAtIndex(int index, T state);

    void PutValueAtDate(DateTime dateTime, T state);

    void PutValuesAtDates(IEnumerable<(DateTime, T)> events);

    void RemoveValueAtDate(DateTime dateTime);

    void Clear();

    /// <summary>
    /// Determines the index of the event whose date immediately precedes the given eventData
    /// </summary>
    /// <param name="eventDate"></param>
    /// <returns></returns>
    int IndexOfClosestEventPriorToDate(DateTime eventDate);

    /// <summary>
    /// Determines the index of the event whose date immediately follows the given eventData
    /// </summary>
    /// <param name="eventDate"></param>
    /// <returns></returns>
    int IndexOfClosestEventSubsequentToDate(DateTime eventDate);
  }
}
