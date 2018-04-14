using System.Collections;
using VSS.VisionLink.Raptor.Interfaces;

namespace VSS.VisionLink.Raptor.Events.Interfaces
{
    public interface IProductionEventChangeList : IList
    {
        void Sort();
        void Collate();

        void SaveToStore(IStorageProxy storageProxy);
        object PutValueAtDate(object Event);

        void SetContainer(object container);

        ProductionEventType EventListType { get; }
    }

    public interface IProductionEventChangeList<T> : IProductionEventChangeList
    {

        IProductionEventChangeList<T> LoadFromStore(IStorageProxy storageProxy);
    }
}
