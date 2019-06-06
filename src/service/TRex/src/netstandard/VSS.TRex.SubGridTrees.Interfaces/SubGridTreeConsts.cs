namespace VSS.TRex.SubGridTrees.Interfaces
{
  public static class SubGridTreeConsts
  {
    /// <summary>
    /// We use 6 levels in our sub grid tree (Root + 4 intermediary + 1 on-the-ground)
    /// </summary>
    public const byte SubGridTreeLevels = 6;

    /// <summary>
    /// The default cell size used when creating a new site model
    /// </summary>
    public const double DefaultCellSize = 0.34; // Units: meters

    /// <summary>
    /// Denotes how many bits of an on-the-ground cell X or Y index reference each level in the sub grid tree represents
    /// </summary>
    public const byte SubGridIndexBitsPerLevel = 5;

    /// <summary>
    /// The number of cells that are in a sub grid X and Y dimension (square grid)
    /// </summary>
    public const byte SubGridTreeDimension = 1 << SubGridIndexBitsPerLevel;

    /// <summary>
    /// The number of cells that are in a sub grid X and Y dimension (square grid), minus 1
    /// </summary>
    public const byte SubGridTreeDimensionMinus1 = (1 << SubGridIndexBitsPerLevel) - 1;

    /// <summary>
    /// The number of cells contained within a sub grid
    /// </summary>
    public const int SubGridTreeCellsPerSubGrid = SubGridTreeDimension * SubGridTreeDimension;

    /// <summary>
    /// The number of cells on-the-ground a single cell in the
    /// root a sub grid tree node spans in the X and Y dimensions. Each level down
    /// will represent a smaller fraction of these on-the-ground cells given by
    /// (1/ kSubGridTreeDimension)
    /// </summary>
    public const uint RootSubGridCellSize = 1 << ((SubGridTreeLevels - 1) * SubGridIndexBitsPerLevel);

    /// <summary>
    /// The mask required to extract the portion of a sub grid key relating to a single level in the tree
    /// This is 0b00011111 (ie: five bits of an X/Y component of the key)
    /// </summary>
    public const int SubGridLocalKeyMask = 0b00011111; //0x1F;

    /// <summary>
    /// The mask required to extract the portion of a sub grid key relating to level in the tree that is the
    /// parent of the sub grid key identified by SubGridLocalKeyMask
    /// This is 0b00011111 (ie: five bits of an X/Y component of the key)
    /// </summary>
    public const int SubGridLocalParentKeyMask = 0b1111100000; //0x1F00;

    /// <summary>
    /// The number of cells (representing node sub grids, leaf sub grids or on-the-ground cells) that
    /// are represented within a sub grid
    /// </summary>
    public const uint CellsPerSubGrid = SubGridTreeDimension * SubGridTreeDimension;

    /// <summary>
    /// The default cell index offset to translate the
    /// centered world coordinate origin into the bottom left grid origin
    /// This may also be thought of as the distance in on-the-ground cells from the
    /// bottom left hand corner (origin) of the grid, to the cell whose bottom
    /// left hand corner is at the exact center of the grid. This offset is
    /// applied to both the X and Y dimensions of the grid.
    /// </summary>
    public const uint DefaultIndexOriginOffset = 1 << ((SubGridTreeLevels * SubGridIndexBitsPerLevel) - 1);
  }
}
