using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace VSS.TRex.Profiling
{
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

      public ProfileLayers()
      {
        InternalCount = 0;
      }

      private void InternalClear()
      {
        base.Clear();
        InternalCount = 0;
      }

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

      public new int Add(ProfileLayer value)
      {
        Log.LogError("Standard TObjectList.Add() not permitted in this descendant");
        throw new ArgumentException("Standard TObjectList.Add() not permitted in this descendant");
      }

      public new void Clear() => InternalCount = 0;

      public ProfileLayer Last() => this[InternalCount - 1];

      public void ClearItems(bool destroyItems)
      {
        if (destroyItems)
          InternalClear();
        else
          Clear();
      }

      public new int Count() => InternalCount;

      public ProfileLayer GetRecycledLayer(out int layerRecycledIndex)
      {
        if (InternalCount > base.Count)
        {
          layerRecycledIndex = InternalCount;
          return base[layerRecycledIndex];
        }

        layerRecycledIndex = -1;
        return null;
      }

      public new void Insert(int index, ProfileLayer value)
      {
        Log.LogCritical("Layers should be added in consistent order - insert inconstent with this work flow");

        //  base.Insert(Index, Value);
        //  InternalCount++;
      }

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
