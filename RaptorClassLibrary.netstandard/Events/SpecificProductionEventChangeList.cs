using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace VSS.VisionLink.Raptor.Events
{
    /// <summary>
    /// Defines a base class for event lists that contain specific business logic governing their behaviour such as machine start
    /// stop and data recording start end events.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class SpecificProductionEventChangeList<T> : ProductionEventChangeList<T, ProductionEventType> where T : ProductionEventChangeBase<ProductionEventType>, new()
    {
        // function MergeSpecificProductionEventChangeLists(const OverrideEvent: TICProductionEventChangeStartEndTimeBase) :  TICProductionEventChangeList;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SpecificProductionEventChangeList()
        {
        }

        public SpecificProductionEventChangeList(ProductionEventChanges container,
                                                 long machineID, long siteModelID,
                                                 ProductionEventType eventListType) : base(container, machineID, siteModelID, eventListType)
        {
        }

        /// <summary>
        /// Reads a binary serialisation of the content of the list
        /// </summary>
        /// <param name="reader"></param>
        public new static SpecificProductionEventChangeList<T> Read(BinaryReader reader)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (SpecificProductionEventChangeList<T>)formatter.Deserialize(reader.BaseStream);
        }
    }
}
