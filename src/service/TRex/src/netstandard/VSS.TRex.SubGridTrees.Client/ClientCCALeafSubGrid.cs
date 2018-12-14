using System.IO;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Events.Models;
using VSS.TRex.Filters.Models;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// The content of each cell in a CCA client leaf sub grid. Each cell stores a CCA related values only.
  /// </summary>
  public class ClientCCALeafSubGrid : GenericClientLeafSubGrid<SubGridCellPassDataCCAEntryRecord>
  {
    /// <summary>
    /// Initialise the null cell values for the client subgrid
    /// </summary>
    static ClientCCALeafSubGrid()
    {
      SubGridUtilities.SubGridDimensionalIterator((x, y) => NullCells[x, y] = SubGridCellPassDataCCAEntryRecord.NullValue);
    }

    /// <summary>
    /// CCA subgrids require lift processing result...
    /// </summary>
    /// <returns></returns>
    public override bool WantsLiftProcessingResults() => true;

    private void Initialise()
    {
      EventPopulationFlags |=
        PopulationControlFlags.WantsTargetCCAValues |
        PopulationControlFlags.WantsEventMinElevMappingValues;

      _gridDataType = TRex.Types.GridDataType.CCA;
    }

    /// <summary>
    /// Determines if the CCA at the cell location is null or not.
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <returns></returns>
    public override bool CellHasValue(byte cellX, byte cellY) => Cells[cellX, cellY].MeasuredCCA != CellPassConsts.NullCCA;

    /// <summary>
    /// Constructs a default client subgrid with no owner or parent, at the standard leaf bottom subgrid level,
    /// and using the default cell size and index origin offset
    /// </summary>
    public ClientCCALeafSubGrid()
    {
      Initialise();
    }

    /// <summary>
    /// Constructor. Set the grid to CCA.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="parent"></param>
    /// <param name="level"></param>
    /// <param name="cellSize"></param>
    /// <param name="indexOriginOffset"></param>
    public ClientCCALeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
    {
      Initialise();
    }

    /// <summary>
    /// Determine if a filtered CCA is valid (not null)
    /// </summary>
    /// <param name="filteredValue"></param>
    /// <returns></returns>
    public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue) => filteredValue.FilteredPass.CCA == CellPassConsts.NullCCA;

    /// <summary>
    /// Assign filtered CCA value from a filtered pass to a cell
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <param name="context"></param>
    public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext context)
    {
      Cells[cellX, cellY].MeasuredCCA = context.FilteredValue.FilteredPassData.FilteredPass.CCA;
      Cells[cellX, cellY].TargetCCA = context.FilteredValue.FilteredPassData.TargetValues.TargetCCA;
      Cells[cellX, cellY].IsUndercompacted = false;
      Cells[cellX, cellY].IsTooThick = false;
      Cells[cellX, cellY].IsTopLayerTooThick = false;
      Cells[cellX, cellY].IsTopLayerUndercompacted = false;
      Cells[cellX, cellY].IsOvercompacted = false;

      var lowLayerIndex = -1;
      var highLayerIndex = -1;

      IProfileLayers layers = ((IProfileCell) context.CellProfile).Layers;

      for (var i = layers.Count() - 1; i >= 0; i--)
      {
        if ((layers[i].Status & LayerStatus.Superseded) == 0)
        {
          lowLayerIndex = highLayerIndex = i;
          break;
        }
      }

      if (highLayerIndex > -1 && lowLayerIndex > -1)
      {
        for (var i = lowLayerIndex; i <= highLayerIndex; i++)
        {
          var layer = layers[i];

          if (layer.FilteredPassCount == 0)
            continue;

          if ((layer.Status & LayerStatus.Undercompacted) != 0)
              Cells[cellX, cellY].IsUndercompacted = true;

          if ((layer.Status & LayerStatus.Overcompacted) != 0)
            Cells[cellX, cellY].IsOvercompacted = true;
        }
      }
    }

    /// <summary>
    /// Fills the contents of the client leaf subgrid with a known, non-null test pattern of values
    /// </summary>
    public override void FillWithTestPattern()
    {
      ForEach((x, y) => Cells[x, y] = new SubGridCellPassDataCCAEntryRecord { MeasuredCCA = x, TargetCCA = y });
    }

    /// <summary>
    /// Determines if the leaf content of this subgrid is equal to 'other'
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool LeafContentEquals(IClientLeafSubGrid other)
    {
      bool result = true;

      IGenericClientLeafSubGrid<SubGridCellPassDataCCAEntryRecord> _other = (IGenericClientLeafSubGrid<SubGridCellPassDataCCAEntryRecord>)other;
      ForEach((x, y) => result &= Cells[x, y].Equals(_other.Cells[x, y]));

      return result;
    }

    public override SubGridCellPassDataCCAEntryRecord NullCell() => SubGridCellPassDataCCAEntryRecord.NullValue;

    /// <summary>
    /// Sets all cell CCAs to null and clears the first pass and surveyed surface pass maps
    /// </summary>
    public override void Clear()
    {
      base.Clear();
    }

    /// <summary>
    /// Dumps CCA values from subgrid to the log
    /// </summary>
    /// <param name="title"></param>
    public override void DumpToLog(string title)
    {
      base.DumpToLog(title);
    }

    /// <summary>
    /// Write the contents of the Items array using the supplied writer
    /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
    /// Override to implement if needed.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    public override void Write(BinaryWriter writer, byte[] buffer)
    {
      base.Write(writer, buffer);

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y].Write(writer));
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
    /// Override to implement if needed.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="buffer"></param>
    public override void Read(BinaryReader reader, byte[] buffer)
    {
      base.Read(reader, buffer);

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y].Read(reader));
    }

    /// <summary>
    /// Return an indicative size for memory consumption of this class to be used in cache tracking
    /// </summary>
    /// <returns></returns>
    public override int IndicativeSizeInBytes()
    {
      return base.IndicativeSizeInBytes() +
             SubGridTreeConsts.SubGridTreeCellsPerSubgrid * SubGridCellPassDataCCAEntryRecord.IndicativeSizeInBytes();
    }
  }
}
