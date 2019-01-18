using VSS.TRex.Cells;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  /// <summary>
  /// A hook for forwarding mutating actions on a cell passes within an individual cell.
  /// These hooks are called before the mutating event is applied to the cell
  /// </summary>
  public interface ICell_NonStatic_MutationHook
  {
    void AddPass(uint X, uint Y, Cell_NonStatic cell, CellPass pass, int position = -1);
    void ReplacePass(uint X, uint Y, Cell_NonStatic cell, int position, CellPass pass);
    void RemovePass(uint X, uint Y, int passIndex);

    void SetActions(ICell_NonStatic_MutationHook actions);
    void ClearActions();
  }
}
