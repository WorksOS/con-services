namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Iterators
{
    /// <summary>
    /// Controls the manner in which a leaf subgrid should be presented to a consumer of the scanner output
    /// </summary>
    public enum LeafSubgridRequestType
    {
        FullFromServer,
        TransientSubgrid,
        SubgridAddressOnly
    }
}
