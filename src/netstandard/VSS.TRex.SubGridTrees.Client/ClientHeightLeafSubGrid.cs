using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using VSS.TRex.Common;
using VSS.TRex.Filters.Models;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// The content of each cell in a height client leaf sub grid. Each cell stores an elevation only.
  /// </summary>
  public class ClientHeightLeafSubGrid : GenericClientLeafSubGrid<float>, IClientHeightLeafSubGrid
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// First pass map records which cells hold cell pass heights that were derived
    /// from the first pass a machine made over the corresponding cell
    /// </summary>
    public SubGridTreeBitmapSubGridBits FirstPassMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

    /// <summary>
    /// Surveyed surface map records which cells hold cell pass heights that were derived
    /// from a surveyed surface
    /// </summary>
    public SubGridTreeBitmapSubGridBits SurveyedSurfaceMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

    /// <summary>
    /// Initialise the null cell values for the client subgrid
    /// </summary>
    static ClientHeightLeafSubGrid()
    {
      SubGridUtilities.SubGridDimensionalIterator((x, y) => NullCells[x, y] = Consts.NullHeight);
    }

    private void Initialise()
    {
      _gridDataType = TRex.Types.GridDataType.Height;
    }

    /// <summary>
    /// Constructs a default client subgrid with no owner or parent, at the standard leaf bottom subgrid level,
    /// and using the default cell size and index origin offset
    /// </summary>
    public ClientHeightLeafSubGrid() : base()
    {
      Initialise();
    }

    /// <summary>
    /// Constructor. Set the grid to HeightAndTime.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="parent"></param>
    /// <param name="level"></param>
    /// <param name="cellSize"></param>
    /// <param name="indexOriginOffset"></param>
    public ClientHeightLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
    {
      Initialise();
    }

    /// <summary>
    /// Assign contents of another height client lead sub grid to this one
    /// </summary>
    /// <param name="heightAndTimeResults"></param>
    public void Assign(ClientHeightAndTimeLeafSubGrid heightAndTimeResults)
    {
      base.Assign(heightAndTimeResults);

      Buffer.BlockCopy(heightAndTimeResults.Cells, 0, Cells, 0, SubGridTreeConsts.SubGridTreeCellsPerSubgrid * sizeof(float));

      SurveyedSurfaceMap.Assign(heightAndTimeResults.SurveyedSurfaceMap);
    }

    /// <summary>
    /// Assign contents of another height client lead sub grid to this one
    /// </summary>
    /// <param name="heightLeaf"></param>
    public void Assign(ClientHeightLeafSubGrid heightLeaf)
    {
      base.Assign(heightLeaf);

      Buffer.BlockCopy(heightLeaf.Cells, 0, Cells, 0, SubGridTreeConsts.SubGridTreeCellsPerSubgrid * sizeof(float));

      SurveyedSurfaceMap.Assign(heightLeaf.SurveyedSurfaceMap);
    }


    /// <summary>
    /// Determine if a filtered height is valid (not null)
    /// </summary>
    /// <param name="filteredValue"></param>
    /// <returns></returns>
    public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue) => filteredValue.FilteredPass.Height == Consts.NullHeight;

    /// <summary>
    /// Assign filtered height value from a filtered pass to a cell
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <param name="Context"></param>
    public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context)
    {
      Cells[cellX, cellY] = Context.FilteredValue.FilteredPassData.FilteredPass.Height;
    }

    /// <summary>
    /// Fills the contents of the client leaf subgrid with a known, non-null test pattern of values
    /// </summary>
    public override void FillWithTestPattern() => ForEach((x, y) => Cells[x, y] = x + y);

    /// <summary>
    /// Determines if the leaf content of this subgrid is equal to 'other'
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool LeafContentEquals(IClientLeafSubGrid other)
    {
      bool result = true;

      IGenericClientLeafSubGrid<float> _other = (IGenericClientLeafSubGrid<float>)other;
      ForEach((x, y) => result &= Cells[x, y] == _other.Cells[x, y]);

      return result;
    }

    /// <summary>
    /// Determines if the height at the cell location is null or not.
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <returns></returns>
    public override bool CellHasValue(byte cellX, byte cellY) => Cells[cellX, cellY] != Consts.NullHeight;

    /// <summary>
    /// Provides a copy of the null value defined for cells in this client leaf subgrid
    /// </summary>
    /// <returns></returns>
    public override float NullCell() => Consts.NullHeight;

    /// <summary>
    /// Sets all cell heights to null and clears the first pass and surveyed surface pass maps
    /// </summary>
    public override void Clear()
    {
      base.Clear();

      FirstPassMap.Clear();
      SurveyedSurfaceMap.Clear();
    }

    /// <summary>
    /// Dumps elevations from subgrid to the log
    /// </summary>
    /// <param name="title"></param>
    public override void DumpToLog(string title)
    {
      base.DumpToLog(title);
      /*
       * var
        I, J : Integer;
        S : String;
      begin
        SIGLogMessage.PublishNoODS(Nil, Format('Dump of height map for subgrid %s', [Moniker]) , slmcDebug);

        for I := 0 to kSubGridTreeDimension - 1 do
          begin
            S := Format('%2d:', [I]);

            for J := 0 to kSubGridTreeDimension - 1 do
              if CellHasValue(I, J) then
                S := S + Format('%9.3f', [Cells[I, J]])
              else
                S := S + '     Null';

            SIGLogMessage.PublishNoODS(Nil, S, slmcDebug);
          end;
      end;
      */
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

      FirstPassMap.Write(writer, buffer);
      SurveyedSurfaceMap.Write(writer, buffer);

      Buffer.BlockCopy(Cells, 0, buffer, 0, SubGridTreeConsts.SubGridTreeCellsPerSubgrid * sizeof(float));
      writer.Write(buffer, 0, SubGridTreeConsts.SubGridTreeCellsPerSubgrid * sizeof(float));
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

      FirstPassMap.Read(reader, buffer);
      SurveyedSurfaceMap.Read(reader, buffer);

      reader.Read(buffer, 0, SubGridTreeConsts.SubGridTreeCellsPerSubgrid * sizeof(float));
      Buffer.BlockCopy(buffer, 0, Cells, 0, SubGridTreeConsts.SubGridTreeCellsPerSubgrid * sizeof(float));
    }

    /// <summary>
    /// Sets all elevations in the height client leaf sub grid to zero (not null)
    /// </summary>
    public void SetToZeroHeight() => ForEach((x, y) => Cells[x, y] = 0);

    /// <summary>
    /// Return an indicative size for memory consumption of this class to be used in cache tracking
    /// </summary>
    /// <returns></returns>
    public override int IndicativeSizeInBytes()
    {
      return base.IndicativeSizeInBytes() + 
             FirstPassMap.IndicativeSizeInBytes() + 
             SurveyedSurfaceMap.IndicativeSizeInBytes() +
             SubGridTreeConsts.SubGridTreeCellsPerSubgrid * sizeof(float);
    }

    /// <summary>
    /// Assign cell information from a previously cached result held in the general subgrid result cache
    /// using the supplied map to control which cells from the caches subgrid should be copied into this
    /// client leaf sub grid
    /// </summary>
    /// <param name="source"></param>
    /// <param name="map"></param>
    public override void AssignFromCachedPreProcessedClientSubgrid(ISubGrid source, SubGridTreeBitmapSubGridBits map)
    {
      var subGrid = (ClientHeightLeafSubGrid)source;

      if (map.IsFull())
        Array.Copy(subGrid.Cells, Cells, SubGridTreeConsts.CellsPerSubgrid);
      else
        map.ForEachSetBit((x, y) => Cells[x, y] = subGrid.Cells[x, y]);
    }
  }
}
