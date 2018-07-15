using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// A set of layers computed from a stack of cell passes
  /// </summary>
    public class ProfileLayers : List<ProfileLayer>
    {
      private static ILogger Log = Logging.Logger.CreateLogger<ProfileLayers>();

    /// <summary>
    /// InternalCount contains the count of the layers held within the list that
    /// are actually used. This permits the same set of layers to be used over and
    /// over again without the overhead of creating and destroying the same set
    /// of layers over and over again
      /// </summary>
      private int InternalCount;

      /// <summary>
      /// Default constructor
      /// </summary>
      public ProfileLayers()
      {
        InternalCount = 0;
      }

      /// <summary>
      /// Clears the set of layers defined over the list of cell passes.
      /// </summary>
      private void InternalClear()
      {
        base.Clear();
        InternalCount = 0;
      }

      /// <summary>
      /// Adds a new profile layer to the set of layers, recylcing a previous layer if available
      /// </summary>
      /// <param name="value"></param>
      /// <param name="layerRecycledIndex"></param>
      /// <returns></returns>
      public int Add(ProfileLayer value, int layerRecycledIndex)
      {
        int result;

        if (layerRecycledIndex == -1)
        {
          base.Add(value);
          result = base.Count - 1;
        }
        else
          result = layerRecycledIndex;

        InternalCount++;

        return result;
      }

      /// <summary>
      /// Adds a new layer ot the list. WARNING: This behaviour supported use AddLayer with a layer and recycle index instead.
      /// </summary>
      /// <param name="value"></param>
      /// <returns></returns>
      public new int Add(ProfileLayer value)
      {
        Log.LogError("Standard TObjectList.Add() not permitted in this descendant");
        throw new ArgumentException("Standard TObjectList.Add() not permitted in this descendant");
      }

      /// <summary>
      /// Clears the number of layers present to zero.
      /// </summary>
      public new void Clear() => InternalCount = 0;

      /// <summary>
      /// Returns the last layer in the list
      /// </summary>
      /// <returns></returns>
      public ProfileLayer Last() => this[InternalCount - 1];

      /// <summary>
      /// Clears the list, optionally destroying layers already present
      /// </summary>
      /// <param name="destroyItems"></param>
      public void ClearItems(bool destroyItems)
      {
        if (destroyItems)
          InternalClear();
        else
          Clear();
      }

      /// <summary>
      /// Returns the internal count of layers in the list. This returns the number of layers
      /// that have defined state, rather than the total numebr of elements in the list due to
      /// layer state recycling in the analysis engine
      /// </summary>
      /// <returns></returns>
      public new int Count() => InternalCount;

      /// <summary>
      /// Retrieves a layer from the recycling use for reuse by the analysis engine
      /// </summary>
      /// <param name="layerRecycledIndex"></param>
      /// <returns></returns>
      public ProfileLayer GetRecycledLayer(out int layerRecycledIndex)
      {
        if (InternalCount < base.Count)
        {
          layerRecycledIndex = InternalCount;
          return base[layerRecycledIndex];
        }

        layerRecycledIndex = -1;
        return null;
      }

      /// <summary>
      /// Inserts a layer into the list. Construction semantics mean all layers are added in the correct order meaning
      /// use of Insert() is invalid and will log a critical error and throw an assert.
      /// </summary>
      /// <param name="index"></param>
      /// <param name="value"></param>
      public new void Insert(int index, ProfileLayer value)
      {
        Log.LogCritical("Layers should be added in consistent order - insert inconstent with this work flow");
        Debug.Assert(false, "Layers should be added in consistent order - insert inconstent with this work flow");

        //  base.Insert(Index, Value);
        //  InternalCount++;
      }

      /// <summary>
      /// Determines if the cell pass in the stack of cell passes identified by passIndex is within a layer that
      /// has been superceded.
      /// </summary>
      /// <param name="passIndex"></param>
      /// <returns></returns>
      public bool IsCellPassInSupersededLayer(int passIndex)
      {
        for (int i = InternalCount - 1; i >= 0; i--)
        {
          if (passIndex >= this[i].StartCellPassIdx && passIndex <= this[i].EndCellPassIdx)
            return (this[i].Status & LayerStatus.Superseded) != 0;
        }

        return true;
      }

      /// <summary>
      /// Removes the last layer in the list of layers by decrementing the internal count of layers
      /// </summary>
      public void RemoveLastLayer()
      {
        if (InternalCount > 0)
          InternalCount--;
      }
  }
}
