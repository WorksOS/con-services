namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  public abstract class ProfileResultBase
  {
    /// <summary>
    /// The type of cell, either a cell edge intersection or the mid point of a segment cutting the cell. A edge can also be the start of a gap in the data.
    /// </summary>
    public ProfileCellType cellType;

    /// <summary>
    /// The station value, or distance from start of the profile line at which the profile line intersects this cell for cell edges 
    /// or the mid point of the line segment cutting through the cell for mid point type cells.
    /// </summary>
    public double station;

    protected ProfileResultBase()
    { }
  }
}