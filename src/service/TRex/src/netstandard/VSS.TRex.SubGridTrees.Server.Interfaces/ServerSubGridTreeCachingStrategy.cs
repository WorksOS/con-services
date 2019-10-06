namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public enum ServerSubGridTreeCachingStrategy
  {
    /// <summary>
    /// Sub grids loaded from the grid layer are independently cached within a server sub grid tree as
    /// fully hydrated sub grids read from the grid cache/persistence layer
    /// </summary>
    CacheSubGridsInTree,

    /// <summary>
    /// Sub grids are only cached within the grid cache/persistence layer in their un-hydrated/compressed form.
    /// Sub grids retrieved using this strategy have a life cycle constrained by the involvement of the sub grid
    /// in the processing of the request that stimulated the request for the sub grid in question
    /// </summary>
    CacheSubGridsInIgniteGridCache
  }
}
