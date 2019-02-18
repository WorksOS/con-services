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
  }

  public interface IProductionEvents<T> : IProductionEvents
  {
    void CopyEventsFrom(IProductionEvents<T> eventsList);

    T GetValueAtDate(DateTime eventDate, out int stateChangeIndex, T defaultValue = default(T));

    T LastStateValue(T defaultValue = default(T));
    DateTime LastStateDate();

    void GetStateAtIndex(int index, out DateTime dateTime, out T state);

    void SetStateAtIndex(int index, T state);

    void PutValueAtDate(DateTime dateTime, T state);

    void PutValuesAtDates(IEnumerable<(DateTime, T)> events);
  }
}
