using System.Collections;
using VSS.VisionLink.Raptor.Interfaces;

namespace VSS.VisionLink.Raptor.Events.Interfaces
{
    public interface IProductionEvents
    {
        int Count();

        void Sort();

        void Collate();

        void SaveToStore(IStorageProxy storageProxy);

        void SetContainer(object container);

        ProductionEventType EventListType { get; }

        void CopyEventsFrom(IProductionEvents eventsList);
    }
}
