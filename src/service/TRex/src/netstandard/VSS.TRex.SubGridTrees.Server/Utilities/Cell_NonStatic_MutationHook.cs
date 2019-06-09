using System;
using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Utilities
{
  /// <summary>
  /// Implements a mutation hook for cell pass changes during TAG file processing using action delegates to provided
  /// dynamic control of the hook behaviours.
  /// </summary>
  public class Cell_NonStatic_MutationHook : ICell_NonStatic_MutationHook
  {
    /// <summary>
    /// The action to take when a pass is added
    /// </summary>
    public Action<int, int, Cell_NonStatic, CellPass, int> AddPassAction { get; set; }

    /// <summary>
    /// The action to take when a pass is replaced
    /// </summary>
    public Action<int, int, Cell_NonStatic, int, CellPass> ReplacePassAction { get; set; }

    /// <summary>
    /// The action to take when a pass is deleted
    /// </summary>
    public Action<int, int, int> RemovePassAction { get; set; }

    /// <summary>
    /// Emits an arbitrary note to the mutation hook
    /// </summary>
    public Action<string> EmitNoteAction { get; set; } = null;

    // Default constructor for the mutation hook.
    public Cell_NonStatic_MutationHook()
    {
    }

    /// <summary>
    /// Constructor accepting the three action delegates for the mutation hook
    /// </summary>
    /// <param name="addPassAction"></param>
    /// <param name="replaceAction"></param>
    /// <param name="removeAction"></param>
    public Cell_NonStatic_MutationHook(Action<int, int, Cell_NonStatic, CellPass, int> addPassAction,
      Action<int, int, Cell_NonStatic, int, CellPass> replaceAction,
      Action<int, int, int> removeAction,
      Action<string> emitNoteAction)
    {
      AddPassAction = addPassAction;
      ReplacePassAction = replaceAction;
      RemovePassAction = removeAction;
      EmitNoteAction = emitNoteAction;
    }

    public Cell_NonStatic_MutationHook(ICell_NonStatic_MutationHook actions) : this()
    {
      SetActions(actions);
    }

    /// <summary>
    /// Receives the details for a pass to be added and relays it to the defined action
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="cell"></param>
    /// <param name="pass"></param>
    /// <param name="position"></param>
    public virtual void AddPass(int X, int Y, Cell_NonStatic cell, CellPass pass, int position = -1)
    {
      AddPassAction?.Invoke(X, Y, cell, pass, position);
    }

    /// <summary>
    /// Receives the details for a pass to be replaced and relays it to the defined action
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="cell"></param>
    /// <param name="pass"></param>
    /// <param name="position"></param>
    public virtual void ReplacePass(int X, int Y, Cell_NonStatic cell, int position, CellPass pass)
    {
      ReplacePassAction?.Invoke(X, Y, cell, position, pass);
    }

    /// <summary>
    /// Receives the details for a pass to be deleted and relays it to the defined action
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    public virtual void RemovePass(int X, int Y, int passIndex)
    {
      RemovePassAction?.Invoke(X, Y, passIndex);
    }

    /// <summary>
    /// Emits a note string to the mutation hook
    /// </summary>
    /// <param name="note"></param>
    public virtual void EmitNote(string note)
    {
      EmitNoteAction?.Invoke(note);
    }

    public void SetActions(ICell_NonStatic_MutationHook actions)
    {
      AddPassAction = actions.AddPass;
      RemovePassAction = actions.RemovePass;
      ReplacePassAction = actions.ReplacePass;
      EmitNoteAction = actions.EmitNote;
    }

    public void ClearActions()
    {
      AddPassAction = null;
      RemovePassAction = null;
      ReplacePassAction = null;
      EmitNoteAction = null;
    }
  }
}
