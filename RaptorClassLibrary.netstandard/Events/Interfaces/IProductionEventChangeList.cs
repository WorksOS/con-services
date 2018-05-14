using VSS.TRex.Events.Interfaces;
using VSS.VisionLink.Raptor.Interfaces;

namespace VSS.VisionLink.Raptor.Events.Interfaces
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
