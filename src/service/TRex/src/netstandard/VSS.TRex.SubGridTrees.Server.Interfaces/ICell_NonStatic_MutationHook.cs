using VSS.TRex.Cells;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  /// <summary>
  /// A hook for forwarding mutating actions on a cell passes within an individual cell.
  /// These hooks are called before the mutating event is applied to the cell
  /// </summary>
  public interface ICell_NonStatic_MutationHook
  {
    void AddPass(int X, int Y, Cell_NonStatic cell, CellPass pass, int position = -1);
    void ReplacePass(int X, int Y, Cell_NonStatic cell, int position, CellPass pass);
    void RemovePass(int X, int Y, int passIndex);

    void EmitNote(string note);

    void SetActions(ICell_NonStatic_MutationHook actions);
    void ClearActions();
  }
}
