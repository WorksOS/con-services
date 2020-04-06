using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Iterators
{
  // SubGridTreeIteratorStateIndex records iteration progress across a sub grid
  public struct SubGridTreeIteratorStateIndex
  {
    private ISubGrid _subGrid;

    private void SetSubGrid(ISubGrid value)
    {
      _subGrid = value;

      XIdx = -1;
      YIdx = -1;
    }

    // The current X/Y index of the cell at this point in the iteration
    public int XIdx { get; set; }
    public int YIdx { get; set; }

    // The sub grid (at any level) being iterated across
    public ISubGrid SubGrid { get => _subGrid; set => SetSubGrid(value); }

    public void Initialise()
    {
      SubGrid = null;
      XIdx = -1;
      YIdx = -1;
    }

    public bool NextCell()
    {
      if (YIdx == -1)
        YIdx = 0;

      XIdx++;
      if (XIdx == SubGridTreeConsts.SubGridTreeDimension)
      {
        YIdx++;
        XIdx = 0;
      }

      return YIdx < SubGridTreeConsts.SubGridTreeDimension;
    }
  }
}
