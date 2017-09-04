using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;

namespace VSS.VisionLink.Raptor.SubGridTrees.Client
{   
    /// <summary>
    /// The content of each cell in a temperature client leaf sub grid. Each cell stores a temperature only.
    /// </summary>
    public class ClientTemperatureLeafSubGrid : GenericClientLeafSubGrid<ushort>
    {
        /// <summary>
        /// First pass map records which cells hold cell pass machine speeds that were derived
        /// from the first pass a machine made over the corresponding cell
        /// </summary>
        public SubGridTreeBitmapSubGridBits FirstPassMap = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);

        /// <summary>
        /// Constructor. Set the grid to HeightAndTime.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        /// <param name="cellSize"></param>
        /// <param name="indexOriginOffset"></param>
        public ClientTemperatureLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
        {
            GridDataType = Raptor.Types.GridDataType.Temperature;
        }

        /// <summary>
        /// Determine if a filtered machine speed is valid (not null)
        /// </summary>
        /// <param name="filteredValue"></param>
        /// <returns></returns>
        public override bool AssignableFilteredValueIsNull(FilteredPassData filteredValue) => filteredValue.FilteredPass.MaterialTemperature == CellPass.NullMaterialTemp;

        /// <summary>
        /// Assign filtered height value from a filtered pass to a cell
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <param name="Context"></param>
        public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context)
        {
            Cells[cellX, cellY] = Context.FilteredValue.FilteredPassData.FilteredPass.MaterialTemperature;
        }

        /// <summary>
        /// Determines if the height at the cell location is null or not.
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <returns></returns>
        public override bool CellHasValue(byte cellX, byte cellY) => Cells[cellX, cellY] != CellPass.NullMaterialTemp;

        /// <summary>
        /// Sets all cell heights to null and clears the first pass and sureyed surface pass maps
        /// </summary>
        public override void Clear()
        {
            base.Clear();

            ForEach((x, y) => Cells[x, y] = CellPass.NullMaterialTemp); // TODO: Optimisation: Use PassData_MachineSpeed_Null assignment as in current gen;

            FirstPassMap.Clear();
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
              SIGLogMessage.PublishNoODS(Nil, Format('Dump of machine speed map for subgrid %s', [Moniker]) , slmcDebug);

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
        /// Reads an elevation client leaf sub grid from a stream using a binary formatter
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="stream"></param>
        public override void Read(BinaryFormatter formatter, Stream stream)
        {
            base.Read(formatter, stream);

            FirstPassMap = (SubGridTreeBitmapSubGridBits)formatter.Deserialize(stream);
        }

        /// <summary>
        /// Writes an elevation client leaf sub grid to a stream using a binary formatter
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="stream"></param>
        public override void Write(BinaryFormatter formatter, Stream stream)
        {
            base.Write(formatter, stream);

            formatter.Serialize(stream, FirstPassMap);
        }

        /// <summary>
        /// Write the contents of the Items array using the supplied writer
        /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
        /// Override to implement if needed.
        /// </summary>
        /// <param name="writer"></param>
        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);

            FirstPassMap.Write(writer);

            SubGridUtilities.SubGridDimensionalIterator((x, y) => writer.Write(Cells[x, y]));
        }

        /// <summary>
        /// Fill the items array by reading the binary representation using the provided reader. 
        /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
        /// Override to implement if needed.
        /// </summary>
        /// <param name="reader"></param>
        public override void Read(BinaryReader reader)
        {
            base.Read(reader);

            FirstPassMap.Read(reader);

            SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y] = reader.ReadUInt16());
        }
    }
}
