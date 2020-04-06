namespace VSS.TRex.SubGridTrees.Server.Iterators
{
    /// <summary>
    /// Controls the manner in which a leaf sub grid should be presented to a consumer of the scanner output
    /// </summary>
    public enum LeafSubGridRequestType
    {
        FullFromServer,
        TrasientSubGrid,
        SubGridAddressOnly
    }
}
