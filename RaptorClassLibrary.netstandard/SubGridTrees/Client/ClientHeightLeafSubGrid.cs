using log4net;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees.Client
{
    /// <summary>
    /// The content of each cell in a height client leaf sub grid. Each cell stores an elevation only.
    /// </summary>
    [Serializable]
    public class ClientHeightLeafSubGrid : GenericClientLeafSubGrid<float>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
        /// Constructor. Set the grid to HeightAndTime.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        /// <param name="cellSize"></param>
        /// <param name="indexOriginOffset"></param>
        public ClientHeightLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
        {
            _gridDataType = Raptor.Types.GridDataType.Height;
        }

        
        /// <summary>
        /// Assign contents of another height client lead sub grid to this one
        /// </summary>
        /// <param name="heightAndTimeResults"></param>
        public void Assign(ClientHeightAndTimeLeafSubGrid heightAndTimeResults)
        {
            base.Assign(heightAndTimeResults);

            Buffer.BlockCopy(heightAndTimeResults.Cells, 0, Cells, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(float));

            SurveyedSurfaceMap.Assign(heightAndTimeResults.SurveyedSurfaceMap);
        }
        

        /// <summary>
        /// Assign contents of another height client lead sub grid to this one
        /// </summary>
        /// <param name="heightLeaf"></param>
        public void Assign(ClientHeightLeafSubGrid heightLeaf)
        {
            base.Assign(heightLeaf);

            Buffer.BlockCopy(heightLeaf.Cells, 0, Cells, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(float));

            SurveyedSurfaceMap.Assign(heightLeaf.SurveyedSurfaceMap);
        }


        /// <summary>
        /// Determine if a filtered height is valid (not null)
        /// </summary>
        /// <param name="filteredValue"></param>
        /// <returns></returns>
        public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue) => filteredValue.FilteredPass.Height == Consts.NullSingle;

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
        /// Determines if the height at the cell location is null or not.
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <returns></returns>
        public override bool CellHasValue(byte cellX, byte cellY) => Cells[cellX, cellY] != Consts.NullHeight;

        /// <summary>
        /// An array containing the content of a fully null subgrid
        /// </summary>
        public static float[,] nullCells = NullHeights();

        /// <summary>
        /// Sets all cell heights to null and clears the first pass and surveyed surface pass maps
        /// </summary>
        public override void Clear()
        {
            if (Cells == null)
            {
                base.Clear();
            }

            Buffer.BlockCopy(nullCells, 0, Cells, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(float));

            FirstPassMap.Clear();
            SurveyedSurfaceMap.Clear();
        }

        private static float[,] NullHeights()
        {
            float[,] result = new float[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

            ForEach((x, y) => result[x, y] = Consts.NullHeight);

            return result;
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
        /// Reads an elevation client leaf sub grid from a stream using a binary formatter
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="stream"></param>
        public override void Read(BinaryFormatter formatter, Stream stream)
        {
            base.Read(formatter, stream);

            FirstPassMap = (SubGridTreeBitmapSubGridBits)formatter.Deserialize(stream);
            SurveyedSurfaceMap = (SubGridTreeBitmapSubGridBits)formatter.Deserialize(stream);
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
            formatter.Serialize(stream, SurveyedSurfaceMap);
        }

        /// <summary>
        /// Write the contents of the Items array using the supplied writer
        /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
        /// Override to implement if needed.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="buffer"></param>
        public override void Write(BinaryWriter writer, byte [] buffer)
        {
            base.Write(writer, buffer);

            FirstPassMap.Write(writer, buffer);
            SurveyedSurfaceMap.Write(writer, buffer);

            Buffer.BlockCopy(Cells, 0, buffer, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(float));
            writer.Write(buffer, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(float));
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

            reader.Read(buffer, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(float));
            Buffer.BlockCopy(buffer, 0, Cells, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(float));
        }

        /// <summary>
        /// Sets all elevations in the height client leaf sub grid to zero (not null)
        /// </summary>
        public void SetToZeroHeight() => ForEach((x, y) => Cells[x, y] = 0); // TODO: Optimisation: Use single array assignment
    }
}
