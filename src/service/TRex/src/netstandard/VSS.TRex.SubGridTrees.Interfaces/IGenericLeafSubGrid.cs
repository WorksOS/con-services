using System;

namespace VSS.TRex.SubGridTrees.Interfaces
{
  public interface IGenericLeafSubGrid<T> : ILeafSubGrid
  {
    T[,] Items { get; }

    bool ForEach(Func<T, bool> functor);
  }
}
