using System.Collections.Generic;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface IProfileLayers : IList<IProfileLayer>
  {
    /// <summary>
    /// Adds a new profile layer to the set of layers, recylcing a previous layer if available
    /// </summary>
    /// <param name="value"></param>
    /// <param name="layerRecycledIndex"></param>
    /// <returns></returns>
    int Add(IProfileLayer value, int layerRecycledIndex);

    /// <summary>
    /// Adds a new layer ot the list. WARNING: This behaviour supported use AddLayer with a layer and recycle index instead.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    new int Add(IProfileLayer value);

    /// <summary>
    /// Clears the number of layers present to zero.
    /// </summary>
    new void Clear();

    /// <summary>
    /// Returns the last layer in the list
    /// </summary>
    /// <returns></returns>
    IProfileLayer Last();

    /// <summary>
    /// Clears the list, optionally destroying layers already present
    /// </summary>
    /// <param name="destroyItems"></param>
    void ClearItems(bool destroyItems);

    /// <summary>
    /// Returns the internal count of layers in the list. This returns the number of layers
    /// that have defined state, rather than the total numebr of elements in the list due to
    /// layer state recycling in the analysis engine
    /// </summary>
    /// <returns></returns>
    new int Count();

    /// <summary>
    /// Retrieves a layer from the recycling use for reuse by the analysis engine
    /// </summary>
    /// <param name="layerRecycledIndex"></param>
    /// <returns></returns>
    IProfileLayer GetRecycledLayer(out int layerRecycledIndex);

    /// <summary>
    /// Inserts a layer into the list. Construction semantics mean all layers are added in the correct order meaning
    /// use of Insert() is invalid and will log a critical error and throw an assert.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    new void Insert(int index, IProfileLayer value);

    /// <summary>
    /// Determines if the cell pass in the stack of cell passes identified by passIndex is within a layer that
    /// has been superceded.
    /// </summary>
    /// <param name="passIndex"></param>
    /// <returns></returns>
    bool IsCellPassInSupersededLayer(int passIndex);

    /// <summary>
    /// Removes the last layer in the list of layers by decrementing the internal count of layers
    /// </summary>
    void RemoveLastLayer();
  }
}
