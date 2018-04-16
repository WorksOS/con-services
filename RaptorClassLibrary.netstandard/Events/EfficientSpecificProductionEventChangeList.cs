using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using VSS.VisionLink.Raptor.Events.Interfaces;

namespace VSS.VisionLink.Raptor.Events
{
/*
    /// <summary>
    /// Defines a base class for event lists that contain specific business logic governing their behaviour such as machine start
    /// stop and data recording start end events.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class EfficientSpecificProductionEventChangeList<T> : EfficientProductionEventChangeList<T, ProductionEventType> where T : struct, IEfficientProductionEventChangeBase<ProductionEventType>
    {
        // function MergeSpecificProductionEventChangeLists(const OverrideEvent: TICProductionEventChangeStartEndTimeBase) :  TICProductionEventChangeList;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public EfficientSpecificProductionEventChangeList()
        {
        }

        public EfficientSpecificProductionEventChangeList(EfficientProductionEventChanges container,
            long machineID, long siteModelID,
            ProductionEventType eventListType) : base(container, machineID, siteModelID, eventListType)
        {
        }

        public EfficientSpecificProductionEventChangeList(EfficientProductionEventChanges container,
            long machineID, long siteModelID,
            ProductionEventType eventListType,
            Action<BinaryWriter, ProductionEventType> serialiseStateOut,
            Func<BinaryReader, ProductionEventType> serialiseStateIn) : base(container, machineID, siteModelID, eventListType, serialiseStateOut, serialiseStateIn)
        {
        }
    }
*/
}
