namespace VSS.TRex.GridFabric.Requests.Interfaces
{
    /// <summary>
    /// Defines the interface for an aggregator that reduces, for example, cluster compute responses into a single response
    /// </summary>
    public interface IAggregateWith<T>
    {
        /// <summary>
        /// Aggregates the state contained in other with the state in 'this' and returns the result.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
       T AggregateWith(T other);
    }
}
