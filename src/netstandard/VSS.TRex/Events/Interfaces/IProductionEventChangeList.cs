using VSS.TRex.Events.Interfaces;
using VSS.TRex.Interfaces;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Events.Interfaces
{
    public interface IProductionEvents
    {
        int Count();

        void Sort();

        void Collate();

        void SaveToStore(IStorageProxy storageProxy);

        void SetContainer(IProductionEventLists container);

        ProductionEventType EventListType { get; }

        void CopyEventsFrom(IProductionEvents eventsList);

        bool EventsChanged { get; set; }
    }    
}
