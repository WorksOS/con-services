using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using VSS.VisionLink.Raptor.Events.Interfaces;

namespace VSS.VisionLink.Raptor.Events
{
    /// <summary>
    /// Defines a base class for event lists that contain specific business logic governing their behaviour such as machine start
    /// stop and data recording start end events.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class EfficientSpecificProductionEventChangeList<T> : EfficientProductionEventChangeList<T, ProductionEventType> where T : IEfficientProductionEventChangeBase<ProductionEventType>, new()
    {
        // function MergeSpecificProductionEventChangeLists(const OverrideEvent: TICProductionEventChangeStartEndTimeBase) :  TICProductionEventChangeList;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public EfficientSpecificProductionEventChangeList()
        {
        }

        public EfficientSpecificProductionEventChangeList(long machineID, long siteModelID,
            ProductionEventType eventListType) : base(machineID, siteModelID, eventListType)
        {
        }

        public EfficientSpecificProductionEventChangeList(ProductionEventChanges container,
            long machineID, long siteModelID,
            ProductionEventType eventListType) : base(container, machineID, siteModelID, eventListType)
        {
        }

        /// <summary>
        /// Reads a binary serialisation of the content of the list
        /// </summary>
        /// <param name="reader"></param>
        public new static EfficientSpecificProductionEventChangeList<T> Read(BinaryReader reader)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (EfficientSpecificProductionEventChangeList<T>)formatter.Deserialize(reader.BaseStream);
        }
    }
}
