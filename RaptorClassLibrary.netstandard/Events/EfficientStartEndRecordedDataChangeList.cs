using System;
using System.Diagnostics;

namespace VSS.VisionLink.Raptor.Events
{
    /// <summary>
    /// Implements an event list containing events that detail when a machine started recording production data, and when it stopped
    /// recording production data.
    /// </summary>
    [Serializable]
    public class EfficientStartEndRecordedDataChangeList : EfficientSpecificProductionEventChangeList<EfficientProductionEventChangeBase<ProductionEventType>>
    {
        public EfficientStartEndRecordedDataChangeList()
        {
        }

        public EfficientStartEndRecordedDataChangeList(long machineID, long siteModelID,
                                              ProductionEventType eventListType) : base(machineID, siteModelID, eventListType)
        {
        }

        public EfficientStartEndRecordedDataChangeList(ProductionEventChanges container,
                                              long machineID, long siteModelID,
                                              ProductionEventType eventListType) : base(container, machineID, siteModelID, eventListType)
        {
        }

        //    function GetEventFilename: TFilename; override;
        //    function Find(const Value : TICProductionEventChangeBase;
        //    var Index: Integer): Boolean; overload; override;
        //          protected Compare(Item1, Item2 : Pointer) : Integer; overload; override;

        /// <summary>
        /// Implements search semantics for paired events where it is important to locate bracketing pairs of
        /// start and stop evnets given a date/time
        /// </summary>
        /// <param name="eventDate"></param>
        /// <param name="StartEvent"></param>
        /// <param name="EndEvent"></param>
        /// <returns></returns>
        public bool FindStartEventPairAtTime(DateTime eventDate, out EfficientProductionEventChangeBase<ProductionEventType> StartEvent, out EfficientProductionEventChangeBase<ProductionEventType> EndEvent)
        {
            int StartIndex = IndexOfClosestEventPriorToDate(eventDate, ProductionEventType.StartRecordedData);

            if (StartIndex > -1 && StartIndex < Count - 1)
            {
                StartEvent = this[StartIndex];
                EndEvent = this[StartIndex + 1];
                return true;
            }
            else
            {
                StartEvent = default(EfficientProductionEventChangeBase<ProductionEventType>);
                EndEvent = default(EfficientProductionEventChangeBase<ProductionEventType>);

                if (StartIndex == Count - 1)
                {
                    // TODO add when logging available
                    //                    SIGLogMessage.PublishNoODS(Self,
                    //                                               Format('FindStartEventPairAtTime located only one event (index:%d) at search time %.6f (%s)', { SKIP}
                    //                                            [StartIndex, EventDate, FormatDateTime(FormatSettings.LongDateFormat + ' ' + FormatSettings.LongTimeFormat, EventDate)]),
                    //                                     slmcError);
                }
            }

            return false;
        }

        /// <summary>
        /// Implements collation semantics for event lists that do not contain homogenous lists of events. Machine start/stop and 
        /// data recording start/end are examples
        /// </summary>
        public override void Collate()
        {
            // First, deal with any nested events
            // ie: Structures of the form <Start><Start><End><Start<End><End>
            // in these instances, all events bracketed by the double <start><end> events should be removed

            int I = 0;
            int NestingLevel = 0;
            while (I < Count)
            {
                bool DecNestingLevel = false;

                if (this[I].State == ProductionEventType.StartRecordedData)
                {
                    NestingLevel++;
                }
                else
                {
                    if (this[I].State == ProductionEventType.EndRecordedData)
                    {
                        DecNestingLevel = true;
                    }
                    else
                    {
                        Debug.Assert(false, "Unknown event type in list");
                    }

                    if (NestingLevel > 1)
                    {
                        RemoveAt(I);
                    }

                    if (DecNestingLevel)
                    {
                        NestingLevel--;
                    }
                }

                I++;
            }

            // Deal with collation of non-nested events
            I = 0;
            while (I < Count - 1)
            {
                Debug.Assert(this[I].Date <= this[I + 1].Date, "Start/end recorded data events are out of order");

                if (this[I].EquivalentTo(this[I + 1]) &&
                    this[I].State == ProductionEventType.StartRecordedData &&
                    this[I + 1].State == ProductionEventType.EndRecordedData)
                {
                    //Don't collate this pair - it's a single epoch tag file
                }
                else
                {
                    if (this[I].EquivalentTo(this[I + 1]) && this[I].State != this[I + 1].State)
                    {
                        RemoveAt(I); // this[I] = null;
                        RemoveAt(I); // this[I + 1] = null;
                    }
                }

                I++;
            }
        }
    }
}
