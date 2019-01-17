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
    public TextWriter Writer { get; set; }

    public CellPassWriter(TextWriter writer)
    {
      Writer = writer;
    }

    public void AddPass(uint X, uint Y, Cell_NonStatic cell, CellPass pass, int position)
    {
      Writer.WriteLine($"AddPass {X}:{Y}->{pass}");
    }

    public void RemovePass(uint X, uint Y, int passIndex)
    {
      Writer.WriteLine($"RemovePass {X}:{Y}, Position:{passIndex}");
    }

    public void ReplacePass(uint X, uint Y, Cell_NonStatic cell, int position, CellPass pass)
    {
      Writer.WriteLine($"ReplacePass {X}:{Y}:{position}->{pass}");
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
