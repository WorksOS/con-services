namespace VSS.TRex.Profiling.Models
{
  /// <summary>
  /// ProfileCellState is a package of information relating to one cell in a profile drawn across IC data
  /// </summary>
  public class ProfileCellState : ProfileCellStateBase
  {
    /// <summary>
    /// A collection of layers constituting a profile through a cell.
    /// Depending on the context, the layers may be equivalent to the passes over a cell
    /// or may represent the lifts over a cell, in which case the Passes collection
    /// for an individual layer will contain the passes making up that lift.
    /// </summary>
    //public IProfileLayers Layers; // May need to expose this as a generic object reference when needed

    public float CellLowestElev;
    public float CellHighestElev;
    public float CellLastElev;
    public float CellFirstElev;
    public float CellLowestCompositeElev;
    public float CellHighestCompositeElev;
    public float CellLastCompositeElev;
    public float CellFirstCompositeElev;

    public short CellCCV, CellTargetCCV, CellPreviousMeasuredCCV, CellPreviousMeasuredTargetCCV;
    public float CellCCVElev;

    public short CellMDP, CellTargetMDP;
    public float CellMDPElev;

    public byte CellCCA;
    public short CellTargetCCA;
    public float CellCCAElev;

    public float CellTopLayerThickness;
    public bool IncludesProductionData;

    public ushort TopLayerPassCount;
    public ushort TopLayerPassCountTargetRangeMin;
    public ushort TopLayerPassCountTargetRangeMax;

    public ushort CellMaxSpeed;
    public ushort CellMinSpeed;

    /// <summary>
    // Passes contains the entire list of passes that all the layers in the layer collection refer to
    /// </summary>
    // public FilteredMultiplePassInfo Passes; // May have to expose as a generic object reference when required

    public bool[] FilteredPassFlags = new bool[0];
    public int FilteredPassCount;
    public int FilteredHalfPassCount;
    public ProfileCellAttributeExistenceFlags AttributeExistenceFlags;

    public ushort CellMaterialTemperature;
    public ushort CellMaterialTemperatureWarnMin, CellMaterialTemperatureWarnMax;
    public float CellMaterialTemperatureElev;
  }

}
