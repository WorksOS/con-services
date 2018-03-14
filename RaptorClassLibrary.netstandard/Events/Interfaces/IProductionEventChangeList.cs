using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
