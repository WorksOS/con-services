using System.Collections;
using VSS.VisionLink.Raptor.Interfaces;

namespace VSS.VisionLink.Raptor.Events.Interfaces
{
    public interface IProductionEventChangeList : IList
    {
        void Sort();
        void Collate();
        object PutValueAtDate(object Event);

        void SaveToStore(IStorageProxy storageProxy);
        IProductionEventChangeList LoadFromStore(IStorageProxy storageProxy);
    }
}
