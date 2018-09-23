namespace VSS.TRex.Events.Interfaces
{
  public interface IMachinesProductionEventLists
  {
    /// <summary>
    /// Default (short) indexer for the machines target values that uses the machine ID map dictionary to locate the set of 
    /// events lists for that machine.
    /// </summary>
    /// <param name="machineID"></param>
    /// <returns></returns>
    IProductionEventLists this[short machineID] { get; }

    /// <summary>
    /// Overrides the base List T Add() method to add the item to the local machine ID map dictionary as well as add it to the list
    /// </summary>
    /// <param name="events"></param>
    void Add(IProductionEventLists events);
  }
}
