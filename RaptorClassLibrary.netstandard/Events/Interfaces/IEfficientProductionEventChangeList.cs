using System.Collections;
using VSS.VisionLink.Raptor.Interfaces;

namespace VSS.VisionLink.Raptor.Events.Interfaces
{
    public interface IEfficientProductionEventChangeList : IList

    {
    void Sort();
    void Collate();
    object PutValueAtDate(object Event);

    void SaveToStore(IStorageProxy storageProxy);
    IEfficientProductionEventChangeList LoadFromStore(IStorageProxy storageProxy);
    }
}
