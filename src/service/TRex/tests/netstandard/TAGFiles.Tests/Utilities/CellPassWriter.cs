using System;
using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace TAGFiles.Tests.Utilities
{
  /// <summary>
  /// Writes cell pass information for add/remove/replace mutation events during TAG file processing
  /// </summary>
  public class CellPassWriter : ICellPassWriter
  {
    /// <summary>
    /// The supplied delegate to handle each formatted line representing a cell pass mutation action (add, remove, replace)
    /// </summary>
    public Action<string> WriteLine { get; set; }

    /// <summary>
    /// Controls whether to output short form pass information (just location and time), or full pass information
    /// </summary>
    public bool ShortFormOutput { get; set; } = true;

    private void WriteToOutput(string line)
    {
      WriteLine.Invoke(line);
    }

    public CellPassWriter(Action<string> writeLine)
    {
      WriteLine = writeLine;
    }

    public void AddPass(uint X, uint Y, Cell_NonStatic cell, CellPass pass, int position)
    {
      var passString = ShortFormOutput ? $"Time:{pass.Time:yyyy-MM-dd hh-mm-ss.fff}" : $"{pass}";
      WriteToOutput($"AddPass {X}:{Y}->{passString}");
    }

    public void RemovePass(uint X, uint Y, int passIndex)
    {
      WriteToOutput($"RemovePass {X}:{Y}, Position:{passIndex}");
    }

    public void ReplacePass(uint X, uint Y, Cell_NonStatic cell, int position, CellPass pass)
    {
      var passString = ShortFormOutput ? $"Time:{pass.Time:yyyy-MM-dd hh-mm-ss.fff}" : $"{pass}";
      WriteToOutput($"ReplacePass {X}:{Y}:{position}->{passString}");
    }

    public void SetActions(ICell_NonStatic_MutationHook actions)
    {
      // Nothing to do
    }

    public void ClearActions()
    {
      // Nothing to do
    }
  }
}
