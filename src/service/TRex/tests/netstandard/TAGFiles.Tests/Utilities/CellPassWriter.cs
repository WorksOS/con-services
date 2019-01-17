using System.IO;
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
    /// The TextWriter used to emit cell pass information
    /// </summary>
    public TextWriter Writer { get; set; }

    /// <summary>
    /// Controls whether to output short form pass information (just location and time), or full pass information
    /// </summary>
    public bool ShortFormOutput { get; set; } = true;

    public CellPassWriter(TextWriter writer)
    {
      Writer = writer;
    }

    public void AddPass(uint X, uint Y, Cell_NonStatic cell, CellPass pass, int position)
    {
      var passString = ShortFormOutput ? $"Time:{pass.Time:yyyy-MM-dd hh-mm-ss.fff}" : $"{pass}";
      Writer.WriteLine($"AddPass {X}:{Y}->{passString}");
    }

    public void RemovePass(uint X, uint Y, int passIndex)
    {
      Writer.WriteLine($"RemovePass {X}:{Y}, Position:{passIndex}");
    }

    public void ReplacePass(uint X, uint Y, Cell_NonStatic cell, int position, CellPass pass)
    {
      var passString = ShortFormOutput ? $"Time:{pass.Time:yyyy-MM-dd hh-mm-ss.fff}" : $"{pass}";
      Writer.WriteLine($"ReplacePass {X}:{Y}:{position}->{passString}");
    }

    public void Close() => Writer.Close();

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
