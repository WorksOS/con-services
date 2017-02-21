using VSS.Raptor.Service.Common.Contracts;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling
{
  public class PatchResult 
    {
   /// <summary>
    /// Private constructor
    /// </summary>
      private PatchResult()
    {}


        public byte[] PatchData { get; private set; }

        /// <summary>
        /// Create instance of PatchResult
        /// </summary>
        public static PatchResult CreatePatchResult(byte[] data)
        {
          return new PatchResult
          {
            PatchData = data,
          };
        }
    }


    /// <summary>
    /// A single cell of information within a subgrid in a patch
    /// </summary>
    public class PatchCellResult
    {
       /// <summary>
    /// Private constructor
    /// </summary>
      private PatchCellResult()
      {}

        /// <summary>
        /// Elevation at the cell center point
        /// </summary>
        public float elevation { get; private set; }

        /// <summary>
        /// Requested thematic datum. Intepretation and parsing depends on the thematic domain
        /// </summary>
        public ushort datum { get; private set; }

        /// <summary>
        /// The color the cell is rendered in. Only present if renderColorValues is true.
        /// </summary>
        public uint color { get; private set; }

        /// <summary>
        /// Create instance of PatchCellResult
        /// </summary>
        public static PatchCellResult CreatePatchCellResult(
          float elevation,
          ushort datum,
          uint color)
        {
          return new PatchCellResult
          {
            elevation = elevation,
            datum = datum,
            color = color
          };
        }
    }

    /// <summary>
    /// A subgrid of information within a patch result
    /// </summary>
    public class PatchSubgridResult
    {
           /// <summary>
    /// Private constructor
    /// </summary>
      private PatchSubgridResult()
      {}

        /// <summary>
        /// The X ordinate location of the cell in the catersian coorindate space of cell address indexes centered on the catersian grid coordinate system of the project
        /// NOTE: As of the time of writing the cell origin positions returned need to be adjusted by the subgridtree IndexOriginOffset value - this should be taken into account when the subgrid is written into the patch.
        /// </summary>
        public int cellOriginX { get; private set; }

        /// <summary>
        /// The X ordinate location of the cell in the catersian coorindate space of cell address indexes centered on the catersian grid coordinate system of the project
        /// NOTE: As of the time of writing the cell origin positions returned need to be adjusted by the subgridtree IndexOriginOffset value - this should be taken into account when the subgrid is written into the patch.
        /// </summary>
        public int cellOriginY { get; private set; }

        /// <summary>
        /// If true there are no non-null cells of information retruned by the query for this subgrid.
        /// </summary>
        public bool isNull { get; private set; }

        /// <summary>
        /// The elevation origin referenced by all cell elevations in the binary representation of the patch subgrids. Values are expressed in meters.
        /// </summary>
        public float elevationOrigin { get; private set; }

        /// <summary>
        /// The grid of cells that make up this subgrid in the patch
        /// </summary>
        public PatchCellResult[,] cells { get; private set; }

        /// <summary>
        /// Create instance of PatchSubgridResult
        /// </summary>
        public static PatchSubgridResult CreatePatchSubgridResult(
          int cellOriginX,
          int cellOriginY,
          bool isNull,
          float elevationOrigin,
          PatchCellResult[,] cells)
        {
          return new PatchSubgridResult
          {
            cellOriginX = cellOriginX,
            cellOriginY = cellOriginY,
            isNull = isNull,
            elevationOrigin = elevationOrigin,
            cells = cells
          };
        }
    }

    /// <summary>
    /// A structured representation of the data retruned by the Patch request
    /// </summary>
    public class PatchResultStructured : ContractExecutionResult
    {

   /// <summary>
    /// Private constructor
    /// </summary>
      private PatchResultStructured()
    {}

        /// <summary>
        /// All cells in the patch are of this size. All measurements relating to the cell in the patch are made at the center point of each cell.
        /// </summary>
        public double cellSize { get; private set; }

        /// <summary>
        /// The number of subgrids returned in this patch request
        /// </summary>
        public int numSubgridsInPatch { get; private set; }

        /// <summary>
        /// The total number of patch requests that must be made to retrieve all the information identified by the parameters of the patch query. Only returned for requests
        /// that identify patch number 0 in the set to be retrieved.
        /// </summary>
        public int totalNumPatchesRequired { get; private set; }

        /// <summary>
        /// The cells in theh subgrids in the patch result have had colors rendered for the thematic data in the cells.
        /// </summary>
        public bool valuesRenderedToColors { get; private set; }

        /// <summary>
        /// The collection of subgrids returned in this patch request result.
        /// </summary>
        public PatchSubgridResult[] subgrids { get; private set; }

        /// <summary>
        /// Create instance of PatchResultStructured
        /// </summary>
        public static PatchResultStructured CreatePatchResultStructured(
          double cellSize,
          int numSubgridsInPatch,
          int totalNumPatchesRequired,
          bool valuesRenderedToColors,
          PatchSubgridResult[] subgrids
          )
        {
          return new PatchResultStructured
          {
            cellSize = cellSize,
            numSubgridsInPatch = numSubgridsInPatch,
            totalNumPatchesRequired = totalNumPatchesRequired,
            valuesRenderedToColors = valuesRenderedToColors,
            subgrids = subgrids,
          };
        }

        /// <summary>
        /// Create example instance of PatchResultStructured to display in Help documentation.
        /// </summary>
        public static PatchResultStructured HelpSample
        {
          get
          {
            return new PatchResultStructured()
            {
            };
          }
        }
    }

}