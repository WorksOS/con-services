using System;

namespace VSS.TRex.Events.Interfaces
{
    public interface IProductionEventPairs : IProductionEvents
    {
        bool FindStartEventPairAtTime(DateTime eventDate, out DateTime startEventDate, out DateTime endEventDate);
    }
}
