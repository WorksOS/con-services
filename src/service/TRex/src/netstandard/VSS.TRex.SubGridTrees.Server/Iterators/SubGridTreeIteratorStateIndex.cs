using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Iterators
{
  // SubGridTreeIteratorStateIndex records iteration progress across a sub grid
  public struct SubGridTreeIteratorStateIndex
  {
    private ISubGrid subGrid;

    private void SetSubGrid(ISubGrid Value)
    {
      subGrid = Value;

      XIdx = -1;
      YIdx = -1;
    }

    // The current X/Y index of the cell at this point in the iteration
    public int XIdx;
    public int YIdx;

    // The sub grid (at any level) being iterated across
    public ISubGrid SubGrid { get => subGrid; set => SetSubGrid(value); }

    public void Initialise()
    {
      SubGrid = null;
      XIdx = -1;
      YIdx = -1;
    }

    public bool NextCell()
    {
      if (YIdx == -1)
      {
        YIdx = 0;
      }

      XIdx++;
      if (XIdx == SubGridTreeConsts.SubGridTreeDimension)
      {
        YIdx++;
        XIdx = 0;
      }

      return YIdx < SubGridTreeConsts.SubGridTreeDimension;
    }

    public bool AtLastCell() => XIdx >= SubGridTreeConsts.SubGridTreeDimensionMinus1 && YIdx >= SubGridTreeConsts.SubGridTreeDimensionMinus1;
  }
}
