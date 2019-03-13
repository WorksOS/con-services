namespace VSS.TRex.GridFabric.Interfaces
{
  public interface IActivatePersistentGridServer
  {
    /// <summary>
    /// Set the state of a grid to active. If the grid is available and is already active, or can be set active this returns true
    /// </summary>
    /// <param name="gridName">The name of the grid to be set to active</param>
    /// <returns>True if the grid was successfully set to active, or was already in an active state</returns>
    bool SetGridActive(string gridName);

    /// <summary>
    /// Set the state of a grid to inactive. If the grid is available and is already inactive, or can be set inactive this returns true
    /// </summary>
    /// <param name="gridName">The name of the grid to be set to inactive</param>
    /// <returns>True if the grid was successfully set to inactive, or was already in an inactive state</returns>
    bool SetGridInActive(string gridName);

    /// <summary>
    /// Wait until the grid reports itself as active
    /// </summary>
    /// <param name="gridName">The name of the grid to wait for</param>
    bool WaitUntilGridActive(string gridName);
  }
}
