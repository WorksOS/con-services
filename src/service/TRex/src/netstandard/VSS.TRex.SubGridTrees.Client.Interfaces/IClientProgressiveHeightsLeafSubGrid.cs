using System;
using System.Collections.Generic;

namespace VSS.TRex.SubGridTrees.Client.Interfaces
{
  public interface IClientProgressiveHeightsLeafSubGrid
  {
    List<(float[,] Heights, long[,] Times, DateTime Date)> Layers { get; set; }
    int NumberOfHeightLayers { get; set; }
    void AssignFilteredValue(int heightIndex, byte cellX, byte cellY, float height, long time);
  }
}
