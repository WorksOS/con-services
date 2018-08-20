namespace VSS.TRex.Storage.Models
{
    /// <summary>
    /// Denotes if a data store has mutable (read-write) or immutable (write-new, readonly) semantics
    /// </summary>
    public enum StorageMutability
    {
        Mutable,
        Immutable
    }
}
