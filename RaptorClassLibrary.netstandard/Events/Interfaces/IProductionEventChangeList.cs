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
        object PutValueAtDate(object Event);

        void SetContainer(object container);

        ProductionEventType EventListType { get; }

        IList EventsList();
    }

    /*
    public interface IProductionEventChangeList : IList
    {
        void Sort();
        void Collate();

        void SaveToStore(IStorageProxy storageProxy);
        object PutValueAtDate(object Event);

        void SetContainer(object container);

        ProductionEventType EventListType { get; }
    }
    */

    /*
     * public interface IProductionEventChangeList<T> : IProductionEventChangeList
     
    {

        IProductionEventChangeList<T> LoadFromStore(IStorageProxy storageProxy);
    }
    */

    /*
    public interface IEfficientProductionEventChangeList<T, V> : IProductionEventChangeList
    {

        IEfficientProductionEventChangeList<T, V> LoadFromStore(IStorageProxy storageProxy);

        Action<BinaryWriter, V> SerialiseStateOut { get; set; }

        Func<BinaryReader, V> SerialiseStateIn { get; set; }
    }
    */
}
