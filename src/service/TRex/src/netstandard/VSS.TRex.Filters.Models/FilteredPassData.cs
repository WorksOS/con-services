using Apache.Ignite.Core.Binary;
using VSS.TRex.Cells;
using VSS.TRex.Types;

namespace VSS.TRex.Filters.Models
{
  /// <summary>
  /// Contains a filtered cell pass including the events and target values relevant to the 
  /// cell pass at the time it was measured
  /// </summary>
  public struct FilteredPassData
  {
    /// <summary>
    /// The type of the machine (eg: bulldozer, grader, compactor, etc) that collected the pass represented here.
    /// </summary>
    public MachineType MachineType;   // Derived from the machine ID in the FilteredPass record

    public CellPass FilteredPass;
    public CellTargets TargetValues;
    public CellEvents EventValues;

    /// <summary>
    /// Initialise all state to null
    /// </summary>
    public void Clear()
    {
      MachineType = 0;
      FilteredPass.Clear();
      TargetValues.Clear();
      EventValues.Clear();
    }

    /// <summary>
    /// Copy the state of another FilteredPassData instance to this one
    /// </summary>
    /// <param name="source"></param>
    public void Assign(FilteredPassData source)
    {
      MachineType = source.MachineType;
      FilteredPass.Assign(source.FilteredPass);
      TargetValues.Assign(source.TargetValues);
      EventValues.Assign(source.EventValues);
    }

    /// <summary>
    /// Serializes content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte((byte)MachineType);

      FilteredPass.ToBinary(writer);
      TargetValues.ToBinary(writer);
      EventValues.ToBinary(writer);
    }

    /// <summary>
    /// Serializes content of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      MachineType = (MachineType)reader.ReadByte();

      FilteredPass.FromBinary(reader);
      TargetValues.FromBinary(reader);
      EventValues.FromBinary(reader);
    }
  }
}
